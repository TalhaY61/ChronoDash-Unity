using System.Collections.Generic;
using UnityEngine;
using ChronoDash.Managers;

namespace ChronoDash.Obstacles
{
    /// <summary>
    /// Manages obstacle spawning, movement, and cleanup
    /// Dynamically uses world-specific obstacles from WorldManager
    /// </summary>
    public class ObstaclesManager : MonoBehaviour
    {
        [Header("World Integration")]
        [SerializeField] private WorldManager worldManager;

        [Header("Spawn Settings")]
        [SerializeField] private Transform spawnPoint;
        [SerializeField] private float flyingHeightMax = 2f;
        [SerializeField] private float flyingHeightMin = 0.5f; // Minimum height for flying obstacles
        
        [Header("Entertainment Patterns")]
        [SerializeField] [Range(0f, 1f)] private float burstChance = 0.3f; // 30% chance for burst pattern
        [SerializeField] private int burstCount = 2; // 2-3 obstacles in burst
        [SerializeField] private float burstGap = 250f; // Gap between obstacles in burst (enough to land & jump)
        [SerializeField] [Range(0f, 1f)] private float mixedPatternChance = 0.25f; // 25% chance for mixed (flying + ground)
        
        private List<GameObject> activeObstacles = new List<GameObject>();
        private GameObject[] currentWorldObstacles;
        private bool isGameActive = false;
        
        private int currentLevel = 1;
        private float currentObstacleSpeed = 400f;
        private float currentObstacleGap = 600f;
        private const float GAP_VARIATION = 150f; // Increased for more variety!
        private const float MIN_GAP = 300f;
        
        // Burst tracking
        private int burstRemaining = 0;
        private bool isInBreathingRoom = false;
        private float breathingRoomTimer = 0f;
        
        private bool isTimeSlowed = false;
        private float slowdownFactor = 0.5f;
        private float speedMultiplier = 1f;
        
        public System.Action OnObstaclePassed;

        private void Start()
        {
            InitializeObstacleSystem();
            SetObstacleValues(1);
        }
        
        private void Update()
        {
            if (!isGameActive) return;
            
            // Handle breathing room timer
            if (isInBreathingRoom)
            {
                breathingRoomTimer -= Time.deltaTime;
                if (breathingRoomTimer <= 0f)
                {
                    isInBreathingRoom = false;
                    Debug.Log("😮‍💨 Breathing room over - resuming spawns");
                }
            }
            
            CleanupOffscreenObstacles();
            CheckSpawnCondition();
        }
        
        private void InitializeObstacleSystem()
        {
            if (worldManager != null)
            {
                worldManager.OnWorldObstaclesChanged += OnWorldObstaclesChanged;
                currentWorldObstacles = worldManager.GetCurrentWorldObstacles();
                LogObstacleStatus();
            }
            else
            {
                Debug.LogWarning("⚠️ WorldManager not assigned! Using fallback obstacles.");
            }
        }
        
        private void LogObstacleStatus()
        {
            if (currentWorldObstacles != null && currentWorldObstacles.Length > 0)
            {
                Debug.Log($"✅ ObstaclesManager: Using {currentWorldObstacles.Length} obstacles from {worldManager.CurrentWorld}");
            }
            else
            {
                Debug.LogWarning("⚠️ No obstacles assigned for current world! Using fallback.");
            }
        }
        
        private void OnWorldObstaclesChanged(WorldType worldType, GameObject[] obstacles)
        {
            currentWorldObstacles = obstacles;
            Debug.Log($"🔄 ObstaclesManager: Switched to {worldType} obstacles ({obstacles.Length} types)");
        }
        
        public void SetGameActive(bool active)
        {
            isGameActive = active;
        }

        private void CheckSpawnCondition()
        {
            // Skip spawning during breathing room
            if (isInBreathingRoom)
                return;
            
            float rightEdge = Camera.main.ViewportToWorldPoint(new Vector3(1, 0, 0)).x;
            float gapToUse;
            
            // Determine gap based on burst state or random pattern
            if (burstRemaining > 0)
            {
                // In burst mode - use small gap
                gapToUse = burstGap;
                Debug.Log($"💥 Burst spawn! {burstRemaining} obstacles remaining in burst");
            }
            else
            {
                // Random chance for breathing room (no obstacles for a moment)
                if (Random.value < 0.15f) // 15% chance after each obstacle
                {
                    isInBreathingRoom = true;
                    breathingRoomTimer = Random.Range(1.5f, 3f); // 1.5-3 second break
                    Debug.Log($"😮‍💨 Breathing room! {breathingRoomTimer:F1}s pause");
                    return;
                }
                
                // Normal gap with large variation for entertainment
                gapToUse = currentObstacleGap + Random.Range(-GAP_VARIATION, GAP_VARIATION);
                gapToUse = Mathf.Max(MIN_GAP, gapToUse);
            }
            
            if (activeObstacles.Count == 0 || (rightEdge - GetLastObstacleX() > gapToUse / 100f))
            {
                SpawnObstacle();
                
                // Decrement burst counter
                if (burstRemaining > 0)
                {
                    burstRemaining--;
                }
                else
                {
                    // Check if we should start a burst
                    if (Random.value < burstChance)
                    {
                        burstRemaining = Random.Range(1, burstCount); // 1-2 more obstacles (2-3 total)
                        Debug.Log($"🔥 Starting burst pattern! {burstRemaining + 1} obstacles incoming");
                    }
                }
            }
        }

        private float GetLastObstacleX()
        {
            if (activeObstacles.Count == 0)
                return -1000f;

            float maxX = float.MinValue;
            foreach (var obstacle in activeObstacles)
            {
                if (obstacle != null && obstacle.transform.position.x > maxX)
                {
                    maxX = obstacle.transform.position.x;
                }
            }
            return maxX;
        }

        private void SpawnObstacle()
        {
            GameObject prefabToSpawn = SelectObstaclePrefab();
            
            if (prefabToSpawn == null)
            {
                Debug.LogWarning("No obstacle prefab available to spawn!");
                return;
            }

            // Check for mixed pattern (flying + ground together)
            bool shouldSpawnMixed = Random.value < mixedPatternChance && currentWorldObstacles != null && currentWorldObstacles.Length > 1;
            
            if (shouldSpawnMixed)
            {
                // Spawn mixed pattern: one flying + one ground
                SpawnSingleObstacle(prefabToSpawn);
                
                // Get opposite type obstacle
                GameObject secondPrefab = SelectOppositeTypeObstacle(prefabToSpawn);
                if (secondPrefab != null)
                {
                    // Spawn with 32px gap (3.2 units) - double the mage size for fair gameplay
                    Vector3 mixedSpawnPos = CalculateSpawnPosition(secondPrefab);
                    mixedSpawnPos.x += 3.2f; // 32 pixels = 3.2 Unity units (mage is 16x16, doubled for fair gap)
                    
                    GameObject secondObstacle = Instantiate(secondPrefab, mixedSpawnPos, Quaternion.identity, null);
                    AdjustObstacleYPosition(secondObstacle, mixedSpawnPos.y);
                    ConfigureObstacle(secondObstacle);
                    activeObstacles.Add(secondObstacle);
                    
                    Debug.Log($"🎭 Mixed pattern! {prefabToSpawn.name} + {secondPrefab.name} - 32px gap for fair play!");
                }
            }
            else
            {
                // Normal single obstacle spawn
                SpawnSingleObstacle(prefabToSpawn);
            }
        }
        
        private void SpawnSingleObstacle(GameObject prefabToSpawn)
        {
            Vector3 spawnPosition = CalculateSpawnPosition(prefabToSpawn);
            GameObject obstacle = Instantiate(prefabToSpawn, spawnPosition, Quaternion.identity, null);
            
            // Adjust Y position based on sprite bounds to account for scaling
            AdjustObstacleYPosition(obstacle, spawnPosition.y);
            
            ConfigureObstacle(obstacle);
            activeObstacles.Add(obstacle);
        }
        
        private GameObject SelectOppositeTypeObstacle(GameObject originalPrefab)
        {
            bool originalIsFlying = IsFlyingObstacle(originalPrefab);
            
            // Try to find opposite type
            var validObstacles = System.Array.FindAll(currentWorldObstacles, obj => obj != null && IsFlyingObstacle(obj) != originalIsFlying);
            
            if (validObstacles.Length > 0)
            {
                return validObstacles[Random.Range(0, validObstacles.Length)];
            }
            
            return null;
        }
        
        private Vector3 CalculateSpawnPosition(GameObject prefab)
        {
            float spawnX = spawnPoint != null ? spawnPoint.position.x : 15f;
            float spawnY = DetermineSpawnHeight(prefab);
            
            return new Vector3(spawnX, spawnY, 0f);
        }
        
        private float DetermineSpawnHeight(GameObject prefab)
        {
            float groundY = spawnPoint != null ? spawnPoint.position.y : -3.7f;
            
            if (IsFlyingObstacle(prefab))
            {
                // Random height between minimum and maximum flying height
                float minFlyingHeight = Mathf.Max(flyingHeightMin, 0.5f); // At least 0.5 units above ground
                float randomHeight = Random.Range(minFlyingHeight, flyingHeightMax);
                float flyingY = groundY + randomHeight;
                
                Debug.Log($"🦅 Flying obstacle spawning at height +{randomHeight:F2} (range: {minFlyingHeight:F2} to {flyingHeightMax:F2})");
                return flyingY;
            }
            
            return groundY;
        }
        
        private bool IsFlyingObstacle(GameObject prefab)
        {
            string obstacleName = prefab.name.ToLower();
            return obstacleName.Contains("vulture") || 
                   obstacleName.Contains("owl") || 
                   obstacleName.Contains("bird") || 
                   obstacleName.Contains("fly");
        }
        
        private void AdjustObstacleYPosition(GameObject obstacle, float targetGroundY)
        {
            SpriteRenderer spriteRenderer = obstacle.GetComponent<SpriteRenderer>();
            if (spriteRenderer == null || spriteRenderer.sprite == null)
            {
                Debug.LogWarning($"⚠️ {obstacle.name} has no SpriteRenderer or sprite!");
                return;
            }
            
            // Get the sprite's bounds (accounts for scale)
            Bounds bounds = spriteRenderer.bounds;
            float spriteBottomY = bounds.min.y;
            
            // Calculate offset needed to place bottom at target ground level
            float yOffset = targetGroundY - spriteBottomY;
            
            // Apply adjustment
            Vector3 pos = obstacle.transform.position;
            pos.y += yOffset;
            obstacle.transform.position = pos;
            
            Debug.Log($"📏 {obstacle.name}: Scale={obstacle.transform.localScale}, BoundsHeight={bounds.size.y:F2}, Adjusted Y from {spriteBottomY:F2} to {pos.y:F2}");
        }
        
        private void ConfigureObstacle(GameObject obstacleObj)
        {
            Obstacle obstacleComponent = obstacleObj.GetComponent<Obstacle>();
            if (obstacleComponent == null) return;
            
            float finalSpeed = currentObstacleSpeed / 100f;
            
            var powerupEffects = FindFirstObjectByType<ChronoDash.Powerups.PowerupEffectsManager>();
            if (powerupEffects != null)
            {
                finalSpeed *= powerupEffects.SpeedMultiplier;
            }
            
            obstacleComponent.MoveSpeed = finalSpeed;
            obstacleComponent.SetSlowdown(isTimeSlowed, slowdownFactor);
            obstacleComponent.SetSpeedMultiplier(speedMultiplier);
            obstacleComponent.OnObstaclePassed += HandleObstaclePassed;
        }
        
        private void HandleObstaclePassed(Obstacle obstacle)
        {
            // Player successfully passed this obstacle!
            OnObstaclePassed?.Invoke();
            
            // Unsubscribe to prevent multiple calls
            if (obstacle != null)
            {
                obstacle.OnObstaclePassed -= HandleObstaclePassed;
            }
        }

        private GameObject SelectObstaclePrefab()
        {
            if (currentWorldObstacles != null && currentWorldObstacles.Length > 0)
            {
                var validObstacles = System.Array.FindAll(currentWorldObstacles, obj => obj != null);
                
                if (validObstacles.Length > 0)
                {
                    int randomIndex = Random.Range(0, validObstacles.Length);
                    return validObstacles[randomIndex];
                }
            }
            
            Debug.LogWarning("⚠️ No valid world obstacles! Cannot spawn obstacle.");
            return null;
        }

        private void CleanupOffscreenObstacles()
        {
            float leftEdge = Camera.main.ViewportToWorldPoint(new Vector3(0, 0, 0)).x;
            
            for (int i = activeObstacles.Count - 1; i >= 0; i--)
            {
                if (activeObstacles[i] == null)
                {
                    activeObstacles.RemoveAt(i);
                    continue;
                }
                
                // Check if obstacle has moved past the left edge of the screen - just cleanup
                if (activeObstacles[i].transform.position.x < leftEdge - 2f)
                {
                    // Unsubscribe from events before destroying
                    Obstacle obstacleComponent = activeObstacles[i].GetComponent<Obstacle>();
                    if (obstacleComponent != null)
                    {
                        obstacleComponent.OnObstaclePassed -= HandleObstaclePassed;
                    }
                    
                    Destroy(activeObstacles[i]);
                    activeObstacles.RemoveAt(i);
                    
                    // NOTE: Don't call OnObstaclePassed here - it should be called when player passes, not when off-screen
                }
            }
        }

        public void SetLevel(int level)
        {
            currentLevel = level;
            SetObstacleValues(level);
        }

        private void SetObstacleValues(int level)
        {
            // DYNAMIC DIFFICULTY - Speed, gaps, AND patterns change!
            switch (level)
            {
                case 1:
                    currentObstacleSpeed = 500f;
                    currentObstacleGap = 450f;
                    burstChance = 0.2f; // Less bursts at start
                    mixedPatternChance = 0.15f; // Less mixed patterns
                    break;
                case 2:
                    currentObstacleSpeed = 550f;
                    currentObstacleGap = 400f;
                    burstChance = 0.25f;
                    mixedPatternChance = 0.2f;
                    break;
                case 3:
                    currentObstacleSpeed = 600f;
                    currentObstacleGap = 380f;
                    burstChance = 0.3f;
                    mixedPatternChance = 0.25f;
                    break;
                case 4:
                    currentObstacleSpeed = 650f;
                    currentObstacleGap = 360f;
                    burstChance = 0.35f;
                    mixedPatternChance = 0.3f;
                    break;
                case 5:
                    currentObstacleSpeed = 700f;
                    currentObstacleGap = 340f;
                    burstChance = 0.4f;
                    mixedPatternChance = 0.35f;
                    break;
                case 6:
                    currentObstacleSpeed = 750f;
                    currentObstacleGap = 320f;
                    burstChance = 0.45f;
                    mixedPatternChance = 0.4f;
                    break;
                case 7:
                    currentObstacleSpeed = 800f;
                    currentObstacleGap = 300f;
                    burstChance = 0.5f;
                    mixedPatternChance = 0.45f;
                    break;
                default:
                    // Level 8+: Max difficulty - Chaos mode!
                    currentObstacleSpeed = 850f;
                    currentObstacleGap = 280f;
                    burstChance = 0.55f; // Very frequent bursts
                    mixedPatternChance = 0.5f; // Very frequent mixed patterns
                    break;
            }
            
            Debug.Log($"⚡ Level {level}: Speed={currentObstacleSpeed}, Gap={currentObstacleGap}, Burst%={burstChance*100:F0}, Mixed%={mixedPatternChance*100:F0}");
        }

        public void SetTimeSlowdown(bool slowdown, float factor = 0.5f)
        {
            isTimeSlowed = slowdown;
            slowdownFactor = factor;

            // Apply to all active obstacles
            foreach (var obstacleObj in activeObstacles)
            {
                if (obstacleObj != null)
                {
                    Obstacle obstacle = obstacleObj.GetComponent<Obstacle>();
                    if (obstacle != null)
                    {
                        obstacle.SetSlowdown(slowdown, factor);
                    }
                }
            }
        }

        public void SetSpeedMultiplier(float multiplier)
        {
            speedMultiplier = multiplier;
            Debug.Log($"Obstacle speed multiplier set to {multiplier}x");

            // Apply to all active obstacles
            foreach (var obstacleObj in activeObstacles)
            {
                if (obstacleObj != null)
                {
                    Obstacle obstacle = obstacleObj.GetComponent<Obstacle>();
                    if (obstacle != null)
                    {
                        obstacle.SetSpeedMultiplier(multiplier);
                    }
                }
            }
        }

        public void ClearAllObstacles()
        {
            foreach (var obstacle in activeObstacles)
            {
                if (obstacle != null)
                {
                    Destroy(obstacle);
                }
            }
            activeObstacles.Clear();
        }
    }
}
