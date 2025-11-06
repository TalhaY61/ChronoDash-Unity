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
        protected Camera mainCamera; // Reference to check if gravity is flipped

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
            mainCamera = Camera.main;
            
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
            // Check if gravity is flipped (Z-axis rotation check)
            bool isGravityFlipped = mainCamera != null && Mathf.Abs(mainCamera.transform.rotation.eulerAngles.z - 180f) < 10f;
            
            if (isGravityFlipped)
            {
                // During gravity flip, DON'T change direction - camera rotation handles visual flip
                // Move LEFT in world space (which appears RIGHT on flipped camera)
                transform.Translate(Vector3.left * currentSpeed * Time.deltaTime);
            }
            else
            {
                // Normal: Move left continuously
                transform.Translate(Vector3.left * currentSpeed * Time.deltaTime);
            }
            
        }
        
        protected virtual void CheckIfPassedPlayer(float prevX)
        {
            if (playerObj == null) return;
            float playerX = playerObj.transform.position.x;
            
            // Obstacles ALWAYS move LEFT in world space
            // Check if crossed player from right to left
            if (!hasPassed && prevX > playerX && lastX <= playerX)
            {
                hasPassed = true;
                if (!playerWasHit)
                {
                    OnObstaclePassed?.Invoke(this);
                }
            }
        }

        protected virtual void CheckOffScreen()
        {
            // Obstacles ALWAYS move LEFT in world space (camera rotation makes it appear different)
            // Always destroy when they go off the LEFT side
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
        }
    }
}
