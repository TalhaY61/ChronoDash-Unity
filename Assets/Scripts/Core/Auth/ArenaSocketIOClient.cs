using UnityEngine;
using System;
using System.Collections.Generic;
using SocketIOClient;
using ChronoDash.Core;

namespace ChronoDash.Core.Auth
{
    /// <summary>
    /// Socket.IO client for Arena Arcade real-time communication.
    /// Handles authentication, room joining, and all game events.
    /// Based on Arena Arcade Socket.IO protocol.
    /// </summary>
    public class ArenaSocketIOClient
    {
        private SocketIOUnity socket;
        private VorldConfig config;
        private string gameId;
        private string jwtToken;
        private string vorldAppId;
        private string arenaGameId;
        
        // Connection state
        public bool IsConnected { get; private set; }
        
        // Events matching Arena Arcade protocol
        public event Action OnConnected;
        public event Action OnDisconnected;
        public event Action<string> OnError;
        
        // Arena-specific events (same as your original implementation)
        public event Action<string> OnImmediateItemDrop;
        public event Action<string> OnArenaBegins;
        public event Action<string> OnArenaCountdownStarted;
        public event Action<string> OnBoostReceived;
        public event Action<string> OnPackageDrop;
        public event Action<string> OnArenaComplete;
        public event Action<string> OnCycleStart;
        public event Action<string> OnCycleEnd;
        
        /// <summary>
        /// Constructor - matches organizer's Socket.IO client setup
        /// </summary>
        public ArenaSocketIOClient(
            string serverUrl,
            string token,
            string appId,
            string gameId,
            string arenaGameId,
            VorldConfig vorldConfig)
        {
            this.jwtToken = token;
            this.vorldAppId = appId;
            this.gameId = gameId;
            this.arenaGameId = arenaGameId;
            this.config = vorldConfig;
            
            // Convert WebSocket URL to HTTP/HTTPS for Socket.IO
            string socketIOUrl = ConvertToSocketIOUrl(serverUrl);
            
            config.Log($"Initializing Socket.IO client: {socketIOUrl}");
            
            // Create Socket.IO connection with authentication
            var uri = new Uri(socketIOUrl);
            socket = new SocketIOUnity(uri, new SocketIOOptions
            {
                Transport = SocketIOClient.Transport.TransportProtocol.WebSocket,
                Reconnection = true,
                ReconnectionAttempts = 10,
                ReconnectionDelay = 1000,
                ReconnectionDelayMax = 5000,
                ConnectionTimeout = TimeSpan.FromSeconds(30),
                
                // Authentication payload - CRITICAL for Arena Arcade
                Auth = new Dictionary<string, string>
                {
                    { "token", jwtToken },
                    { "gameId", gameId },
                    { "appId", vorldAppId },
                    { "arenaGameId", arenaGameId }
                }
            });
            
            SetupEventHandlers();
        }
        
        /// <summary>
        /// Convert WebSocket URL to Socket.IO compatible URL
        /// </summary>
        private string ConvertToSocketIOUrl(string wsUrl)
        {
            try
            {
                var uri = new Uri(wsUrl);
                
                // Convert ws:// or wss:// to http:// or https://
                string protocol = uri.Scheme == "wss" ? "https" : "http";
                
                // Socket.IO connects to base domain (no /ws/{gameId} path)
                return $"{protocol}://{uri.Host}:{uri.Port}";
            }
            catch
            {
                // Fallback to Arena Arcade production server
                config.LogWarning("Failed to parse WebSocket URL, using default Arena server");
                return "https://vorld-arena-server.onrender.com";
            }
        }
        
        /// <summary>
        /// Setup all Socket.IO event handlers
        /// </summary>
        private void SetupEventHandlers()
        {
            // Connection events
            socket.OnConnected += (sender, e) =>
            {
                IsConnected = true;
                config.Log($"✅ Socket.IO connected! Socket ID: {socket.Id}");
                
                // Join game room - REQUIRED by Arena Arcade protocol
                socket.Emit("join_game", gameId);
                config.Log($"🎮 Joined game room: {gameId}");
                
                OnConnected?.Invoke();
            };
            
            socket.OnDisconnected += (sender, e) =>
            {
                IsConnected = false;
                config.Log($"🔌 Socket.IO disconnected: {e}");
                OnDisconnected?.Invoke();
            };
            
            socket.OnError += (sender, e) =>
            {
                config.LogError($"Socket.IO error: {e}");
                OnError?.Invoke(e);
            };
            
            socket.OnReconnectAttempt += (sender, attempt) =>
            {
                config.Log($"🔄 Reconnection attempt {attempt}...");
            };
            
            socket.OnReconnected += (sender, e) =>
            {
                config.Log($"✅ Socket.IO reconnected! Socket ID: {socket.Id}");
                
                // Re-join game room after reconnection
                socket.Emit("join_game", gameId);
            };
            
            // Arena Arcade game events
            SetupGameEventHandlers();
        }
        
        /// <summary>
        /// Setup Arena Arcade specific event handlers
        /// </summary>
        private void SetupGameEventHandlers()
        {
            // Immediate item drop (viewer sends powerup/gemstone/effect)
            socket.On("immediate_item_drop", response =>
            {
                string jsonData = response.ToString();
                config.Log($"📦 Immediate item drop received: {jsonData}");
                OnImmediateItemDrop?.Invoke(jsonData);
            });
            
            // Arena begins (game starts)
            socket.On("arena_begins", response =>
            {
                string jsonData = response.ToString();
                config.Log($"🎮 Arena begins: {jsonData}");
                OnArenaBegins?.Invoke(jsonData);
            });
            
            // Arena countdown started
            socket.On("countdown", response =>
            {
                string jsonData = response.ToString();
                config.Log($"⏱️ Arena countdown: {jsonData}");
                OnArenaCountdownStarted?.Invoke(jsonData);
            });
            
            // Boost received (viewer boosts player)
            socket.On("boost", response =>
            {
                string jsonData = response.ToString();
                config.Log($"🚀 Boost received: {jsonData}");
                OnBoostReceived?.Invoke(jsonData);
            });
            
            // Package drop
            socket.On("package_drop", response =>
            {
                string jsonData = response.ToString();
                config.Log($"📦 Package drop: {jsonData}");
                OnPackageDrop?.Invoke(jsonData);
            });
            
            // Arena complete
            socket.On("arena_complete", response =>
            {
                string jsonData = response.ToString();
                config.Log($"🏁 Arena complete: {jsonData}");
                OnArenaComplete?.Invoke(jsonData);
            });
            
            // Cycle start
            socket.On("cycle_start", response =>
            {
                string jsonData = response.ToString();
                config.Log($"🔄 Cycle start: {jsonData}");
                OnCycleStart?.Invoke(jsonData);
            });
            
            // Cycle end
            socket.On("cycle_end", response =>
            {
                string jsonData = response.ToString();
                config.Log($"⏹️ Cycle end: {jsonData}");
                OnCycleEnd?.Invoke(jsonData);
            });
            
            // Generic event logger for debugging
            socket.OnAny((eventName, response) =>
            {
                config.Log($"📨 Socket.IO event received: {eventName}");
            });
        }
        
        /// <summary>
        /// Connect to Socket.IO server
        /// </summary>
        public async void ConnectAsync()
        {
            try
            {
                config.Log($"Connecting to Arena Arcade Socket.IO server...");
                await socket.ConnectAsync();
            }
            catch (Exception ex)
            {
                config.LogError($"Socket.IO connection failed: {ex.Message}");
                OnError?.Invoke(ex.Message);
            }
        }
        
        /// <summary>
        /// Disconnect from Socket.IO server
        /// </summary>
        public async void Disconnect()
        {
            if (socket != null && IsConnected)
            {
                config.Log("Disconnecting from Socket.IO...");
                await socket.DisconnectAsync();
                IsConnected = false;
            }
        }
        
        /// <summary>
        /// Update method - must be called every frame from MonoBehaviour
        /// (Socket.IO Unity requires this for event dispatching)
        /// </summary>
        public void Update()
        {
            // Socket.IO Unity handles updates internally
            // Keep this method for compatibility
        }
        
        /// <summary>
        /// Emit custom event to server (for testing or future features)
        /// </summary>
        public void EmitEvent(string eventName, object data = null)
        {
            if (socket != null && IsConnected)
            {
                socket.Emit(eventName, data);
                config.Log($"📤 Emitted event: {eventName}");
            }
            else
            {
                config.LogWarning($"Cannot emit event {eventName}: Socket.IO not connected");
            }
        }
    }
}
