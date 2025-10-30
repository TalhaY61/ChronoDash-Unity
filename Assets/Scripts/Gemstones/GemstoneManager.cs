using System.Collections.Generic;
using UnityEngine;

namespace ChronoDash.Gemstones
{
    public class GemstoneManager : MonoBehaviour
    {
        [Header("Gemstone Prefabs")]
        [SerializeField] private GameObject blueGemstonePrefab;
        [SerializeField] private GameObject redGemstonePrefab;
        [SerializeField] private GameObject greenGemstonePrefab;

        [Header("Spawn Settings")]
        [SerializeField] private Transform spawnPoint;
        [SerializeField] private float spawnInterval = 3f; // Spawn more frequently!
        [SerializeField] private float baseGroundY = -3.7f; // Ground level
        
        [Header("Risky Position Patterns")]
        [SerializeField] private bool spawnAtRiskyPositions = true;
        [SerializeField] [Range(0f, 1f)] private float riskySpawnChance = 0.7f; // 70% risky, 30% safe
        
        [Header("Rarity System (Must total 1.0)")]
        [SerializeField] [Tooltip("Chance for Blue gemstones (Common)")] private float blueChance = 0.34f;
        [SerializeField] [Tooltip("Chance for Green gemstones (Uncommon)")] private float greenChance = 0.33f;
        [SerializeField] [Tooltip("Chance for Red gemstones (Rare)")] private float redChance = 0.33f;

        private float spawnTimer = 0f;
        private List<GameObject> activeGemstones = new List<GameObject>();

        // Events
        public System.Action<GemstoneType, int> OnGemstoneCollected; // For score updates
        public System.Action<Gemstone> OnGemstoneCollectedWithGem; // For notifications
        
        private bool isGameActive = false;
        
        public void SetGameActive(bool active)
        {
            isGameActive = active;
        }

        private void Update()
        {
            if (!isGameActive) return;
            
            spawnTimer += Time.deltaTime;
            
            if (spawnTimer >= spawnInterval)
            {
                SpawnGemstone();
                spawnTimer = 0f;
            }

            CleanupDestroyedGemstones();
        }

        private void SpawnGemstone()
        {
            GemstoneType type = SelectGemstoneType();
            GameObject prefab = GetGemstonePrefab(type);

            if (prefab == null)
            {
                Debug.LogWarning($"No prefab found for gemstone type: {type}");
                return;
            }

            Vector3 spawnPosition = CalculateRiskySpawnPosition();

            GameObject gemstone = Instantiate(prefab, spawnPosition, Quaternion.identity, transform);
            
            Gemstone gemstoneComponent = gemstone.GetComponent<Gemstone>();
            if (gemstoneComponent != null)
            {
                gemstoneComponent.SetType(type);
            }

            activeGemstones.Add(gemstone);
        }
        
        /// <summary>
        /// Calculate spawn positions that require player skill:
        /// - High up (requires jump/double jump)
        /// - Low (requires duck)
        /// - Mid-air patterns
        /// - Behind/between obstacles
        /// </summary>
        private Vector3 CalculateRiskySpawnPosition()
        {
            Vector3 spawnPosition = spawnPoint != null ? spawnPoint.position : new Vector3(15f, 0f, 0f);
            
            if (spawnAtRiskyPositions && Random.value < riskySpawnChance)
            {
                // RISKY POSITIONS - Require skill!
                int pattern = Random.Range(0, 6);
                
                switch (pattern)
                {
                    case 0: // HIGH - Requires double jump
                        spawnPosition.y = baseGroundY + 2.8f; // Reduced from 3.5f - reachable with double jump
                        Debug.Log("💎 Spawned HIGH gemstone (double jump needed)");
                        break;
                        
                    case 1: // MEDIUM HIGH - Requires single jump
                        spawnPosition.y = baseGroundY + 2.0f; // Reduced from 2.5f - single jump reach
                        Debug.Log("💎 Spawned MEDIUM gemstone (jump needed)");
                        break;
                        
                    case 2: // LOW - Requires duck/slide
                        spawnPosition.y = baseGroundY + 0.3f; // Just above ground, easier to duck under obstacles and grab
                        Debug.Log("💎 Spawned LOW gemstone (duck to grab safely)");
                        break;
                        
                    case 3: // GROUND LEVEL - Risky, might need duck timing
                        spawnPosition.y = baseGroundY;
                        Debug.Log("💎 Spawned GROUND gemstone (watch for obstacles!)");
                        break;
                        
                    case 4: // FLOATING PATTERN - Diagonal line (3 gems)
                        spawnPosition.y = baseGroundY + Random.Range(1.2f, 2.5f); // Reduced from 1.5-3.5 range
                        // Could spawn multiple in a line pattern (implement later)
                        Debug.Log("💎 Spawned FLOATING gemstone (varied height)");
                        break;
                        
                    case 5: // EXTREME HIGH - Max double jump height
                        spawnPosition.y = baseGroundY + 3.2f; // Reduced from 5.5f - must be reachable!
                        Debug.Log("💎 Spawned EXTREME HIGH gemstone (perfect double jump!)");
                        break;
                }
            }
            else
            {
                // SAFE POSITIONS - Easy to collect
                spawnPosition.y = baseGroundY + Random.Range(1.0f, 2.0f); // Mid-height, easy jump
                Debug.Log("💎 Spawned SAFE gemstone (easy height)");
            }
            
            // Add small horizontal variation
            spawnPosition.x += Random.Range(-0.3f, 0.3f);
            
            return spawnPosition;
        }

        private GemstoneType SelectGemstoneType()
        {
            // Use weighted random selection based on rarity
            float roll = Random.value;
            
            // Validate that chances add up to 1.0 (only in editor)
            #if UNITY_EDITOR
            float total = redChance + greenChance + blueChance;
            if (Mathf.Abs(total - 1.0f) > 0.01f)
            {
                Debug.LogWarning($"⚠️ Gemstone rarity chances don't add up to 1.0 (currently {total:F2})");
            }
            #endif
            
            if (roll < redChance) // 0.0 - 0.1 = Red (10%)
            {
                return GemstoneType.Red;
            }
            else if (roll < redChance + greenChance) // 0.1 - 0.4 = Green (30%)
            {
                return GemstoneType.Green;
            }
            else // 0.4 - 1.0 = Blue (60%)
            {
                return GemstoneType.Blue;
            }
        }

        private GameObject GetGemstonePrefab(GemstoneType type)
        {
            switch (type)
            {
                case GemstoneType.Blue:
                    return blueGemstonePrefab;
                case GemstoneType.Red:
                    return redGemstonePrefab;
                case GemstoneType.Green:
                    return greenGemstonePrefab;
                default:
                    return blueGemstonePrefab;
            }
        }

        private void CleanupDestroyedGemstones()
        {
            activeGemstones.RemoveAll(g => g == null);
        }

        public void NotifyGemstoneCollected(Gemstone gemstone)
        {
            int finalScore = gemstone.ScoreValue;
            
            var powerupEffects = FindFirstObjectByType<ChronoDash.Powerups.PowerupEffectsManager>();
            if (powerupEffects != null)
            {
                finalScore *= powerupEffects.ScoreMultiplier;
            }
            
            // Play gemstone sound through AudioManager
            if (Managers.AudioManager.Instance != null)
            {
                Managers.AudioManager.Instance.PlayGemstoneSound();
            }
            
            OnGemstoneCollected?.Invoke(gemstone.Type, finalScore);
            OnGemstoneCollectedWithGem?.Invoke(gemstone); // For notifications
        }

        public void ClearAllGemstones()
        {
            foreach (var gemstone in activeGemstones)
            {
                if (gemstone != null)
                {
                    Destroy(gemstone);
                }
            }
            activeGemstones.Clear();
        }
    }
}
