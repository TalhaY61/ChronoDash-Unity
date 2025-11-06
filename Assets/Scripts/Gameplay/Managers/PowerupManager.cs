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
            
            if (prefab == null) return;
            
            Vector3 spawnPos = CalculateSpawnPosition(type);
            GameObject powerup = Instantiate(prefab, spawnPos, Quaternion.identity);
        }
        
        private PowerupType SelectRandomPowerupType()
        {
            float roll = Random.value;
            
            if (roll < 0.10f) return PowerupType.Invincibility;
            if (roll < 0.20f) return PowerupType.Heart;
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
                case PowerupType.Heart: return healthPrefab;
                default: return null;
            }
        }
        
        private Vector3 CalculateSpawnPosition(PowerupType type)
        {
            // Always spawn on RIGHT side
            // Camera rotation handles visual appearance - when flipped, right appears left!
            float xPos = spawnX;
            float yPos;
            
            // Calculate Y offset (always add upwards - camera rotation handles visual flip)
            switch (type)
            {
                case PowerupType.Invincibility:
                case PowerupType.Heart:
                    yPos = baseGroundY + Random.Range(2.5f, 3.5f);
                    break;
                    
                case PowerupType.Speed:
                case PowerupType.Magnet:
                    yPos = baseGroundY + Random.Range(1.5f, 2.5f);
                    break;
                    
                default: // Shield, Multiply2x
                    yPos = baseGroundY + Random.Range(0.8f, 2.0f);
                    break;
            }
            
            return new Vector3(xPos, yPos, 0f);
        }
    }
}
