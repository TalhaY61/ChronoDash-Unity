using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ChronoDash.Managers;

namespace ChronoDash.UI
{
    /// <summary>
    /// Settings screen with 2 volume sliders (Music + VFX).
    /// </summary>
    public class SettingsUI : MonoBehaviour
    {
        [Header("Navigation")]
        [SerializeField] private Button backButton;
        
        [Header("Music Slider")]
        [SerializeField] private Slider musicVolumeSlider;
        [SerializeField] private TextMeshProUGUI musicLabel;
        [SerializeField] private TextMeshProUGUI musicPercentage;
        
        [Header("VFX Slider")]
        [SerializeField] private Slider vfxVolumeSlider;
        [SerializeField] private TextMeshProUGUI vfxLabel;
        [SerializeField] private TextMeshProUGUI vfxPercentage;
        
        private void Start()
        {
            SetupListeners();
            LoadSettings();
        }
        
        private void SetupListeners()
        {
            if (backButton != null)
                backButton.onClick.AddListener(OnBackClicked);
            
            if (musicVolumeSlider != null)
                musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
            
            if (vfxVolumeSlider != null)
                vfxVolumeSlider.onValueChanged.AddListener(OnVFXVolumeChanged);
            
            if (musicLabel != null)
                musicLabel.text = "Music";
            
            if (vfxLabel != null)
                vfxLabel.text = "VFX";
        }
        
        private void LoadSettings()
        {
            float musicVolume = AudioManager.Instance.MusicVolume;
            float sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 0.5f);
            
            if (musicVolumeSlider != null)
                musicVolumeSlider.SetValueWithoutNotify(musicVolume);
            
            if (vfxVolumeSlider != null)
                vfxVolumeSlider.SetValueWithoutNotify(sfxVolume);
            
            UpdateMusicPercentage(musicVolume);
            UpdateVFXPercentage(sfxVolume);
        }
        
        private void OnMusicVolumeChanged(float value)
        {
            AudioManager.Instance.MusicVolume = value;
            UpdateMusicPercentage(value);
        }
        
        private void OnVFXVolumeChanged(float value)
        {
            PlayerPrefs.SetFloat("SFXVolume", value);
            PlayerPrefs.Save();
            AudioManager.Instance.SetSFXVolume(value);
            UpdateVFXPercentage(value);
        }
        
        private void UpdateMusicPercentage(float volume)
        {
            if (musicPercentage != null)
                musicPercentage.text = $"{(volume * 100f):F0}%";
        }
        
        private void UpdateVFXPercentage(float volume)
        {
            if (vfxPercentage != null)
                vfxPercentage.text = $"{(volume * 100f):F0}%";
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
            if (vfxVolumeSlider != null)
                vfxVolumeSlider.onValueChanged.RemoveListener(OnVFXVolumeChanged);
        }
    }
}
