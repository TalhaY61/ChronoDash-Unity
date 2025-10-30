using UnityEngine;
using System.Collections;
using ChronoDash.WorldEffects;

namespace ChronoDash.Managers
{
    public enum WorldType
    {
        Desert,
        Jungle,
        Ice,
        Lava
    }
    
    /// <summary>
    /// Manages world/environment cycling with different backgrounds and platforms
    /// Randomly selects a world at game start or cycles during gameplay
    /// Each world has unique mechanics and obstacles
    /// </summary>
    public class WorldManager : MonoBehaviour
    {
        [System.Serializable]
        public class WorldData
        {
            public WorldType type;
            public Sprite backgroundSprite;
            public Sprite platformSprite;
            public Color ambientColor = Color.white;
            public string displayName;
            
            [Header("World-Specific Obstacle Types")]
            [Tooltip("Desert: Cactus, Scorpion, Vulture")]
            public GameObject[] obstacleVariants;
        }
        
        [Header("World Definitions")]
        [SerializeField] private WorldData[] worlds;
        
        [Header("References")]
        [SerializeField] private SpriteRenderer backgroundRenderer;
        [SerializeField] private SpriteRenderer[] platformRenderers; // Multiple platform segments
        
        [Header("Scrolling References")]
        [SerializeField] private ChronoDash.Environment.BackgroundScroller[] scrollers; // Both background and ground
        
        [Header("World Rotation Settings")]
        [SerializeField] private float worldRotationInterval = 60f; // Switch worlds every 60 seconds
        [SerializeField] private bool autoRotateWorlds = true;
        [SerializeField] private float difficultyScalingDelay = 10f; // Start easy for 10 seconds
        
        [Header("World-Specific Effect Prefabs")]
        [SerializeField] private GameObject iceTornadoPrefab;
        [SerializeField] private GameObject fireBlastPrefab;
        
        [Header("World-Specific Effect Instances")]
        [SerializeField] private SandstormEffect sandstormEffect;
        [SerializeField] private GravityFlipEffect gravityFlipEffect;
        
        [Header("World Effect Timing")]
        [SerializeField] private float jungleFlipMinInterval = 30f;
        [SerializeField] private float jungleFlipMaxInterval = 60f;
        [SerializeField] private float iceMinInterval = 30f;
        [SerializeField] private float iceMaxInterval = 60f;
        [SerializeField] private float lavaMinInterval = 30f;
        [SerializeField] private float lavaMaxInterval = 60f;
        
        [Header("Spawn Settings")]
        [SerializeField] private Transform playerTransform;
        [SerializeField] private Obstacles.ObstaclesManager obstaclesManager;
        
        private WorldType currentWorld;
        private int currentWorldIndex = 0;
        private float worldTimer = 0f;
        private bool isGameActive = false;
        private bool isDifficultyScaled = false;
        
        private Coroutine jungleEffectCoroutine;
        private Coroutine iceEffectCoroutine;
        private Coroutine lavaEffectCoroutine;
        
        // Events
        public System.Action<WorldType> OnWorldChanged;
        public System.Action<WorldType, GameObject[]> OnWorldObstaclesChanged; // Notify ObstaclesManager of new obstacle types
        
        // Properties
        public WorldType CurrentWorld => currentWorld;
        public string CurrentWorldName => worlds[currentWorldIndex].displayName;
        public GameObject[] CurrentWorldObstacles => GetCurrentWorldObstacles();
        
        private void Start()
        {
            if (worlds == null || worlds.Length == 0)
            {
                Debug.LogError("❌ WorldManager: No worlds defined!");
                return;
            }
            
            // Find player if not assigned
            if (playerTransform == null)
            {
                GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
                if (playerObj != null)
                {
                    playerTransform = playerObj.transform;
                }
            }
            
            // DON'T start with random world - wait for game to start
            // SelectRandomWorld() will be called from SetGameActive
        }
        
        private void Update()
        {
            if (!isGameActive) return;
            
            // Handle difficulty scaling delay ONLY for Desert world (first world)
            if (!isDifficultyScaled && currentWorld == WorldType.Desert && worldTimer >= difficultyScalingDelay)
            {
                isDifficultyScaled = true;
                ScaleUpDifficulty();
                Debug.Log($"⚡ Difficulty scaled up after {difficultyScalingDelay}s!");
            }
            
            // Handle world rotation
            if (autoRotateWorlds)
            {
                worldTimer += Time.deltaTime;
                
                if (worldTimer >= worldRotationInterval)
                {
                    worldTimer = 0f;
                    isDifficultyScaled = false; // Reset for next world (only Desert uses it)
                    SelectRandomWorld(); // Switch to a different random world
                }
            }
        }
        
        /// <summary>
        /// Called by GameManager when game starts/stops
        /// </summary>
        public void SetGameActive(bool active)
        {
            isGameActive = active;
            
            if (active)
            {
                // Start fresh when game begins
                worldTimer = 0f;
                isDifficultyScaled = false;
                SelectRandomWorld(); // Choose random starting world
                
                // Start scrolling
                StartScrolling();
            }
            else
            {
                // Clean up world effects when game stops
                CleanupWorldEffects();
                
                // Stop scrolling
                StopScrolling();
            }
        }
        
        private void StartScrolling()
        {
            Debug.Log($"🎬 WorldManager: Starting scrolling for {(scrollers != null ? scrollers.Length : 0)} scrollers");
            
            if (scrollers != null)
            {
                foreach (var scroller in scrollers)
                {
                    if (scroller != null)
                    {
                        scroller.SetScrolling(true);
                    }
                    else
                    {
                        Debug.LogWarning("⚠️ WorldManager: Null scroller in array!");
                    }
                }
            }
            else
            {
                Debug.LogWarning("⚠️ WorldManager: Scrollers array is null!");
            }
        }
        
        private void StopScrolling()
        {
            if (scrollers != null)
            {
                foreach (var scroller in scrollers)
                {
                    if (scroller != null)
                    {
                        scroller.SetScrolling(false);
                    }
                }
            }
        }
        
        /// <summary>
        /// Randomly select and apply a world
        /// </summary>
        public void SelectRandomWorld()
        {
            if (worlds.Length == 0) return;
            
            currentWorldIndex = Random.Range(0, worlds.Length);
            ApplyWorld(currentWorldIndex);
        }
        
        /// <summary>
        /// Select specific world by type
        /// </summary>
        public void SelectWorld(WorldType type)
        {
            for (int i = 0; i < worlds.Length; i++)
            {
                if (worlds[i].type == type)
                {
                    ApplyWorld(i);
                    return;
                }
            }
            
            Debug.LogWarning($"⚠️ World type {type} not found!");
        }
        
        /// <summary>
        /// Cycle to next world (for testing or viewer interaction)
        /// </summary>
        public void CycleToNextWorld()
        {
            currentWorldIndex = (currentWorldIndex + 1) % worlds.Length;
            ApplyWorld(currentWorldIndex);
        }
        
        private void ApplyWorld(int index)
        {
            if (index < 0 || index >= worlds.Length) return;
            
            WorldData world = worlds[index];
            currentWorld = world.type;
            
            // Apply background
            if (backgroundRenderer != null && world.backgroundSprite != null)
            {
                backgroundRenderer.sprite = world.backgroundSprite;
            }
            
            // Apply platform sprites
            if (platformRenderers != null && world.platformSprite != null)
            {
                foreach (var platformRenderer in platformRenderers)
                {
                    if (platformRenderer != null)
                    {
                        platformRenderer.sprite = world.platformSprite;
                    }
                }
            }
            
            // Apply ambient color (optional lighting)
            if (Camera.main != null)
            {
                Camera.main.backgroundColor = world.ambientColor;
            }
            
            // Notify observers of world change
            OnWorldChanged?.Invoke(currentWorld);
            
            // Notify ObstaclesManager of new obstacle types
            if (world.obstacleVariants != null && world.obstacleVariants.Length > 0)
            {
                OnWorldObstaclesChanged?.Invoke(currentWorld, world.obstacleVariants);
            }
            
            Debug.Log($"🌍 World changed to: {world.displayName} ({currentWorld})");
            LogWorldInfo(world);
            
            // Apply world-specific mechanics
            ApplyWorldMechanics(currentWorld);
        }
        
        private void LogWorldInfo(WorldData world)
        {
            string obstacleList = "None";
            if (world.obstacleVariants != null && world.obstacleVariants.Length > 0)
            {
                obstacleList = string.Join(", ", System.Array.ConvertAll(world.obstacleVariants, 
                    obj => obj != null ? obj.name : "null"));
            }
            
            Debug.Log($"   Obstacles: {obstacleList}");
            
            switch (world.type)
            {
                case WorldType.Desert:
                    Debug.Log("   Mechanic: Sandstorm visual effect (after 10s)");
                    break;
                case WorldType.Jungle:
                    Debug.Log("   Mechanic: Random gravity flips (after 10s)");
                    break;
                case WorldType.Ice:
                    Debug.Log("   Mechanic: Freezing projectiles (after 10s)");
                    break;
                case WorldType.Lava:
                    Debug.Log("   Mechanic: Floor bursts at player position (after 10s)");
                    break;
            }
        }
        
        private void ApplyWorldMechanics(WorldType world)
        {
            // Clean up previous world effects first
            CleanupWorldEffects();
            
            // Apply new world-specific mechanics
            switch (world)
            {
                case WorldType.Desert:
                    ApplyDesertMechanics();
                    break;
                    
                case WorldType.Jungle:
                    ApplyJungleMechanics();
                    break;
                    
                case WorldType.Ice:
                    ApplyIceMechanics();
                    break;
                    
                case WorldType.Lava:
                    ApplyLavaMechanics();
                    break;
            }
        }
        
        #region World-Specific Mechanics
        
        private void ApplyDesertMechanics()
        {
            if (sandstormEffect == null)
            {
                Debug.LogError("❌ Desert: sandstormEffect is NULL! Assign in Inspector.");
                return;
            }
            
            Debug.Log("🏜️ Desert: Starting sandstorm cycle");
            sandstormEffect.StartSandstormCycle();
        }
        
        private void ApplyJungleMechanics()
        {
            if (gravityFlipEffect == null)
            {
                Debug.LogError("❌ Jungle: gravityFlipEffect is NULL! Assign in Inspector.");
                return;
            }
            
            Debug.Log("🌲 Jungle: Starting gravity flip cycle");
            jungleEffectCoroutine = StartCoroutine(JungleGravityFlipCoroutine());
        }
        
        private void ApplyIceMechanics()
        {
            if (iceTornadoPrefab == null)
            {
                Debug.LogError("❌ Ice: iceTornadoPrefab is NULL! Assign in Inspector.");
                return;
            }
            
            Debug.Log("❄️ Ice: Starting tornado cycle");
            iceEffectCoroutine = StartCoroutine(SpawnIceTornadoesCoroutine());
        }
        
        private void ApplyLavaMechanics()
        {
            if (fireBlastPrefab == null)
            {
                Debug.LogError("❌ Lava: fireBlastPrefab is NULL! Assign in Inspector.");
                return;
            }
            
            if (playerTransform == null)
            {
                Debug.LogError("❌ Lava: playerTransform is NULL! Assign in Inspector.");
                return;
            }
            
            Debug.Log("🔥 Lava: Starting fire blast cycle");
            lavaEffectCoroutine = StartCoroutine(SpawnFireBlastsCoroutine());
        }
        
        #endregion
        
        #region World Mechanics Coroutines
        
        private IEnumerator JungleGravityFlipCoroutine()
        {
            while (isGameActive && currentWorld == WorldType.Jungle)
            {
                float waitTime = Random.Range(jungleFlipMinInterval, jungleFlipMaxInterval);
                Debug.Log($"🌲 Jungle: Next flip in {waitTime:F1}s");
                
                yield return new WaitForSeconds(waitTime);
                
                if (isGameActive && currentWorld == WorldType.Jungle && gravityFlipEffect != null)
                {
                    gravityFlipEffect.TriggerGravityFlip();
                    Debug.Log("🌲 Jungle: Gravity flip triggered!");
                }
            }
        }
        
        private IEnumerator SpawnIceTornadoesCoroutine()
        {
            while (isGameActive && currentWorld == WorldType.Ice)
            {
                float waitTime = Random.Range(iceMinInterval, iceMaxInterval);
                Debug.Log($"❄️ Ice: Next tornado in {waitTime:F1}s");
                
                yield return new WaitForSeconds(waitTime);
                
                if (!isGameActive || currentWorld != WorldType.Ice) break;
                
                float randomY = Random.Range(-4.35f, -2.30f);
                Vector3 spawnPos = new Vector3(-7f, randomY, 0f);
                
                GameObject tornado = Instantiate(iceTornadoPrefab, spawnPos, Quaternion.identity);
                
                Debug.Log($"❄️ Ice: Tornado spawned at {spawnPos}");
            }
        }
        
        private IEnumerator SpawnFireBlastsCoroutine()
        {
            while (isGameActive && currentWorld == WorldType.Lava)
            {
                float waitTime = Random.Range(lavaMinInterval, lavaMaxInterval);
                Debug.Log($"🔥 Lava: Next fire blast in {waitTime:F1}s");
                
                yield return new WaitForSeconds(waitTime);
                
                if (!isGameActive || currentWorld != WorldType.Lava || playerTransform == null) break;
                
                // No obstacle freeze - let game continue
                
                // Player X, Fixed Y: -3.7
                Vector3 spawnPos = new Vector3(playerTransform.position.x, -3.7f, 0f);
                
                GameObject blast = Instantiate(fireBlastPrefab, spawnPos, Quaternion.identity);
                
                Debug.Log($"🔥 Lava: Fire blast spawned at {spawnPos}");
            }
        }
        
        private IEnumerator FreezeObstaclesCoroutine(float duration)
        {
            if (obstaclesManager == null)
            {
                Debug.LogWarning("⚠️ ObstaclesManager is NULL - cannot freeze obstacles");
                yield break;
            }
            
            obstaclesManager.SetSpeedMultiplier(0f);
            Debug.Log($"🧊 Obstacles frozen for {duration:F1}s");
            
            yield return new WaitForSeconds(duration);
            
            obstaclesManager.SetSpeedMultiplier(1f);
            Debug.Log("✅ Obstacles unfrozen");
        }
        
        #endregion
        
        private void ScaleUpDifficulty()
        {
            // Desert difficulty scaling disabled for now
            Debug.Log("⚡ Difficulty scaling (sandstorm disabled for now)");
        }
        
        private void CleanupWorldEffects()
        {
            if (jungleEffectCoroutine != null)
            {
                StopCoroutine(jungleEffectCoroutine);
                jungleEffectCoroutine = null;
            }
            
            if (iceEffectCoroutine != null)
            {
                StopCoroutine(iceEffectCoroutine);
                iceEffectCoroutine = null;
            }
            
            if (lavaEffectCoroutine != null)
            {
                StopCoroutine(lavaEffectCoroutine);
                lavaEffectCoroutine = null;
            }
            
            if (sandstormEffect != null)
            {
                sandstormEffect.StopSandstormCycle();
            }
            
            if (gravityFlipEffect != null)
            {
                gravityFlipEffect.ForceRestoreGravity();
            }
        }
        
        public WorldData GetCurrentWorldData()
        {
            if (currentWorldIndex >= 0 && currentWorldIndex < worlds.Length)
            {
                return worlds[currentWorldIndex];
            }
            return null;
        }
        
        /// <summary>
        /// Get obstacle variants for current world
        /// </summary>
        public GameObject[] GetCurrentWorldObstacles()
        {
            if (currentWorldIndex >= 0 && currentWorldIndex < worlds.Length)
            {
                return worlds[currentWorldIndex].obstacleVariants;
            }
            return null;
        }
        
        /// <summary>
        /// Get obstacle variants for specific world type
        /// </summary>
        public GameObject[] GetWorldObstacles(WorldType worldType)
        {
            foreach (var world in worlds)
            {
                if (world.type == worldType)
                {
                    return world.obstacleVariants;
                }
            }
            return null;
        }
        
        /// <summary>
        /// Get time remaining until next world rotation
        /// </summary>
        public float GetTimeUntilNextWorld()
        {
            return worldRotationInterval - worldTimer;
        }
        
        /// <summary>
        /// Force immediate world rotation (for testing or viewer interaction)
        /// </summary>
        public void ForceWorldRotation()
        {
            worldTimer = 0f;
            SelectRandomWorld();
        }
        
        // TODO: Viewer interaction - allow viewers to purchase/trigger world changes
        public void OnViewerWorldChange(WorldType requestedWorld)
        {
            // This will be called from Solana integration later
            Debug.Log($"👁️ Viewer requested world change to: {requestedWorld}");
            worldTimer = 0f; // Reset rotation timer
            SelectWorld(requestedWorld);
        }
    }
}
