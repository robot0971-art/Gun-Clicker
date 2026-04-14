using DI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UIManager : MonoBehaviour
{
    [Header("Top Bar")]
    [SerializeField] private TMP_Text currentGunText;

    [Header("Gun Display")]
    [SerializeField] private Image gunImage;
    [SerializeField] private SpriteRenderer currentGunSpriteRenderer;
    [SerializeField] private ClickHandler clickHandler;

    [Header("Monster UI")]
    [SerializeField] private GameObject monsterPanel;
    [SerializeField] private TMP_Text monsterNameText;
    [SerializeField] private Slider monsterHPSlider;
    [SerializeField] private TMP_Text monsterHPText;

    [Header("Experience UI")]
    [SerializeField] private Slider expSlider;
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private TMP_Text expText;
    [SerializeField] private int expPerLevelStep = 100;
    [SerializeField] private float expFillAnimationDuration = 0.2f;
    [SerializeField] private float levelUpPauseDuration = 0.1f;

    [Header("Level Up VFX")]
    [SerializeField] private ParticleSystem levelUpParticlePrefab;
    [SerializeField] private Transform levelUpVfxAnchor;
    [SerializeField] private Vector3 levelUpVfxOffset = Vector3.zero;
    [SerializeField] private Vector3 levelUpVfxRotation = Vector3.zero;
    [SerializeField] private float levelUpVfxScale = 1f;
    [SerializeField] private float levelUpVfxLifetime = 2f;

    private GameManager gameManager;
    private GameDataAsset gameDataAsset;
    private CombatManager combatManager;
    private GameData gameData;
    private int currentMonsterMaxHP;
    private int displayedGunLevel;
    private int displayedGunExp;
    private int displayedExpToNextLevel;
    private Coroutine expAnimationCoroutine;

    private void Start()
    {
        gameManager = DIContainer.Resolve<GameManager>();
        gameDataAsset = DIContainer.Resolve<GameDataAsset>();
        combatManager = DIContainer.Resolve<CombatManager>();
        gameData = DIContainer.Resolve<GameData>();
        GunExperienceData.ConfigureExperience(expPerLevelStep);

        if (gameData?.GunExperiences != null)
        {
            for (int i = 0; i < gameData.GunExperiences.Length; i++)
            {
                gameData.GunExperiences[i]?.Normalize();
            }
        }

        EventBus<GameInitializedEvent>.Subscribe(OnGameInitialized);
        EventBus<MonsterSpawnedEvent>.Subscribe(OnMonsterSpawned);
        EventBus<MonsterHitEvent>.Subscribe(OnMonsterHit);
        EventBus<MonsterKilledEvent>.Subscribe(OnMonsterKilled);
        EventBus<GunLevelUpEvent>.Subscribe(OnGunLevelUp);
        EventBus<GunEvolvedEvent>.Subscribe(OnGunEvolved);

        // GameInitialized 이벤트를 놓쳐도 UI가 올바른 현재값으로 시작되도록 즉시 동기화한다.
        if (gameData != null)
        {
            UpdateGunDisplay(gameData.CurrentGunIndex);
            SyncExpDisplayToCurrent();
        }
    }

    private void OnDestroy()
    {
        EventBus<GameInitializedEvent>.Unsubscribe(OnGameInitialized);
        EventBus<MonsterSpawnedEvent>.Unsubscribe(OnMonsterSpawned);
        EventBus<MonsterHitEvent>.Unsubscribe(OnMonsterHit);
        EventBus<MonsterKilledEvent>.Unsubscribe(OnMonsterKilled);
        EventBus<GunLevelUpEvent>.Unsubscribe(OnGunLevelUp);
        EventBus<GunEvolvedEvent>.Unsubscribe(OnGunEvolved);
    }

    private void OnGameInitialized(GameInitializedEvent e)
    {
        UpdateGunDisplay(gameManager.GetCurrentGunIndex());
        SyncExpDisplayToCurrent();
    }

    private void UpdateGunDisplay(int gunId)
    {
        if (gameDataAsset == null || gameDataAsset.guns == null || gunId < 0 || gunId >= gameDataAsset.guns.Count)
        {
            Debug.LogWarning($"[UIManager] Cannot update gun display. Invalid gunId={gunId} or missing GameDataAsset.");
            return;
        }

        var gun = gameDataAsset.guns[gunId];

        if (currentGunText != null)
        {
            currentGunText.text = gun.Name;
        }
        else
        {
            Debug.LogWarning("[UIManager] Current Gun Text is not assigned in the Inspector.");
        }

        if (gunImage != null && gun.GunSprite != null)
        {
            gunImage.sprite = gun.GunSprite;
            gunImage.SetNativeSize();
        }

        if (currentGunSpriteRenderer != null && gun.GunSprite != null)
        {
            currentGunSpriteRenderer.sprite = gun.GunSprite;
        }
    }

    private void OnMonsterSpawned(MonsterSpawnedEvent e)
    {
        currentMonsterMaxHP = e.MaxHP;
        if (monsterNameText != null)
            monsterNameText.text = e.MonsterName;

        UpdateMonsterHP(e.MaxHP, e.MaxHP);
    }

    private void OnMonsterHit(MonsterHitEvent e)
    {
        if (e.MonsterId == -1)
        {
            return;
        }

        UpdateMonsterHP(e.CurrentHP, currentMonsterMaxHP);
    }

    private void OnMonsterKilled(MonsterKilledEvent e)
    {
        AnimateExpDisplayToCurrent();
    }

    private void OnGunLevelUp(GunLevelUpEvent e)
    {
        PlayLevelUpVfx();
        AnimateExpDisplayToCurrent();
    }

    private void OnGunEvolved(GunEvolvedEvent e)
    {
        UpdateGunDisplay(e.NewGunId);
        SyncExpDisplayToCurrent();
    }

    private void UpdateMonsterHP(int currentHP, int maxHP)
    {
        if (monsterHPSlider != null)
        {
            monsterHPSlider.maxValue = maxHP;
            monsterHPSlider.value = currentHP;
        }

        if (monsterHPText != null)
            monsterHPText.text = $"{currentHP}/{maxHP}";
    }

    private void AnimateExpDisplayToCurrent()
    {
        if (expAnimationCoroutine != null)
        {
            StopCoroutine(expAnimationCoroutine);
        }

        expAnimationCoroutine = StartCoroutine(AnimateExpDisplayCoroutine());
    }

    private IEnumerator AnimateExpDisplayCoroutine()
    {
        var gunExp = gameData.GunExperiences[gameData.CurrentGunIndex];
        int targetLevel = gunExp.Level;
        int targetExp = gunExp.CurrentExp;
        int targetExpToNext = gunExp.ExpToNextLevel;

        while (displayedGunLevel < targetLevel)
        {
            int fillTarget = displayedExpToNextLevel > 0 ? displayedExpToNextLevel : 1;
            yield return AnimateSlider(displayedGunExp, fillTarget, fillTarget, displayedGunLevel);

            displayedGunLevel++;

            if (displayedGunLevel >= GunExperienceData.MaxGunLevel)
            {
                displayedGunLevel = GunExperienceData.MaxGunLevel;
                displayedGunExp = 0;
                displayedExpToNextLevel = 0;
                ApplyExpDisplay(displayedGunLevel, displayedGunExp, displayedExpToNextLevel);
                expAnimationCoroutine = null;
                yield break;
            }

            displayedGunExp = 0;
            displayedExpToNextLevel = GunExperienceData.CalculateRequiredExpForLevel(displayedGunLevel + 1);
            ApplyExpDisplay(displayedGunLevel, displayedGunExp, displayedExpToNextLevel);
            yield return new WaitForSeconds(levelUpPauseDuration);
        }

        displayedGunExp = targetExp;
        displayedExpToNextLevel = targetExpToNext;
        yield return AnimateSlider(expSlider != null ? (int)expSlider.value : 0, targetExp, targetExpToNext, targetLevel);
        ApplyExpDisplay(targetLevel, targetExp, targetExpToNext);
        expAnimationCoroutine = null;
    }

    private IEnumerator AnimateSlider(int from, int to, int maxValue, int level)
    {
        if (expSlider == null)
        {
            ApplyExpDisplay(level, to, maxValue);
            yield break;
        }

        float duration = Mathf.Max(0.01f, expFillAnimationDuration);
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            int currentValue = Mathf.RoundToInt(Mathf.Lerp(from, to, t));
            ApplyExpDisplay(level, currentValue, maxValue);
            yield return null;
        }

        ApplyExpDisplay(level, to, maxValue);
    }

    private void SyncExpDisplayToCurrent()
    {
        var gunExp = gameData.GunExperiences[gameData.CurrentGunIndex];
        displayedGunLevel = gunExp.Level;
        displayedGunExp = gunExp.CurrentExp;
        displayedExpToNextLevel = gunExp.ExpToNextLevel;
        ApplyExpDisplay(displayedGunLevel, displayedGunExp, displayedExpToNextLevel);
    }

    private void ApplyExpDisplay(int level, int currentExp, int expToNext)
    {
        bool isMaxLevel = level >= GunExperienceData.MaxGunLevel;

        if (levelText != null)
            levelText.text = $"Lv.{level}";

        if (expSlider != null)
        {
            expSlider.maxValue = isMaxLevel ? 1 : Mathf.Max(1, expToNext);
            expSlider.value = isMaxLevel ? 1 : Mathf.Clamp(currentExp, 0, expSlider.maxValue);
        }

        if (expText != null)
            expText.text = isMaxLevel
                ? "MAX"
                : $"{currentExp}/{expToNext}";
    }

    private void PlayLevelUpVfx()
    {
        if (levelUpParticlePrefab == null)
        {
            return;
        }

        Transform anchor = levelUpVfxAnchor != null ? levelUpVfxAnchor : transform;
        Quaternion rotation = Quaternion.Euler(levelUpVfxRotation);
        ParticleSystem particle = Instantiate(levelUpParticlePrefab, anchor.position + levelUpVfxOffset, rotation);
        particle.transform.localScale = Vector3.one * levelUpVfxScale;
        particle.Play();
        Destroy(particle.gameObject, levelUpVfxLifetime);
    }
}
