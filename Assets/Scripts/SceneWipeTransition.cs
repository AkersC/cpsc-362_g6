
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Drops a patterned curtain from the top of the screen, loads the next
// scene while the screen is covered, then lifts the curtain back up.
//
// Cover:   fast for the first 2/3 of the screen, then eases out
//          (exponential slow-down) over the last 1/3.
// Hold:    stays fully covered for holdDuration (and until the scene has loaded).
// Uncover: eases in (exponential slow start) over the first 1/3,
//          then moves fast for the remaining 2/3.
public class SceneWipeTransition : MonoBehaviour
{
    public static SceneWipeTransition Instance;

    [SerializeField] private float fastDuration = 0.3f;   // time for the fast 2/3 part
    [SerializeField] private float easeDuration = 0.5f;   // time for the slow 1/3 part
    [SerializeField] private float holdDuration = 0.5f;   // time fully covered
    [SerializeField] private float patternScale = 4f;     // pattern pixels drawn at 4x
    [SerializeField] private Texture2D patternTexture;     // drag Assets/Art/UI/TransitionPattern.png here
    [SerializeField] private Color wipeColor =
        new Color(0.79f, 0.66f, 0.51f, 1f);               // used only if the pattern is missing

    private const float FastPart = 2f / 3f;

    private RectTransform wipeRect;
    private RawImage wipeImage;
    private Texture2D pattern;
    private bool isTransitioning;
    private float expo; // strength of the exponential curve, matched to the fast speed

    // Call this from anywhere (menus, endpoints). Uses the SceneTransition object in the scene;
    // if there is none, it creates a plain-color one so the game still works.
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
        transform.SetParent(null); // DontDestroyOnLoad only works on root objects
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
        wipeImage.raycastTarget = true; // blocks clicks while it is on screen

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

        // 1. Drop down: fast 2/3, then exponential slow-down for the last 1/3.
        yield return Animate(fastDuration, t => FastPart * t);
        yield return Animate(easeDuration, t => FastPart + (1f - FastPart) * EaseOutExpo(t));
        SetCover(1f);

        // 2. Load the next scene while covered and hold for at least holdDuration.
        float covered = 0f;
        AsyncOperation load = SceneManager.LoadSceneAsync(sceneName);
        while (!load.isDone || covered < holdDuration)
        {
            covered += Time.unscaledDeltaTime;
            yield return null;
        }

        Time.timeScale = 1f;

        // 3. Lift up: exponential slow start for the first 1/3, then fast 2/3.
        yield return Animate(easeDuration, t => 1f - (1f - FastPart) * EaseInExpo(t));
        yield return Animate(fastDuration, t => FastPart * (1f - t));
        SetCover(0f);

        isTransitioning = false;
    }

    // Runs curve(t) for t = 0..1 over the given time, using unscaled time so it works while paused.
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

    // amount: 0 = hidden above the screen, 1 = covering the whole screen.
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

    // Picks the curve strength so the slow part starts (or ends) at exactly the
    // same speed as the fast part, so there is no visible jolt between them.
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
