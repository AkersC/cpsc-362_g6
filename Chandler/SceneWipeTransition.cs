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

    public static bool IsBusy
    {
        get { return Instance != null && Instance.isTransitioning; }
    }

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
