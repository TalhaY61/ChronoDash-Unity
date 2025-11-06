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
        [SerializeField] private GameplayEffectsManager gameplayEffectsManager;

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
            var healthManager = FindFirstObjectByType<HealthManager>();
            if (healthManager != null)
            {
                healthManager.OnPlayerDied += OnPlayerDeath;
                healthManager.OnHealthChanged += OnPlayerHealthChanged;
            }

            if (obstaclesManager != null)
            {
                obstaclesManager.OnObstaclePassed += OnObstaclePassed;
            }

            if (gemstoneManager != null)
            {
                gemstoneManager.OnGemstoneCollected += OnGemstoneCollected;
            }

            if (difficultyManager != null)
            {
                difficultyManager.OnScoreChanged += OnScoreChanged;
                difficultyManager.OnDifficultyTierChanged += OnDifficultyTierChanged;
            }

            // Subscribe to combo events
            if (comboManager != null && obstaclesManager != null)
            {
                obstaclesManager.OnObstaclePassed += comboManager.OnSuccessfulDodge;
            }

            if (comboManager != null && healthManager != null)
            {
                healthManager.OnPlayerDied += comboManager.OnPlayerHit;
            }

            // Connect ComboManager multiplier to DifficultyManager
            if (comboManager != null && difficultyManager != null)
            {
                comboManager.OnMultiplierChanged += (multiplier) =>
                {
                    difficultyManager.SetScoreMultiplier(multiplier);
                };
            }

            // Start countdown
            BeginCountdown();
        }

        private void BeginCountdown()
        {
            countdownTimer = countdownDuration;
            isGameActive = false;

            if (player != null)
            {
                player.SetCanControl(false);
            }

            if (obstaclesManager != null)
            {
                obstaclesManager.SetGameActive(false);
            }

            if (uiManager != null)
            {
                uiManager.ShowCountdown(true);
            }
        }

        private void StartGame()
        {
            isGameActive = true;

            if (player != null)
            {
                player.SetCanControl(true);
            }

            if (obstaclesManager != null)
            {
                obstaclesManager.SetGameActive(true);
            }

            if (gemstoneManager != null)
            {
                gemstoneManager.SetGameActive(true);
            }

            var powerupSpawnManager = FindFirstObjectByType<ChronoDash.Powerups.PowerupManager>();
            if (powerupSpawnManager != null)
            {
                powerupSpawnManager.StartSpawning();
            }

            if (worldManager != null) {
                worldManager.SetGameActive(true);
            }
            
            if (gameplayEffectsManager != null) {
                gameplayEffectsManager.SetGameActive(true);
            }

            if (uiManager != null) {
                uiManager.ShowCountdown(false);
                uiManager.ShowGameUI(true);
            }
        }

        public void RestartGame()
        {
            Time.timeScale = 1f;

            if (uiManager != null)
            {
                uiManager.ShowGameOver(false, 0);
            }

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
            var keyboard = Keyboard.current;
            if (keyboard == null) return;

            if (isGameActive && (keyboard.pKey.wasPressedThisFrame || keyboard.escapeKey.wasPressedThisFrame))
            {
                if (isPaused)
                    ResumeGame();
                else
                    PauseGame();
            }

            if (!isGameActive && countdownTimer <= 0 && keyboard.rKey.wasPressedThisFrame)
            {
                RestartGame();
            }
        }

        private void OnPlayerDeath()
        {
            isGameActive = false;
            Time.timeScale = 0f;

            if (player != null)
            {
                player.SetCanControl(false);
            }

            if (obstaclesManager != null)
            {
                obstaclesManager.SetGameActive(false);
            }

            if (gemstoneManager != null)
            {
                gemstoneManager.SetGameActive(false);
            }

            if (worldManager != null) {
                worldManager.SetGameActive(false);
            }
            
            if (gameplayEffectsManager != null) {
                gameplayEffectsManager.SetGameActive(false);
            }
            
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
        }

        private void OnDifficultyTierChanged(int tier)
        {
            if (obstaclesManager != null)
            {
                obstaclesManager.SetLevel(tier + 1);
            }

            var powerupSpawnManager = FindFirstObjectByType<ChronoDash.Powerups.PowerupManager>();
            if (powerupSpawnManager != null)
            {
                powerupSpawnManager.SetDifficultyLevel(tier + 1);
            }

            if (uiManager != null)
            {
            }
        }

        private void OnScoreChanged(int newScore)
        {
            if (uiManager != null)
            {
                uiManager.UpdateScore(newScore);
            }
        }

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

            // Unsubscribe from combo events
            if (comboManager != null && obstaclesManager != null)
            {
                obstaclesManager.OnObstaclePassed -= comboManager.OnSuccessfulDodge;
            }

            if (comboManager != null && healthManager != null)
            {
                healthManager.OnPlayerDied -= comboManager.OnPlayerHit;
            }
        }
    }
}
