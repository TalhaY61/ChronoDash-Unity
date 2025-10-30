using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace ChronoDash.WorldEffects
{
    /// <summary>
    /// Gravity Flip Effect - Rotates entire world 180° (Jungle World)
    /// Player, background, ground, obstacles all flip upside down
    /// Duration: 10 seconds, then returns to normal
    /// </summary>
    public class GravityFlipEffect : MonoBehaviour
    {
        [Header("Gravity Flip Settings")]
        [SerializeField] private float warningDuration = 1.5f;
        [SerializeField] private float flipDuration = 10f; // Lasts 10 seconds
        [SerializeField] private float rotationSpeed = 2f; // Speed of rotation animation
        
        [Header("Game Objects to Rotate")]
        [SerializeField] private Transform playerTransform;
        [SerializeField] private Transform[] backgroundTransforms;
        [SerializeField] private Transform[] groundTransforms;
        
        [Header("Audio")]
        [SerializeField] private AudioClip warningSound;
        [SerializeField] private AudioClip flipSound;
        
        private bool isWarning = false;
        private bool isFlipped = false;
        private AudioSource audioSource;
        private Coroutine currentFlipCoroutine;
        private List<Transform> allObjectsToRotate = new List<Transform>();
        
        public bool IsFlipped => isFlipped;
        public bool IsWarning => isWarning;
        
        public System.Action<bool> OnGravityFlipped;
        
        private void Awake()
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
            
            if (playerTransform == null)
            {
                GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
                if (playerObj != null)
                {
                    playerTransform = playerObj.transform;
                }
            }
        }
        
        public void TriggerGravityFlip()
        {
            Debug.Log($"🌲 TriggerGravityFlip called! isWarning={isWarning}, isFlipped={isFlipped}");
            
            if (isWarning || isFlipped)
            {
                Debug.LogWarning("🌲 Cannot trigger gravity flip - already in progress!");
                return;
            }
            
            if (currentFlipCoroutine != null)
            {
                Debug.Log("🌲 Stopping previous coroutine");
                StopCoroutine(currentFlipCoroutine);
            }
            
            Debug.Log("🌲 Starting gravity flip sequence!");
            currentFlipCoroutine = StartCoroutine(GravityFlipSequence());
        }
        
        private IEnumerator GravityFlipSequence()
        {
            Debug.Log("🌲 === GRAVITY FLIP SEQUENCE STARTED ===");
            
            // 1. Show warning
            Debug.Log("🌲 Step 1: Showing warning...");
            yield return StartCoroutine(ShowWarning());
            
            // 2. Flip the world
            Debug.Log("🌲 Step 2: Flipping world...");
            yield return StartCoroutine(FlipWorld());
            
            // 3. Wait for flip duration
            Debug.Log($"🌲 Step 3: Waiting {flipDuration}s...");
            yield return new WaitForSeconds(flipDuration);
            
            // 4. Restore to normal
            Debug.Log("🌲 Step 4: Restoring world...");
            yield return StartCoroutine(RestoreWorld());
            
            Debug.Log("🌲 === GRAVITY FLIP SEQUENCE COMPLETE ===");
        }
        
        private IEnumerator ShowWarning()
        {
            isWarning = true;
            float timer = warningDuration;
            
            if (warningSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(warningSound);
            }
            
            Debug.Log("⚠️ GRAVITY FLIP WARNING!");
            
            while (timer > 0f)
            {
                timer -= Time.deltaTime;
                yield return null;
            }
            
            isWarning = false;
        }
        
        private IEnumerator FlipWorld()
        {
            isFlipped = true;
            
            // Gather all objects to rotate
            GatherObjectsToRotate();
            
            if (flipSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(flipSound);
            }
            
            // Rotate everything from 0° to 180° smoothly
            float elapsed = 0f;
            float transitionTime = 1f / rotationSpeed;
            
            while (elapsed < transitionTime)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / transitionTime;
                
                foreach (Transform tr in allObjectsToRotate)
                {
                    if (tr != null)
                    {
                        float targetRotation = Mathf.Lerp(0f, 180f, t);
                        tr.rotation = Quaternion.Euler(0f, 0f, targetRotation);
                    }
                }
                
                yield return null;
            }
            
            // Ensure exactly 180°
            foreach (Transform tr in allObjectsToRotate)
            {
                if (tr != null)
                {
                    tr.rotation = Quaternion.Euler(0f, 0f, 180f);
                }
            }
            
            OnGravityFlipped?.Invoke(true);
            Debug.Log("🔄 WORLD FLIPPED 180°! Everything is upside down!");
        }
        
        private IEnumerator RestoreWorld()
        {
            // Re-gather objects (in case new obstacles spawned)
            GatherObjectsToRotate();
            
            // Rotate everything from 180° back to 0° smoothly
            float elapsed = 0f;
            float transitionTime = 1f / rotationSpeed;
            
            while (elapsed < transitionTime)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / transitionTime;
                
                foreach (Transform tr in allObjectsToRotate)
                {
                    if (tr != null)
                    {
                        float targetRotation = Mathf.Lerp(180f, 0f, t);
                        tr.rotation = Quaternion.Euler(0f, 0f, targetRotation);
                    }
                }
                
                yield return null;
            }
            
            // Ensure exactly 0°
            foreach (Transform tr in allObjectsToRotate)
            {
                if (tr != null)
                {
                    tr.rotation = Quaternion.identity;
                }
            }
            
            isFlipped = false;
            OnGravityFlipped?.Invoke(false);
            Debug.Log("✅ World RESTORED to normal!");
        }
        
        private void GatherObjectsToRotate()
        {
            allObjectsToRotate.Clear();
            
            // Add player
            if (playerTransform != null)
            {
                allObjectsToRotate.Add(playerTransform);
            }
            
            // Add backgrounds
            if (backgroundTransforms != null)
            {
                foreach (Transform bg in backgroundTransforms)
                {
                    if (bg != null)
                    {
                        allObjectsToRotate.Add(bg);
                    }
                }
            }
            
            // Add grounds
            if (groundTransforms != null)
            {
                foreach (Transform ground in groundTransforms)
                {
                    if (ground != null)
                    {
                        allObjectsToRotate.Add(ground);
                    }
                }
            }
            
            // Add all active obstacles
            GameObject[] obstacles = GameObject.FindGameObjectsWithTag("Obstacle");
            foreach (GameObject obs in obstacles)
            {
                if (obs != null && !allObjectsToRotate.Contains(obs.transform))
                {
                    allObjectsToRotate.Add(obs.transform);
                }
            }
        }
        
        public void ForceRestoreGravity()
        {
            if (currentFlipCoroutine != null)
            {
                StopCoroutine(currentFlipCoroutine);
            }
            
            StopAllCoroutines();
            
            // Restore all rotations to normal
            GatherObjectsToRotate();
            foreach (Transform tr in allObjectsToRotate)
            {
                if (tr != null)
                {
                    tr.rotation = Quaternion.identity;
                }
            }
            
            isFlipped = false;
            isWarning = false;
        }
    }
}
