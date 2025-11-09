using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace ChronoDash.Effects {
    public class DarknessEffect : MonoBehaviour {
        [Header("Effect Settings")]
        [SerializeField] private float effectDuration = 10f;
        [SerializeField] private float spotlightRadius = 250f;
        [SerializeField] private float edgeSoftness = 50f;
        [SerializeField] private float fadeInDuration = 1.0f;
        [SerializeField] private float fadeOutDuration = 1.0f;

        private GameObject overlayObj;
        private Canvas overlayCanvas;
        private Image overlayImage;
        private Material darknessMaterial;
        private Transform playerTransform;
        private Camera mainCamera;
        private bool isActive = false;
        private float currentOpacity = 0f;
        private bool isFadingIn = false;
        private bool isFadingOut = false;

        public bool IsActive => isActive;

        private void Awake() {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) playerTransform = player.transform;
            mainCamera = Camera.main;
            overlayCanvas = FindCanvasForOverlay();
        }

        public IEnumerator TriggerEffect(float warningTime) {
            if (isActive || playerTransform == null) yield break;
            isActive = true;
            CreateOverlay();
            yield return StartCoroutine(FadeIn());
            yield return new WaitForSeconds(effectDuration);
            yield return StartCoroutine(FadeOut());
            DestroyOverlay();
            isActive = false;
        }

        public void ForceStopEffect() {
            StopAllCoroutines();
            DestroyOverlay();
            isActive = false;
        }

        private void CreateOverlay() {
            if (overlayCanvas == null) return;
            overlayObj = new GameObject("DarknessOverlay");
            overlayObj.transform.SetParent(overlayCanvas.transform, false);
            RectTransform rectTransform = overlayObj.AddComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
            overlayImage = overlayObj.AddComponent<Image>();
            overlayImage.raycastTarget = false;
            Shader shader = Shader.Find("Custom/SpotlightDarkness");
            if (shader != null) {
                darknessMaterial = new Material(shader);
                overlayImage.material = darknessMaterial;
                Texture2D whiteTexture = new Texture2D(1, 1);
                whiteTexture.SetPixel(0, 0, Color.white);
                whiteTexture.Apply();
                overlayImage.sprite = Sprite.Create(whiteTexture, new Rect(0, 0, 1, 1), Vector2.zero);
            } else {
                Debug.LogError("[DarknessEffect] Shader not found!");
            }
        }

        private IEnumerator FadeIn() {
            float t = 0f;
            isFadingIn = true;
            while (t < fadeInDuration) {
                t += Time.deltaTime;
                currentOpacity = Mathf.Lerp(0f, 1f, t / fadeInDuration);
                yield return null;
            }
            currentOpacity = 1f;
            isFadingIn = false;
        }

        private IEnumerator FadeOut() {
            float t = 0f;
            isFadingOut = true;
            while (t < fadeOutDuration) {
                t += Time.deltaTime;
                currentOpacity = Mathf.Lerp(1f, 0f, t / fadeOutDuration);
                yield return null;
            }
            currentOpacity = 0f;
            isFadingOut = false;
        }

        private void Update() {
            if (!isActive || overlayImage == null || darknessMaterial == null || playerTransform == null || mainCamera == null) return;
            Vector3 playerWorldPos = playerTransform.position;
            Vector3 screenPoint = mainCamera.WorldToScreenPoint(playerWorldPos);
            darknessMaterial.SetVector("_SpotlightCenter", new Vector4(screenPoint.x, screenPoint.y, 0, 0));
            darknessMaterial.SetFloat("_SpotlightRadius", spotlightRadius);
            darknessMaterial.SetFloat("_EdgeSoftness", edgeSoftness);
            darknessMaterial.SetFloat("_Opacity", currentOpacity);
        }

        private void DestroyOverlay() {
            if (darknessMaterial != null) {
                Destroy(darknessMaterial);
                darknessMaterial = null;
            }
            if (overlayObj != null) {
                Destroy(overlayObj);
                overlayObj = null;
                overlayImage = null;
            }
        }

        private Canvas FindCanvasForOverlay() {
            Canvas[] canvases = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
            foreach (Canvas canvas in canvases) {
                if (canvas.renderMode == RenderMode.ScreenSpaceOverlay) {
                    return canvas;
                }
            }
            GameObject canvasObj = new GameObject("DarknessCanvas");
            Canvas newCanvas = canvasObj.AddComponent<Canvas>();
            newCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            newCanvas.sortingOrder = 1000;
            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280, 720);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            canvasObj.AddComponent<GraphicRaycaster>();
            return newCanvas;
        }
    }
}
