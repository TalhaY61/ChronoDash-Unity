using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ChronoDash.Managers;
using ChronoDash.Core.Auth;

namespace ChronoDash.UI {
    public class MainMenuUI : MonoBehaviour {
        [Header("UI References")]
        [SerializeField] private Button playButton;
        [SerializeField] private Button howToPlayButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button authButton;
        [SerializeField] private Button quitButton;
        
        [Header("Auth Panel")]
        [SerializeField] private GameObject authPanel;
        [SerializeField] private AuthPanelUI authPanelUI;
        
        private void Start() {
            SetupButtons();
            UpdateAuthButtonText();
        }
        
        private void OnEnable() {
            UpdateAuthButtonText();
        }
        
        private void SetupButtons() {
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
            
            if (authButton != null)
            {
                authButton.onClick.AddListener(OnAuthClicked);
                
                // Hide auth button if in offline mode
                if (AuthManager.Instance != null && AuthManager.Instance.IsOfflineMode)
                {
                    authButton.gameObject.SetActive(false);
                }
            }
            
            if (quitButton != null)
            {
                quitButton.onClick.AddListener(OnQuitClicked);
            }
            
            // Hide auth panel by default
            if (authPanel != null)
            {
                authPanel.SetActive(false);
            }
        }
        
        private void OnPlayClicked() {
            SceneController.Instance.LoadScene(SceneController.GAMEPLAY);
        }
        
        private void OnHowToPlayClicked() {
            SceneController.Instance.LoadScene(SceneController.HOW_TO_PLAY);
        }
        
        private void OnSettingsClicked() {
            SceneController.Instance.LoadScene(SceneController.SETTINGS);
        }
        
        private void OnAuthClicked() {
            if (authPanel != null)
            {
                authPanel.SetActive(true);
            }
        }
        
        private void OnQuitClicked() {
            SceneController.Instance.QuitGame();
        }
        
        private void UpdateAuthButtonText() {
            if (authButton == null) return;
            
            // Get the TextMeshProUGUI component from the button
            TextMeshProUGUI buttonText = authButton.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText == null) return;
            
            // Change text based on authentication status
            if (AuthManager.Instance != null && AuthManager.Instance.IsAuthenticated)
            {
                buttonText.text = "PROFILE";
            }
            else
            {
                buttonText.text = "LOGIN";
            }
        }
        
        private void OnDestroy() {
            // Clean up listeners
            if (playButton != null)
                playButton.onClick.RemoveListener(OnPlayClicked);
            if (howToPlayButton != null)
                howToPlayButton.onClick.RemoveListener(OnHowToPlayClicked);
            if (settingsButton != null)
                settingsButton.onClick.RemoveListener(OnSettingsClicked);
            if (authButton != null)
                authButton.onClick.RemoveListener(OnAuthClicked);
            if (quitButton != null)
                quitButton.onClick.RemoveListener(OnQuitClicked);
        }
    }
}
