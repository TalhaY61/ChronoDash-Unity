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

            if (prefab == null) return;

            Vector3 spawnPosition = CalculateRiskySpawnPosition();

            GameObject gemstone = Instantiate(prefab, spawnPosition, Quaternion.identity, null);
            
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
            // Always spawn on RIGHT side
            // Camera rotation handles visual appearance - when flipped, right appears left!
            Vector3 spawnPosition = spawnPoint != null ? spawnPoint.position : new Vector3(15f, -3.7f, 0f);
            
            if (spawnAtRiskyPositions && Random.value < riskySpawnChance)
            {
                // RISKY POSITIONS - Require skill!
                int pattern = Random.Range(0, 6);
                
                float yOffset = 0f;
                switch (pattern)
                {
                    case 0: // HIGH - Requires double jump
                        yOffset = 2.8f; // Reduced from 3.5f - reachable with double jump
                        break;
                        
                    case 1: // MEDIUM HIGH - Requires single jump
                        yOffset = 2.0f; // Reduced from 2.5f - single jump reach
                        break;
                        
                    case 2: // LOW - Requires duck/slide
                        yOffset = 0.3f; // Just above ground, easier to duck under obstacles and grab
                        break;
                        
                    case 3: // GROUND LEVEL - Risky, might need duck timing
                        yOffset = 0f;
                        break;
                        
                    case 4: // FLOATING PATTERN - Diagonal line (3 gems)
                        yOffset = Random.Range(1.2f, 2.5f); // Reduced from 1.5-3.5 range
                        // Could spawn multiple in a line pattern (implement later)
                        break;
                        
                    case 5: // EXTREME HIGH - Max double jump height
                        yOffset = 3.2f; // Reduced from 5.5f - must be reachable!
                        break;
                }
                
                // Always add offset (camera rotation handles visual direction)
                spawnPosition.y += yOffset;
            }
            else
            {
                // SAFE POSITIONS - Easy to collect
                float safeOffset = Random.Range(1.0f, 2.0f); // Mid-height, easy jump
                spawnPosition.y += safeOffset;
            }
            
            // Add small horizontal variation
            spawnPosition.x += Random.Range(-0.3f, 0.3f);
            
            return spawnPosition;
        }

        private GemstoneType SelectGemstoneType()
        {
            // Use weighted random selection based on rarity
            float roll = Random.value;
            
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
