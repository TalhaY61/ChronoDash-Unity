using System.Collections;
using UnityEngine;

namespace ChronoDash.Powerups
{
    public class PowerupManager : MonoBehaviour
    {
        [Header("Powerup Prefabs")]
        [SerializeField] private GameObject invincibilityPrefab;
        [SerializeField] private GameObject speedPrefab;
        [SerializeField] private GameObject magnetPrefab;
        [SerializeField] private GameObject shieldPrefab;
        [SerializeField] private GameObject multiply2xPrefab;
        [SerializeField] private GameObject healthPrefab;
        
        [Header("Spawn Settings")]
        [SerializeField] private float minSpawnInterval = 15f;
        [SerializeField] private float maxSpawnInterval = 20f;
        [SerializeField] private float spawnX = 12f;
        [SerializeField] private float baseGroundY = -4.5f;
        
        private bool isGameActive = false;
        private int currentDifficultyLevel = 1;
        
        public void StartSpawning()
        {
            isGameActive = true;
            StartCoroutine(SpawnPowerupsCoroutine());
        }
        
        public void StopSpawning()
        {
            isGameActive = false;
            StopAllCoroutines();
        }
        
        public void ClearAllPowerups()
        {
            // Find and destroy all powerup objects in the scene
            GameObject[] powerups = GameObject.FindGameObjectsWithTag("Powerup");
            foreach (GameObject powerup in powerups)
            {
                Destroy(powerup);
            }
            Debug.Log($"🧹 Cleared {powerups.Length} powerup objects from scene");
        }
        
        public void SetDifficultyLevel(int level)
        {
            currentDifficultyLevel = level;
            float reduction = (level - 1) * 1f;
            minSpawnInterval = Mathf.Max(10f, 15f - reduction);
            maxSpawnInterval = Mathf.Max(12f, 20f - reduction);
        }
        
        private IEnumerator SpawnPowerupsCoroutine()
        {
            while (isGameActive)
            {
                float waitTime = Random.Range(minSpawnInterval, maxSpawnInterval);
                yield return new WaitForSeconds(waitTime);
                
                if (!isGameActive) break;
                
                SpawnRandomPowerup();
            }
        }
        
        private void SpawnRandomPowerup()
        {
            PowerupType type = SelectRandomPowerupType();
            GameObject prefab = GetPrefabForType(type);
            
            if (prefab == null)
            {
                Debug.LogWarning($"No prefab assigned for {type}");
                return;
            }
            
            Vector3 spawnPos = CalculateSpawnPosition(type);
            Instantiate(prefab, spawnPos, Quaternion.identity);
        }
        
        private PowerupType SelectRandomPowerupType()
        {
            float roll = Random.value;
            
            if (roll < 0.10f) return PowerupType.Invincibility;
            if (roll < 0.20f) return PowerupType.Health;
            if (roll < 0.45f) return PowerupType.Shield;
            if (roll < 0.63f) return PowerupType.Speed;
            if (roll < 0.81f) return PowerupType.Magnet;
            return PowerupType.Multiply2x;
        }
        
        private GameObject GetPrefabForType(PowerupType type)
        {
            switch (type)
            {
                case PowerupType.Invincibility: return invincibilityPrefab;
                case PowerupType.Speed: return speedPrefab;
                case PowerupType.Magnet: return magnetPrefab;
                case PowerupType.Shield: return shieldPrefab;
                case PowerupType.Multiply2x: return multiply2xPrefab;
                case PowerupType.Health: return healthPrefab;
                default: return null;
            }
        }
        
        private Vector3 CalculateSpawnPosition(PowerupType type)
        {
            float yPos;
            
            switch (type)
            {
                case PowerupType.Invincibility:
                case PowerupType.Health:
                    yPos = baseGroundY + Random.Range(2.5f, 3.5f);
                    break;
                    
                case PowerupType.Speed:
                    yPos = baseGroundY + Random.Range(0.3f, 1.2f);
                    break;
                    
                default:
                    yPos = baseGroundY + Random.Range(1.2f, 2.5f);
                    break;
            }
            
            return new Vector3(spawnX, yPos, 0f);
        }
    }
}
