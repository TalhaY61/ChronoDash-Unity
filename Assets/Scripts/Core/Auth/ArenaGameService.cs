using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace ChronoDash.Core.Auth
{
    /// <summary>
    /// Arena Arcade game service for multiplayer streaming integration.
    /// Handles game initialization, player boosts, item drops, and WebSocket events.
    /// This enables viewers to interact with the game in real-time.
    /// </summary>
    public class ArenaGameService
    {
        private readonly VorldConfig config;
        private readonly VorldAuthService authService;
        
        private GameState currentGameState;
        private ArenaWebSocketClient websocketClient;
        
        // Events for WebSocket - Will be used when WebSocket is implemented
        #pragma warning disable CS0067 // Event is never used (will be invoked by WebSocket client)
        public event Action<ArenaCountdownEvent> OnArenaCountdownStarted;
        public event Action<CountdownUpdateEvent> OnCountdownUpdate;
        public event Action<ArenaBeginsEvent> OnArenaBegins;
        public event Action<PlayerBoostEvent> OnPlayerBoostActivated;
        public event Action<PackageDropEvent> OnPackageDrop;
        public event Action<ImmediateItemDropEvent> OnImmediateItemDrop;
        public event Action<string> OnGameCompleted;
        public event Action<string> OnGameStopped;
        #pragma warning restore CS0067
        
        public GameState CurrentGameState => currentGameState;
        public bool IsGameActive => currentGameState != null && currentGameState.arenaActive;
        
        public ArenaGameService(VorldConfig vorldConfig, VorldAuthService vorldAuthService)
        {
            config = vorldConfig;
            authService = vorldAuthService;
        }
        
        /// <summary>
        /// Initialize game with stream URL.
        /// Creates a new Arena Arcade game session.
        /// </summary>
        public IEnumerator InitializeGame(string streamUrl, Action<GameInitResponse> callback)
        {
            if (!authService.IsAuthenticated)
            {
                config.LogError("Cannot initialize game: Not authenticated");
                callback?.Invoke(new GameInitResponse { success = false, error = "Not authenticated" });
                yield break;
            }
            
            config.Log($"Initializing Arena game with stream: {streamUrl}");
            
            GameInitRequest initRequest = new GameInitRequest
            {
                streamUrl = streamUrl
            };
            
            string jsonData = JsonUtility.ToJson(initRequest);
            string url = $"{config.gameApiUrl}/games/init";
            
            using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
            {
                byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();
                
                request.SetRequestHeader("Content-Type", "application/json");
                request.SetRequestHeader("Authorization", $"Bearer {authService.AccessToken}");
                request.SetRequestHeader("X-Arena-Arcade-Game-ID", config.arenaGameId);
                request.SetRequestHeader("X-Vorld-App-ID", config.vorldAppId);
                
                yield return request.SendWebRequest();
                
                GameInitResponse response = new GameInitResponse();
                
                if (request.result == UnityWebRequest.Result.Success)
                {
                    try
                    {
                        response = JsonUtility.FromJson<GameInitResponse>(request.downloadHandler.text);
                        
                        if (response.success && response.data != null)
                        {
                            currentGameState = response.data;
                            config.Log($"Game initialized: {currentGameState.gameId}");
                            config.Log($"WebSocket URL: {currentGameState.websocketUrl}");
                            config.Log($"Arena Active: {currentGameState.arenaActive}");
                            config.Log($"Countdown Started: {currentGameState.countdownStarted}");
                            
                            // Create and connect WebSocket
                            websocketClient = new ArenaWebSocketClient(
                                currentGameState.websocketUrl,
                                authService.AccessToken,
                                config.vorldAppId,
                                config
                            );
                            
                            // Connect asynchronously
                            _ = websocketClient.ConnectAsync();
                            
                            config.Log("WebSocket connection initiated");
                        }
                        else
                        {
                            config.LogError($"Failed to initialize game: {response.error}");
                        }
                    }
                    catch (Exception ex)
                    {
                        config.LogError($"Failed to parse game init response: {ex.Message}");
                        response.success = false;
                        response.error = "Failed to parse server response";
                    }
                }
                else
                {
                    string errorMsg = $"Network error: {request.error}";
                    config.LogError(errorMsg);
                    response.success = false;
                    response.error = errorMsg;
                }
                
                callback?.Invoke(response);
            }
        }
        
        /// <summary>
        /// Boost a player with Arena coins.
        /// Viewers use this to support their favorite player.
        /// </summary>
        public IEnumerator BoostPlayer(string gameId, string playerId, int amount, string username, Action<BoostPlayerResponse> callback)
        {
            if (!authService.IsAuthenticated)
            {
                config.LogError("Cannot boost player: Not authenticated");
                callback?.Invoke(new BoostPlayerResponse { success = false, error = "Not authenticated" });
                yield break;
            }
            
            config.Log($"Boosting player {playerId} with {amount} coins");
            
            BoostPlayerRequest boostRequest = new BoostPlayerRequest
            {
                amount = amount,
                username = username
            };
            
            string jsonData = JsonUtility.ToJson(boostRequest);
            string url = $"{config.gameApiUrl}/games/boost/player/{gameId}/{playerId}";
            
            using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
            {
                byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();
                
                request.SetRequestHeader("Content-Type", "application/json");
                request.SetRequestHeader("Authorization", $"Bearer {authService.AccessToken}");
                request.SetRequestHeader("X-Arena-Arcade-Game-ID", config.arenaGameId);
                request.SetRequestHeader("X-Vorld-App-ID", config.vorldAppId);
                
                yield return request.SendWebRequest();
                
                BoostPlayerResponse response = new BoostPlayerResponse();
                
                if (request.result == UnityWebRequest.Result.Success)
                {
                    try
                    {
                        response = JsonUtility.FromJson<BoostPlayerResponse>(request.downloadHandler.text);
                        
                        if (response.success && response.data != null)
                        {
                            config.Log($"Boost successful: {response.data.playerName} +{response.data.currentCyclePoints}");
                        }
                        else
                        {
                            config.LogError($"Failed to boost player: {response.error}");
                        }
                    }
                    catch (Exception ex)
                    {
                        config.LogError($"Failed to parse boost response: {ex.Message}");
                        response.success = false;
                        response.error = "Failed to parse server response";
                    }
                }
                else
                {
                    string errorMsg = $"Network error: {request.error}";
                    config.LogError(errorMsg);
                    response.success = false;
                    response.error = errorMsg;
                }
                
                callback?.Invoke(response);
            }
        }
        
        /// <summary>
        /// Drop immediate item to target player.
        /// </summary>
        public IEnumerator DropImmediateItem(string gameId, string itemId, string targetPlayer, Action<ItemDropResponse> callback)
        {
            if (!authService.IsAuthenticated)
            {
                config.LogError("Cannot drop item: Not authenticated");
                callback?.Invoke(new ItemDropResponse { success = false, error = "Not authenticated" });
                yield break;
            }
            
            config.Log($"Dropping item {itemId} to player {targetPlayer}");
            
            ItemDropRequest dropRequest = new ItemDropRequest
            {
                itemId = itemId,
                targetPlayer = targetPlayer
            };
            
            string jsonData = JsonUtility.ToJson(dropRequest);
            string url = $"{config.gameApiUrl}/items/drop/{gameId}";
            
            using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
            {
                byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();
                
                request.SetRequestHeader("Content-Type", "application/json");
                request.SetRequestHeader("Authorization", $"Bearer {authService.AccessToken}");
                request.SetRequestHeader("X-Arena-Arcade-Game-ID", config.arenaGameId);
                request.SetRequestHeader("X-Vorld-App-ID", config.vorldAppId);
                
                yield return request.SendWebRequest();
                
                ItemDropResponse response = new ItemDropResponse();
                
                if (request.result == UnityWebRequest.Result.Success)
                {
                    try
                    {
                        response = JsonUtility.FromJson<ItemDropResponse>(request.downloadHandler.text);
                        
                        if (response.success && response.data != null)
                        {
                            config.Log($"Item dropped: {response.data.itemDropped.itemName}");
                        }
                        else
                        {
                            config.LogError($"Failed to drop item: {response.error}");
                        }
                    }
                    catch (Exception ex)
                    {
                        config.LogError($"Failed to parse item drop response: {ex.Message}");
                        response.success = false;
                        response.error = "Failed to parse server response";
                    }
                }
                else
                {
                    string errorMsg = $"Network error: {request.error}";
                    config.LogError(errorMsg);
                    response.success = false;
                    response.error = errorMsg;
                }
                
                callback?.Invoke(response);
            }
        }
        
        /// <summary>
        /// Get game details by ID.
        /// </summary>
        public IEnumerator GetGameDetails(string gameId, Action<GameInitResponse> callback)
        {
            if (!authService.IsAuthenticated)
            {
                config.LogError("Cannot get game details: Not authenticated");
                callback?.Invoke(new GameInitResponse { success = false, error = "Not authenticated" });
                yield break;
            }
            
            config.Log($"Fetching game details for: {gameId}");
            
            string url = $"{config.gameApiUrl}/games/{gameId}";
            
            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                request.SetRequestHeader("Authorization", $"Bearer {authService.AccessToken}");
                request.SetRequestHeader("X-Arena-Arcade-Game-ID", config.arenaGameId);
                request.SetRequestHeader("X-Vorld-App-ID", config.vorldAppId);
                
                yield return request.SendWebRequest();
                
                GameInitResponse response = new GameInitResponse();
                
                if (request.result == UnityWebRequest.Result.Success)
                {
                    try
                    {
                        response = JsonUtility.FromJson<GameInitResponse>(request.downloadHandler.text);
                        
                        if (response.success && response.data != null)
                        {
                            currentGameState = response.data;
                            config.Log($"Game details loaded: {currentGameState.gameId}");
                        }
                        else
                        {
                            config.LogError($"Failed to get game details: {response.error}");
                        }
                    }
                    catch (Exception ex)
                    {
                        config.LogError($"Failed to parse game details response: {ex.Message}");
                        response.success = false;
                        response.error = "Failed to parse server response";
                    }
                }
                else
                {
                    string errorMsg = $"Network error: {request.error}";
                    config.LogError(errorMsg);
                    response.success = false;
                    response.error = errorMsg;
                }
                
                callback?.Invoke(response);
            }
        }
        
        /// <summary>
        /// Disconnect from game and cleanup.
        /// </summary>
        public void Disconnect()
        {
            if (websocketClient != null)
            {
                websocketClient.Disconnect();
                websocketClient = null;
            }
            
            currentGameState = null;
            config.Log("Disconnected from Arena game");
        }
        
        /// <summary>
        /// Get the WebSocket client instance (used by ArenaManager).
        /// </summary>
        public ArenaWebSocketClient GetWebSocketClient()
        {
            return websocketClient;
        }
    }
}
