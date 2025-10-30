using UnityEngine;
using System.Collections;

namespace ChronoDash.WorldEffects
{
    /// <summary>
    /// Sandstorm Effect - Visual overlay that obscures vision (Desert World)
    /// Appears randomly every 40-60 seconds
    /// Lasts for 2-3 seconds
    /// </summary>
    public class SandstormEffect : MonoBehaviour
    {
        [Header("Sandstorm Settings")]
        [SerializeField] private float intensity = 0.4f;
        [SerializeField] private Color sandColor = new Color(0.9f, 0.7f, 0.4f, 0.4f);
        [SerializeField] private float minInterval = 40f; // Min time between sandstorms
        [SerializeField] private float maxInterval = 60f; // Max time between sandstorms
        [SerializeField] private float minDuration = 2f;  // Min sandstorm duration
        [SerializeField] private float maxDuration = 3f;  // Max sandstorm duration
        
        [Header("References")]
        [SerializeField] private SpriteRenderer overlayRenderer;
        [SerializeField] private ParticleSystem sandParticles;
        
        [Header("Audio")]
        [SerializeField] private AudioClip sandstormAmbience;
        
        private AudioSource audioSource;
        private bool isActive = false;
        private Coroutine sandstormCoroutine;
        
        private void Awake()
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.loop = false; // Changed to false - will play per storm
            }
            
            if (overlayRenderer != null)
            {
                overlayRenderer.color = sandColor;
            }
            
            // Start deactivated
            DeactivateImmediate();
        }
        
        /// <summary>
        /// Start random sandstorm spawning
        /// </summary>
        public void StartSandstormCycle()
        {
            if (sandstormCoroutine != null)
            {
                StopCoroutine(sandstormCoroutine);
            }
            
            sandstormCoroutine = StartCoroutine(SandstormCycleCoroutine());
        }
        
        /// <summary>
        /// Stop sandstorm spawning
        /// </summary>
        public void StopSandstormCycle()
        {
            if (sandstormCoroutine != null)
            {
                StopCoroutine(sandstormCoroutine);
                sandstormCoroutine = null;
            }
            
            DeactivateImmediate();
        }
        
        private IEnumerator SandstormCycleCoroutine()
        {
            while (true)
            {
                // Wait random interval before next sandstorm
                float waitTime = Random.Range(minInterval, maxInterval);
                Debug.Log($"🏜️ Next sandstorm in {waitTime:F1}s");
                yield return new WaitForSeconds(waitTime);
                
                // Activate sandstorm for random duration
                float duration = Random.Range(minDuration, maxDuration);
                Debug.Log($"🌪️ Sandstorm starting! Duration: {duration:F1}s");
                Activate();
                
                yield return new WaitForSeconds(duration);
                
                // Deactivate
                Debug.Log("✅ Sandstorm ended");
                Deactivate();
            }
        }
        
        public void Activate()
        {
            if (isActive) return;
            
            isActive = true;
            
            if (overlayRenderer != null)
            {
                overlayRenderer.enabled = true;
            }
            
            if (sandParticles != null)
            {
                sandParticles.Play();
            }
            
            if (sandstormAmbience != null && audioSource != null)
            {
                audioSource.PlayOneShot(sandstormAmbience);
            }
        }
        
        public void Deactivate()
        {
            isActive = false;
            
            if (overlayRenderer != null)
            {
                overlayRenderer.enabled = false;
            }
            
            if (sandParticles != null)
            {
                sandParticles.Stop();
            }
            
            if (audioSource != null && audioSource.isPlaying)
            {
                audioSource.Stop();
            }
        }
        
        private void DeactivateImmediate()
        {
            isActive = false;
            
            if (overlayRenderer != null)
            {
                overlayRenderer.enabled = false;
            }
            
            if (sandParticles != null)
            {
                sandParticles.Stop();
                sandParticles.Clear();
            }
        }
        
        public void SetIntensity(float newIntensity)
        {
            intensity = Mathf.Clamp01(newIntensity);
            
            if (overlayRenderer != null)
            {
                Color c = overlayRenderer.color;
                c.a = intensity;
                overlayRenderer.color = c;
            }
            
            if (sandParticles != null)
            {
                var emission = sandParticles.emission;
                emission.rateOverTime = 50f * intensity;
            }
        }
    }
}
