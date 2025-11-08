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
        
        [Header("Audio")]
        [SerializeField] private AudioClip activationSound;
        [SerializeField] private AudioClip deactivationSound;
        
        // State
        private bool isActive = false;
        private bool isOnCooldown = false;
        private float activeTimer = 0f;
        private float cooldownTimer = 0f;
        
        // Components
        private AudioSource audioSource;
        
        // Events
        public System.Action<bool> OnBubbleStateChanged;
        public System.Action<float> OnCooldownChanged;
        
        // Properties
        public bool IsActive => isActive;
        public bool IsOnCooldown => isOnCooldown;
        public float ActiveTimeRemaining => activeTimer;
        public float Duration => duration;
        public float CooldownRemaining => cooldownTimer;
        public float CooldownProgress => 1f - (cooldownTimer / cooldown);
        public float Cooldown => cooldown;
        public float BubbleRadius => bubbleRadius;
        
        private void Awake()
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
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
            if (!isActive && !isOnCooldown) ActivateBubble();
        }
        
        private void ActivateBubble()
        {
            isActive = true;
            activeTimer = duration;
            
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
            
            if (deactivationSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(deactivationSound);
            }
            
            OnBubbleStateChanged?.Invoke(false);
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
        }
    }
}
