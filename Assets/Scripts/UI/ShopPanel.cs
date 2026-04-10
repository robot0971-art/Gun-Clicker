using DI;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 업그레이드 상점 UI
/// </summary>
public class ShopPanel : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private TMP_Text upgradeLevelText;
    [SerializeField] private TMP_Text costText;
    [SerializeField] private TMP_Text currentValueText;
    [SerializeField] private Button buyButton;
    
    private GameManager gameManager;
    private GameDataAsset gameDataAsset;
    
    private int currentGunId;
    
    private void Start()
    {
        // DI 주입
        gameManager = DIContainer.Resolve<GameManager>();
        gameDataAsset = DIContainer.Resolve<GameDataAsset>();
        
        // 버튼 이벤트
        buyButton.onClick.AddListener(OnBuyButtonClick);
        
        // 이벤트 구독
        EventBus<MoneyChangedEvent>.Subscribe(OnMoneyChanged);
        EventBus<GunSwitchedEvent>.Subscribe(OnGunSwitched);
        EventBus<GameInitializedEvent>.Subscribe(OnGameInitialized);
    }
    
    private void OnDestroy()
    {
        EventBus<MoneyChangedEvent>.Unsubscribe(OnMoneyChanged);
        EventBus<GunSwitchedEvent>.Unsubscribe(OnGunSwitched);
        EventBus<GameInitializedEvent>.Unsubscribe(OnGameInitialized);
    }
    
    private void OnGameInitialized(GameInitializedEvent e)
    {
        UpdateDisplay(gameManager.GetCurrentGunIndex());
    }
    
    private void OnMoneyChanged(MoneyChangedEvent e)
    {
        UpdateAffordability();
    }
    
    private void OnGunSwitched(GunSwitchedEvent e)
    {
        UpdateDisplay(e.GunId);
    }
    
    private void UpdateDisplay(int gunId)
    {
        currentGunId = gunId;
        
        var gun = gameDataAsset.guns[gunId];
        var upgrade = gameDataAsset.upgrades[gunId];
        var level = gameManager.GetUpgradeLevel(gunId);
        
        upgradeLevelText.text = $"Lv.{level}/{upgrade.MaxLevel}";
        currentValueText.text = $"Click Value: ${gameManager.CalculateClickValue(gunId)}";
        
        if (level >= upgrade.MaxLevel)
        {
            costText.text = "MAX";
            buyButton.interactable = false;
        }
        else
        {
            costText.text = $"Cost: ${gameManager.CalculateUpgradeCost(gunId)}";
            UpdateAffordability();
        }
    }
    
    private void UpdateAffordability()
    {
        if (currentGunId < 0) return;
        
        bool canAfford = gameManager.CanPurchaseUpgrade(currentGunId);
        buyButton.interactable = canAfford;
    }
    
    private void OnBuyButtonClick()
    {
        if (!gameManager.CanPurchaseUpgrade(currentGunId)) return;
        
        // 구매 처리
        EventBus<UpgradePurchasedEvent>.Publish(new UpgradePurchasedEvent 
        { 
            GunId = currentGunId, 
            Level = gameManager.GetUpgradeLevel(currentGunId) + 1 
        });
        
        // UI 업데이트
        UpdateDisplay(currentGunId);
    }
}