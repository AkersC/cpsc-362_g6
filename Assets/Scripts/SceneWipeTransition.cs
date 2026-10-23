
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneWipeTransition : MonoBehaviour
{
    public static SceneWipeTransition Instance;

    [SerializeField] private float wipeDuration = 0.65f;
    [SerializeField] private Color wipeColor =
        new Color(0.79f, 0.66f, 0.51f, 1f);

    private RectTransform wipeRect;
    private Canvas canvas;
    private bool isTransitioning;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        CreateWipe();
    }

    private void CreateWipe()
    {
        GameObject canvasObject = new GameObject(
            "TransitionCanvas",
            typeof(Canvas),
            typeof(CanvasScaler)
        );

        canvasObject.transform.SetParent(transform);

        canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 1000;

        GameObject wipeObject = new GameObject(
            "TanWipe",
            typeof(RectTransform),
            typeof(Image)
        );

        wipeObject.transform.SetParent(
            canvasObject.transform, false
        );

        wipeRect = wipeObject.GetComponent<RectTransform>();

        wipeRect.anchorMin = new Vector2(0, 0);
        wipeRect.anchorMax = new Vector2(1, 1);
        wipeRect.pivot = new Vector2(0, 0.5f);
        wipeRect.offsetMin = Vector2.zero;
        wipeRect.offsetMax = Vector2.zero;

        Image image = wipeObject.GetComponent<Image>();
        image.color = wipeColor;

        RectTransform canvasRect =
            canvasObject.GetComponent<RectTransform>();

        float width = canvasRect.rect.width;

        wipeRect.anchoredPosition = new Vector2(width, 0);
    }

    public void TransitionTo(string sceneName)
    {
        if (!isTransitioning)
            StartCoroutine(WipeScene(sceneName));
    }

    
private IEnumerator WipeScene(string sceneName)
{
    isTransitioning = true;
    Time.timeScale = 1f;

    float width = canvas.GetComponent<RectTransform>()
        .rect.width;

    float elapsed = 0f;
    float totalDuration = wipeDuration * 2f;

 
    while (elapsed < wipeDuration)
    {
        elapsed += Time.unscaledDeltaTime;

        float progress = Mathf.Clamp01(elapsed / totalDuration);

        float x = Mathf.Lerp(width, -width, progress);

        wipeRect.anchoredPosition = new Vector2(x, 0);

        yield return null;
    }


    wipeRect.anchoredPosition = Vector2.zero;

    yield return SceneManager.LoadSceneAsync(sceneName);


    while (elapsed < totalDuration)
    {
        elapsed += Time.unscaledDeltaTime;

        float progress = Mathf.Clamp01(elapsed / totalDuration);

        float x = Mathf.Lerp(width, -width, progress);

        wipeRect.anchoredPosition = new Vector2(x, 0);

        yield return null;
    }

    wipeRect.anchoredPosition = new Vector2(width, 0);

    isTransitioning = false;
}

}
