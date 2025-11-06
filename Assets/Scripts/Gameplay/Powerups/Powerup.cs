using UnityEngine;

namespace ChronoDash.Powerups
{
    public enum PowerupType
    {
        Invincibility,
        Speed,
        Magnet,
        Shield,
        Multiply2x,
        Heart
    }
    
    [System.Serializable]
    public class PowerupData
    {
        public PowerupType type;
        public float duration;
        public Sprite icon;
        public Color color;
    }
    
    public class Powerup : MonoBehaviour
    {
        [SerializeField] private PowerupType powerupType;
        [SerializeField] private float moveSpeed = 3f;
        [SerializeField] private int pointValue = 0;
        
        private bool isCollected = false;
        private Camera mainCamera;
        
        public PowerupType Type => powerupType;
        public int PointValue => pointValue;
        
        private void Awake()
        {
            mainCamera = Camera.main;
        }
        
        private void Update()
        {
            float finalSpeed = moveSpeed;
            
            var powerupEffects = FindFirstObjectByType<PowerupEffectsManager>();
            if (powerupEffects != null)
            {
                finalSpeed *= powerupEffects.SpeedMultiplier;
            }
            
            // Always move LEFT in world space - camera rotation handles visual direction
            transform.Translate(Vector3.left * finalSpeed * Time.deltaTime);
            
            // Check if gravity is flipped for despawn boundaries
            bool isGravityFlipped = mainCamera != null && Mathf.Abs(mainCamera.transform.rotation.eulerAngles.z - 180f) < 10f;
            
            if (isGravityFlipped)
            {
                // During flip: spawned at X=-15 (left), moving left means MORE negative
                // Destroy if too far left
                if (transform.position.x < -20f)
                {
                    Destroy(gameObject);
                }
            }
            else
            {
                // Normal: spawned at X=15 (right), moving left
                // Destroy if off left side
                if (transform.position.x < -15f)
                {
                    Destroy(gameObject);
                }
            }
        }
        
        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (isCollected) return;
            
            if (collision.CompareTag("Player"))
            {
                isCollected = true;
                
                var effectsManager = FindFirstObjectByType<PowerupEffectsManager>();
                if (effectsManager != null)
                {
                    effectsManager.CollectPowerup(powerupType);
                }
                
                Destroy(gameObject);
            }
        }
    }
}
