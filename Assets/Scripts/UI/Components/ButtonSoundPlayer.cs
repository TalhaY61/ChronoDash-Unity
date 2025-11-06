using UnityEngine;
using UnityEngine.UI;
using ChronoDash.Managers;

namespace ChronoDash.UI
{
    /// <summary>
    /// Plays a sound effect when a button is clicked.
    /// Attach this component to any button to automatically play the select sound.
    /// Follows Single Responsibility Principle - only handles button click sounds.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class ButtonSoundPlayer : MonoBehaviour
    {
        private Button button;
        
        private void Awake()
        {
            button = GetComponent<Button>();
        }
        
        private void OnEnable()
        {
            if (button != null)
            {
                button.onClick.AddListener(PlaySelectSound);
            }
        }
        
        private void OnDisable()
        {
            if (button != null)
            {
                button.onClick.RemoveListener(PlaySelectSound);
            }
        }
        
        private void PlaySelectSound()
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySelectSound();
            }
        }
    }
}
