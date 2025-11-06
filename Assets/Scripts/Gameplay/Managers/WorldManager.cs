using UnityEngine;

namespace ChronoDash.Managers {
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
    /// Each world provides visual variety and world-specific obstacles
    /// Global effects (gravity flip, screen shake, darkness) are handled by GameplayEffectsManager
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
        
        private WorldType currentWorld;
        private int currentWorldIndex = 0;
        private float worldTimer = 0f;
        private bool isGameActive = false;
        
        // Events
        public System.Action<WorldType> OnWorldChanged;
        public System.Action<WorldType, GameObject[]> OnWorldObstaclesChanged; // Notify ObstaclesManager of new obstacle types
        
        // Properties
        public WorldType CurrentWorld => currentWorld;
        public string CurrentWorldName => worlds[currentWorldIndex].displayName;
        public GameObject[] CurrentWorldObstacles => GetCurrentWorldObstacles();
        
        private void Start()
        {
            if (worlds == null || worlds.Length == 0) return;
            
            // World selection happens when game starts (SetGameActive)
        }
        
        private void Update()
        {
            if (!isGameActive) return;
            
            // Handle world rotation
            if (autoRotateWorlds)
            {
                worldTimer += Time.deltaTime;
                
                if (worldTimer >= worldRotationInterval)
                {
                    worldTimer = 0f;
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
                SelectRandomWorld(); // Choose random starting world
                
                // Start scrolling
                StartScrolling();
            }
            else
            {
                // Stop scrolling when game ends
                StopScrolling();
            }
        }
        
        private void StartScrolling()
        {
            
            if (scrollers != null)
            {
                foreach (var scroller in scrollers)
                {
                    if (scroller != null)
                    {
                        scroller.SetScrolling(true);
                    }
                }
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
