using UnityEngine;

namespace ChronoDash.Environment
{
    public class BackgroundScroller : MonoBehaviour
    {
        [Header("Scroll Settings")]
        [SerializeField] private float scrollSpeed = 0.2f;
        [SerializeField] private bool isBackground = false;
        [SerializeField] private float parallaxMultiplier = 0.5f;
        [SerializeField] private bool autoStartScrolling = true;
        
        private SpriteRenderer spriteRenderer;
        private Material material;
        private bool isScrolling = false;
        private float currentOffset = 0f;

        private void Awake()
        {
            InitializeRenderer();
        }

        private void Start()
        {
            if (autoStartScrolling)
            {
                SetScrolling(true);
                Debug.Log($"🚀 BackgroundScroller on {gameObject.name}: AUTO-STARTED scrolling for testing");
            }
        }

        private void Update()
        {
            if (isScrolling && material != null)
            {
                ScrollTexture();
            }
        }
        
        private void OnDestroy()
        {
            CleanupMaterial();
        }
        
        private void InitializeRenderer()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            
            if (spriteRenderer == null)
            {
                Debug.LogError($"BackgroundScroller on {gameObject.name}: No SpriteRenderer found!");
                return;
            }
            
            material = new Material(spriteRenderer.sharedMaterial);
            spriteRenderer.material = material;
            
            if (material.mainTexture != null)
            {
                material.mainTexture.wrapMode = TextureWrapMode.Repeat;
            }
            
        }
        
        private void ScrollTexture()
        {
            float speed = isBackground ? scrollSpeed * parallaxMultiplier : scrollSpeed;
            currentOffset += speed * Time.deltaTime;
            
            material.mainTextureOffset = new Vector2(currentOffset, 0f);
            
            if (Time.frameCount % 60 == 0)
            {
            }
        }
        
        public void SetScrolling(bool enabled)
        {
            isScrolling = enabled;
        }
        
        public void SetScrollSpeed(float speed)
        {
            scrollSpeed = speed;
        }
        
        private void CleanupMaterial()
        {
            if (material != null)
            {
                Destroy(material);
            }
        }
    }
}
