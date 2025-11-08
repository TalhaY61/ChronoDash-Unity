using System;
using System.Collections.Generic;

namespace ChronoDash.Core.Auth
{
    /// <summary>
    /// Data models for Vorld Auth and Arena Arcade API responses.
    /// Matches the TypeScript interfaces from documentation.
    /// </summary>
    
    #region Auth Models
    
    [Serializable]
    public class LoginRequest
    {
        public string email;
        public string password; // Will be SHA-256 hashed before sending
    }
    
    [Serializable]
    public class LoginResponse
    {
        public bool success;
        public LoginData data;
        public string error;
    }
    
    [Serializable]
    public class LoginData
    {
        public UserProfile user;
        public string accessToken;
        public string refreshToken;
        public bool requiresOTP;
        public string message;
    }
    
    [Serializable]
    public class OTPVerifyRequest
    {
        public string email;
        public string otp;
    }
    
    [Serializable]
    public class UserProfile
    {
        public string id;
        public string email;
        public string username;
        public bool verified;
        public string[] authMethods;
        public int totalConnectedAccounts;
        public UserStates states;
        public WalletInfo[] wallets;
    }
    
    [Serializable]
    public class UserStates
    {
        public string developer;
        public string gameDeveloper;
    }
    
    [Serializable]
    public class WalletInfo
    {
        public string address;
        public string type;
        public bool isDefault;
    }
    
    [Serializable]
    public class ProfileResponse
    {
        public bool success;
        public ProfileData data;
        public string error;
    }
    
    [Serializable]
    public class ProfileData
    {
        public UserProfile profile;
    }
    
    #endregion
    
    #region Arena Arcade Models
    
    [Serializable]
    public class GameInitRequest
    {
        public string streamUrl;
    }
    
    [Serializable]
    public class GameInitResponse
    {
        public bool success;
        public GameState data;
        public string message;
        public string timestamp;
        public string error;
    }
    
    [Serializable]
    public class GameState
    {
        public string gameId;
        public string expiresAt;
        public string status; // pending, active, completed, cancelled
        public string websocketUrl;
        public EvaGameDetails evaGameDetails;
        public bool arenaActive;
        public bool countdownStarted;
    }
    
    [Serializable]
    public class EvaGameDetails
    {
        public string _id;
        public string gameId;
        public string vorldAppId;
        public string appName;
        public string gameDeveloperId;
        public string arcadeGameId;
        public bool isActive;
        public int numberOfCycles;
        public int cycleTime;
        public int waitingTime;
        public GamePlayer[] players;
        public GameEvent[] events;
        public GamePackage[] packages;
        public string createdAt;
        public string updatedAt;
    }
    
    [Serializable]
    public class GamePlayer
    {
        public string id;
        public string name;
        public string createdAt;
        public string updatedAt;
    }
    
    [Serializable]
    public class GameEvent
    {
        public string id;
        public string eventName;
        public bool isFinal;
        public string createdAt;
        public string updatedAt;
    }
    
    [Serializable]
    public class GamePackage
    {
        public string id;
        public string name;
        public string image;
        public PackageStat[] stats;
        public string[] players;
        public string type; // immediate or cycle
        public int cost;
        public int unlockAtPoints;
        public PackageMetadata metadata;
    }
    
    [Serializable]
    public class PackageStat
    {
        public string name;
        public int currentValue;
        public int maxValue;
        public string description;
    }
    
    [Serializable]
    public class PackageMetadata
    {
        public string id;
        public string type;
        public string quantity;
    }
    
    [Serializable]
    public class BoostPlayerRequest
    {
        public int amount;
        public string username;
    }
    
    [Serializable]
    public class BoostPlayerResponse
    {
        public bool success;
        public BoostData data;
        public string message;
        public string timestamp;
        public string error;
    }
    
    [Serializable]
    public class BoostData
    {
        public string playerId;
        public string playerName;
        public int currentCyclePoints;
        public int totalPoints;
        public int arenaCoinsSpent;
        public int newArenaCoinsBalance;
    }
    
    [Serializable]
    public class ItemDropRequest
    {
        public string itemId;
        public string targetPlayer;
    }
    
    [Serializable]
    public class ItemDropResponse
    {
        public bool success;
        public ItemDropData data;
        public string timestamp;
        public string error;
    }
    
    [Serializable]
    public class ItemDropData
    {
        public ItemDropInfo itemDropped;
        public int newBalance;
    }
    
    [Serializable]
    public class ItemDropInfo
    {
        public string itemId;
        public string itemName;
        public string targetPlayer;
        public int cost;
    }
    
    #endregion
    
    #region WebSocket Event Models
    
    [Serializable]
    public class ArenaCountdownEvent
    {
        public string type;
        public string gameId;
        public bool arenaActive;
        public bool countdownStarted;
        public string expiresAt;
        public string websocketUrl;
        public string timestamp;
    }
    
    [Serializable]
    public class CountdownUpdateEvent
    {
        public string type;
        public string gameId;
        public int secondsRemaining;
        public string expiresAt;
        public string timestamp;
    }
    
    [Serializable]
    public class ArenaBeginsEvent
    {
        public string type;
        public string gameId;
        public bool arenaActive;
        public EvaGameDetails evaGameDetails;
        public string timestamp;
    }
    
    [Serializable]
    public class PlayerBoostEvent
    {
        public string type;
        public string gameId;
        public string playerId;
        public string playerName;
        public string boosterUsername;
        public int boostAmount;
        public int playerCurrentCyclePoints;
        public int playerTotalPoints;
        public int arenaCoinsSpent;
        public int newArenaCoinsBalance;
        public string timestamp;
    }
    
    [Serializable]
    public class PackageDropEvent
    {
        public string type;
        public string gameId;
        public int currentCycle;
        public Dictionary<string, int> playerPoints; // playerId -> points
        public Dictionary<string, PackageItem[]> playerItems; // playerId -> items
        public string timestamp;
    }
    
    [Serializable]
    public class PackageItem
    {
        public string id;
        public int quantity;
    }
    
    [Serializable]
    public class ImmediateItemDropEvent
    {
        public string type;
        public string gameId;
        public ItemDropInfo itemDropped;
        public int newBalance;
        public string timestamp;
    }
    
    #endregion
}
