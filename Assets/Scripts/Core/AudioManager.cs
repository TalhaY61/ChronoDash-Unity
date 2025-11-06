using UnityEngine;

namespace ChronoDash.Managers
{
    public class AudioManager : MonoBehaviour
    {
        [Header("Music")]
        [SerializeField] private AudioClip backgroundMusic;
        [SerializeField] private float musicVolume = 0.5f;

        [Header("Sound Effects")]
        [SerializeField] private AudioClip jumpSound;
        [SerializeField] private AudioClip hitSound;
        [SerializeField] private AudioClip deathSound;
        [SerializeField] private AudioClip gemstoneSound;
        [SerializeField] private AudioClip timeControlSound;
        [SerializeField] private AudioClip selectSound;
        [SerializeField] private float sfxVolume = 0.3f;

        private AudioSource musicSource;
        private AudioSource sfxSource;
        
        // Settings
        private bool musicEnabled = true;
        private bool vfxEnabled = true;
        
        // Events
        public System.Action<bool> OnMusicEnabledChanged;
        public System.Action<bool> OnVFXEnabledChanged;
        public System.Action<float> OnMusicVolumeChanged;
        
        // Properties
        public bool MusicEnabled
        {
            get => musicEnabled;
            set
            {
                musicEnabled = value;
                PlayerPrefs.SetInt("MusicEnabled", value ? 1 : 0);
                PlayerPrefs.Save();
                OnMusicEnabledChanged?.Invoke(value);
                ApplyMusicSettings();
            }
        }
        
        public bool VFXEnabled
        {
            get => vfxEnabled;
            set
            {
                vfxEnabled = value;
                PlayerPrefs.SetInt("VFXEnabled", value ? 1 : 0);
                PlayerPrefs.Save();
                OnVFXEnabledChanged?.Invoke(value);
            }
        }
        
        public float MusicVolume
        {
            get => musicVolume;
            set
            {
                musicVolume = Mathf.Clamp01(value);
                PlayerPrefs.SetFloat("MusicVolume", musicVolume);
                PlayerPrefs.Save();
                OnMusicVolumeChanged?.Invoke(musicVolume);
                ApplyMusicSettings();
            }
        }

        public static AudioManager Instance { get; private set; }

        private void Awake()
        {
            // Singleton pattern
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeAudioSources();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void InitializeAudioSources()
        {
            // Create music source
            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.loop = true;
            musicSource.volume = musicVolume;
            musicSource.playOnAwake = false;

            // Create SFX source
            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.loop = false;
            sfxSource.volume = sfxVolume;
            sfxSource.playOnAwake = false;
        }

        private void Start()
        {
            LoadSettings();
            PlayMusic();
        }
        
        private void LoadSettings()
        {
            musicEnabled = PlayerPrefs.GetInt("MusicEnabled", 1) == 1;
            vfxEnabled = PlayerPrefs.GetInt("VFXEnabled", 1) == 1;
            musicVolume = PlayerPrefs.GetFloat("MusicVolume", 0.5f);
            ApplyMusicSettings();
        }
        
        private void ApplyMusicSettings()
        {
            if (musicSource != null)
            {
                musicSource.mute = !musicEnabled;
                musicSource.volume = musicVolume;
            }
        }

        public void PlayMusic()
        {
            if (backgroundMusic != null && musicSource != null)
            {
                musicSource.clip = backgroundMusic;
                musicSource.Play();
            }
        }

        public void StopMusic()
        {
            if (musicSource != null)
            {
                musicSource.Stop();
            }
        }

        public void PauseMusic()
        {
            if (musicSource != null && musicSource.isPlaying)
            {
                musicSource.Pause();
            }
        }
        
        public void UnpauseMusic()
        {
            if (musicSource != null && !musicSource.isPlaying)
            {
                musicSource.UnPause();
            }
        }

        public void PlaySFX(AudioClip clip)
        {
            // Only play if VFX is enabled
            if (clip != null && sfxSource != null && vfxEnabled)
            {
                sfxSource.PlayOneShot(clip);
            }
        }

        public void PlayJumpSound()
        {
            PlaySFX(jumpSound);
        }

        public void PlayHitSound()
        {
            PlaySFX(hitSound);
        }

        public void PlayDeathSound()
        {
            PlaySFX(deathSound);
        }

        public void PlayGemstoneSound()
        {
            PlaySFX(gemstoneSound);
        }

        public void PlayTimeControlSound()
        {
            PlaySFX(timeControlSound);
        }

        public void PlaySelectSound()
        {
            PlaySFX(selectSound);
        }

        public void SetMusicVolume(float volume)
        {
            musicVolume = Mathf.Clamp01(volume);
            if (musicSource != null)
            {
                musicSource.volume = musicVolume;
            }
        }

        public void SetSFXVolume(float volume)
        {
            sfxVolume = Mathf.Clamp01(volume);
            if (sfxSource != null)
            {
                sfxSource.volume = sfxVolume;
            }
        }
        
        /// <summary>
        /// Toggle music on/off.
        /// </summary>
        public void ToggleMusic()
        {
            MusicEnabled = !MusicEnabled;
        }
        
        /// <summary>
        /// Toggle VFX on/off.
        /// </summary>
        public void ToggleVFX()
        {
            VFXEnabled = !VFXEnabled;
        }
    }
}
