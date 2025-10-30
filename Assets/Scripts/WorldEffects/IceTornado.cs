using UnityEngine;

namespace ChronoDash.WorldEffects
{
    public class IceTornado : MonoBehaviour
    {
        private Animator animator;
        private BoxCollider2D damageCollider;
        private bool canDamage = false;
        private bool hasHit = false;
        
        private void Awake()
        {
            animator = GetComponent<Animator>();
            damageCollider = GetComponent<BoxCollider2D>();
            
            if (damageCollider == null)
            {
                damageCollider = gameObject.AddComponent<BoxCollider2D>();
                damageCollider.size = new Vector2(2f, 3f);
            }
            damageCollider.isTrigger = true;
            
            var rb = GetComponent<Rigidbody2D>();
            if (rb == null)
            {
                rb = gameObject.AddComponent<Rigidbody2D>();
            }
            rb.bodyType = RigidbodyType2D.Static;
        }
        
        private void Start()
        {
            Invoke(nameof(EnableDamage), 0.5f);
            Destroy(gameObject, 2f);
        }
        
        private void EnableDamage()
        {
            canDamage = true;
        }
        
        public void OnAnimationComplete()
        {
            Destroy(gameObject);
        }
        
        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (!canDamage || hasHit) return;
            
            if (collision.CompareTag("Player"))
            {
                hasHit = true;
                var player = collision.GetComponent<ChronoDash.Player.PlayerController>();
                if (player != null && !player.IsInvincible)
                {
                    player.TakeDamage(1);
                }
            }
        }
    }
}
