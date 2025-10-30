using UnityEngine;
using UnityEngine.InputSystem;

namespace ChronoDash.Abilities
{
    /// <summary>
    /// Time Bubble - Slows down all obstacles in radius when activated
    /// Controls: Press T to activate (3s duration, 10s cooldown)
    /// Animation: Shows bubble during active, plays closing animation on deactivate
    /// </summary>
    public class TimeBubble : MonoBehaviour
    {
        [Header("Time Bubble Settings")]
        [SerializeField] private float bubbleRadius = 3.5f;
        [SerializeField] private float slowdownFactor = 0.5f;
        [SerializeField] private float duration = 3f;
        [SerializeField] private float cooldown = 10f;
        
        [Header("Visual Prefab")]
        [SerializeField] private GameObject timeBubblePrefab;
        [SerializeField] private float bubbleScale = 7f;
        
        [Header("Rendering")]
    [SerializeField] private int bubbleSortingOrder = 11; // Background is 10, so bubble is above
        [Tooltip("Set sorting order: Background=-10, Bubble=-5 to 10, Player=0, Obstacles=5")]
        
        [Header("Audio")]
        [SerializeField] private AudioClip activationSound;
        [SerializeField] private AudioClip deactivationSound;
        
        // State
        private bool isActive = false;
        private bool isOnCooldown = false;
        private float activeTimer = 0f;
        private float cooldownTimer = 0f;
        
        // Components
        private GameObject bubbleInstance;
        private Animator bubbleAnimator;
        private AudioSource audioSource;
        
        // Events
        public System.Action<bool> OnBubbleStateChanged;
        public System.Action<float> OnCooldownChanged;
        
        // Properties
        public bool IsActive => isActive;
        public bool IsOnCooldown => isOnCooldown;
        public float CooldownRemaining => cooldownTimer;
        public float CooldownProgress => 1f - (cooldownTimer / cooldown);
        public float BubbleRadius => bubbleRadius;
        
        private void Awake()
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
            
            Debug.Log($"✅ TimeBubble initialized. Prefab: {(timeBubblePrefab != null ? "✓" : "✗")}");
        }
        
        private void Update()
        {
            HandleInput();
            UpdateTimers();
        }
        
        private void HandleInput()
        {
            if (Keyboard.current != null && Keyboard.current.tKey.wasPressedThisFrame)
            {
                TryActivateBubble();
            }
        }
        
        private void TryActivateBubble()
        {
            if (!isActive && !isOnCooldown)
            {
                ActivateBubble();
            }
            else if (isOnCooldown)
            {
                Debug.Log($"⏰ Cooldown: {cooldownTimer:F1}s remaining");
            }
        }
        
        private void ActivateBubble()
        {
            isActive = true;
            activeTimer = duration;
            
            if (timeBubblePrefab != null)
            {
                // Spawn bubble visual
                bubbleInstance = Instantiate(timeBubblePrefab, transform.position, Quaternion.identity);
                bubbleInstance.name = "ActiveTimeBubble";
                bubbleInstance.transform.SetParent(transform, worldPositionStays: true);
                bubbleInstance.transform.localPosition = Vector3.zero;
                // Reset Z position to 0 for manual adjustment
                var pos = bubbleInstance.transform.localPosition;
                pos.z = 0f;
                bubbleInstance.transform.localPosition = pos;
                bubbleInstance.transform.localScale = Vector3.one * bubbleScale;
                
                // Get animator if exists
                bubbleAnimator = bubbleInstance.GetComponent<Animator>();
                if (bubbleAnimator != null)
                {
                    // Animation should start on first frame (bubble) and stay there
                    // It will play fade frames when we trigger Close
                    bubbleAnimator.speed = 0f; // Pause on first frame
                    Debug.Log("🕐 Time Bubble animator paused on first frame");
                }
                
                // Set sorting order - adjustable in Inspector
                SpriteRenderer sr = bubbleInstance.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    sr.sortingOrder = bubbleSortingOrder;
                    Debug.Log($"🕐 Time Bubble sorting order set to {bubbleSortingOrder}");
                }
                
                Debug.Log("🕐 Time Bubble ACTIVATED!");
            }
            else
            {
                Debug.LogError("❌ TimeBubble Prefab not assigned!");
            }
            
            if (activationSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(activationSound);
            }
            
            OnBubbleStateChanged?.Invoke(true);
        }
        
        private void DeactivateBubble()
        {
            isActive = false;
            isOnCooldown = true;
            cooldownTimer = cooldown;
            
            // Play closing animation before destroying
            if (bubbleInstance != null)
            {
                if (bubbleAnimator != null)
                {
                    // Resume animation to play fade frames
                    bubbleAnimator.speed = 1f;
                    bubbleAnimator.SetTrigger("Close");
                    
                    // Destroy after fade animation completes (estimate 0.5s)
                    Destroy(bubbleInstance, 0.5f);
                    Debug.Log("💨 Playing fade animation...");
                }
                else
                {
                    // No animator - destroy immediately
                    Destroy(bubbleInstance);
                }
                
                bubbleInstance = null;
                bubbleAnimator = null;
            }
            
            if (deactivationSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(deactivationSound);
            }
            
            OnBubbleStateChanged?.Invoke(false);
            Debug.Log($"⏰ Time Bubble expired. Cooldown: {cooldown}s");
        }
        
        private void UpdateTimers()
        {
            if (isActive)
            {
                activeTimer -= Time.deltaTime;
                if (activeTimer <= 0f)
                {
                    DeactivateBubble();
                }
            }
            
            if (isOnCooldown)
            {
                cooldownTimer -= Time.deltaTime;
                OnCooldownChanged?.Invoke(cooldownTimer);
                
                if (cooldownTimer <= 0f)
                {
                    isOnCooldown = false;
                    cooldownTimer = 0f;
                    Debug.Log("✅ Time Bubble ready!");
                }
            }
        }
        
        public bool IsPositionInBubble(Vector3 position)
        {
            if (!isActive) return false;
            float distance = Vector3.Distance(transform.position, position);
            return distance <= bubbleRadius;
        }
        
        public float GetSlowFactorAt(Vector3 position)
        {
            return IsPositionInBubble(position) ? slowdownFactor : 1f;
        }
        
        public void Reset()
        {
            if (isActive)
            {
                DeactivateBubble();
            }
            
            isOnCooldown = false;
            cooldownTimer = 0f;
            
            if (bubbleInstance != null)
            {
                Destroy(bubbleInstance);
                bubbleInstance = null;
            }
        }
        
        private void OnDestroy()
        {
            if (bubbleInstance != null)
            {
                Destroy(bubbleInstance);
            }
        }
    }
}
