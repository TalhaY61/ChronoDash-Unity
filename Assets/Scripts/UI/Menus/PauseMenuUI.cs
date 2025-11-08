using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ChronoDash.Managers
{
    public class PauseMenuUI : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private TextMeshProUGUI pauseText;
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button mainMenuButton;

        private GameManager gameManager;

        private void Start()
        {
            gameManager = FindFirstObjectByType<GameManager>();
            SetupButtons();
        }

        private void SetupButtons()
        {
            if (resumeButton != null)
            {
                resumeButton.onClick.AddListener(OnResumeClicked);
            }

            if (mainMenuButton != null)
            {
                mainMenuButton.onClick.AddListener(OnMainMenuClicked);
            }
        }

        private void OnResumeClicked()
        {
            if (gameManager != null)
            {
                gameManager.ResumeGame();
            }
        }

        private void OnMainMenuClicked()
        {
            Time.timeScale = 1f;
            SceneController.Instance.LoadScene(SceneController.MAIN_MENU);
        }

        private void OnDestroy()
        {
            if (resumeButton != null)
                resumeButton.onClick.RemoveListener(OnResumeClicked);
            if (mainMenuButton != null)
                mainMenuButton.onClick.RemoveListener(OnMainMenuClicked);
        }
    }
}
