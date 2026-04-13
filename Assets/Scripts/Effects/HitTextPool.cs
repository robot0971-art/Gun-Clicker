using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class HitTextPool : MonoBehaviour
{
    [Header("Critical Settings")]
    [SerializeField] private Color criticalColor = new Color(1f, 0.85f, 0f, 1f);
    [SerializeField] private float criticalFontSize = 72f;
    [SerializeField] private string criticalText = "Critical!";
    [SerializeField] private float criticalRiseDistance = 0.5f;
    [SerializeField] private float criticalRiseDuration = 0.5f;
    [SerializeField] private float criticalFadeDuration = 0.3f;

    [Header("Position")]
    [SerializeField] private Vector3 baseOffset = new Vector3(0f, 0.6f, 0f);
    [SerializeField] private Vector2 randomOffset = new Vector2(0.15f, 0.1f);
    [SerializeField] private float canvasScale = 0.01f;

    [Header("Pool")]
    [SerializeField] private TextMeshProUGUI textPrefab;
    [SerializeField] private TextMeshProUGUI criticalTextPrefab;
    [SerializeField] private int poolSize = 8;
    [SerializeField] private int criticalPoolSize = 4;
    [SerializeField] private Transform poolParent;

    [Header("Animation")]
    [SerializeField] private float riseDistance = 0.35f;
    [SerializeField] private float riseDuration = 0.35f;
    [SerializeField] private float fadeDuration = 0.2f;
    [SerializeField] private float fontSize = 48f;

    private readonly Queue<HitTextItem> available = new Queue<HitTextItem>();
    private readonly Queue<HitTextItem> criticalAvailable = new Queue<HitTextItem>();
    private string defaultMessage = "Hit";
    private string defaultCriticalMessage = "Critical!";

    private void Awake()
    {
        EnsureWorldSpaceCanvas();

        if (poolParent == null)
        {
            poolParent = transform;
        }

        InitializePool();
    }

    private void EnsureWorldSpaceCanvas()
    {
        var canvas = GetComponent<Canvas>();
        if (canvas == null)
        {
            canvas = gameObject.AddComponent<Canvas>();
        }

        canvas.renderMode = RenderMode.WorldSpace;
        canvas.worldCamera = Camera.main;
        canvas.sortingOrder = 1000;

        var rectTransform = canvas.GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            rectTransform.sizeDelta = new Vector2(400f, 200f);
        }

        transform.localScale = Vector3.one * canvasScale;
    }

    private void InitializePool()
    {
        if (textPrefab == null)
        {
            Debug.LogError("[HitTextPool] Text prefab is not assigned.");
            return;
        }

        defaultMessage = string.IsNullOrEmpty(textPrefab.text) ? "Hit" : textPrefab.text;

        for (int i = 0; i < poolSize; i++)
        {
            CreateItem(false);
        }

        if (criticalTextPrefab != null)
        {
            defaultCriticalMessage = string.IsNullOrEmpty(criticalTextPrefab.text) ? "Critical!" : criticalTextPrefab.text;
            for (int i = 0; i < criticalPoolSize; i++)
            {
                CreateItem(true);
            }
        }
    }

    private HitTextItem CreateItem(bool isCritical)
    {
        var prefabToUse = isCritical && criticalTextPrefab != null ? criticalTextPrefab : textPrefab;
        var poolToUse = isCritical && criticalTextPrefab != null ? criticalAvailable : available;

        var instance = Instantiate(prefabToUse, poolParent);
        instance.transform.localPosition = Vector3.zero;
        instance.transform.localRotation = Quaternion.identity;
        instance.transform.localScale = Vector3.one;
        instance.gameObject.SetActive(false);

        var item = instance.GetComponent<HitTextItem>();
        if (item == null)
        {
            item = instance.gameObject.AddComponent<HitTextItem>();
        }

        item.Initialize(this);
        poolToUse.Enqueue(item);
        return item;
    }

    public void ShowHitText(Vector3 worldPosition, bool isCritical = false, string message = null)
    {
        if (textPrefab == null)
        {
            Debug.LogError("[HitTextPool] Text prefab is not assigned.");
            return;
        }

        bool useCriticalPrefab = isCritical && criticalTextPrefab != null;
        var poolToUse = useCriticalPrefab ? criticalAvailable : available;
        HitTextItem item = poolToUse.Count > 0 ? poolToUse.Dequeue() : CreateItem(isCritical);

        string displayMessage;
        float displayFontSize;
        float displayRiseDistance;
        float displayRiseDuration;
        float displayFadeDuration;
        Color displayColor;

        if (useCriticalPrefab)
        {
            displayMessage = string.IsNullOrEmpty(message) ? defaultCriticalMessage : message;
            displayFontSize = criticalTextPrefab.fontSize;
            displayRiseDistance = criticalRiseDistance;
            displayRiseDuration = criticalRiseDuration;
            displayFadeDuration = criticalFadeDuration;
            displayColor = criticalTextPrefab.color;
        }
        else if (isCritical)
        {
            displayMessage = criticalText;
            displayFontSize = criticalFontSize;
            displayRiseDistance = criticalRiseDistance;
            displayRiseDuration = criticalRiseDuration;
            displayFadeDuration = criticalFadeDuration;
            displayColor = criticalColor;
        }
        else
        {
            displayMessage = string.IsNullOrEmpty(message) ? defaultMessage : message;
            displayFontSize = fontSize;
            displayRiseDistance = riseDistance;
            displayRiseDuration = riseDuration;
            displayFadeDuration = fadeDuration;
            displayColor = Color.white;
        }

        item.Play(
            displayMessage,
            worldPosition + baseOffset + new Vector3(
                Random.Range(-randomOffset.x, randomOffset.x),
                Random.Range(0f, randomOffset.y),
                0f
            ),
            displayRiseDistance,
            displayRiseDuration,
            displayFadeDuration,
            displayFontSize,
            displayColor,
            isCritical
        );
    }

    public void Return(HitTextItem item, bool critical)
    {
        if (item == null)
        {
            return;
        }

        item.gameObject.SetActive(false);
        var poolToUse = critical && criticalTextPrefab != null ? criticalAvailable : available;
        poolToUse.Enqueue(item);
    }
}

public class HitTextItem : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI textComponent;

    private HitTextPool pool;
    private Coroutine animationRoutine;
    private bool isCritical;

    public void Initialize(HitTextPool hitTextPool)
    {
        pool = hitTextPool;

        if (textComponent == null)
        {
            textComponent = GetComponent<TextMeshProUGUI>();
        }
    }

    public void Play(string message, Vector3 worldPosition, float riseDistance, float riseDuration, float fadeDuration, float fontSize, Color color, bool critical)
    {
        isCritical = critical;

        if (textComponent == null)
        {
            textComponent = GetComponent<TextMeshProUGUI>();
        }

        if (textComponent == null)
        {
            return;
        }

        transform.position = worldPosition;
        textComponent.text = message;
        textComponent.fontSize = fontSize;
        textComponent.enableVertexGradient = false;
        textComponent.color = color;
        textComponent.ForceMeshUpdate(true, true);
        transform.localRotation = Quaternion.identity;
        gameObject.SetActive(true);

        if (animationRoutine != null)
        {
            StopCoroutine(animationRoutine);
        }

        animationRoutine = StartCoroutine(Animate(worldPosition, riseDistance, riseDuration, fadeDuration));
    }

    private IEnumerator Animate(Vector3 startPosition, float riseDistance, float riseDuration, float fadeDuration)
    {
        float elapsed = 0f;
        Color originalColor = textComponent.color;
        Vector3 endPosition = startPosition + new Vector3(0f, riseDistance, 0f);

        while (elapsed < riseDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / riseDuration);
            transform.position = Vector3.Lerp(startPosition, endPosition, t);

            if (t > 1f - fadeDuration / riseDuration)
            {
                float fadeT = Mathf.InverseLerp(1f - fadeDuration / riseDuration, 1f, t);
                textComponent.color = new Color(originalColor.r, originalColor.g, originalColor.b, 1f - fadeT);
            }

            yield return null;
        }

        pool?.Return(this, isCritical);
    }
}
