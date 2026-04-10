using DI;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// UI 전체 관리, 이벤트 수신하여 UI 업데이트
/// </summary>
public class UIManager : MonoBehaviour
{
    [Header("Top Bar")]
    [SerializeField] private TMP_Text moneyText;
    [SerializeField] private TMP_Text currentGunText;
    
    [Header("Gun Display")]
    [SerializeField] private Image gunImage;
    [SerializeField] private ClickHandler clickHandler;
    
    [Header("Tabs")]
    [SerializeField] private Button shopTabButton;
    [SerializeField] private Button collectionTabButton;
    [SerializeField] private GameObject shopPanel;
    [SerializeField] private GameObject collectionPanel;
    
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
    
    private void Start()
    {
        // DI 주입
        gameManager = DIContainer.Resolve<GameManager>();
        gameDataAsset = DIContainer.Resolve<GameDataAsset>();
        combatManager = DIContainer.Resolve<CombatManager>();
        gameData = DIContainer.Resolve<GameData>();
        
        // 이벤트 구독
        EventBus<MoneyChangedEvent>.Subscribe(OnMoneyChanged);
        EventBus<GunSwitchedEvent>.Subscribe(OnGunSwitched);
        EventBus<GunUnlockedEvent>.Subscribe(OnGunUnlocked);
        EventBus<GameInitializedEvent>.Subscribe(OnGameInitialized);
        
        // 전투 이벤트 구독
        EventBus<MonsterSpawnedEvent>.Subscribe(OnMonsterSpawned);
        EventBus<MonsterHitEvent>.Subscribe(OnMonsterHit);
        EventBus<MonsterKilledEvent>.Subscribe(OnMonsterKilled);
        EventBus<GunLevelUpEvent>.Subscribe(OnGunLevelUp);
        EventBus<GunEvolvedEvent>.Subscribe(OnGunEvolved);
        
        // 버튼 이벤트
        shopTabButton.onClick.AddListener(() => SwitchTab("shop"));
        collectionTabButton.onClick.AddListener(() => SwitchTab("collection"));
        
        // 기본 탭
        SwitchTab("shop");
    }
    
    private void OnDestroy()
    {
        EventBus<MoneyChangedEvent>.Unsubscribe(OnMoneyChanged);
        EventBus<GunSwitchedEvent>.Unsubscribe(OnGunSwitched);
        EventBus<GunUnlockedEvent>.Unsubscribe(OnGunUnlocked);
        EventBus<GameInitializedEvent>.Unsubscribe(OnGameInitialized);
        
        EventBus<MonsterSpawnedEvent>.Unsubscribe(OnMonsterSpawned);
        EventBus<MonsterHitEvent>.Unsubscribe(OnMonsterHit);
        EventBus<MonsterKilledEvent>.Unsubscribe(OnMonsterKilled);
        EventBus<GunLevelUpEvent>.Unsubscribe(OnGunLevelUp);
        EventBus<GunEvolvedEvent>.Unsubscribe(OnGunEvolved);
    }
    
    private void OnGameInitialized(GameInitializedEvent e)
    {
        UpdateMoneyDisplay(gameManager.GetTotalGold());
        UpdateGunDisplay(gameManager.GetCurrentGunIndex());
        UpdateExpDisplay();
    }
    
    private void OnMoneyChanged(MoneyChangedEvent e)
    {
        UpdateMoneyDisplay(e.Amount);
    }
    
    private void OnGunSwitched(GunSwitchedEvent e)
    {
        UpdateGunDisplay(e.GunId);
        UpdateExpDisplay();
    }
    
    private void OnGunUnlocked(GunUnlockedEvent e)
    {
        // 해금 이펙트는 별도 Effect 클래스에서 처리
        Debug.Log($"[UIManager] Gun unlocked: {gameDataAsset.guns[e.GunId].Name}");
    }
    
    private void UpdateMoneyDisplay(long amount)
    {
        moneyText.text = $"${FormatNumber(amount)}";
    }
    
    private void UpdateGunDisplay(int gunId)
    {
        var gun = gameDataAsset.guns[gunId];
        currentGunText.text = gun.Name;
        
        // 스프라이트 로드 (Resources 폴더에서)
        // var sprite = Resources.Load<Sprite>($"Guns/{gun.SpriteName}");
        // if (sprite != null) gunImage.sprite = sprite;
    }
    
    private void SwitchTab(string tabName)
    {
        shopPanel.SetActive(tabName == "shop");
        collectionPanel.SetActive(tabName == "collection");
    }
    
    // 전투 UI 업데이트
    private void OnMonsterSpawned(MonsterSpawnedEvent e)
    {
        if (monsterNameText != null)
            monsterNameText.text = e.MonsterName;
        
        UpdateMonsterHP(e.MaxHP, e.MaxHP);
    }
    
    private void OnMonsterHit(MonsterHitEvent e)
    {
        int maxHP = combatManager.GetCurrentMonsterMaxHP();
        UpdateMonsterHP(e.CurrentHP, maxHP);
        
        if (e.IsCritical)
        {
            Debug.Log($"[UIManager] CRITICAL! Damage: {e.Damage}");
        }
    }
    
    private void OnMonsterKilled(MonsterKilledEvent e)
    {
        Debug.Log($"[UIManager] Monster killed! +{e.ExpReward} EXP, +{e.GoldReward} Gold");
        UpdateExpDisplay();
    }
    
    private void OnGunLevelUp(GunLevelUpEvent e)
    {
        Debug.Log($"[UIManager] Level Up! Gun {e.GunId} is now Lv.{e.NewLevel}");
        UpdateExpDisplay();
    }
    
    private void OnGunEvolved(GunEvolvedEvent e)
    {
        Debug.Log($"[UIManager] EVOLUTION! {gameDataAsset.guns[e.PreviousGunId].Name} -> {e.NewGunName}");
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
    
    private string FormatNumber(long num)
    {
        if (num >= 1000000) return $"{num / 1000000f:F1}M";
        if (num >= 1000) return $"{num / 1000f:F1}K";
        return num.ToString();
    }
}