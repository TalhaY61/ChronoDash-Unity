using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ChronoDash.Managers;

namespace ChronoDash.UI
{
    /// <summary>
    /// Main menu UI controller.
    /// Handles navigation to other scenes.
    /// </summary>
    public class MainMenuUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Button playButton;
        [SerializeField] private Button howToPlayButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button quitButton;
        
        [Header("Title (Optional)")]
        [SerializeField] private TextMeshProUGUI titleText;
        
        private void Start()
        {
            SetupButtons();
            
            if (titleText != null)
            {
                titleText.text = "CHRONO DASH";
            }
        }
        
        private void SetupButtons()
        {
            if (playButton != null)
            {
                playButton.onClick.AddListener(OnPlayClicked);
            }
            
            if (howToPlayButton != null)
            {
                howToPlayButton.onClick.AddListener(OnHowToPlayClicked);
            }
            
            if (settingsButton != null)
            {
                settingsButton.onClick.AddListener(OnSettingsClicked);
            }
            
            if (quitButton != null)
            {
                quitButton.onClick.AddListener(OnQuitClicked);
            }
        }
        
        private void OnPlayClicked()
        {
            Debug.Log("▶️ Play button clicked");
            SceneController.Instance.LoadScene(SceneController.GAMEPLAY);
        }
        
        private void OnHowToPlayClicked()
        {
            Debug.Log("❓ How To Play button clicked");
            SceneController.Instance.LoadScene(SceneController.HOW_TO_PLAY);
        }
        
        private void OnSettingsClicked()
        {
            Debug.Log("⚙️ Settings button clicked");
            SceneController.Instance.LoadScene(SceneController.SETTINGS);
        }
        
        private void OnQuitClicked()
        {
            Debug.Log("🚪 Quit button clicked");
            SceneController.Instance.QuitGame();
        }
        
        private void OnDestroy()
        {
            // Clean up listeners
            if (playButton != null)
                playButton.onClick.RemoveListener(OnPlayClicked);
            if (howToPlayButton != null)
                howToPlayButton.onClick.RemoveListener(OnHowToPlayClicked);
            if (settingsButton != null)
                settingsButton.onClick.RemoveListener(OnSettingsClicked);
            if (quitButton != null)
                quitButton.onClick.RemoveListener(OnQuitClicked);
        }
    }
}
