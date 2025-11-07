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
        [SerializeField] private float sfxVolume = 0.5f;

        private AudioSource musicSource;
        private AudioSource sfxSource;
        
        // Events
        public System.Action<float> OnMusicVolumeChanged;
        
        // Properties
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
            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.loop = true;
            musicSource.volume = musicVolume;
            musicSource.playOnAwake = false;

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
            musicVolume = PlayerPrefs.GetFloat("MusicVolume", 0.5f);
            sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 0.5f);
            ApplyMusicSettings();
            ApplySFXSettings();
        }
        
        private void ApplyMusicSettings()
        {
            if (musicSource != null)
                musicSource.volume = musicVolume;
        }
        
        private void ApplySFXSettings()
        {
            if (sfxSource != null)
                sfxSource.volume = sfxVolume;
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
                musicSource.Stop();
        }

        public void PauseMusic()
        {
            if (musicSource != null && musicSource.isPlaying)
                musicSource.Pause();
        }
        
        public void UnpauseMusic()
        {
            if (musicSource != null && !musicSource.isPlaying)
                musicSource.UnPause();
        }

        public void PlaySFX(AudioClip clip)
        {
            if (clip != null && sfxSource != null)
                sfxSource.PlayOneShot(clip);
        }

        public void PlayJumpSound() => PlaySFX(jumpSound);
        public void PlayHitSound() => PlaySFX(hitSound);
        public void PlayDeathSound() => PlaySFX(deathSound);
        public void PlayGemstoneSound() => PlaySFX(gemstoneSound);
        public void PlayTimeControlSound() => PlaySFX(timeControlSound);
        public void PlaySelectSound() => PlaySFX(selectSound);

        public void SetSFXVolume(float volume)
        {
            sfxVolume = Mathf.Clamp01(volume);
            ApplySFXSettings();
        }
    }
}
