using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ChronoDash.Managers;

namespace ChronoDash.UI
{
    /// <summary>
    /// Settings screen UI controller.
    /// Allows players to toggle sound and VFX.
    /// </summary>
    public class SettingsUI : MonoBehaviour
    {
        [Header("Navigation")]
        [SerializeField] private Button backButton;
        
        [Header("Music Volume")]
        [SerializeField] private Slider musicVolumeSlider;
        [SerializeField] private TextMeshProUGUI musicVolumeLabel;
        [SerializeField] private TextMeshProUGUI musicVolumeValue;
        
        [Header("Music Toggle")]
        [SerializeField] private Toggle musicToggle;
        [SerializeField] private TextMeshProUGUI musicToggleLabel;
        
        [Header("VFX Toggle")]
        [SerializeField] private Toggle vfxToggle;
        [SerializeField] private TextMeshProUGUI vfxToggleLabel;
        
        private void Start()
        {
            SetupButtons();
            LoadCurrentSettings();
        }
        
        private void SetupButtons()
        {
            // Back button
            if (backButton != null)
            {
                backButton.onClick.AddListener(OnBackClicked);
            }
            
            // Music volume slider
            if (musicVolumeSlider != null)
            {
                musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
            }
            
            // Music toggle
            if (musicToggle != null)
            {
                musicToggle.onValueChanged.AddListener(OnMusicToggled);
            }
            
            // VFX toggle
            if (vfxToggle != null)
            {
                vfxToggle.onValueChanged.AddListener(OnVFXToggled);
            }
        }
        
        private void LoadCurrentSettings()
        {
            float musicVolume = AudioManager.Instance.MusicVolume;
            bool musicEnabled = AudioManager.Instance.MusicEnabled;
            bool vfxEnabled = AudioManager.Instance.VFXEnabled;
            
            // Update slider
            if (musicVolumeSlider != null)
            {
                musicVolumeSlider.SetValueWithoutNotify(musicVolume);
            }
            
            // Update toggles
            if (musicToggle != null)
            {
                musicToggle.SetIsOnWithoutNotify(musicEnabled);
            }
            
            if (vfxToggle != null)
            {
                vfxToggle.SetIsOnWithoutNotify(vfxEnabled);
            }
            
            // Update labels
            UpdateMusicVolumeLabel(musicVolume);
            UpdateMusicToggleLabel(musicEnabled);
            UpdateVFXToggleLabel(vfxEnabled);
        }
        
        private void OnMusicVolumeChanged(float value)
        {
            AudioManager.Instance.MusicVolume = value;
            UpdateMusicVolumeLabel(value);
        }
        
        private void OnMusicToggled(bool isOn)
        {
            AudioManager.Instance.MusicEnabled = isOn;
            UpdateMusicToggleLabel(isOn);
        }
        
        private void OnVFXToggled(bool isOn)
        {
            AudioManager.Instance.VFXEnabled = isOn;
            UpdateVFXToggleLabel(isOn);
        }
        
        private void UpdateMusicVolumeLabel(float volume)
        {
            if (musicVolumeLabel != null)
            {
                musicVolumeLabel.text = "Music Volume";
            }
            
            if (musicVolumeValue != null)
            {
                musicVolumeValue.text = $"{(volume * 100f):F0}%";
            }
        }
        
        private void UpdateMusicToggleLabel(bool isOn)
        {
            if (musicToggleLabel != null)
            {
                musicToggleLabel.text = $"Music: {(isOn ? "ON" : "OFF")}";
            }
        }
        
        private void UpdateVFXToggleLabel(bool isOn)
        {
            if (vfxToggleLabel != null)
            {
                vfxToggleLabel.text = $"VFX: {(isOn ? "ON" : "OFF")}";
            }
        }
        
        private void OnBackClicked()
        {
            SceneController.Instance.LoadScene(SceneController.MAIN_MENU);
        }
        
        private void OnDestroy()
        {
            if (backButton != null)
                backButton.onClick.RemoveListener(OnBackClicked);
            if (musicVolumeSlider != null)
                musicVolumeSlider.onValueChanged.RemoveListener(OnMusicVolumeChanged);
            if (musicToggle != null)
                musicToggle.onValueChanged.RemoveListener(OnMusicToggled);
            if (vfxToggle != null)
                vfxToggle.onValueChanged.RemoveListener(OnVFXToggled);
        }
    }
}
