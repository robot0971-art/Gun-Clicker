using DI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("Top Bar")]
    [SerializeField] private TMP_Text currentGunText;

    [Header("Gun Display")]
    [SerializeField] private Image gunImage;
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

    private GameManager gameManager;
    private GameDataAsset gameDataAsset;
    private CombatManager combatManager;
    private GameData gameData;
    private int currentMonsterMaxHP;

    private void Start()
    {
        gameManager = DIContainer.Resolve<GameManager>();
        gameDataAsset = DIContainer.Resolve<GameDataAsset>();
        combatManager = DIContainer.Resolve<CombatManager>();
        gameData = DIContainer.Resolve<GameData>();

        EventBus<GameInitializedEvent>.Subscribe(OnGameInitialized);
        EventBus<MonsterSpawnedEvent>.Subscribe(OnMonsterSpawned);
        EventBus<MonsterHitEvent>.Subscribe(OnMonsterHit);
        EventBus<MonsterKilledEvent>.Subscribe(OnMonsterKilled);
        EventBus<GunLevelUpEvent>.Subscribe(OnGunLevelUp);
        EventBus<GunEvolvedEvent>.Subscribe(OnGunEvolved);
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
        UpdateExpDisplay();
    }

    private void UpdateGunDisplay(int gunId)
    {
        var gun = gameDataAsset.guns[gunId];
        currentGunText.text = gun.Name;
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
        UpdateExpDisplay();
    }

    private void OnGunLevelUp(GunLevelUpEvent e)
    {
        UpdateExpDisplay();
    }

    private void OnGunEvolved(GunEvolvedEvent e)
    {
        UpdateGunDisplay(e.NewGunId);
        UpdateExpDisplay();
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

    private void UpdateExpDisplay()
    {
        var gunExp = gameData.GunExperiences[gameData.CurrentGunIndex];

        if (levelText != null)
            levelText.text = $"Lv.{gunExp.Level}";

        if (expSlider != null)
        {
            expSlider.maxValue = gunExp.ExpToNextLevel;
            expSlider.value = gunExp.CurrentExp;
        }

        if (expText != null)
            expText.text = $"{gunExp.CurrentExp}/{gunExp.ExpToNextLevel}";
    }
}
