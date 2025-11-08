using System;
using UnityEngine;

namespace ChronoDash.Core.Auth
{
    /// <summary>
    /// Central authentication manager for Vorld Auth and Arena Arcade.
    /// Singleton pattern - accessible from anywhere in the game.
    /// Manages authentication state, game sessions, and offline mode.
    /// </summary>
    public class AuthManager : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private VorldConfig config;
        
        [Header("Offline Mode")]
        [Tooltip("If enabled, game runs without authentication (for Itch.io builds)")]
        [SerializeField] private bool forceOfflineMode = false;
        
        private VorldAuthService authService;
        private ArenaGameService arenaService;
        
        public static AuthManager Instance { get; private set; }
        
        // Properties
        public bool IsOfflineMode => forceOfflineMode || config.offlineMode;
        public bool IsAuthenticated => authService != null && authService.IsAuthenticated;
        public bool IsArenaActive => arenaService != null && arenaService.IsGameActive;
        public VorldAuthService AuthService => authService;
        public ArenaGameService ArenaService => arenaService;
        public UserProfile CurrentUser => authService?.CurrentUser;
        
        // Events
        public event Action<UserProfile> OnLoginSuccess;
        public event Action OnLogout;
        public event Action<GameState> OnArenaGameStarted;
        
        private void Awake()
        {
            // Singleton pattern
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                Initialize();
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        private void Initialize()
        {
            if (config == null)
            {
                Debug.LogError("❌ AuthManager: VorldConfig is not assigned!");
                return;
            }
            
            config.Log("AuthManager initialized");
            
            if (IsOfflineMode)
            {
                config.LogWarning("Running in OFFLINE MODE - Authentication disabled");
                config.LogWarning("This build is suitable for Itch.io or non-streaming platforms");
                return;
            }
            
            // Initialize services
            authService = new VorldAuthService(config);
            arenaService = new ArenaGameService(config, authService);
            
            // Subscribe to auth events
            authService.OnLoginSuccess += HandleLoginSuccess;
            authService.OnLogout += HandleLogout;
            
            config.Log("Auth and Arena services ready");
        }
        
        #region Authentication Methods
        
        /// <summary>
        /// Login with email and password.
        /// </summary>
        public void LoginWithEmail(string email, string password, Action<bool, string> callback)
        {
            if (IsOfflineMode)
            {
                callback?.Invoke(false, "Authentication disabled in offline mode");
                return;
            }
            
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                callback?.Invoke(false, "Email and password are required");
                return;
            }
            
            StartCoroutine(authService.LoginWithEmail(email, password, (response) =>
            {
                if (response.success)
                {
                    if (response.data.requiresOTP)
                    {
                        callback?.Invoke(true, "OTP_REQUIRED");
                    }
                    else
                    {
                        callback?.Invoke(true, "Login successful");
                    }
                }
                else
                {
                    callback?.Invoke(false, response.error);
                }
            }));
        }
        
        /// <summary>
        /// Verify OTP code.
        /// </summary>
        public void VerifyOTP(string email, string otp, Action<bool, string> callback)
        {
            if (IsOfflineMode)
            {
                callback?.Invoke(false, "Authentication disabled in offline mode");
                return;
            }
            
            StartCoroutine(authService.VerifyOTP(email, otp, (response) =>
            {
                callback?.Invoke(response.success, response.success ? "OTP verified" : response.error);
            }));
        }
        
        /// <summary>
        /// Logout current user.
        /// </summary>
        public void Logout()
        {
            if (IsOfflineMode) return;
            
            authService?.Logout();
            arenaService?.Disconnect();
        }
        
        /// <summary>
        /// Get user profile.
        /// </summary>
        public void GetUserProfile(Action<UserProfile, string> callback)
        {
            if (IsOfflineMode)
            {
                callback?.Invoke(null, "Authentication disabled in offline mode");
                return;
            }
            
            StartCoroutine(authService.GetProfile((response) =>
            {
                callback?.Invoke(response.success ? response.data.profile : null, response.error);
            }));
        }
        
        #endregion
        
        #region Arena Arcade Methods
        
        /// <summary>
        /// Initialize Arena game session.
        /// Required for streaming integration and viewer boosts.
        /// </summary>
        public void InitializeArenaGame(string streamUrl, Action<bool, string> callback)
        {
            if (IsOfflineMode)
            {
                callback?.Invoke(false, "Arena Arcade disabled in offline mode");
                return;
            }
            
            if (!IsAuthenticated)
            {
                callback?.Invoke(false, "Must be logged in to start Arena game");
                return;
            }
            
            if (string.IsNullOrEmpty(streamUrl))
            {
                streamUrl = config.defaultStreamUrl;
            }
            
            StartCoroutine(arenaService.InitializeGame(streamUrl, (response) =>
            {
                if (response.success)
                {
                    OnArenaGameStarted?.Invoke(response.data);
                    callback?.Invoke(true, $"Arena game started: {response.data.gameId}");
                }
                else
                {
                    callback?.Invoke(false, response.error);
                }
            }));
        }
        
        /// <summary>
        /// Boost a player with Arena coins.
        /// Used by viewers to support players during gameplay.
        /// </summary>
        public void BoostPlayer(string playerId, int amount, string username, Action<bool, string> callback)
        {
            if (IsOfflineMode)
            {
                callback?.Invoke(false, "Arena Arcade disabled in offline mode");
                return;
            }
            
            if (!IsArenaActive)
            {
                callback?.Invoke(false, "No active Arena game");
                return;
            }
            
            string gameId = arenaService.CurrentGameState.gameId;
            
            StartCoroutine(arenaService.BoostPlayer(gameId, playerId, amount, username, (response) =>
            {
                if (response.success)
                {
                    callback?.Invoke(true, $"Boosted {response.data.playerName} with +{amount} coins");
                }
                else
                {
                    callback?.Invoke(false, response.error);
                }
            }));
        }
        
        /// <summary>
        /// Drop immediate item to target player.
        /// </summary>
        public void DropItemToPlayer(string itemId, string targetPlayer, Action<bool, string> callback)
        {
            if (IsOfflineMode)
            {
                callback?.Invoke(false, "Arena Arcade disabled in offline mode");
                return;
            }
            
            if (!IsArenaActive)
            {
                callback?.Invoke(false, "No active Arena game");
                return;
            }
            
            string gameId = arenaService.CurrentGameState.gameId;
            
            StartCoroutine(arenaService.DropImmediateItem(gameId, itemId, targetPlayer, (response) =>
            {
                if (response.success)
                {
                    callback?.Invoke(true, $"Dropped {response.data.itemDropped.itemName}");
                }
                else
                {
                    callback?.Invoke(false, response.error);
                }
            }));
        }
        
        #endregion
        
        private void HandleLoginSuccess(UserProfile user)
        {
            config.Log($"✅ Login successful: {user.username} ({user.email})");
            OnLoginSuccess?.Invoke(user);
        }
        
        private void HandleLogout()
        {
            config.Log("User logged out");
            OnLogout?.Invoke();
        }
        
        /// <summary>
        /// Get the ArenaGameService instance (used by ArenaManager).
        /// </summary>
        public ArenaGameService GetArenaGameService()
        {
            return arenaService;
        }
        
        private void OnDestroy()
        {
            if (Instance == this)
            {
                arenaService?.Disconnect();
                authService?.Logout();
            }
        }
    }
}
