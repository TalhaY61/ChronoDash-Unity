using UnityEngine;
using ChronoDash.Managers;
using System.Reflection;
using UnityEngine.InputSystem;

namespace ChronoDash.UI.Debug
{
    /// <summary>
    /// Super simple keyboard shortcuts to test Arena without UI!
    /// Just press keys during gameplay to spawn items.
    /// </summary>
    public class ArenaKeyboardDebug : MonoBehaviour
    {
        private ArenaManager arenaManager;
        private bool shortcutsLogged = false;
        
        private void Start()
        {
            UnityEngine.Debug.Log("[ArenaKeyboardDebug] ===== KEYBOARD DEBUG ACTIVE =====");
            UnityEngine.Debug.Log("[ArenaKeyboardDebug] Looking for ArenaManager...");
            FindArenaManager();
        }
        
        private void FindArenaManager()
        {
            arenaManager = FindFirstObjectByType<ArenaManager>();
            
            if (arenaManager == null)
            {
                UnityEngine.Debug.LogWarning("[ArenaKeyboardDebug] ⚠️ ArenaManager not found in scene!");
                UnityEngine.Debug.LogWarning("[ArenaKeyboardDebug] Make sure this is in the GAMEPLAY scene, not MainMenu!");
            }
            else if (!shortcutsLogged)
            {
                UnityEngine.Debug.Log("[ArenaKeyboardDebug] ✅ ArenaManager found!");
                UnityEngine.Debug.Log("[ArenaKeyboardDebug] 🎮 Arena keyboard shortcuts enabled!");
                UnityEngine.Debug.Log("  [1] = Speed Boost");
                UnityEngine.Debug.Log("  [2] = Invincibility");
                UnityEngine.Debug.Log("  [3] = Magnet");
                UnityEngine.Debug.Log("  [4] = Shield");
                UnityEngine.Debug.Log("  [5] = Multiply 2x");
                UnityEngine.Debug.Log("  [6] = Heart");
                UnityEngine.Debug.Log("  [7] = Blue Gems");
                UnityEngine.Debug.Log("  [8] = Green Gems");
                UnityEngine.Debug.Log("  [9] = Red Gems");
                UnityEngine.Debug.Log("  [G] = Gravity Flip");
                UnityEngine.Debug.Log("  [S] = Screen Shake");
                UnityEngine.Debug.Log("  [D] = Darkness");
                shortcutsLogged = true;
            }
        }
        
        private void Update()
        {
#if UNITY_EDITOR
            // Try to find ArenaManager if we don't have it yet
            if (arenaManager == null)
            {
                FindArenaManager();
                return;
            }
            
            var keyboard = Keyboard.current;
            if (keyboard == null) return;
            
            // Powerups
            if (keyboard.digit1Key.wasPressedThisFrame)
                SimulatePowerup("speed", "Speed Boost");
            
            if (keyboard.digit2Key.wasPressedThisFrame)
                SimulatePowerup("invincibility", "Invincibility");
            
            if (keyboard.digit3Key.wasPressedThisFrame)
                SimulatePowerup("magnet", "Magnet");
            
            if (keyboard.digit4Key.wasPressedThisFrame)
                SimulatePowerup("shield", "Shield");
            
            if (keyboard.digit5Key.wasPressedThisFrame)
                SimulatePowerup("multiply2x", "Multiply 2x");
            
            if (keyboard.digit6Key.wasPressedThisFrame)
                SimulatePowerup("heart", "Heart");
            
            // Gemstones
            if (keyboard.digit7Key.wasPressedThisFrame)
                SimulateGemstone("blue", 5, 10, false);
            
            if (keyboard.digit8Key.wasPressedThisFrame)
                SimulateGemstone("green", 5, 20, false);
            
            if (keyboard.digit9Key.wasPressedThisFrame)
                SimulateGemstone("red", 3, 50, true);
            
            // Effects
            if (keyboard.gKey.wasPressedThisFrame)
                SimulateEffect("gravity_flip", "Gravity Flip");
            
            if (keyboard.sKey.wasPressedThisFrame)
                SimulateEffect("screen_shake", "Screen Shake");
            
            if (keyboard.dKey.wasPressedThisFrame)
                SimulateEffect("darkness", "Darkness");
#endif
        }
        
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
            UnityEngine.Debug.Log($"[ArenaKeyboardDebug] 🎮 Viewer sent: {displayName}!");
        }
        
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
            UnityEngine.Debug.Log($"[ArenaKeyboardDebug] 💎 Viewer sent: {quantity}x {gemType} gems!");
        }
        
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
            UnityEngine.Debug.Log($"[ArenaKeyboardDebug] ⚡ Viewer sent: {displayName}!");
        }
        
        private void CallArenaHandler(string json)
        {
            if (arenaManager == null) return;
            
            MethodInfo method = arenaManager.GetType().GetMethod(
                "HandleImmediateItemDrop", 
                BindingFlags.NonPublic | BindingFlags.Instance
            );
            
            method?.Invoke(arenaManager, new object[] { json });
        }
    }
}
