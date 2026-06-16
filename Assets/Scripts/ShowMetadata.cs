using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;   // Added for New Input System
using UnityEngine.EventSystems;  // Added for UI protection

public class ShowMetadata : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject entryPrefab;
    [SerializeField] private ScrollRect entriesPanel;
    private List<GameObject> entries = new List<GameObject>();

    [Header("Position")]
    [SerializeField] private TextMeshProUGUI corHeader;
    [SerializeField] private TextMeshProUGUI xcor;
    [SerializeField] private TextMeshProUGUI ycor;
    [SerializeField] private TextMeshProUGUI zcor;

    private GameObject lastSelectedObject; 
    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;
        
        if (entryPrefab != null) 
            entryPrefab.SetActive(false);
            
        ClearUI(); // Ensure the UI is clean on start
    }

    void Update()
    {
        // Safety checks
        if (Mouse.current == null || mainCamera == null) return;

        // Check if the Left Mouse Button was clicked this exact frame
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            // CRITICAL: Prevent the raycast if the user is clicking on a UI element (like the scrollbar)
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                return;

            Vector2 mousePos = Mouse.current.position.ReadValue();
            Ray ray = mainCamera.ScreenPointToRay(mousePos);

            // Shoot the raycast to see what we clicked
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                GameObject clickedObject = hit.collider.gameObject;

                // Only update everything if we clicked a DIFFERENT object
                if (clickedObject != lastSelectedObject)
                {
                    lastSelectedObject = clickedObject;
                    OnObjectSelected(clickedObject);
                }
            }
            else
            {
                // We clicked on empty space. Clear the selection and the UI.
                if (lastSelectedObject != null)
                {
                    lastSelectedObject = null;
                    ClearUI();
                }
            }
        }
    }

    private void OnObjectSelected(GameObject selectedObj)
    {
        CleanEntries();
        if (selectedObj != null)
        {
            ShowingMetaData(selectedObj);
        }
    }

    private void ShowingMetaData(GameObject obj)
    {
        corHeader.text = obj.name;
        xcor.text = "X: " + Mathf.Round(obj.transform.position.x);
        ycor.text = "Y: " + Mathf.Round(obj.transform.position.y);
        zcor.text = "Z: " + Mathf.Round(obj.transform.position.z);

        // Check for your specific PiXYZ Metadata component
        if (obj.TryGetComponent(out Pixyz.ImportSDK.Metadata pixyzMetadata))
        {
            foreach (var property in pixyzMetadata.getProperties())
            {
                GameObject newEntry = Instantiate(entryPrefab, entriesPanel.content);
                newEntry.SetActive(true);
                newEntry.transform.Find("Key").GetComponentInChildren<TextMeshProUGUI>().SetText(property.Key);
                newEntry.transform.Find("Value").GetComponentInChildren<TextMeshProUGUI>().SetText(property.Value);
                entries.Add(newEntry);
            }
        }
    }

    private void CleanEntries()
    {
        if (entries.Count == 0) return;
        foreach (var entry in entries)
        {
            Destroy(entry);
        }
        entries.Clear();
    }

    // Helper method to reset the UI text when nothing is selected
    private void ClearUI()
    {
        CleanEntries();
        corHeader.text = "No Object Selected";
        xcor.text = "X: --";
        ycor.text = "Y: --";
        zcor.text = "Z: --";
    }
}