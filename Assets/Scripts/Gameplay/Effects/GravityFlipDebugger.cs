using UnityEngine;

namespace ChronoDash.Effects {
    /// <summary>
    /// Debug helper to monitor gravity flip state and game objects
    /// Attach to any GameObject to see real-time flip status
    /// </summary>
    public class GravityFlipDebugger : MonoBehaviour {
        [Header("References")]
        [SerializeField] private GravityFlipEffect gravityFlipEffect;
        [SerializeField] private Canvas gameplayCanvas;
        
        [Header("Debug Settings")]
        [SerializeField] private bool showDebugGUI = true;
        [SerializeField] private bool logEveryFrame = false;
        
        private Camera mainCamera;
        
        private void Start() {
            mainCamera = Camera.main;
            
            if (gravityFlipEffect == null) {
                gravityFlipEffect = FindFirstObjectByType<GravityFlipEffect>();
            }
            
            if (gameplayCanvas == null) {
                gameplayCanvas = FindFirstObjectByType<Canvas>();
            }
        }
        
        private void Update() {
            if (logEveryFrame) {
                LogState();
            }
        }
        
        private void OnGUI() {
            if (!showDebugGUI) return;
            
            GUILayout.BeginArea(new Rect(10, 100, 400, 400));
            GUILayout.BeginVertical("box");
            
            GUILayout.Label("=== GRAVITY FLIP DEBUG ===", GUI.skin.box);
            GUILayout.Space(10);
            
            // Camera rotation
            if (mainCamera != null) {
                Vector3 rot = mainCamera.transform.rotation.eulerAngles;
                GUILayout.Label($"Camera Rotation: ({rot.x:F1}°, {rot.y:F1}°, {rot.z:F1}°)");
                
                bool isFlipped = Mathf.Abs(rot.z - 180f) < 10f;
                GUILayout.Label($"Is Flipped (Z-axis): {isFlipped}", isFlipped ? GUI.skin.box : GUI.skin.label);
            }
            
            GUILayout.Space(10);
            
            // Gravity flip effect
            if (gravityFlipEffect != null) {
                GUILayout.Label($"Effect Active: {gravityFlipEffect.IsActive}");
                GUILayout.Label($"Effect Flipped: {gravityFlipEffect.IsFlipped}");
            } else {
                GUILayout.Label("⚠️ GravityFlipEffect: NULL");
            }
            
            GUILayout.Space(10);
            
            // Canvas rotation
            if (gameplayCanvas != null) {
                Vector3 canvasRot = gameplayCanvas.transform.rotation.eulerAngles;
                GUILayout.Label($"Canvas Rotation: ({canvasRot.x:F1}°, {canvasRot.y:F1}°, {canvasRot.z:F1}°)");
            } else {
                GUILayout.Label("⚠️ Canvas: NULL");
            }
            
            GUILayout.Space(10);
            
            // Obstacle count
            var obstacles = FindObjectsByType<Obstacles.Obstacle>(FindObjectsSortMode.None);
            GUILayout.Label($"Active Obstacles: {obstacles.Length}");
            
            // Gemstone count
            var gemstones = FindObjectsByType<Gemstones.Gemstone>(FindObjectsSortMode.None);
            GUILayout.Label($"Active Gemstones: {gemstones.Length}");
            
            // Powerup count
            var powerups = FindObjectsByType<Powerups.Powerup>(FindObjectsSortMode.None);
            GUILayout.Label($"Active Powerups: {powerups.Length}");
            
            GUILayout.EndVertical();
            GUILayout.EndArea();
        }
        
        private void LogState() {
            if (mainCamera == null) return;
            
            Vector3 rot = mainCamera.transform.rotation.eulerAngles;
            bool isFlipped = Mathf.Abs(rot.z - 180f) < 10f;
        }
        
        [ContextMenu("Force Log State")]
        public void ForceLogState() {
            Debug.Log("=== GRAVITY FLIP STATE ===");
            
            if (mainCamera != null) {
                Vector3 rot = mainCamera.transform.rotation.eulerAngles;
                Debug.Log($"Camera: ({rot.x:F1}°, {rot.y:F1}°, {rot.z:F1}°)");
                Debug.Log($"Is Flipped: {Mathf.Abs(rot.z - 180f) < 10f}");
            }
            
            if (gravityFlipEffect != null) {
                Debug.Log($"Effect Active: {gravityFlipEffect.IsActive}");
                Debug.Log($"Effect Flipped: {gravityFlipEffect.IsFlipped}");
            }
            
            if (gameplayCanvas != null) {
                Vector3 canvasRot = gameplayCanvas.transform.rotation.eulerAngles;
                Debug.Log($"Canvas: ({canvasRot.x:F1}°, {canvasRot.y:F1}°, {canvasRot.z:F1}°)");
            }
            
            var obstacles = FindObjectsByType<Obstacles.Obstacle>(FindObjectsSortMode.None);
            Debug.Log($"Active Obstacles: {obstacles.Length}");
            
            var gemstones = FindObjectsByType<Gemstones.Gemstone>(FindObjectsSortMode.None);
            Debug.Log($"Active Gemstones: {gemstones.Length}");
            
            var powerups = FindObjectsByType<Powerups.Powerup>(FindObjectsSortMode.None);
            Debug.Log($"Active Powerups: {powerups.Length}");
        }
    }
}
