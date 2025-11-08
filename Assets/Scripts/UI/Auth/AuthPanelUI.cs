using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ChronoDash.Core.Auth;
using ChronoDash.Managers;

namespace ChronoDash.UI
{
    /// <summary>
    /// Authentication panel UI for email/password login and OTP verification.
    /// Follows the Vorld Auth documentation flow.
    /// </summary>
    public class AuthPanelUI : MonoBehaviour
    {
        [Header("Login UI")]
        [SerializeField] private GameObject loginPanel;
        [SerializeField] private TMP_InputField emailInput;
        [SerializeField] private TMP_InputField passwordInput;
        [SerializeField] private Button loginButton;
        [SerializeField] private Button closeButton;
        [SerializeField] private TextMeshProUGUI statusText;
        
        [Header("OTP UI")]
        [SerializeField] private GameObject otpPanel;
        [SerializeField] private TMP_InputField otpInput;
        [SerializeField] private Button verifyOtpButton;
        [SerializeField] private Button backToLoginButton;
        [SerializeField] private TextMeshProUGUI otpStatusText;
        
        [Header("Profile UI")]
        [SerializeField] private GameObject profilePanel;
        [SerializeField] private TextMeshProUGUI usernameText;
        [SerializeField] private TextMeshProUGUI emailText;
        [SerializeField] private TMP_InputField streamUrlInput;
        [SerializeField] private TextMeshProUGUI profileStatusText;
        [SerializeField] private Button logoutButton;
        [SerializeField] private Button startArenaButton;
        
        private string currentEmail;
        private bool isProcessing;
        
        private void Start()
        {
            SetupButtons();
            UpdateUI();
        }
        
        private void OnEnable()
        {
            UpdateUI();
        }
        
        private void SetupButtons()
        {
            if (loginButton != null)
                loginButton.onClick.AddListener(OnLoginClicked);
            
            if (verifyOtpButton != null)
                verifyOtpButton.onClick.AddListener(OnVerifyOtpClicked);
            
            if (backToLoginButton != null)
                backToLoginButton.onClick.AddListener(ShowLoginPanel);
            
            if (logoutButton != null)
                logoutButton.onClick.AddListener(OnLogoutClicked);
            
            if (startArenaButton != null)
                startArenaButton.onClick.AddListener(OnStartArenaClicked);
            
            if (closeButton != null)
                closeButton.onClick.AddListener(OnCloseClicked);
        }
        
        private void UpdateUI()
        {
            if (AuthManager.Instance == null) return;
            
            if (AuthManager.Instance.IsAuthenticated)
            {
                ShowProfilePanel();
            }
            else
            {
                ShowLoginPanel();
            }
        }
        
        private void ShowLoginPanel()
        {
            if (loginPanel != null) loginPanel.SetActive(true);
            if (otpPanel != null) otpPanel.SetActive(false);
            if (profilePanel != null) profilePanel.SetActive(false);
            
            ClearInputs();
            SetStatus("", false);
        }
        
        private void ShowOTPPanel()
        {
            if (loginPanel != null) loginPanel.SetActive(false);
            if (otpPanel != null) otpPanel.SetActive(true);
            if (profilePanel != null) profilePanel.SetActive(false);
            
            if (otpStatusText != null)
                otpStatusText.text = $"Enter the 6-digit code sent to {currentEmail}";
        }
        
        private void ShowProfilePanel()
        {
            if (loginPanel != null) loginPanel.SetActive(false);
            if (otpPanel != null) otpPanel.SetActive(false);
            if (profilePanel != null) profilePanel.SetActive(true);
            
            if (AuthManager.Instance != null && AuthManager.Instance.CurrentUser != null)
            {
                var user = AuthManager.Instance.CurrentUser;
                if (usernameText != null)
                    usernameText.text = $"Welcome, {user.username}!";
                if (emailText != null)
                    emailText.text = user.email;
            }
            
            // Clear profile status text
            SetProfileStatus("", false);
        }
        
        private void OnLoginClicked()
        {
            if (isProcessing) return;
            
            string email = emailInput != null ? emailInput.text.Trim() : "";
            string password = passwordInput != null ? passwordInput.text : "";
            
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                SetStatus("Please enter email and password", true);
                return;
            }
            
            currentEmail = email;
            isProcessing = true;
            SetStatus("Logging in...", false);
            SetLoginButtonEnabled(false);
            
            AuthManager.Instance.LoginWithEmail(email, password, (success, message) =>
            {
                isProcessing = false;
                SetLoginButtonEnabled(true);
                
                if (success)
                {
                    if (message == "OTP_REQUIRED")
                    {
                        ShowOTPPanel();
                    }
                    else
                    {
                        SetStatus("Login successful!", false);
                        ShowNotification("✅ Login Successful!", $"Welcome, {AuthManager.Instance.CurrentUser.username}!");
                        UpdateUI();
                    }
                }
                else
                {
                    SetStatus($"Login failed: {message}", true);
                    ShowNotification("❌ Login Failed", message);
                }
            });
        }
        
        private void OnVerifyOtpClicked()
        {
            if (isProcessing) return;
            
            string otp = otpInput != null ? otpInput.text.Trim() : "";
            
            if (string.IsNullOrEmpty(otp) || otp.Length != 6)
            {
                SetOTPStatus("Please enter a 6-digit OTP code", true);
                return;
            }
            
            isProcessing = true;
            SetOTPStatus("Verifying OTP...", false);
            SetVerifyButtonEnabled(false);
            
            AuthManager.Instance.VerifyOTP(currentEmail, otp, (success, message) =>
            {
                isProcessing = false;
                SetVerifyButtonEnabled(true);
                
                if (success)
                {
                    SetOTPStatus("OTP verified!", false);
                    ShowNotification("✅ OTP Verified!", $"Welcome, {AuthManager.Instance.CurrentUser.username}!");
                    UpdateUI();
                }
                else
                {
                    SetOTPStatus($"Verification failed: {message}", true);
                    ShowNotification("❌ Verification Failed", message);
                }
            });
        }
        
        private void OnLogoutClicked()
        {
            AuthManager.Instance.Logout();
            ShowNotification("👋 Logged Out", "You have been logged out successfully");
            UpdateUI();
        }
        
        private void OnStartArenaClicked()
        {
            if (isProcessing) return;
            
            // Get stream URL from input field
            string streamUrl = streamUrlInput != null ? streamUrlInput.text.Trim() : "";
            
            // Validate stream URL
            if (string.IsNullOrEmpty(streamUrl))
            {
                SetProfileStatus("Please enter a valid stream URL", true);
                return;
            }
            
            isProcessing = true;
            SetProfileStatus("Starting Arena...", false);
            
            AuthManager.Instance.InitializeArenaGame(streamUrl, (success, message) =>
            {
                isProcessing = false;
                
                if (success)
                {
                    SetProfileStatus("✅ Arena started successfully!", false);
                    ShowNotification("Arena Started!", "Arena Arcade game is now active! Viewers can now interact!");
                    OnCloseClicked();
                }
                else
                {
                    SetProfileStatus($"❌ Arena failed: {message}", true);
                    ShowNotification("Arena Failed", message);
                }
            });
        }
        
        private void OnCloseClicked()
        {
            gameObject.SetActive(false);
        }
        
        private void ClearInputs()
        {
            if (emailInput != null) emailInput.text = "";
            if (passwordInput != null) passwordInput.text = "";
            if (otpInput != null) otpInput.text = "";
        }
        
        private void SetStatus(string message, bool isError)
        {
            if (statusText != null)
            {
                statusText.text = message;
                statusText.color = isError ? Color.red : Color.white;
            }
        }
        
        private void SetOTPStatus(string message, bool isError)
        {
            if (otpStatusText != null)
            {
                otpStatusText.text = message;
                otpStatusText.color = isError ? Color.red : Color.white;
            }
        }
        
        private void SetProfileStatus(string message, bool isError)
        {
            if (profileStatusText != null)
            {
                profileStatusText.text = message;
                profileStatusText.color = isError ? Color.red : Color.white;
            }
        }
        
        private void SetLoginButtonEnabled(bool enabled)
        {
            if (loginButton != null)
                loginButton.interactable = enabled;
        }
        
        private void SetVerifyButtonEnabled(bool enabled)
        {
            if (verifyOtpButton != null)
                verifyOtpButton.interactable = enabled;
        }
        
        private void ShowNotification(string title, string message)
        {
            // Log to console - NotificationUI is in HUD during gameplay, not in menus
            UnityEngine.Debug.Log($"[Auth] {title}: {message}");
        }
        
        private void OnDestroy()
        {
            // Clean up listeners
            if (loginButton != null)
                loginButton.onClick.RemoveListener(OnLoginClicked);
            if (verifyOtpButton != null)
                verifyOtpButton.onClick.RemoveListener(OnVerifyOtpClicked);
            if (backToLoginButton != null)
                backToLoginButton.onClick.RemoveListener(ShowLoginPanel);
            if (logoutButton != null)
                logoutButton.onClick.RemoveListener(OnLogoutClicked);
            if (startArenaButton != null)
                startArenaButton.onClick.RemoveListener(OnStartArenaClicked);
            if (closeButton != null)
                closeButton.onClick.RemoveListener(OnCloseClicked);
        }
    }
}
