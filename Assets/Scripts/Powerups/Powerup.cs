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
        Health
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
        
        public PowerupType Type => powerupType;
        public int PointValue => pointValue;
        
        private void Update()
        {
            float finalSpeed = moveSpeed;
            
            var powerupEffects = FindFirstObjectByType<PowerupEffectsManager>();
            if (powerupEffects != null)
            {
                finalSpeed *= powerupEffects.SpeedMultiplier;
            }
            
            transform.Translate(Vector3.left * finalSpeed * Time.deltaTime);
            
            if (transform.position.x < -15f)
            {
                Destroy(gameObject);
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
