using System.Collections;
using System.Collections.Generic;
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
    [SerializeField] private Color wipeColor = new Color(0.79f, 0.66f, 0.51f, 1f);

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
        return SolveExpo(fastDuration, easeDuration);
    }

    public const float MenuInSpeedUp = 1.5f;

    public const float MenuOutSpeedUp = 2f;

    public static float MenuFastDuration(float speedUp)
    {
        return (Instance != null ? Instance.fastDuration : 0.3f) / speedUp;
    }

    public static float MenuEaseDuration(float speedUp)
    {
        return (Instance != null ? Instance.easeDuration : 0.5f) / speedUp;
    }

    public static float CoverProgress(float elapsed, float fast, float ease, float k)
    {
        if (elapsed < fast)
        {
            return FastPart * (elapsed / fast);
        }

        float t = Mathf.Clamp01((elapsed - fast) / ease);
        float eased = (1f - Mathf.Pow(2f, -k * t)) / (1f - Mathf.Pow(2f, -k));
        return FastPart + (1f - FastPart) * eased;
    }

    public static float UncoverProgress(float elapsed, float fast, float ease, float k)
    {
        if (elapsed < ease)
        {
            float t = elapsed / ease;
            float eased = (Mathf.Pow(2f, k * t) - 1f) / (Mathf.Pow(2f, k) - 1f);
            return (1f - FastPart) * eased;
        }

        float f = Mathf.Clamp01((elapsed - ease) / fast);
        return (1f - FastPart) + FastPart * f;
    }

    public static float SolveExpo(float fastDuration, float easeDuration)
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

public class MenuSlideIn
{
    private readonly MonoBehaviour host;
    private readonly Canvas canvas;
    private readonly List<RectTransform> items = new List<RectTransform>();
    private readonly List<Vector2> restPositions = new List<Vector2>();
    private Coroutine running;
    private bool closing;

    public MenuSlideIn(MonoBehaviour host, Transform root)
    {
        this.host = host;
        canvas = root.GetComponentInParent<Canvas>(true);
        Collect(root);
    }

    public bool IsClosing
    {
        get { return closing; }
    }

    private void Collect(Transform parent)
    {
        foreach (Transform child in parent)
        {
            RectTransform rt = child as RectTransform;
            if (rt == null)
            {
                continue;
            }

            bool fullScreen = rt.anchorMin == Vector2.zero && rt.anchorMax == Vector2.one;
            if (fullScreen)
            {
                Collect(rt);
            }
            else
            {
                items.Add(rt);
                restPositions.Add(rt.anchoredPosition);
            }
        }
    }

    public void Play()
    {
        Stop();
        running = host.StartCoroutine(SlideIn());
    }

    public void PlayOut(System.Action onDone)
    {
        if (closing)
        {
            return;
        }

        if (running != null)
        {
            host.StopCoroutine(running);
            running = null;
        }

        closing = true;
        running = host.StartCoroutine(SlideOut(onDone));
    }

    public void Stop()
    {
        if (running != null)
        {
            host.StopCoroutine(running);
            running = null;
        }

        closing = false;
        SetOffset(0f);
    }

    private IEnumerator SlideIn()
    {
        float distance = ScreenHeight();
        float fast = SceneWipeTransition.MenuFastDuration(SceneWipeTransition.MenuInSpeedUp);
        float ease = SceneWipeTransition.MenuEaseDuration(SceneWipeTransition.MenuInSpeedUp);
        float k = SceneWipeTransition.SolveExpo(fast, ease);
        float elapsed = 0f;

        SetOffset(-distance);
        while (elapsed < fast + ease)
        {
            yield return null;
            elapsed += Time.unscaledDeltaTime;
            float progress = SceneWipeTransition.CoverProgress(elapsed, fast, ease, k);
            SetOffset(-distance * (1f - progress));
        }

        SetOffset(0f);
        running = null;
    }

    private IEnumerator SlideOut(System.Action onDone)
    {
        float distance = ScreenHeight();
        float fast = SceneWipeTransition.MenuFastDuration(SceneWipeTransition.MenuOutSpeedUp);
        float ease = SceneWipeTransition.MenuEaseDuration(SceneWipeTransition.MenuOutSpeedUp);
        float k = SceneWipeTransition.SolveExpo(fast, ease);
        float start = CurrentOffset();
        float elapsed = 0f;

        while (elapsed < fast + ease)
        {
            yield return null;
            elapsed += Time.unscaledDeltaTime;
            float progress = SceneWipeTransition.UncoverProgress(elapsed, fast, ease, k);
            SetOffset(Mathf.Lerp(start, -distance, progress));
        }

        SetOffset(-distance);
        running = null;
        closing = false;

        if (onDone != null)
        {
            onDone();
        }
    }

    private float CurrentOffset()
    {
        if (items.Count == 0)
        {
            return 0f;
        }

        return items[0].anchoredPosition.y - restPositions[0].y;
    }

    private float ScreenHeight()
    {
        Canvas.ForceUpdateCanvases();
        if (canvas != null)
        {
            float height = canvas.rootCanvas.GetComponent<RectTransform>().rect.height;
            if (height > 0f)
            {
                return height;
            }
        }

        return 1080f;
    }

    private void SetOffset(float y)
    {
        for (int i = 0; i < items.Count; i++)
        {
            items[i].anchoredPosition = restPositions[i] + new Vector2(0f, y);
        }
    }
}
