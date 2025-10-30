using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace ChronoDash.UI
{
    /// <summary>
    /// Automatically selects a UI element when the scene starts.
    /// This enables keyboard navigation (arrow keys, Enter/Space).
    /// Attach this to any UI controller (MainMenuUI, SettingsUI, etc.).
    /// Follows Single Responsibility Principle - only handles initial UI selection.
    /// </summary>
    public class UIFirstSelected : MonoBehaviour
    {
        [Header("First Selected Element")]
        [Tooltip("The UI element to select when this scene/menu loads")]
        [SerializeField] private Selectable firstSelected;
        
        private void Start()
        {
            SelectFirstElement();
        }
        
        private void OnEnable()
        {
            // Re-select when menu becomes active
            SelectFirstElement();
        }
        
        private void SelectFirstElement()
        {
            if (firstSelected == null)
            {
                Debug.LogWarning("⚠️ UIFirstSelected: No first selected element assigned!");
                return;
            }
            
            // Ensure EventSystem exists
            if (EventSystem.current == null)
            {
                Debug.LogError("❌ No EventSystem found in scene! Add one via: GameObject → UI → Event System");
                return;
            }
            
            // Clear any previous selection
            EventSystem.current.SetSelectedGameObject(null);
            
            // Select the first element
            firstSelected.Select();
            
            Debug.Log($"🎯 Selected first UI element: {firstSelected.name}");
        }
    }
}
