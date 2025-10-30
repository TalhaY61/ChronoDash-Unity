using UnityEngine;
using ChronoDash.Abilities;

namespace ChronoDash.Obstacles
{
    [RequireComponent(typeof(SpriteRenderer))]
    public abstract class Obstacle : MonoBehaviour
    {
        [Header("Obstacle Settings")]
        [SerializeField] protected float moveSpeed = 400f;
        
        protected SpriteRenderer spriteRenderer;
        protected bool isSlowedDown = false;
        protected float currentSpeed;
        protected bool hasPassed = false; // Track if player successfully passed this obstacle
        protected bool playerWasHit = false; // Track if player was hit by this obstacle
        protected GameObject playerObj;
        protected float lastX;
        protected float speedMultiplier = 1f; // For powerups like SpeedBoost
        protected TimeBubble timeBubble; // Reference to TimeBubble

        // Properties
        public float MoveSpeed { get => moveSpeed; set => moveSpeed = value; }
        
        // Event when player successfully passes this obstacle
        public System.Action<Obstacle> OnObstaclePassed;

        protected virtual void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            currentSpeed = moveSpeed;
            playerObj = GameObject.FindGameObjectWithTag("Player");
            lastX = transform.position.x;
            
            // Find TimeBubble (it's on the player)
            if (playerObj != null)
            {
                timeBubble = playerObj.GetComponent<TimeBubble>();
            }
        }

        protected virtual void Update()
        {
            float prevX = lastX;
            lastX = transform.position.x;
            
            // Check if inside Time Bubble and apply slowdown
            CheckTimeBubbleSlowdown();
            
            Move();
            CheckIfPassedPlayer(prevX);
            CheckOffScreen();
        }
        
        protected virtual void CheckTimeBubbleSlowdown()
        {
            if (timeBubble != null && timeBubble.IsActive)
            {
                // Check if this obstacle is within the bubble radius
                bool isInBubble = timeBubble.IsPositionInBubble(transform.position);
                
                if (isInBubble != isSlowedDown)
                {
                    SetSlowdown(isInBubble, timeBubble.GetSlowFactorAt(transform.position));
                }
            }
            else if (isSlowedDown)
            {
                // Bubble is not active but obstacle is still slowed - reset
                SetSlowdown(false);
            }
        }

        protected virtual void Move()
        {
            // Move left continuously
            transform.Translate(Vector3.left * currentSpeed * Time.deltaTime);
        }
        
        protected virtual void CheckIfPassedPlayer(float prevX)
        {
            if (playerObj == null) return;
            float playerX = playerObj.transform.position.x;
            // If obstacle crossed from right to left past the player X position
            if (!hasPassed && prevX > playerX && lastX <= playerX)
            {
                hasPassed = true;
                if (!playerWasHit)
                {
                    OnObstaclePassed?.Invoke(this);
                    Debug.Log($"✅ Player successfully avoided {gameObject.name}! +1 point");
                }
                else
                {
                    Debug.Log($"❌ Player was hit by {gameObject.name} - No point awarded");
                }
            }
        }

        protected virtual void CheckOffScreen()
        {
            // Destroy if off-screen (left side)
            if (transform.position.x < -10f)
            {
                Destroy(gameObject);
            }
        }

        public virtual void SetSlowdown(bool slowdown, float slowdownFactor = 0.5f)
        {
            isSlowedDown = slowdown;
            RecalculateSpeed();
        }

        public virtual void SetSpeedMultiplier(float multiplier)
        {
            speedMultiplier = multiplier;
            RecalculateSpeed();
        }

        private void RecalculateSpeed()
        {
            float slowFactor = isSlowedDown ? 0.5f : 1f;
            currentSpeed = moveSpeed * slowFactor * speedMultiplier;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                OnPlayerCollision();
            }
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.gameObject.CompareTag("Player"))
            {
                OnPlayerCollision();
            }
        }

        protected virtual void OnPlayerCollision()
        {
            // Mark that player was hit - no point will be awarded
            playerWasHit = true;
            Debug.Log($"Player hit {gameObject.name} - point will NOT be awarded");
        }
    }
}
