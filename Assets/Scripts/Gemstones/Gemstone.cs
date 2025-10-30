using UnityEngine;

namespace ChronoDash.Gemstones
{
    public enum GemstoneType
    {
        Blue,   // +10 score (Common - 60%)
        Green,  // +20 score (Uncommon - 30%)
        Red     // +50 score (Rare - 10%)
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
        
        public GemstoneType Type => gemstoneType;
        public int ScoreValue => scoreValue;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
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
            
            transform.Translate(Vector3.left * finalSpeed * Time.deltaTime);
        }

        private void CheckOffScreen()
        {
            if (transform.position.x < -10f)
            {
                Destroy(gameObject);
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
                Debug.Log("💎 Red gemstone collected - calling Heal()");
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
