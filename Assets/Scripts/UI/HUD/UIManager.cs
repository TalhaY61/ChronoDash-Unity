using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ChronoDash.Managers
{
    public class UIManager : MonoBehaviour
    {
        [Header("Game UI")]
        [SerializeField] private TextMeshProUGUI scoreText;
        [SerializeField] private TextMeshProUGUI levelText;
        [SerializeField] private TextMeshProUGUI fpsText;
        [SerializeField] private GameObject[] healthHearts;

        [Header("Countdown UI")]
        [SerializeField] private GameObject countdownPanel;
        [SerializeField] private TextMeshProUGUI countdownText;

        [Header("Pause Menu")]
        [SerializeField] private GameObject pausePanel;
        [SerializeField] private TextMeshProUGUI pauseText;

        [Header("Game Over UI")]
        [SerializeField] private GameObject gameOverPanel;
        [SerializeField] private TextMeshProUGUI gameOverScoreText;
        [SerializeField] private TextMeshProUGUI gameOverText;
        [SerializeField] private Button restartButton;
        [SerializeField] private Button mainMenuButton;

        [Header("Ability UI")]
        [SerializeField] private Image abilityIcon;
        [SerializeField] private Image abilityCooldownOverlay;
        
        private GameManager gameManager;

        private void Start()
        {
            gameManager = FindFirstObjectByType<GameManager>();
            SetupButtons();
        }
        
        private void SetupButtons()
        {
            if (restartButton != null)
            {
                restartButton.onClick.AddListener(OnRestartClicked);
            }
            
            if (mainMenuButton != null)
            {
                mainMenuButton.onClick.AddListener(OnMainMenuClicked);
            }
        }
        
        private void OnRestartClicked()
        {
            if (gameManager != null)
            {
                gameManager.RestartGame();
            }
        }
        
        private void OnMainMenuClicked()
        {
            Time.timeScale = 1f; // Reset time scale
            SceneController.Instance.LoadScene(SceneController.MAIN_MENU);
        }

        private void Update()
        {
            UpdateFPS();
        }

        public void UpdateScore(int score)
        {
            if (scoreText != null)
            {
                scoreText.text = $"Score: {score}";
            }
        }

        public void UpdateLevel(int level)
        {
            if (levelText != null)
            {
                levelText.text = $"Level: {level}";
            }
        }

        public void UpdateHealth(int health)
        {
            if (healthHearts != null)
            {
                for (int i = 0; i < healthHearts.Length; i++)
                {
                    if (healthHearts[i] != null)
                    {
                        healthHearts[i].SetActive(i < health);
                    }
                }
            }
        }

        public void UpdateCountdown(int countdown)
        {
            if (countdownText != null)
            {
                countdownText.text = countdown.ToString();
            }
        }

        public void ShowCountdown(bool show)
        {
            if (countdownPanel != null)
            {
                countdownPanel.SetActive(show);
            }
        }

        public void ShowGameUI(bool show)
        {
            if (scoreText != null)
                scoreText.gameObject.SetActive(show);
            
            if (levelText != null)
                levelText.gameObject.SetActive(show);
        }

        public void ShowPauseMenu(bool show)
        {
            if (pausePanel != null)
            {
                pausePanel.SetActive(show);
            }
        }

        public void ShowGameOver(bool show, int finalScore = 0)
        {
            if (gameOverPanel != null)
            {
                gameOverPanel.SetActive(show);
            }

            if (show && gameOverScoreText != null)
            {
                gameOverScoreText.text = $"Final Score: {finalScore}";
            }
        }

        public void UpdateAbilityCooldown(float progress)
        {
            if (abilityCooldownOverlay != null)
            {
                abilityCooldownOverlay.fillAmount = 1f - progress;
            }
        }

        private void UpdateFPS()
        {
            if (fpsText != null)
            {
                int fps = Mathf.RoundToInt(1f / Time.deltaTime);
                fps = Mathf.Min(fps, 60);
                fpsText.text = $"FPS: {fps}";
            }
        }
        
        private void OnDestroy()
        {
            if (restartButton != null)
                restartButton.onClick.RemoveListener(OnRestartClicked);
            if (mainMenuButton != null)
                mainMenuButton.onClick.RemoveListener(OnMainMenuClicked);
        }
    }
}
