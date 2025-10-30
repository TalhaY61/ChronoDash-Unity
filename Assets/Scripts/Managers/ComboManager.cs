using UnityEngine;

namespace ChronoDash.Managers
{
    /// <summary>
    /// Manages combo streaks and score multipliers for consecutive successful obstacle dodges
    /// </summary>
    public class ComboManager : MonoBehaviour
    {
        [Header("Combo Settings")]
        [SerializeField] private int[] comboThresholds = { 3, 5, 10, 20, 30 }; // Combo milestones
        [SerializeField] private float[] comboMultipliers = { 1.5f, 2.0f, 2.5f, 3.0f, 4.0f }; // Score multipliers per threshold
        [SerializeField] private float comboResetTime = 2f; // Time before combo resets if no obstacle passed
        
        private int currentStreak = 0;
        private float currentMultiplier = 1f;
        private float timeSinceLastDodge = 0f;
        private bool isComboActive = false;
        
        // Events
        public System.Action<int> OnComboChanged; // Current streak count
        public System.Action<float> OnMultiplierChanged; // Multiplier changed
        public System.Action OnComboLost; // Combo reset
        
        // Properties
        public int CurrentStreak => currentStreak;
        public float CurrentMultiplier => currentMultiplier;
        public bool IsComboActive => isComboActive;

        private void Start()
        {
            Debug.Log("=== ComboManager Initialization ===");
            Debug.Log($"Combo Thresholds: {string.Join(", ", comboThresholds)}");
            Debug.Log($"Combo Multipliers: {string.Join("x, ", comboMultipliers)}x");
            Debug.Log($"Combo Reset Time: {comboResetTime}s");
            Debug.Log("ComboManager is ready! Listening for obstacle dodges...");
        }
        
        private void Update()
        {
            if (isComboActive)
            {
                timeSinceLastDodge += Time.deltaTime;
                
                // Reset combo if too much time passes without dodging
                if (timeSinceLastDodge > comboResetTime)
                {
                    ResetCombo();
                }
            }
        }
        
        /// <summary>
        /// Call when player successfully dodges an obstacle
        /// </summary>
        public void OnSuccessfulDodge()
        {
            currentStreak++;
            timeSinceLastDodge = 0f;
            isComboActive = true;
            
            OnComboChanged?.Invoke(currentStreak);
            
            // Check for multiplier milestones
            UpdateMultiplier();
            
            Debug.Log($"🔥 Combo: {currentStreak}x (Multiplier: x{currentMultiplier:F1})");
        }
        
        /// <summary>
        /// Call when player gets hit by an obstacle
        /// </summary>
        public void OnPlayerHit()
        {
            if (currentStreak > 0)
            {
                Debug.Log($"❌ Combo broken at {currentStreak}x! Multiplier reset.");
                ResetCombo();
            }
        }
        
        private void UpdateMultiplier()
        {
            float previousMultiplier = currentMultiplier;
            currentMultiplier = 1f; // Base multiplier
            
            for (int i = 0; i < comboThresholds.Length; i++)
            {
                if (currentStreak >= comboThresholds[i])
                {
                    currentMultiplier = comboMultipliers[i];
                }
            }
            
            // Notify if multiplier changed
            if (currentMultiplier != previousMultiplier)
            {
                OnMultiplierChanged?.Invoke(currentMultiplier);
                Debug.Log($"🎉 MULTIPLIER UP! x{currentMultiplier:F1} at {currentStreak}x combo!");
            }
        }
        
        private void ResetCombo()
        {
            currentStreak = 0;
            currentMultiplier = 1f;
            isComboActive = false;
            OnComboLost?.Invoke();
            OnMultiplierChanged?.Invoke(currentMultiplier);
        }
        
        public void Reset()
        {
            ResetCombo();
            timeSinceLastDodge = 0f;
        }
    }
}
