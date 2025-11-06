using UnityEngine;

namespace ChronoDash.Gemstones
{
    public enum GemstoneType
    {
        Blue,   // +10 score (Common - 50%)
        Green,  // +20 score (Uncommon - 30%)
        Red     // +50 score + 1 Heart (Rare - 20%)
    }

    public class Gemstone : MonoBehaviour
    {
        [Header("Gemstone Settings")]
        [SerializeField] private GemstoneType gemstoneType;
        [SerializeField] private float moveSpeed = 400f;
        [SerializeField] private int scoreValue;

        [Header("Audio")]
        [SerializeField] private AudioClip collectSound;

        private SpriteRenderer spriteRenderer;
        private Camera mainCamera;
        
        public GemstoneType Type => gemstoneType;
        public int ScoreValue => scoreValue;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            mainCamera = Camera.main;
            SetScoreValue();
        }

        private void Update()
        {
            Move();
            CheckOffScreen();
        }

        private void Move()
        {
            float finalSpeed = moveSpeed / 100f;
            
            var powerupEffects = FindFirstObjectByType<ChronoDash.Powerups.PowerupEffectsManager>();
            if (powerupEffects != null)
            {
                finalSpeed *= powerupEffects.SpeedMultiplier;
            }
            
            // Always move LEFT in world space - camera rotation handles visual direction
            transform.Translate(Vector3.left * finalSpeed * Time.deltaTime);
        }

        private void CheckOffScreen()
        {
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
                if (transform.position.x < -10f)
                {
                    Destroy(gameObject);
                }
            }
        }

        private void SetScoreValue()
        {
            switch (gemstoneType)
            {
                case GemstoneType.Blue:
                    scoreValue = 10; // Common
                    break;
                case GemstoneType.Green:
                    scoreValue = 20; // Uncommon
                    break;
                case GemstoneType.Red:
                    scoreValue = 50; // Rare
                    break;
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                Collect(other.gameObject);
            }
        }

        private void Collect(GameObject player)
        {
            if (collectSound != null)
            {
                AudioSource.PlayClipAtPoint(collectSound, transform.position);
            }

            var playerController = player.GetComponent<ChronoDash.Player.PlayerController>();
            if (playerController != null && gemstoneType == GemstoneType.Red)
            {
                playerController.Heal(1);
            }

            var gemstoneManager = FindFirstObjectByType<GemstoneManager>();
            if (gemstoneManager != null)
            {
                gemstoneManager.NotifyGemstoneCollected(this);
            }

            Destroy(gameObject);
        }

        public void SetType(GemstoneType type)
        {
            gemstoneType = type;
            SetScoreValue();
        }
    }
}
