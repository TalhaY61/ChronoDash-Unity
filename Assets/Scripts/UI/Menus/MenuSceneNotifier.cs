using UnityEngine;
using UnityEngine.SceneManagement;

namespace ChronoDash.Managers {
    /// <summary>
    /// Notifies MenuBackgroundManager when menu scenes load.
    /// Attach this to any GameObject in menu scenes (MainMenu, Settings, HowToPlay).
    /// Follows Single Responsibility Principle - only handles scene load notification.
    /// </summary>
    public class MenuSceneNotifier : MonoBehaviour {
        private void Start() {
            // Notify MenuBackgroundManager that scene has loaded
            if (MenuBackgroundManager.Instance != null) {
                MenuBackgroundManager.Instance.OnSceneLoaded();
            }
        }
    }
}
