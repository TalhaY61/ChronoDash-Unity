using UnityEngine;
using UnityEngine.SceneManagement;

namespace ChronoDash.Managers
{
    /// <summary>
    /// Handles scene transitions.
    /// Follows Single Responsibility Principle - only manages scene loading.
    /// </summary>
    public class SceneController : MonoBehaviour
    {
        private static SceneController instance;
        
        public static SceneController Instance
        {
            get
            {
                if (instance == null)
                {
                    GameObject go = new GameObject("SceneController");
                    instance = go.AddComponent<SceneController>();
                    DontDestroyOnLoad(go);
                }
                return instance;
            }
        }
        
        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else if (instance != this)
            {
                Destroy(gameObject);
            }
        }
        
        /// <summary>
        /// Load a scene by name.
        /// </summary>
        public void LoadScene(string sceneName)
        {
            Debug.Log($"🔄 Loading scene: {sceneName}");
            Time.timeScale = 1f; // Reset time scale when changing scenes
            SceneManager.LoadScene(sceneName);
        }
        
        /// <summary>
        /// Load scene by build index.
        /// </summary>
        public void LoadScene(int sceneIndex)
        {
            Debug.Log($"🔄 Loading scene index: {sceneIndex}");
            Time.timeScale = 1f;
            SceneManager.LoadScene(sceneIndex);
        }
        
        /// <summary>
        /// Reload the current scene.
        /// </summary>
        public void ReloadCurrentScene()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
        
        /// <summary>
        /// Quit the application.
        /// </summary>
        public void QuitGame()
        {
            Debug.Log("🚪 Quitting game...");
            
            #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
            #else
                Application.Quit();
            #endif
        }
        
        // Scene name constants for easy reference
        public const string MAIN_MENU = "MainMenu";
        public const string GAMEPLAY = "Gameplay";
        public const string HOW_TO_PLAY = "HowToPlay";
        public const string SETTINGS = "Settings";
    }
}
