using System;
using System.Threading.Tasks;
using UnityEngine;
using NativeWebSocket;

namespace ChronoDash.Core.Auth
{
    /// <summary>
    /// WebSocket client for Arena Arcade real-time events.
    /// Handles real-time communication for viewer interactions, boosts, and item drops.
    /// </summary>
    public class ArenaWebSocketClient
    {
        private readonly string websocketUrl;
        private readonly string authToken;
        private readonly string appId;
        private readonly VorldConfig config;
        
        // WebSocket instance
        private WebSocket webSocket;
        private bool isConnecting;
        
        // Reconnection logic
        private int reconnectAttempts = 0;
        private const int maxReconnectAttempts = 5;
        private bool shouldReconnect = true;
        
        public bool IsConnected { get; private set; }
        
        // Connection events
        public event Action OnConnected;
        public event Action OnDisconnected;
        public event Action<string> OnError;
        
        // Arena events - passes raw JSON string for now
        public event Action<string> OnArenaCountdownStarted;
        public event Action<string> OnCountdownUpdate;
        public event Action<string> OnArenaBegins;
        
        // Boost events - passes raw JSON string for now
        public event Action<string> OnPlayerBoostActivated;
        public event Action<string> OnBoostCycleUpdate;
        public event Action<string> OnBoostCycleComplete;
        
        // Package events - passes raw JSON string for now
        public event Action<string> OnPackageDrop;
        public event Action<string> OnImmediateItemDrop;
        
        public ArenaWebSocketClient(string url, string token, string vorldAppId, VorldConfig vorldConfig)
        {
            websocketUrl = url;
            authToken = token;
            appId = vorldAppId;
            config = vorldConfig;
        }
        
        /// <summary>
        /// Connect to WebSocket server.
        /// </summary>
        public async Task ConnectAsync()
        {
            if (isConnecting || IsConnected)
            {
                config.LogWarning("WebSocket already connected or connecting");
                return;
            }
            
            isConnecting = true;
            shouldReconnect = true; // Enable auto-reconnection
            
            try
            {
                config.Log($"Connecting to WebSocket: {websocketUrl}");
                
                webSocket = new WebSocket(websocketUrl);
                
                webSocket.OnOpen += () =>
                {
                    config.Log("✅ WebSocket connected successfully");
                    IsConnected = true;
                    isConnecting = false;
                    reconnectAttempts = 0; // Reset reconnection counter on successful connection
                    OnConnected?.Invoke();
                };
                
                webSocket.OnMessage += (bytes) =>
                {
                    var message = System.Text.Encoding.UTF8.GetString(bytes);
                    config.Log($"WebSocket message received: {message}");
                    HandleMessage(message);
                };
                
                webSocket.OnError += (errorMsg) =>
                {
                    config.LogError($"WebSocket error: {errorMsg}");
                    OnError?.Invoke(errorMsg);
                };
                
                webSocket.OnClose += async (closeCode) =>
                {
                    config.Log($"⚠️ WebSocket closed: {closeCode}");
                    IsConnected = false;
                    isConnecting = false;
                    OnDisconnected?.Invoke();
                    
                    // Auto-reconnect with exponential backoff
                    if (shouldReconnect && reconnectAttempts < maxReconnectAttempts)
                    {
                        reconnectAttempts++;
                        int delayMs = (int)Math.Pow(2, reconnectAttempts) * 1000; // 2s, 4s, 8s, 16s, 32s
                        config.Log($"🔄 Reconnecting in {delayMs / 1000}s (attempt {reconnectAttempts}/{maxReconnectAttempts})");
                        
                        await Task.Delay(delayMs);
                        
                        if (shouldReconnect) // Check again in case Disconnect() was called during delay
                        {
                            await ConnectAsync();
                        }
                    }
                    else if (reconnectAttempts >= maxReconnectAttempts)
                    {
                        config.LogError("❌ Max reconnection attempts reached. WebSocket disconnected permanently.");
                        OnError?.Invoke("Max reconnection attempts reached");
                    }
                };
                
                await webSocket.Connect();
            }
            catch (Exception ex)
            {
                config.LogError($"Failed to connect WebSocket: {ex.Message}");
                isConnecting = false;
                OnError?.Invoke(ex.Message);
            }
        }
        
        /// <summary>
        /// Handle incoming WebSocket messages.
        /// </summary>
        private void HandleMessage(string message)
        {
            try
            {
                // Parse the message type
                var msgData = JsonUtility.FromJson<WebSocketMessage>(message);
                
                switch (msgData.type)
                {
                    case "arena_countdown_started":
                        OnArenaCountdownStarted?.Invoke(message);
                        break;
                    case "countdown_update":
                        OnCountdownUpdate?.Invoke(message);
                        break;
                    case "arena_begins":
                        OnArenaBegins?.Invoke(message);
                        break;
                    case "player_boost_activated":
                        OnPlayerBoostActivated?.Invoke(message);
                        break;
                    case "boost_cycle_update":
                        OnBoostCycleUpdate?.Invoke(message);
                        break;
                    case "boost_cycle_complete":
                        OnBoostCycleComplete?.Invoke(message);
                        break;
                    case "package_drop":
                        OnPackageDrop?.Invoke(message);
                        break;
                    case "immediate_item_drop":
                        OnImmediateItemDrop?.Invoke(message);
                        break;
                    default:
                        config.LogWarning($"Unknown WebSocket message type: {msgData.type}");
                        break;
                }
            }
            catch (Exception ex)
            {
                config.LogError($"Failed to handle WebSocket message: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Update method - must be called from MonoBehaviour Update().
        /// Required for NativeWebSocket to process messages.
        /// </summary>
        public void Update()
        {
            #if !UNITY_WEBGL || UNITY_EDITOR
            webSocket?.DispatchMessageQueue();
            #endif
        }
        
        /// <summary>
        /// Disconnect from WebSocket server (synchronous version).
        /// </summary>
        public void Disconnect()
        {
            shouldReconnect = false; // Stop auto-reconnection
            reconnectAttempts = 0;
            
            if (!IsConnected)
            {
                return;
            }
            
            try
            {
                webSocket?.Close();
                
                IsConnected = false;
                config.Log("🔌 WebSocket disconnected (manual)");
                OnDisconnected?.Invoke();
            }
            catch (Exception ex)
            {
                config.LogError($"Error disconnecting WebSocket: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Disconnect from WebSocket server (async version).
        /// </summary>
        public async Task DisconnectAsync()
        {
            shouldReconnect = false; // Stop auto-reconnection
            reconnectAttempts = 0;
            
            if (!IsConnected)
            {
                return;
            }
            
            try
            {
                if (webSocket != null)
                {
                    await webSocket.Close();
                }
                
                IsConnected = false;
                config.Log("🔌 WebSocket disconnected (manual)");
                OnDisconnected?.Invoke();
            }
            catch (Exception ex)
            {
                config.LogError($"Error disconnecting WebSocket: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Send message to WebSocket server.
        /// </summary>
        public async Task SendAsync(string message)
        {
            if (!IsConnected)
            {
                config.LogWarning("Cannot send message: WebSocket not connected");
                return;
            }
            
            try
            {
                await webSocket.SendText(message);
                config.Log($"Sent WebSocket message: {message}");
            }
            catch (Exception ex)
            {
                config.LogError($"Failed to send WebSocket message: {ex.Message}");
            }
        }
    }
    
    /// <summary>
    /// Base WebSocket message structure for parsing message types.
    /// </summary>
    [Serializable]
    public class WebSocketMessage
    {
        public string type;
        public string gameId;
    }
}
