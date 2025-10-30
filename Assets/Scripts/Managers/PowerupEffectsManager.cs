using UnityEngine;
using System.Collections.Generic;
using System;
using ChronoDash.Managers;

namespace ChronoDash.Powerups
{
    public class PowerupEffectsManager : MonoBehaviour
    {
        private Dictionary<PowerupType, PowerupEffect> activePowerups = new Dictionary<PowerupType, PowerupEffect>();
        
        private HealthManager healthManager;
        private bool isInvincible = false;
        private int shieldCharges = 0;
        private float speedMultiplier = 1f;
        private float magnetRadius = 0f;
        private int scoreMultiplier = 1;
        
        public bool IsInvincible => isInvincible;
        public int ShieldCharges => shieldCharges;
        public float SpeedMultiplier => speedMultiplier;
        public float MagnetRadius => magnetRadius;
        public int ScoreMultiplier => scoreMultiplier;
        
        public event Action<PowerupType, int> OnPowerupCollected;
        public event Action<PowerupType, float, int> OnPowerupActivated;
        public event Action<PowerupType> OnPowerupExpired;
        
        public class PowerupEffect
        {
            public float remainingTime;
            public int stackCount;
            public float duration;
        }
        
        private void Awake()
        {
            healthManager = FindFirstObjectByType<HealthManager>();
        }
        
        private void Update()
        {
            List<PowerupType> toRemove = new List<PowerupType>();
            
            foreach (var kvp in activePowerups)
            {
                var effect = kvp.Value;
                effect.remainingTime -= Time.deltaTime;
                
                if (effect.remainingTime <= 0)
                {
                    if (effect.stackCount > 1)
                    {
                        effect.stackCount--;
                        effect.remainingTime = effect.duration;
                    }
                    else
                    {
                        toRemove.Add(kvp.Key);
                    }
                }
            }
            
            foreach (var type in toRemove)
            {
                DeactivatePowerup(type);
            }
        }
        
        public void CollectPowerup(PowerupType type)
        {
            if (type == PowerupType.Health)
            {
                if (healthManager != null)
                {
                    healthManager.AddMaxHealth(1);
                }
                OnPowerupCollected?.Invoke(type, 1);
                return;
            }
            
            if (activePowerups.ContainsKey(type))
            {
                activePowerups[type].stackCount++;
            }
            else
            {
                float duration = GetDuration(type);
                activePowerups[type] = new PowerupEffect 
                { 
                    remainingTime = duration, 
                    stackCount = 1,
                    duration = duration
                };
                ActivatePowerup(type);
            }
            
            int stackCount = activePowerups[type].stackCount;
            OnPowerupCollected?.Invoke(type, stackCount);
            OnPowerupActivated?.Invoke(type, activePowerups[type].remainingTime, stackCount);
        }
        
        private void ActivatePowerup(PowerupType type)
        {
            // Play time control sound through AudioManager
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayTimeControlSound();
            }
            
            switch (type)
            {
                case PowerupType.Invincibility:
                    isInvincible = true;
                    break;
                case PowerupType.Speed:
                    speedMultiplier = 1.5f;
                    break;
                case PowerupType.Magnet:
                    magnetRadius = 4f;
                    break;
                case PowerupType.Shield:
                    shieldCharges++;
                    break;
                case PowerupType.Multiply2x:
                    scoreMultiplier = 2;
                    break;
            }
        }
        
        private void DeactivatePowerup(PowerupType type)
        {
            switch (type)
            {
                case PowerupType.Invincibility:
                    isInvincible = false;
                    break;
                case PowerupType.Speed:
                    speedMultiplier = 1f;
                    break;
                case PowerupType.Magnet:
                    magnetRadius = 0f;
                    break;
                case PowerupType.Multiply2x:
                    scoreMultiplier = 1;
                    break;
            }
            
            activePowerups.Remove(type);
            OnPowerupExpired?.Invoke(type);
        }
        
        public bool TryBlockDamage()
        {
            if (isInvincible) return true;
            
            if (shieldCharges > 0)
            {
                shieldCharges--;
                
                if (shieldCharges == 0 && activePowerups.ContainsKey(PowerupType.Shield))
                {
                    var effect = activePowerups[PowerupType.Shield];
                    if (effect.stackCount > 1)
                    {
                        // Has more shields in queue, activate next one
                        effect.stackCount--;
                        shieldCharges = 1;
                        OnPowerupActivated?.Invoke(PowerupType.Shield, effect.duration, effect.stackCount);
                    }
                    else
                    {
                        // No more shields, remove the powerup completely
                        DeactivatePowerup(PowerupType.Shield);
                        Debug.Log("🛡️ All shield charges depleted - icon removed");
                    }
                }
                
                return true;
            }
            
            return false;
        }
        
        private float GetDuration(PowerupType type)
        {
            switch (type)
            {
                case PowerupType.Invincibility: return 5f;
                case PowerupType.Speed: return 8f;
                case PowerupType.Magnet: return 10f;
                case PowerupType.Shield: return 999f;
                case PowerupType.Multiply2x: return 10f;
                default: return 0f;
            }
        }
        
        public float GetRemainingTime(PowerupType type)
        {
            return activePowerups.ContainsKey(type) ? activePowerups[type].remainingTime : 0f;
        }
        
        public int GetStackCount(PowerupType type)
        {
            return activePowerups.ContainsKey(type) ? activePowerups[type].stackCount : 0;
        }
        
        public Dictionary<PowerupType, PowerupEffect> GetActivePowerups()
        {
            return new Dictionary<PowerupType, PowerupEffect>(activePowerups);
        }
        
        public void Reset()
        {
            // Clear all active powerups
            List<PowerupType> allTypes = new List<PowerupType>(activePowerups.Keys);
            foreach (var type in allTypes)
            {
                DeactivatePowerup(type);
            }
            
            activePowerups.Clear();
            
            // Reset all states
            isInvincible = false;
            shieldCharges = 0;
            speedMultiplier = 1f;
            magnetRadius = 0f;
            scoreMultiplier = 1;
            
            Debug.Log("⚡ PowerupEffectsManager: Reset all powerups");
        }
    }
}
