using System;
using UnityEngine;
using ChronoDash.Core.Auth;
using ChronoDash.Powerups;
using ChronoDash.Gemstones;

namespace ChronoDash.Managers
{
    /// <summary>
    /// Manages Arena Arcade integration during gameplay.
    /// Listens for WebSocket events and spawns items when viewers send them.
    /// </summary>
    public class ArenaManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PowerupManager powerupManager;
        [SerializeField] private GemstoneManager gemstoneManager;
        [SerializeField] private GameplayEffectsManager effectsManager;
        
        [Header("Arena Status")]
        [SerializeField] private bool arenaActive = false;
        
        private ArenaWebSocketClient websocketClient;
        
        public bool IsArenaActive => arenaActive;
        
        // Events for viewer notifications
        public System.Action<string, PowerupType> OnPowerupSpawned;
        public System.Action<string, GemstoneType, int> OnGemstonesSpawned;
        public System.Action<string, string> OnEffectTriggered;
        
        private void Start()
        {
            // Auto-find managers if not assigned
            if (powerupManager == null)
                powerupManager = FindFirstObjectByType<PowerupManager>();
            
            if (gemstoneManager == null)
                gemstoneManager = FindFirstObjectByType<GemstoneManager>();
            
            if (effectsManager == null)
                effectsManager = FindFirstObjectByType<GameplayEffectsManager>();
            
            // Check Arena status
            CheckArenaStatus();
        }
        
        /// <summary>
        /// Check and setup Arena if active
        /// </summary>
        private void CheckArenaStatus()
        {
            // Get WebSocket client from AuthManager
            if (AuthManager.Instance != null && AuthManager.Instance.IsArenaActive)
            {
                var arenaService = AuthManager.Instance.GetArenaGameService();
                if (arenaService != null)
                {
                    websocketClient = arenaService.GetWebSocketClient();
                    
                    if (websocketClient != null && !arenaActive)
                    {
                        SetupWebSocketListeners();
                        arenaActive = true;
                        Debug.Log("[ArenaManager] ✅ Arena is NOW ACTIVE - listening for viewer interactions!");
                    }
                }
            }
        }
        
        private void Update()
        {
            // Check if Arena became active (in case it was started after scene load)
            if (!arenaActive && AuthManager.Instance != null && AuthManager.Instance.IsArenaActive)
            {
                CheckArenaStatus();
            }
            
            // Process WebSocket messages (required for NativeWebSocket)
            if (websocketClient != null && arenaActive)
            {
                websocketClient.Update();
            }
        }
        
        /// <summary>
        /// Setup WebSocket event listeners.
        /// </summary>
        private void SetupWebSocketListeners()
        {
            websocketClient.OnArenaCountdownStarted += HandleArenaCountdown;
            websocketClient.OnArenaBegins += HandleArenaBegins;
            websocketClient.OnImmediateItemDrop += HandleImmediateItemDrop;
            websocketClient.OnPlayerBoostActivated += HandlePlayerBoost;
            websocketClient.OnPackageDrop += HandlePackageDrop;
            
            Debug.Log("[ArenaManager] WebSocket listeners registered");
        }
        
        /// <summary>
        /// Handle arena countdown event.
        /// </summary>
        private void HandleArenaCountdown(string jsonData)
        {
            Debug.Log($"[ArenaManager] Arena countdown started!");
            // TODO: Show countdown UI to streamer
        }
        
        /// <summary>
        /// Handle arena begins event.
        /// </summary>
        private void HandleArenaBegins(string jsonData)
        {
            Debug.Log($"[ArenaManager] Arena is NOW LIVE! Viewers can interact!");
            arenaActive = true;
            // TODO: Show "Arena is LIVE" notification
        }
        
        /// <summary>
        /// Handle immediate item drop from viewer.
        /// This is the MAIN method that spawns items when viewers buy them!
        /// </summary>
        private void HandleImmediateItemDrop(string jsonData)
        {
            try
            {
                Debug.Log($"[ArenaManager] Received item drop: {jsonData}");
                
                // Parse the item drop data
                var itemDrop = JsonUtility.FromJson<ImmediateItemDropData>(jsonData);
                
                if (itemDrop == null || itemDrop.metadata == null)
                {
                    Debug.LogError("[ArenaManager] Invalid item drop data");
                    return;
                }
                
                // Show notification
                Debug.Log($"[ArenaManager] Viewer sent: {itemDrop.packageName} (Type: {itemDrop.metadata.itemType})");
                
                // Spawn item based on type
                switch (itemDrop.metadata.itemType)
                {
                    case "powerup":
                        SpawnPowerup(itemDrop);
                        break;
                    
                    case "gemstone":
                        SpawnGemstones(itemDrop);
                        break;
                    
                    case "effect":
                        TriggerEffect(itemDrop);
                        break;
                    
                    default:
                        Debug.LogWarning($"[ArenaManager] Unknown item type: {itemDrop.metadata.itemType}");
                        break;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ArenaManager] Failed to handle item drop: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Spawn a powerup from viewer interaction.
        /// </summary>
        private void SpawnPowerup(ImmediateItemDropData itemDrop)
        {
            if (powerupManager == null)
            {
                Debug.LogError("[ArenaManager] PowerupManager not found!");
                return;
            }
            
            // Map itemId to PowerupType enum
            PowerupType powerupType = itemDrop.metadata.itemId switch
            {
                "invincibility" => PowerupType.Invincibility,
                "speed" => PowerupType.Speed,
                "magnet" => PowerupType.Magnet,
                "shield" => PowerupType.Shield,
                "multiply2x" => PowerupType.Multiply2x,
                "heart" => PowerupType.Heart,
                _ => PowerupType.Invincibility // Default fallback
            };
            
            Debug.Log($"[ArenaManager] Spawning powerup: {powerupType}");
            
            // Spawn the powerup
            powerupManager.SpawnSpecificPowerup(powerupType);
            
            // Notify listeners (for viewer notification)
            OnPowerupSpawned?.Invoke(itemDrop.viewerUsername, powerupType);
        }
        
        /// <summary>
        /// Spawn gemstones from viewer interaction.
        /// </summary>
        private void SpawnGemstones(ImmediateItemDropData itemDrop)
        {
            if (gemstoneManager == null)
            {
                Debug.LogError("[ArenaManager] GemstoneManager not found!");
                return;
            }
            
            // Map gemType to GemstoneType enum
            GemstoneType gemType = itemDrop.metadata.gemType switch
            {
                "blue" => GemstoneType.Blue,
                "green" => GemstoneType.Green,
                "red" => GemstoneType.Red,
                _ => GemstoneType.Blue // Default fallback
            };
            
            int quantity = itemDrop.metadata.quantity;
            
            Debug.Log($"[ArenaManager] Spawning {quantity}x {gemType} gemstones");
            
            // Spawn multiple gemstones
            for (int i = 0; i < quantity; i++)
            {
                gemstoneManager.SpawnSpecificGemstone(gemType);
            }
            
            // Notify listeners (for viewer notification)
            OnGemstonesSpawned?.Invoke(itemDrop.viewerUsername, gemType, quantity);
        }
        
        /// <summary>
        /// Trigger a world effect from viewer interaction.
        /// </summary>
        private void TriggerEffect(ImmediateItemDropData itemDrop)
        {
            if (effectsManager == null)
            {
                Debug.LogError("[ArenaManager] GameplayEffectsManager not found!");
                return;
            }
            
            string effectId = itemDrop.metadata.effectId;
            
            Debug.Log($"[ArenaManager] Triggering effect: {effectId}");
            
            // Trigger the effect based on effectId
            switch (effectId)
            {
                case "gravity_flip":
                    effectsManager.TriggerGravityFlip();
                    break;
                
                case "screen_shake":
                    effectsManager.TriggerScreenShake();
                    break;
                
                case "darkness":
                    effectsManager.TriggerDarkness();
                    break;
                
                default:
                    Debug.LogWarning($"[ArenaManager] Unknown effect: {effectId}");
                    break;
            }
            
            // Notify listeners (for viewer notification)
            OnEffectTriggered?.Invoke(itemDrop.viewerUsername, effectId);
        }
        
        /// <summary>
        /// Handle player boost event.
        /// </summary>
        private void HandlePlayerBoost(string jsonData)
        {
            Debug.Log($"[ArenaManager] Player boosted: {jsonData}");
            // TODO: Show boost notification
        }
        
        /// <summary>
        /// Handle cyclic package drop event.
        /// </summary>
        private void HandlePackageDrop(string jsonData)
        {
            Debug.Log($"[ArenaManager] Cyclic package dropped: {jsonData}");
            // TODO: Handle cyclic rewards
        }
        
        private void OnDestroy()
        {
            // Cleanup event listeners
            if (websocketClient != null)
            {
                websocketClient.OnArenaCountdownStarted -= HandleArenaCountdown;
                websocketClient.OnArenaBegins -= HandleArenaBegins;
                websocketClient.OnImmediateItemDrop -= HandleImmediateItemDrop;
                websocketClient.OnPlayerBoostActivated -= HandlePlayerBoost;
                websocketClient.OnPackageDrop -= HandlePackageDrop;
            }
        }
    }
    
    // ============================================================================
    // DATA STRUCTURES for parsing WebSocket JSON messages
    // ============================================================================
    
    /// <summary>
    /// Immediate item drop event data from WebSocket.
    /// </summary>
    [Serializable]
    public class ImmediateItemDropData
    {
        public string type;
        public string gameId;
        public string packageId;
        public string packageName;
        public string targetPlayerId;
        public string viewerUsername;
        public int cost;
        public PackageMetadata metadata;
    }
    
    /// <summary>
    /// Package metadata containing item information.
    /// This must match the metadata you configured on Vorld dashboard!
    /// </summary>
    [Serializable]
    public class PackageMetadata
    {
        // Common fields
        public string itemType;      // "powerup", "gemstone", "effect"
        
        // Powerup fields
        public string itemId;        // "invincibility", "speed", "magnet", "shield", "multiply2x", "heart"
        public int duration;         // Duration in seconds
        
        // Gemstone fields
        public string gemType;       // "blue", "green", "red"
        public int quantity;         // Number of gems to spawn
        public int pointValue;       // Points per gem
        public bool healsPlayer;     // Red gems heal
        
        // Effect fields
        public string effectId;      // "gravity_flip", "screen_shake", "darkness"
    }
}
