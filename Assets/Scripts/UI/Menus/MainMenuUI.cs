using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ChronoDash.Managers;

namespace ChronoDash.UI {
    public class MainMenuUI : MonoBehaviour {
        [Header("UI References")]
        [SerializeField] private Button playButton;
        [SerializeField] private Button howToPlayButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button quitButton;
        
        private void Start() {
            SetupButtons();
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
            
            if (quitButton != null)
            {
                quitButton.onClick.AddListener(OnQuitClicked);
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
        
        private void OnQuitClicked() {
            SceneController.Instance.QuitGame();
        }
        
        private void OnDestroy() {
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
