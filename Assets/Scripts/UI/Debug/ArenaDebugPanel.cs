using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ChronoDash.Managers;
using System.Reflection;

namespace ChronoDash.UI.Debug
{
    /// <summary>
    /// Debug panel to simulate viewer interactions for testing and demo videos.
    /// This lets you fake viewer purchases without needing real viewers!
    /// </summary>
    public class ArenaDebugPanel : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private GameObject debugPanel;
        [SerializeField] private TextMeshProUGUI statusText;
        
        [Header("Powerup Buttons")]
        [SerializeField] private Button speedButton;
        [SerializeField] private Button invincibilityButton;
        [SerializeField] private Button magnetButton;
        [SerializeField] private Button shieldButton;
        [SerializeField] private Button multiply2xButton;
        [SerializeField] private Button heartButton;
        
        [Header("Gemstone Buttons")]
        [SerializeField] private Button blueGemButton;
        [SerializeField] private Button greenGemButton;
        [SerializeField] private Button redGemButton;
        
        [Header("Effect Buttons")]
        [SerializeField] private Button gravityFlipButton;
        [SerializeField] private Button screenShakeButton;
        [SerializeField] private Button darknessButton;
        
        private ArenaManager arenaManager;
        
        private void Start()
        {
            // Find ArenaManager
            arenaManager = FindFirstObjectByType<ArenaManager>();
            
            if (arenaManager == null)
            {
                UnityEngine.Debug.LogWarning("[ArenaDebug] ArenaManager not found! Debug panel disabled.");
                if (debugPanel != null)
                    debugPanel.SetActive(false);
                return;
            }
            
            // Setup all buttons
            SetupButtons();
            
            // Hide panel by default (toggle with F12)
            if (debugPanel != null)
                debugPanel.SetActive(false);
                
            UpdateStatus("Arena Debug Panel Ready (Press F12 to toggle)");
        }
        
        private void Update()
        {
            // Toggle panel with F12
            if (Input.GetKeyDown(KeyCode.F12))
            {
                if (debugPanel != null)
                {
                    debugPanel.SetActive(!debugPanel.activeSelf);
                }
            }
        }
        
        private void SetupButtons()
        {
            // Powerup buttons
            if (speedButton != null)
                speedButton.onClick.AddListener(() => SimulatePowerup("speed", "Speed Boost"));
            
            if (invincibilityButton != null)
                invincibilityButton.onClick.AddListener(() => SimulatePowerup("invincibility", "Invincibility"));
            
            if (magnetButton != null)
                magnetButton.onClick.AddListener(() => SimulatePowerup("magnet", "Magnet"));
            
            if (shieldButton != null)
                shieldButton.onClick.AddListener(() => SimulatePowerup("shield", "Shield"));
            
            if (multiply2xButton != null)
                multiply2xButton.onClick.AddListener(() => SimulatePowerup("multiply2x", "Score 2x Multiplier"));
            
            if (heartButton != null)
                heartButton.onClick.AddListener(() => SimulatePowerup("heart", "Heart"));
            
            // Gemstone buttons
            if (blueGemButton != null)
                blueGemButton.onClick.AddListener(() => SimulateGemstone("blue", 5, 10, false));
            
            if (greenGemButton != null)
                greenGemButton.onClick.AddListener(() => SimulateGemstone("green", 5, 20, false));
            
            if (redGemButton != null)
                redGemButton.onClick.AddListener(() => SimulateGemstone("red", 3, 50, true));
            
            // Effect buttons
            if (gravityFlipButton != null)
                gravityFlipButton.onClick.AddListener(() => SimulateEffect("gravity_flip", "Gravity Flip"));
            
            if (screenShakeButton != null)
                screenShakeButton.onClick.AddListener(() => SimulateEffect("screen_shake", "Screen Shake"));
            
            if (darknessButton != null)
                darknessButton.onClick.AddListener(() => SimulateEffect("darkness", "Darkness"));
        }
        
        /// <summary>
        /// Simulate a viewer sending a powerup
        /// </summary>
        private void SimulatePowerup(string itemId, string displayName)
        {
            string json = $@"{{
                ""type"": ""immediate_item_drop"",
                ""packageName"": ""{displayName}"",
                ""viewerUsername"": ""TestViewer{Random.Range(1, 999)}"",
                ""metadata"": {{
                    ""itemType"": ""powerup"",
                    ""itemId"": ""{itemId}"",
                    ""duration"": 10
                }}
            }}";
            
            CallArenaHandler(json);
            UpdateStatus($"🎮 Viewer sent: {displayName}!");
            UnityEngine.Debug.Log($"[ArenaDebug] Simulated powerup: {itemId}");
        }
        
        /// <summary>
        /// Simulate a viewer sending gemstones
        /// </summary>
        private void SimulateGemstone(string gemType, int quantity, int pointValue, bool healsPlayer)
        {
            string json = $@"{{
                ""type"": ""immediate_item_drop"",
                ""packageName"": ""{gemType} Gemstone Pack"",
                ""viewerUsername"": ""TestViewer{Random.Range(1, 999)}"",
                ""metadata"": {{
                    ""itemType"": ""gemstone"",
                    ""gemType"": ""{gemType}"",
                    ""quantity"": {quantity},
                    ""pointValue"": {pointValue},
                    ""healsPlayer"": {healsPlayer.ToString().ToLower()}
                }}
            }}";
            
            CallArenaHandler(json);
            UpdateStatus($"💎 Viewer sent: {quantity}x {gemType} gems!");
            UnityEngine.Debug.Log($"[ArenaDebug] Simulated gemstone: {gemType} x{quantity}");
        }
        
        /// <summary>
        /// Simulate a viewer sending a world effect
        /// </summary>
        private void SimulateEffect(string effectId, string displayName)
        {
            string json = $@"{{
                ""type"": ""immediate_item_drop"",
                ""packageName"": ""{displayName}"",
                ""viewerUsername"": ""TestViewer{Random.Range(1, 999)}"",
                ""metadata"": {{
                    ""itemType"": ""effect"",
                    ""effectId"": ""{effectId}"",
                    ""duration"": 10
                }}
            }}";
            
            CallArenaHandler(json);
            UpdateStatus($"⚡ Viewer sent: {displayName}!");
            UnityEngine.Debug.Log($"[ArenaDebug] Simulated effect: {effectId}");
        }
        
        /// <summary>
        /// Call the private HandleImmediateItemDrop method on ArenaManager using reflection
        /// </summary>
        private void CallArenaHandler(string json)
        {
            if (arenaManager == null)
            {
                UnityEngine.Debug.LogError("[ArenaDebug] ArenaManager is null!");
                UpdateStatus("❌ Error: ArenaManager not found!");
                return;
            }
            
            // Use reflection to call the private method
            MethodInfo method = arenaManager.GetType().GetMethod(
                "HandleImmediateItemDrop", 
                BindingFlags.NonPublic | BindingFlags.Instance
            );
            
            if (method != null)
            {
                method.Invoke(arenaManager, new object[] { json });
            }
            else
            {
                UnityEngine.Debug.LogError("[ArenaDebug] HandleImmediateItemDrop method not found!");
                UpdateStatus("❌ Error: Handler method not found!");
            }
        }
        
        private void UpdateStatus(string message)
        {
            if (statusText != null)
            {
                statusText.text = message;
            }
            UnityEngine.Debug.Log($"[ArenaDebug] {message}");
        }
    }
}
