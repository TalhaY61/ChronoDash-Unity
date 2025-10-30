using UnityEngine;
using UnityEngine.InputSystem;
using ChronoDash.Player;
using ChronoDash.Obstacles;
using ChronoDash.Gemstones;
using ChronoDash.Abilities;
using ChronoDash.Powerups;

namespace ChronoDash.Managers
{
    public class GameManager : MonoBehaviour
    {
        [Header("Core Managers - NEW MVP SYSTEM")]
        [SerializeField] private DifficultyManager difficultyManager;
        [SerializeField] private ComboManager comboManager;
        [SerializeField] private TimeBubble timeBubble;
        [SerializeField] private WorldManager worldManager;

        [Header("Component Managers")]
        [SerializeField] private PlayerController player;
        [SerializeField] private ObstaclesManager obstaclesManager;
        [SerializeField] private GemstoneManager gemstoneManager;
        [SerializeField] private UIManager uiManager;
        [SerializeField] private PowerupManager powerupManager;

        [Header("Game Settings")]
        [SerializeField] private float countdownDuration = 3f;

        private bool isGameActive = false;
        private bool isPaused = false;
        private float countdownTimer;

        public bool IsGameActive => isGameActive;
        public bool IsPaused => isPaused;

        private void Start()
        {
            InitializeGame();
        }

        private void Update()
        {
            HandleInput();

            if (!isGameActive && countdownTimer > 0)
            {
                countdownTimer -= Time.deltaTime;
                if (uiManager != null)
                {
                    uiManager.UpdateCountdown(Mathf.CeilToInt(countdownTimer));
                }

                if (countdownTimer <= 0)
                {
                    StartGame();
                }
            }
        }

        private void InitializeGame()
        {
            Debug.Log("=== GameManager Initialization ===");

            var healthManager = FindFirstObjectByType<HealthManager>();
            if (healthManager != null)
            {
                healthManager.OnPlayerDied += OnPlayerDeath;
                healthManager.OnHealthChanged += OnPlayerHealthChanged;
                Debug.Log("✅ HealthManager events subscribed");
            }
            else
            {
                Debug.LogWarning("⚠️ HealthManager reference is NULL!");
            }

            if (obstaclesManager != null)
            {
                obstaclesManager.OnObstaclePassed += OnObstaclePassed;
                Debug.Log("✅ ObstaclesManager events subscribed");
            }
            else
            {
                Debug.LogWarning("⚠️ ObstaclesManager reference is NULL!");
            }

            if (gemstoneManager != null)
            {
                gemstoneManager.OnGemstoneCollected += OnGemstoneCollected;
                Debug.Log("✅ GemstoneManager events subscribed");
            }

            if (difficultyManager != null)
            {
                difficultyManager.OnScoreChanged += OnScoreChanged;
                difficultyManager.OnDifficultyTierChanged += OnDifficultyTierChanged;
                Debug.Log("✅ DifficultyManager events subscribed");
            }
            else
            {
                Debug.LogWarning("⚠️ DifficultyManager reference is NULL!");
            }

            if (timeBubble != null)
            {
                timeBubble.OnBubbleStateChanged += OnTimeBubbleStateChanged;
                Debug.Log("✅ TimeBubble events subscribed");
            }

            if (worldManager != null)
            {
                worldManager.OnWorldChanged += OnWorldChanged;
                Debug.Log("✅ WorldManager events subscribed");
            }

            // Subscribe to combo events
            if (comboManager != null && obstaclesManager != null)
            {
                obstaclesManager.OnObstaclePassed += comboManager.OnSuccessfulDodge;
                Debug.Log("✅ ComboManager connected to obstacle pass events");
            }
            else
            {
                Debug.LogWarning($"⚠️ ComboManager wiring FAILED! ComboManager={comboManager != null}, ObstaclesManager={obstaclesManager != null}");
            }

            if (comboManager != null && healthManager != null)
            {
                healthManager.OnPlayerDied += comboManager.OnPlayerHit;
                Debug.Log("✅ ComboManager connected to player death events");
            }

            // Connect ComboManager multiplier to DifficultyManager
            if (comboManager != null && difficultyManager != null)
            {
                comboManager.OnMultiplierChanged += (multiplier) =>
                {
                    difficultyManager.SetScoreMultiplier(multiplier);
                    if (uiManager != null)
                    {
                        // TODO: Update UI to show multiplier
                        // uiManager.UpdateMultiplier(multiplier);
                    }
                };
                Debug.Log("✅ ComboManager connected to DifficultyManager");
            }

            // Subscribe to powerup events
            var powerupEffectsManager = FindFirstObjectByType<ChronoDash.Powerups.PowerupEffectsManager>();
            if (powerupEffectsManager != null)
            {
                powerupEffectsManager.OnPowerupCollected += OnPowerupCollected;
                Debug.Log("✅ PowerupEffectsManager events subscribed");
            }
            else
            {
                Debug.LogWarning("⚠️ PowerupEffectsManager is NULL!");
            }

            var powerupSpawnManager = FindFirstObjectByType<ChronoDash.Powerups.PowerupManager>();
            if (powerupSpawnManager != null)
            {
                Debug.Log("✅ PowerupManager found");
            }
            else
            {
                Debug.LogWarning("⚠️ PowerupManager is NULL!");
            }

            // Start countdown
            BeginCountdown();
        }

        private void BeginCountdown()
        {
            countdownTimer = countdownDuration;
            isGameActive = false;

            // Disable player control during countdown
            if (player != null)
            {
                player.SetCanControl(false);
            }

            // Stop obstacles from spawning during countdown
            if (obstaclesManager != null)
            {
                obstaclesManager.SetGameActive(false);
            }

            if (uiManager != null)
            {
                uiManager.ShowCountdown(true);
            }

            Debug.Log("Countdown started - Game frozen");
        }

        private void StartGame()
        {
            isGameActive = true;

            // Enable player control
            if (player != null)
            {
                player.SetCanControl(true);
            }

            // Enable obstacles to spawn
            if (obstaclesManager != null)
            {
                obstaclesManager.SetGameActive(true);
            }

            // Enable gemstones to spawn
            if (gemstoneManager != null)
            {
                gemstoneManager.SetGameActive(true);
                var powerupManager = FindFirstObjectByType<ChronoDash.Powerups.PowerupManager>();
                if (powerupManager != null)
                {
                    powerupManager.StartSpawning();
                }
                Debug.Log("✅ Gemstone spawning ENABLED!");
            }

            // Enable powerup spawning
            var powerupSpawnManager = FindFirstObjectByType<ChronoDash.Powerups.PowerupManager>();
            if (powerupSpawnManager != null)
            {
                powerupSpawnManager.StartSpawning();
                Debug.Log("✅ Powerup spawning ENABLED!");
            }

            // Start world rotation and select random starting world
            if (worldManager != null)
            {
                worldManager.SetGameActive(true);
            }

            if (uiManager != null)
            {
                uiManager.ShowCountdown(false);
                uiManager.ShowGameUI(true);
            }

            Debug.Log("Game Started! Player can now control.");
        }

        public void RestartGame()
        {
            Debug.Log("Restarting game...");

            // Unfreeze time first
            Time.timeScale = 1f;

            // Hide game over UI
            if (uiManager != null)
            {
                uiManager.ShowGameOver(false, 0);
            }

            // Reset all managers
            if (difficultyManager != null)
            {
                difficultyManager.Reset();
            }

            if (comboManager != null)
            {
                comboManager.Reset();
            }

            if (obstaclesManager != null)
            {
                obstaclesManager.ClearAllObstacles();
            }

            if (gemstoneManager != null)
            {
                gemstoneManager.ClearAllGemstones();
            }

            var powerupSpawnManager = FindFirstObjectByType<PowerupManager>();
            if (powerupSpawnManager != null)
            {
                powerupSpawnManager.StopSpawning();
                powerupSpawnManager.ClearAllPowerups();
            }

            var healthManager = FindFirstObjectByType<HealthManager>();
            if (healthManager != null)
            {
                healthManager.Reset();
            }
            
            var powerupEffectsManager = FindFirstObjectByType<ChronoDash.Powerups.PowerupEffectsManager>();
            if (powerupEffectsManager != null)
            {
                powerupEffectsManager.Reset();
            }

            // Start countdown again
            BeginCountdown();
        }

        public void PauseGame()
        {
            isPaused = true;
            Time.timeScale = 0f;

            if (uiManager != null)
            {
                uiManager.ShowPauseMenu(true);
            }
        }

        public void ResumeGame()
        {
            isPaused = false;
            Time.timeScale = 1f;

            if (uiManager != null)
            {
                uiManager.ShowPauseMenu(false);
            }
        }

        private void HandleInput()
        {
            // Use Unity's new Input System
            var keyboard = Keyboard.current;
            if (keyboard == null) return;

            // Pause toggle (only when game is active)
            if (isGameActive && (keyboard.pKey.wasPressedThisFrame || keyboard.escapeKey.wasPressedThisFrame))
            {
                if (isPaused)
                    ResumeGame();
                else
                    PauseGame();
            }

            // Restart (only when game over - when game is not active and countdown finished)
            if (!isGameActive && countdownTimer <= 0 && keyboard.rKey.wasPressedThisFrame)
            {
                Debug.Log("R pressed - Restarting game...");
                RestartGame();
            }
        }

        #region Event Handlers

        private void OnPlayerDeath()
        {
            isGameActive = false;

            // FREEZE the game completely
            Time.timeScale = 0f;

            // Disable player control
            if (player != null)
            {
                player.SetCanControl(false);
            }

            // Stop obstacles
            if (obstaclesManager != null)
            {
                obstaclesManager.SetGameActive(false);
            }

            // Stop gemstones
            if (gemstoneManager != null)
            {
                gemstoneManager.SetGameActive(false);
                Debug.Log("❌ Gemstone spawning DISABLED (player died)");
            }

            // Stop world rotation and clean up effects
            if (worldManager != null)
            {
                worldManager.SetGameActive(false);
            }
            
            // Stop powerup spawning and clear active powerups
            var powerupSpawnManager = FindFirstObjectByType<ChronoDash.Powerups.PowerupManager>();
            if (powerupSpawnManager != null)
            {
                powerupSpawnManager.StopSpawning();
                powerupSpawnManager.ClearAllPowerups();
            }
            
            var powerupEffectsManager = FindFirstObjectByType<ChronoDash.Powerups.PowerupEffectsManager>();
            if (powerupEffectsManager != null)
            {
                powerupEffectsManager.Reset();
            }

            if (uiManager != null)
            {
                uiManager.ShowGameOver(true, difficultyManager != null ? difficultyManager.CurrentScore : 0);
            }

            Debug.Log("Game Over! Press R to restart.");
        }

        private void OnPlayerHealthChanged(int newHealth, int maxHealth)
        {
            if (uiManager != null)
            {
                uiManager.UpdateHealth(newHealth);
            }
        }

        private void OnObstaclePassed()
        {
            if (difficultyManager != null)
            {
                difficultyManager.AddScore(1); // 1 point per obstacle dodged
            }
        }

        private void OnGemstoneCollected(GemstoneType type, int scoreValue)
        {
            if (difficultyManager != null)
            {
                difficultyManager.AddScore(scoreValue);
            }

            Debug.Log($"Collected {type} gemstone! +{scoreValue} points");
        }

        private void OnDifficultyTierChanged(int tier)
        {
            if (obstaclesManager != null)
            {
                obstaclesManager.SetLevel(tier + 1);
                Debug.Log($"⚡ ObstaclesManager level set to {tier + 1}");
            }

            var powerupSpawnManager = FindFirstObjectByType<ChronoDash.Powerups.PowerupManager>();
            if (powerupSpawnManager != null)
            {
                powerupSpawnManager.SetDifficultyLevel(tier + 1);
                Debug.Log($"⚡ PowerupManager difficulty set to {tier + 1}");
            }

            if (uiManager != null)
            {
            }

            Debug.Log($"🎯 Difficulty increased to Tier {tier}!");
        }

        private void OnScoreChanged(int newScore)
        {
            if (uiManager != null)
            {
                uiManager.UpdateScore(newScore);
            }
        }

        private void OnTimeBubbleStateChanged(bool isActive)
        {
            Debug.Log($"🕐 Time Bubble {(isActive ? "ACTIVATED" : "DEACTIVATED")}!");
            // Obstacles will check TimeBubble position each frame automatically
        }

        private void OnWorldChanged(WorldType worldType)
        {
            Debug.Log($"🌍 World changed to: {worldType}");

            if (worldManager != null)
            {
                Debug.Log($"   Display Name: {worldManager.CurrentWorldName}");
            }

            // World visuals are handled by WorldManager automatically
        }

        private void OnPowerupCollected(PowerupType type, int count)
        {
            Debug.Log($"💊 Powerup collected: {type} (x{count})");
        }

        private void OnPowerupActivated(PowerupType type, float duration, int stackCount)
        {
            Debug.Log($"✨ Powerup activated: {type} for {duration}s (x{stackCount})");
        }

        private void OnPowerupExpired(PowerupType type)
        {
            Debug.Log($"⏱️ Powerup expired: {type}");
        }

        #endregion

        private void OnDestroy()
        {
            var healthManager = FindFirstObjectByType<HealthManager>();
            if (healthManager != null)
            {
                healthManager.OnPlayerDied -= OnPlayerDeath;
                healthManager.OnHealthChanged -= OnPlayerHealthChanged;
            }

            if (obstaclesManager != null)
            {
                obstaclesManager.OnObstaclePassed -= OnObstaclePassed;
            }

            if (gemstoneManager != null)
            {
                gemstoneManager.OnGemstoneCollected -= OnGemstoneCollected;
            }

            if (difficultyManager != null)
            {
                difficultyManager.OnScoreChanged -= OnScoreChanged;
                difficultyManager.OnDifficultyTierChanged -= OnDifficultyTierChanged;
            }

            if (timeBubble != null)
            {
                timeBubble.OnBubbleStateChanged -= OnTimeBubbleStateChanged;
            }

            if (worldManager != null)
            {
                worldManager.OnWorldChanged -= OnWorldChanged;
            }

            // Unsubscribe from combo events
            if (comboManager != null && obstaclesManager != null)
            {
                obstaclesManager.OnObstaclePassed -= comboManager.OnSuccessfulDodge;
            }

            if (comboManager != null && healthManager != null)
            {
                healthManager.OnPlayerDied -= comboManager.OnPlayerHit;
            }

            var powerupEffectsManager = FindFirstObjectByType<ChronoDash.Powerups.PowerupEffectsManager>();
            if (powerupEffectsManager != null)
            {
                powerupEffectsManager.OnPowerupCollected -= OnPowerupCollected;
            }
        }
    }
}
