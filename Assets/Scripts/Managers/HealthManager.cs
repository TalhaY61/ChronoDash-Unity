using UnityEngine;
using System;

namespace ChronoDash.Managers
{
    public class HealthManager : MonoBehaviour
    {
        private int currentHealth = 3;
        private int maxHealth = 3;
        
        public int CurrentHealth => currentHealth;
        public int MaxHealth => maxHealth;
        
        public event Action<int, int> OnHealthChanged;
        public event Action OnPlayerDied;
        
        private void Start()
        {
            currentHealth = maxHealth;
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
        }
        
        public void TakeDamage(int amount)
        {
            if (currentHealth <= 0) return;
            
            currentHealth = Mathf.Max(0, currentHealth - amount);
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
            
            // Play hit sound
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayHitSound();
            }
            
            if (currentHealth <= 0)
            {
                // Play death sound
                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlayDeathSound();
                }
                OnPlayerDied?.Invoke();
            }
        }
        
        public void Heal(int amount)
        {
            if (currentHealth >= maxHealth)
            {
                Debug.Log($"❤️ Cannot heal - already at max health ({currentHealth}/{maxHealth})");
                return;
            }
            
            int oldHealth = currentHealth;
            currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
            
            Debug.Log($"❤️ Healed! {oldHealth} → {currentHealth}/{maxHealth}");
        }
        
        public void AddMaxHealth(int amount)
        {
            maxHealth += amount;
            currentHealth += amount;
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
        }
        
        public void Reset()
        {
            maxHealth = 3;
            currentHealth = 3;
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
        }
    }
}
