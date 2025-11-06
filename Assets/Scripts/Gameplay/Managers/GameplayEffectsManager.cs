using UnityEngine;
using System.Collections;
using ChronoDash.UI.Notifications;

namespace ChronoDash.Managers {
    /// <summary>
    /// Manages random gameplay effects (Gravity Flip, Screen Shake, Darkness).
    /// Effects are NOT tied to specific worlds - they can happen in any environment.
    /// Difficulty scales with player score:
    /// - Below 1000: Single effects, random 5-40s intervals
    /// - 1000-2500: Can have 2 effects simultaneously
    /// - 2500+: Can have all 3 effects at once
    /// Follows Single Responsibility Principle - only handles gameplay effects.
    /// </summary>
    public class GameplayEffectsManager : MonoBehaviour {
        [Header("Score Thresholds")]
        [SerializeField] private int dualEffectScoreThreshold = 1000;
        [SerializeField] private int tripleEffectScoreThreshold = 2500;
        [SerializeField] private int earlyWarningThreshold = 250;
        
        [Header("Effect Timing")]
        [SerializeField] private float minEffectInterval = 5f;
        [SerializeField] private float maxEffectInterval = 40f;
        [SerializeField] private float earlyWarningTime = 5f;
        [SerializeField] private float normalWarningTime = 2f;
        
        [Header("Effect References")]
        [SerializeField] private Effects.GravityFlipEffect gravityFlipEffect;
        [SerializeField] private Effects.ScreenShakeEffect screenShakeEffect;
        [SerializeField] private Effects.DarknessEffect darknessEffect;
        
        [Header("Score Reference")]
        [SerializeField] private DifficultyManager difficultyManager;
        
        private bool isGameActive = false;
        private Coroutine effectsCoroutine;
        
        private bool isGravityFlipActive = false;
        private bool isScreenShakeActive = false;
        private bool isDarknessActive = false;
        
        public static GameplayEffectsManager Instance { get; private set; }
        
        private void Awake() {
            if (Instance == null) {
                Instance = this;
            } else {
                Destroy(gameObject);
            }
        }
        
        public void SetGameActive(bool active) {
            isGameActive = active;
            
            if (active) {
                StartEffects();
            } else {
                StopEffects();
            }
        }
        
        private void StartEffects() {
            if (effectsCoroutine != null) {
                StopCoroutine(effectsCoroutine);
            }
            
            effectsCoroutine = StartCoroutine(EffectsCoroutine());
        }
        
        private void StopEffects() {
            if (effectsCoroutine != null) {
                StopCoroutine(effectsCoroutine);
                effectsCoroutine = null;
            }
            
            // Stop all active effects
            StopAllActiveEffects();
        }
        
        private IEnumerator EffectsCoroutine() {
            while (isGameActive) {
                // Wait random interval before next effect
                float waitTime = Random.Range(minEffectInterval, maxEffectInterval);
                yield return new WaitForSeconds(waitTime);
                
                if (!isGameActive) break;
                
                // Determine how many effects can be active
                int currentScore = GetCurrentScore();
                int maxSimultaneousEffects = GetMaxSimultaneousEffects(currentScore);
                
                // Get warning time based on score
                float warningTime = currentScore < earlyWarningThreshold ? earlyWarningTime : normalWarningTime;
                
                // Trigger effect(s)
                yield return StartCoroutine(TriggerRandomEffects(maxSimultaneousEffects, warningTime));
            }
        }
        
        private IEnumerator TriggerRandomEffects(int maxEffects, float warningTime) {
            int effectsToTrigger = Random.Range(1, maxEffects + 1);
            
            var availableEffects = GetAvailableEffects();
            
            if (availableEffects.Count == 0) {
                yield break;
            }
            
            Shuffle(availableEffects);
            
            for (int i = 0; i < Mathf.Min(effectsToTrigger, availableEffects.Count); i++) {
                yield return StartCoroutine(TriggerEffect(availableEffects[i], warningTime));
            }
        }
        
        private IEnumerator TriggerEffect(EffectType effectType, float warningTime) {
            // Show warning notification
            ShowEffectWarning(effectType);
            
            // Wait for warning duration
            yield return new WaitForSeconds(warningTime);
            
            switch (effectType) {
                case EffectType.GravityFlip:
                    if (gravityFlipEffect != null && !isGravityFlipActive) {
                        isGravityFlipActive = true;
                        yield return StartCoroutine(gravityFlipEffect.TriggerEffect(warningTime));
                        isGravityFlipActive = false;
                    }
                    break;
                    
                case EffectType.ScreenShake:
                    if (screenShakeEffect != null && !isScreenShakeActive) {
                        isScreenShakeActive = true;
                        yield return StartCoroutine(screenShakeEffect.TriggerEffect(warningTime));
                        isScreenShakeActive = false;
                    }
                    break;
                    
                case EffectType.Darkness:
                    if (darknessEffect != null && !isDarknessActive) {
                        isDarknessActive = true;
                        yield return StartCoroutine(darknessEffect.TriggerEffect(warningTime));
                        isDarknessActive = false;
                    }
                    break;
            }
        }
        
        private void ShowEffectWarning(EffectType effectType) {
            if (EffectNotifications.Instance == null) return;
            
            switch (effectType) {
                case EffectType.GravityFlip:
                    EffectNotifications.Instance.ShowGravityFlipWarning();
                    break;
                    
                case EffectType.ScreenShake:
                    EffectNotifications.Instance.ShowScreenShakeWarning();
                    break;
                    
                case EffectType.Darkness:
                    EffectNotifications.Instance.ShowDarknessWarning();
                    break;
            }
        }
        
        private System.Collections.Generic.List<EffectType> GetAvailableEffects() {
            var available = new System.Collections.Generic.List<EffectType>();
            
            if (gravityFlipEffect != null && !isGravityFlipActive) {
                available.Add(EffectType.GravityFlip);
            }
            
            if (screenShakeEffect != null && !isScreenShakeActive) {
                available.Add(EffectType.ScreenShake);
            }
            
            if (darknessEffect != null && !isDarknessActive) {
                available.Add(EffectType.Darkness);
            }
            
            return available;
        }
        
        private void Shuffle<T>(System.Collections.Generic.List<T> list) {
            for (int i = list.Count - 1; i > 0; i--) {
                int randomIndex = Random.Range(0, i + 1);
                T temp = list[i];
                list[i] = list[randomIndex];
                list[randomIndex] = temp;
            }
        }
        
        private int GetMaxSimultaneousEffects(int score) {
            if (score >= tripleEffectScoreThreshold) {
                return 3; // All effects can happen together
            } else if (score >= dualEffectScoreThreshold) {
                return 2; // Two effects at once
            } else {
                return 1; // Only one effect at a time
            }
        }
        
        private int GetCurrentScore() {
            if (difficultyManager != null) {
                return difficultyManager.CurrentScore;
            }
            
            // Fallback: find difficulty manager dynamically
            var foundDifficultyManager = FindFirstObjectByType<DifficultyManager>();
            if (foundDifficultyManager != null) {
                difficultyManager = foundDifficultyManager;
                return difficultyManager.CurrentScore;
            }
            
            return 0;
        }
        
        private void StopAllActiveEffects() {
            if (gravityFlipEffect != null) {
                gravityFlipEffect.ForceStopEffect();
            }
            
            if (screenShakeEffect != null) {
                screenShakeEffect.ForceStopEffect();
            }
            
            if (darknessEffect != null) {
                darknessEffect.ForceStopEffect();
            }
            
            isGravityFlipActive = false;
            isScreenShakeActive = false;
            isDarknessActive = false;
        }
        
        private enum EffectType {
            GravityFlip,
            ScreenShake,
            Darkness
        }
    }
}
