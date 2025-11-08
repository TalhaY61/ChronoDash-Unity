using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace ChronoDash.Core.Auth
{
    /// <summary>
    /// Vorld authentication service for email/password and OTP verification.
    /// Handles user login, registration, and profile management.
    /// CRITICAL: Passwords are automatically hashed with SHA-256 before sending.
    /// </summary>
    public class VorldAuthService
    {
        private readonly VorldConfig config;
        private string accessToken;
        private string refreshToken;
        private UserProfile currentUser;
        
        // Events
        public event Action<UserProfile> OnLoginSuccess;
        public event Action<string> OnLoginError;
        public event Action<bool> OnOTPRequired; // true if OTP needed
        public event Action OnLogout;
        
        public bool IsAuthenticated => !string.IsNullOrEmpty(accessToken);
        public UserProfile CurrentUser => currentUser;
        public string AccessToken => accessToken;
        
        public VorldAuthService(VorldConfig vorldConfig)
        {
            config = vorldConfig;
            LoadTokensFromPlayerPrefs();
        }
        
        /// <summary>
        /// Login with email and password.
        /// Password will be automatically hashed with SHA-256.
        /// </summary>
        public IEnumerator LoginWithEmail(string email, string password, Action<LoginResponse> callback)
        {
            config.Log($"Attempting login for: {email}");
            
            // Hash password with SHA-256
            string hashedPassword = SHA256Helper.HashPassword(password);
            
            LoginRequest loginRequest = new LoginRequest
            {
                email = email,
                password = hashedPassword
            };
            
            string jsonData = JsonUtility.ToJson(loginRequest);
            string url = $"{config.authServerUrl}/auth/login";
            
            using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
            {
                byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();
                
                request.SetRequestHeader("Content-Type", "application/json");
                request.SetRequestHeader("x-vorld-app-id", config.vorldAppId);
                
                yield return request.SendWebRequest();
                
                LoginResponse response = new LoginResponse();
                
                if (request.result == UnityWebRequest.Result.Success)
                {
                    try
                    {
                        response = JsonUtility.FromJson<LoginResponse>(request.downloadHandler.text);
                        
                        if (response.success && response.data != null)
                        {
                            if (response.data.requiresOTP)
                            {
                                config.Log("OTP verification required");
                                OnOTPRequired?.Invoke(true);
                            }
                            else
                            {
                                // Login successful
                                accessToken = response.data.accessToken;
                                refreshToken = response.data.refreshToken;
                                currentUser = response.data.user;
                                
                                SaveTokensToPlayerPrefs();
                                
                                config.Log($"Login successful: {currentUser.username}");
                                OnLoginSuccess?.Invoke(currentUser);
                            }
                        }
                        else
                        {
                            config.LogError($"Login failed: {response.error}");
                            OnLoginError?.Invoke(response.error);
                        }
                    }
                    catch (Exception ex)
                    {
                        config.LogError($"Failed to parse login response: {ex.Message}");
                        response.success = false;
                        response.error = "Failed to parse server response";
                        OnLoginError?.Invoke(response.error);
                    }
                }
                else
                {
                    string errorMsg = $"Network error: {request.error}";
                    config.LogError(errorMsg);
                    response.success = false;
                    response.error = errorMsg;
                    OnLoginError?.Invoke(errorMsg);
                }
                
                callback?.Invoke(response);
            }
        }
        
        /// <summary>
        /// Verify OTP code sent to email.
        /// </summary>
        public IEnumerator VerifyOTP(string email, string otp, Action<LoginResponse> callback)
        {
            config.Log($"Verifying OTP for: {email}");
            
            OTPVerifyRequest otpRequest = new OTPVerifyRequest
            {
                email = email,
                otp = otp
            };
            
            string jsonData = JsonUtility.ToJson(otpRequest);
            string url = $"{config.authServerUrl}/auth/verify-otp";
            
            using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
            {
                byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();
                
                request.SetRequestHeader("Content-Type", "application/json");
                request.SetRequestHeader("x-vorld-app-id", config.vorldAppId);
                
                yield return request.SendWebRequest();
                
                LoginResponse response = new LoginResponse();
                
                if (request.result == UnityWebRequest.Result.Success)
                {
                    try
                    {
                        response = JsonUtility.FromJson<LoginResponse>(request.downloadHandler.text);
                        
                        if (response.success && response.data != null)
                        {
                            accessToken = response.data.accessToken;
                            refreshToken = response.data.refreshToken;
                            currentUser = response.data.user;
                            
                            SaveTokensToPlayerPrefs();
                            
                            config.Log($"OTP verification successful: {currentUser.username}");
                            OnLoginSuccess?.Invoke(currentUser);
                        }
                        else
                        {
                            config.LogError($"OTP verification failed: {response.error}");
                            OnLoginError?.Invoke(response.error);
                        }
                    }
                    catch (Exception ex)
                    {
                        config.LogError($"Failed to parse OTP response: {ex.Message}");
                        response.success = false;
                        response.error = "Failed to parse server response";
                        OnLoginError?.Invoke(response.error);
                    }
                }
                else
                {
                    string errorMsg = $"Network error: {request.error}";
                    config.LogError(errorMsg);
                    response.success = false;
                    response.error = errorMsg;
                    OnLoginError?.Invoke(errorMsg);
                }
                
                callback?.Invoke(response);
            }
        }
        
        /// <summary>
        /// Get user profile information.
        /// </summary>
        public IEnumerator GetProfile(Action<ProfileResponse> callback)
        {
            if (!IsAuthenticated)
            {
                config.LogError("Cannot get profile: Not authenticated");
                callback?.Invoke(new ProfileResponse { success = false, error = "Not authenticated" });
                yield break;
            }
            
            config.Log("Fetching user profile");
            
            string url = $"{config.authServerUrl}/user/profile";
            
            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                request.SetRequestHeader("Authorization", $"Bearer {accessToken}");
                request.SetRequestHeader("x-vorld-app-id", config.vorldAppId);
                
                yield return request.SendWebRequest();
                
                ProfileResponse response = new ProfileResponse();
                
                if (request.result == UnityWebRequest.Result.Success)
                {
                    try
                    {
                        response = JsonUtility.FromJson<ProfileResponse>(request.downloadHandler.text);
                        
                        if (response.success && response.data != null)
                        {
                            currentUser = response.data.profile;
                            config.Log($"Profile loaded: {currentUser.username}");
                        }
                        else
                        {
                            config.LogError($"Failed to get profile: {response.error}");
                        }
                    }
                    catch (Exception ex)
                    {
                        config.LogError($"Failed to parse profile response: {ex.Message}");
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
        /// Logout and clear tokens.
        /// </summary>
        public void Logout()
        {
            config.Log("Logging out");
            
            accessToken = null;
            refreshToken = null;
            currentUser = null;
            
            PlayerPrefs.DeleteKey("vorld_access_token");
            PlayerPrefs.DeleteKey("vorld_refresh_token");
            PlayerPrefs.Save();
            
            OnLogout?.Invoke();
        }
        
        private void SaveTokensToPlayerPrefs()
        {
            PlayerPrefs.SetString("vorld_access_token", accessToken);
            PlayerPrefs.SetString("vorld_refresh_token", refreshToken);
            PlayerPrefs.Save();
            config.Log("Tokens saved to PlayerPrefs");
        }
        
        private void LoadTokensFromPlayerPrefs()
        {
            accessToken = PlayerPrefs.GetString("vorld_access_token", null);
            refreshToken = PlayerPrefs.GetString("vorld_refresh_token", null);
            
            if (!string.IsNullOrEmpty(accessToken))
            {
                config.Log("Tokens loaded from PlayerPrefs");
            }
        }
    }
}
