using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;   
using UnityEngine.EventSystems;  
// Added Enhanced Touch
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

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

    // Enable touch tracking
    private void OnEnable() { EnhancedTouchSupport.Enable(); }
    private void OnDisable() { EnhancedTouchSupport.Disable(); }

    void Start()
    {
        mainCamera = Camera.main;
        
        if (entryPrefab != null) 
            entryPrefab.SetActive(false);
            
        ClearUI(); 
    }

    void Update()
    {
        if (mainCamera == null) return;

        bool isClicking = false;
        Vector2 clickPosition = Vector2.zero;
        bool isOverUI = false;

        // 1. Check for Touch (Mobile)
        if (Touch.activeTouches.Count > 0)
        {
            Touch touch = Touch.activeTouches[0];
            // Only trigger on the exact frame the finger touches the screen
            if (touch.phase == UnityEngine.InputSystem.TouchPhase.Began)
            {
                isClicking = true;
                clickPosition = touch.screenPosition;
                
                // Touch-specific UI check using the finger's unique ID
                if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(touch.touchId))
                    isOverUI = true;
            }
        }
        // 2. Check for Mouse (PC)
        else if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            isClicking = true;
            clickPosition = Mouse.current.position.ReadValue();
            
            // Mouse-specific UI check
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                isOverUI = true;
        }

        // 3. Process the Raycast if a valid click/tap happened
        if (isClicking)
        {
            if (isOverUI) return; // Prevent raycast if touching the scrollbar or text

            Ray ray = mainCamera.ScreenPointToRay(clickPosition);

            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                GameObject clickedObject = hit.collider.gameObject;

                if (clickedObject != lastSelectedObject)
                {
                    lastSelectedObject = clickedObject;
                    OnObjectSelected(clickedObject);
                }
            }
            else
            {
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

    private void ClearUI()
    {
        CleanEntries();
        corHeader.text = "No Object Selected";
        xcor.text = "X: --";
        ycor.text = "Y: --";
        zcor.text = "Z: --";
    }
}