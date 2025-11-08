using UnityEngine;

namespace ChronoDash.Core
{
    /// <summary>
    /// Configuration for Vorld Auth and Arena Arcade integration.
    /// Create via: Assets > Create > ChronoDash > Vorld Config
    /// </summary>
    [CreateAssetMenu(fileName = "VorldConfig", menuName = "ChronoDash/Vorld Config", order = 1)]
    public class VorldConfig : ScriptableObject
    {
        [Header("Vorld Auth Configuration")]
        [Tooltip("Your Vorld App ID from Vorld Auth app creation")]
        public string vorldAppId = "your_app_id_here";
        
        [Tooltip("Auth server URL")]
        public string authServerUrl = "https://vorld-auth.onrender.com/api";
        
        [Header("Arena Arcade Configuration")]
        [Tooltip("Your Arena Game ID from Arena Arcade setup")]
        public string arenaGameId = "your_arena_game_id_here";
        
        [Tooltip("Arena WebSocket server URL")]
        public string arenaServerUrl = "wss://airdrop-arcade.onrender.com";
        
        [Tooltip("Game REST API URL")]
        public string gameApiUrl = "https://airdrop-arcade.onrender.com/api";
        
        [Header("Game Settings")]
        [Tooltip("Placeholder stream URL for testing")]
        public string defaultStreamUrl = "https://twitch.tv/placeholder";
        
        [Header("Development Settings")]
        [Tooltip("Enable to run game without authentication (for Itch.io builds)")]
        public bool offlineMode = false;
        
        [Tooltip("Enable debug logging")]
        public bool enableDebugLogs = true;
        
        public void Log(string message)
        {
            if (enableDebugLogs)
            {
                Debug.Log($"[Vorld] {message}");
            }
        }
        
        public void LogError(string message)
        {
            Debug.LogError($"[Vorld] {message}");
        }
        
        public void LogWarning(string message)
        {
            if (enableDebugLogs)
            {
                Debug.LogWarning($"[Vorld] {message}");
            }
        }
    }
}
