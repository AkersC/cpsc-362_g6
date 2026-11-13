using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneWipeTransition : MonoBehaviour
{
    public static SceneWipeTransition Instance;

    [SerializeField] private float fastDuration = 0.3f;
    [SerializeField] private float easeDuration = 0.5f;
    [SerializeField] private float holdDuration = 0.5f;
    [SerializeField] private float patternScale = 4f;
    [SerializeField] private Texture2D patternTexture;
    [SerializeField] private Color wipeColor =
        new Color(0.79f, 0.66f, 0.51f, 1f);

    private const float FastPart = 2f / 3f;

    private RectTransform wipeRect;
    private RawImage wipeImage;
    private Texture2D pattern;
    private bool isTransitioning;
    private float expo;

    public static void Load(string sceneName)
    {
        if (Instance == null)
        {
            new GameObject("SceneTransition").AddComponent<SceneWipeTransition>();
        }

        Instance.TransitionTo(sceneName);
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        transform.SetParent(null);
        DontDestroyOnLoad(gameObject);
        CreateWipe();
        expo = SolveExpo();
    }

    private void CreateWipe()
    {
        GameObject canvasObject = new GameObject(
            "TransitionCanvas",
            typeof(Canvas),
            typeof(CanvasScaler),
            typeof(GraphicRaycaster)
        );
        canvasObject.transform.SetParent(transform);

        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 1000;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        GameObject wipeObject = new GameObject(
            "PatternWipe",
            typeof(RectTransform),
            typeof(RawImage)
        );
        wipeObject.transform.SetParent(canvasObject.transform, false);

        wipeRect = wipeObject.GetComponent<RectTransform>();
        wipeRect.pivot = new Vector2(0.5f, 0.5f);
        wipeRect.offsetMin = Vector2.zero;
        wipeRect.offsetMax = Vector2.zero;

        wipeImage = wipeObject.GetComponent<RawImage>();
        wipeImage.raycastTarget = true;

        pattern = patternTexture;
        if (pattern != null)
        {
            pattern.wrapMode = TextureWrapMode.Repeat;
            pattern.filterMode = FilterMode.Point;
            wipeImage.texture = pattern;
            wipeImage.color = Color.white;
        }
        else
        {
            Debug.LogWarning("SceneWipeTransition: no Pattern Texture assigned, using a plain color.");
            wipeImage.color = wipeColor;
        }

        SetCover(0f);
    }

    public void TransitionTo(string sceneName)
    {
        if (!isTransitioning)
        {
            StartCoroutine(WipeScene(sceneName));
        }
    }

    private IEnumerator WipeScene(string sceneName)
    {
        isTransitioning = true;
        Time.timeScale = 1f;

        yield return Animate(fastDuration, t => FastPart * t);
        yield return Animate(easeDuration, t => FastPart + (1f - FastPart) * EaseOutExpo(t));
        SetCover(1f);

        float covered = 0f;
        AsyncOperation load = SceneManager.LoadSceneAsync(sceneName);
        while (!load.isDone || covered < holdDuration)
        {
            covered += Time.unscaledDeltaTime;
            yield return null;
        }

        Time.timeScale = 1f;

        yield return Animate(easeDuration, t => 1f - (1f - FastPart) * EaseInExpo(t));
        yield return Animate(fastDuration, t => FastPart * (1f - t));
        SetCover(0f);

        isTransitioning = false;
    }

    private IEnumerator Animate(float duration, System.Func<float, float> curve)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            SetCover(curve(Mathf.Clamp01(elapsed / duration)));
            yield return null;
        }
    }

    private void SetCover(float amount)
    {
        wipeRect.anchorMin = new Vector2(0f, 1f - amount);
        wipeRect.anchorMax = new Vector2(1f, 2f - amount);
        wipeImage.enabled = amount > 0f;

        if (pattern != null)
        {
            Rect r = wipeRect.rect;
            wipeImage.uvRect = new Rect(
                0f, 0f,
                r.width / (pattern.width * patternScale),
                r.height / (pattern.height * patternScale)
            );
        }
    }

    private float EaseOutExpo(float t)
    {
        return (1f - Mathf.Pow(2f, -expo * t)) / (1f - Mathf.Pow(2f, -expo));
    }

    private float EaseInExpo(float t)
    {
        return (Mathf.Pow(2f, expo * t) - 1f) / (Mathf.Pow(2f, expo) - 1f);
    }

    private float SolveExpo()
    {
        float fastSpeed = FastPart / Mathf.Max(fastDuration, 0.01f);
        float target = fastSpeed * easeDuration / (1f - FastPart);

        float lo = 0.01f, hi = 30f;
        for (int i = 0; i < 40; i++)
        {
            float k = (lo + hi) * 0.5f;
            float edgeSpeed = k * Mathf.Log(2f) / (1f - Mathf.Pow(2f, -k));
            if (edgeSpeed < target) lo = k; else hi = k;
        }
        return (lo + hi) * 0.5f;
    }
}
