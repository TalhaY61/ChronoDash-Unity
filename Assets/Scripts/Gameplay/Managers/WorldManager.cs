using UnityEngine;
using System.Collections;

namespace ChronoDash.Managers
{
    public enum WorldType
    {
        Desert,
        Jungle,
        Ice,
        Lava
    }
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
        [SerializeField] private Environment.BackgroundScroller[] scrollers; // Both background and ground

        [Header("World Rotation Settings")]
        [SerializeField] private float worldRotationInterval = 60f; // Switch worlds every 60 seconds
        [SerializeField] private bool autoRotateWorlds = true;
        [SerializeField] private float difficultyScalingDelay = 10f; // Start easy for 10 seconds

        [Header("Spawn Settings")]
        [SerializeField] private Transform playerTransform;
        [SerializeField] private Obstacles.ObstaclesManager obstaclesManager;

        private WorldType currentWorld;
        private int currentWorldIndex = 0;
        private float worldTimer = 0f;
        private bool isGameActive = false;
        private bool isDifficultyScaled = false;

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

            // Find player if not assigned
            if (playerTransform == null)
            {
                GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
                if (playerObj != null)
                {
                    playerTransform = playerObj.transform;
                }
            }

            // Sync initial background with MenuBackgroundManager if available
            // This ensures all scenes show the same background
            if (MenuBackgroundManager.Instance != null)
            {
                int menuBackgroundIndex = MenuBackgroundManager.Instance.GetCurrentBackgroundIndex();
                // Menu background order matches world order (Desert, Jungle, Ice, Lava)
                if (menuBackgroundIndex >= 0 && menuBackgroundIndex < worlds.Length)
                {
                    currentWorldIndex = menuBackgroundIndex;
                    currentWorld = worlds[currentWorldIndex].type;
                    ApplyWorldVisuals(currentWorldIndex); // Apply visual only, no mechanics
                }
            }

            // DON'T start with random world or mechanics - wait for game to start
            // SelectRandomWorld() will be called from SetGameActive
        }

        private void Update()
        {
            if (!isGameActive) return;

            // Handle difficulty scaling delay ONLY for Desert world (first world)
            if (!isDifficultyScaled && currentWorld == WorldType.Desert && worldTimer >= difficultyScalingDelay)
            {
                isDifficultyScaled = true;
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
                // Stop scrolling
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

        public void SelectRandomWorld()
        {
            if (worlds.Length == 0) return;

            currentWorldIndex = Random.Range(0, worlds.Length);
            ApplyWorld(currentWorldIndex);
        }

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

        /// <summary>
        /// Apply only visual changes (background, platforms) without world mechanics
        /// Used for syncing with menu background at game start
        /// </summary>
        private void ApplyWorldVisuals(int index)
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

        public WorldData GetCurrentWorldData()
        {
            if (currentWorldIndex >= 0 && currentWorldIndex < worlds.Length)
            {
                return worlds[currentWorldIndex];
            }
            return null;
        }

        public GameObject[] GetCurrentWorldObstacles()
        {
            if (currentWorldIndex >= 0 && currentWorldIndex < worlds.Length)
            {
                return worlds[currentWorldIndex].obstacleVariants;
            }
            return null;
        }

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

        public float GetTimeUntilNextWorld()
        {
            return worldRotationInterval - worldTimer;
        }

        public void ForceWorldRotation()
        {
            worldTimer = 0f;
            SelectRandomWorld();
        }
    }
}
