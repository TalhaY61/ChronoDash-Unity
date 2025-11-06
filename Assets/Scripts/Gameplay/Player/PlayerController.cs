using UnityEngine;
using ChronoDash.Abilities;
using ChronoDash.Managers;
using ChronoDash.Powerups;
using UnityEngine.InputSystem;
using System.Collections;

namespace ChronoDash.Player
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(SpriteRenderer))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private float jumpForce = 15f;
        [SerializeField] private float doubleJumpForce = 12f;
        [SerializeField] private float gravity = 40f;
        
        [Header("Duck Settings")]
        [SerializeField] private BoxCollider2D standingCollider;
        [SerializeField] private BoxCollider2D duckCollider;
        
        [Header("Ground Check")]
        [SerializeField] private Transform groundCheck;
        [SerializeField] private float groundCheckRadius = 0.2f;
        [SerializeField] private LayerMask groundLayer;
        
        [Header("Audio")]
        [SerializeField] private AudioClip jumpSound;
        [SerializeField] private AudioClip hitSound;
        [SerializeField] private AudioClip deathSound;

        private Rigidbody2D rb;
        private SpriteRenderer spriteRenderer;
        private AudioSource audioSource;
        private Sprite originalPlayerSprite;
        private Animator animator;
        
        private HealthManager healthManager;
        private PowerupEffectsManager powerupEffects;
        
        private bool isGrounded = false;
        private bool hasUsedDoubleJump = false;
        private bool isDucking = false;
        private bool isFlashing = false;

        public bool IsInvincible => powerupEffects != null && powerupEffects.IsInvincible;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            audioSource = GetComponent<AudioSource>();
            animator = GetComponent<Animator>();
            
            healthManager = FindFirstObjectByType<HealthManager>();
            powerupEffects = FindFirstObjectByType<PowerupEffectsManager>();
            
            if (duckCollider != null)
            {
                duckCollider.enabled = false;
            }
            if (standingCollider != null)
            {
                standingCollider.enabled = true;
            }
            
            if (spriteRenderer != null && spriteRenderer.sprite != null)
            {
                originalPlayerSprite = spriteRenderer.sprite;
            }
        }

        private void Start()
        {
            // Configure Rigidbody2D
            rb.gravityScale = gravity / 10f; // Unity's gravity scale
            rb.constraints = RigidbodyConstraints2D.FreezeRotation | RigidbodyConstraints2D.FreezePositionX; // Keep player at fixed X position!
        }

        private bool canControl = true; // Can player control the character?
        
        public void SetCanControl(bool canControl)
        {
            this.canControl = canControl;
            
            // Stop ducking when losing control
            if (!canControl && isDucking)
            {
                StopDuck();
            }
        }
        
        private void Update()
        {
            HandleInput();
            CheckGroundStatus();
            CheckMagnetCollection();
        }
        
        private void LateUpdate()
        {
            // No longer force sprite restoration if using Animator
            // Animator controls the sprite now
        }

        private void HandleInput()
        {
            if (!canControl) return;
            
            var keyboard = Keyboard.current;
            if (keyboard == null) return;

            // Jump input (W key)
            if (keyboard.wKey.wasPressedThisFrame)
            {
                if (isGrounded)
                {
                    Jump(false);
                    hasUsedDoubleJump = false;
                }
                else if (!hasUsedDoubleJump)
                {
                    Jump(true);
                    hasUsedDoubleJump = true;
                }
            }

            // Duck input (S key - hold to duck)
            if (keyboard.sKey.isPressed && isGrounded)
            {
                if (!isDucking)
                {
                    StartDuck();
                }
            }
            else
            {
                if (isDucking)
                {
                    StopDuck();
                }
            }
        }
        
        private void StartDuck()
        {
            isDucking = true;
            
            // Toggle colliders
            if (standingCollider != null) standingCollider.enabled = false;
            if (duckCollider != null) duckCollider.enabled = true;
            
            if (animator != null)
            {
                animator.SetBool("isDucking", true); // Match the Animator parameter name!
            }
        }
        
        private void StopDuck()
        {
            isDucking = false;
            
            // Toggle colliders back
            if (duckCollider != null) duckCollider.enabled = false;
            if (standingCollider != null) standingCollider.enabled = true;
            
            if (animator != null)
            {
                animator.SetBool("isDucking", false); // Match the Animator parameter name!
            }
        }
        
        private void CheckMagnetCollection()
        {
            if (powerupEffects == null || powerupEffects.MagnetRadius <= 0) return;
            
            Collider2D[] collectibles = Physics2D.OverlapCircleAll(transform.position, powerupEffects.MagnetRadius);
            
            foreach (var col in collectibles)
            {
                if (col.CompareTag("Gemstone") || col.CompareTag("Powerup"))
                {
                    col.transform.position = Vector2.MoveTowards(col.transform.position, transform.position, 15f * Time.deltaTime);
                }
            }
        }
        
        private void UpdateAnimator()
        {
            // Animation is handled in StartDuck/StopDuck now
        }

        private void CheckGroundStatus()
        {
            bool wasGrounded = isGrounded;
            
            if (groundCheck != null)
            {
                isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
            }
            else
            {
                // Fallback: check if velocity is near zero and player is low
                isGrounded = Mathf.Abs(rb.linearVelocity.y) < 0.5f && transform.position.y < -2f;
            }
            
            // Reset double jump flag when landing
            if (isGrounded && !wasGrounded)
            {
                hasUsedDoubleJump = false;
            }
        }

        private void Jump(bool isDoubleJump)
        {
            // Use different jump force for double jump
            float currentJumpForce = isDoubleJump ? doubleJumpForce : jumpForce;
            
            // Reset vertical velocity and apply jump force
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
            rb.AddForce(Vector2.up * currentJumpForce, ForceMode2D.Impulse);
            
            // Play jump sound through AudioManager
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayJumpSound();
            }
        }

        private void UpdateInvincibility()
        {
        }

        public void TakeDamage(int amount = 1)
        {
            if (powerupEffects != null && powerupEffects.TryBlockDamage())
            {
                // Play hit sound through AudioManager
                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlayHitSound();
                }
                return;
            }
            
            if (healthManager != null)
            {
                healthManager.TakeDamage(amount);
                
                if (hitSound != null && audioSource != null)
                {
                    audioSource.PlayOneShot(hitSound);
                }
                
                // Start damage flash effect
                if (!isFlashing)
                {
                    StartCoroutine(DamageFlash());
                }
                
                if (healthManager.CurrentHealth <= 0)
                {
                    Die();
                }
            }
        }
        
        private IEnumerator DamageFlash()
        {
            isFlashing = true;
            int flashCount = 6; // Number of flashes
            float flashDuration = 0.1f; // Duration of each flash
            
            for (int i = 0; i < flashCount; i++)
            {
                // Make sprite semi-transparent (flash effect)
                if (spriteRenderer != null)
                {
                    Color color = spriteRenderer.color;
                    color.a = 0.3f; // 30% opacity
                    spriteRenderer.color = color;
                }
                
                yield return new WaitForSeconds(flashDuration);
                
                // Restore full opacity
                if (spriteRenderer != null)
                {
                    Color color = spriteRenderer.color;
                    color.a = 1f; // 100% opacity
                    spriteRenderer.color = color;
                }
                
                yield return new WaitForSeconds(flashDuration);
            }
            
            isFlashing = false;
        }

        public void Heal(int amount = 1)
        {
            if (healthManager != null)
            {
                healthManager.Heal(amount);
            }
        }

        public void SetInvincible(bool invincible)
        {
        }

        private void Die()
        {
            if (deathSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(deathSound);
            }
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.gameObject.CompareTag("Obstacle"))
            {
                TakeDamage(1);
                
                // Don't let the obstacle push the player - ignore physics collision
                Physics2D.IgnoreCollision(collision.collider, GetComponent<Collider2D>(), true);
                // Re-enable collision after a short delay
                StartCoroutine(ReEnableCollisionAfterDelay(collision.collider, 0.5f));
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Obstacle"))
            {
                TakeDamage(1);
            }
        }
        
        private System.Collections.IEnumerator ReEnableCollisionAfterDelay(Collider2D otherCollider, float delay)
        {
            yield return new WaitForSeconds(delay);
            if (otherCollider != null)
            {
                Physics2D.IgnoreCollision(otherCollider, GetComponent<Collider2D>(), false);
            }
        }

        private void OnDrawGizmosSelected()
        {
            // Draw ground check radius
            if (groundCheck != null)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
            }
        }
    }
}
