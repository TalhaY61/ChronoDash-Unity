using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ChronoDash.Managers;

namespace ChronoDash.UI
{
    /// <summary>
    /// How To Play screen UI controller.
    /// Displays game controls, gemstones, and powerup information.
    /// </summary>
    public class HowToPlayUI : MonoBehaviour
    {
        [Header("Navigation")]
        [SerializeField] private Button backButton;
        
        [Header("Content Panels (Optional - Auto-Create if null)")]
        [SerializeField] private Transform controlsPanel;
        [SerializeField] private Transform gemstonesPanel;
        [SerializeField] private Transform powerupsPanel;
        
        [Header("Sprites for Display")]
        [SerializeField] private Sprite blueGemSprite;
        [SerializeField] private Sprite greenGemSprite;
        [SerializeField] private Sprite redGemSprite;
        [SerializeField] private Sprite invincibilitySprite;
        [SerializeField] private Sprite speedSprite;
        [SerializeField] private Sprite magnetSprite;
        [SerializeField] private Sprite shieldSprite;
        [SerializeField] private Sprite multiply2xSprite;
        [SerializeField] private Sprite healthSprite;
        
        private void Start()
        {
            SetupBackButton();
        }
        
        private void SetupBackButton()
        {
            if (backButton != null)
            {
                backButton.onClick.AddListener(OnBackClicked);
            }
        }
        
        private void OnBackClicked()
        {
            Debug.Log("⬅️ Back button clicked");
            SceneController.Instance.LoadScene(SceneController.MAIN_MENU);
        }
        
        private void OnDestroy()
        {
            if (backButton != null)
                backButton.onClick.RemoveListener(OnBackClicked);
        }
    }
}
