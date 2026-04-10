using DI;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 총 컬렉션 UI (8개 총 표시)
/// </summary>
public class CollectionPanel : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Transform slotContainer;
    [SerializeField] private GameObject slotPrefab;
    
    private GameManager gameManager;
    private GameDataAsset gameDataAsset;
    
    private GunSlot[] slots;
    
    private void Start()
    {
        // DI 주입
        gameManager = DIContainer.Resolve<GameManager>();
        gameDataAsset = DIContainer.Resolve<GameDataAsset>();
        
        // 이벤트 구독
        EventBus<GunUnlockedEvent>.Subscribe(OnGunUnlocked);
        EventBus<GunSwitchedEvent>.Subscribe(OnGunSwitched);
        EventBus<GameInitializedEvent>.Subscribe(OnGameInitialized);
    }
    
    private void OnDestroy()
    {
        EventBus<GunUnlockedEvent>.Unsubscribe(OnGunUnlocked);
        EventBus<GunSwitchedEvent>.Unsubscribe(OnGunSwitched);
        EventBus<GameInitializedEvent>.Unsubscribe(OnGameInitialized);
    }
    
    private void OnGameInitialized(GameInitializedEvent e)
    {
        InitializeSlots();
    }
    
    private void OnGunUnlocked(GunUnlockedEvent e)
    {
        UpdateSlot(e.GunId);
    }
    
    private void OnGunSwitched(GunSwitchedEvent e)
    {
        HighlightCurrentGun(e.GunId);
    }
    
    private void InitializeSlots()
    {
        // 기존 슬롯 제거
        foreach (Transform child in slotContainer)
        {
            Destroy(child.gameObject);
        }
        
        // 슬롯 생성
        slots = new GunSlot[gameDataAsset.guns.Count];
        
        for (int i = 0; i < gameDataAsset.guns.Count; i++)
        {
            var slotObj = Instantiate(slotPrefab, slotContainer);
            var slot = slotObj.GetComponent<GunSlot>();
            
            if (slot != null)
            {
                var gun = gameDataAsset.guns[i];
                bool isUnlocked = gameManager.IsGunUnlocked(i);
                bool isCurrent = i == gameManager.GetCurrentGunIndex();
                
                slot.Setup(i, gun.Name, gun.UnlockClicks, isUnlocked, isCurrent);
                
                // 버튼 이벤트
                var button = slotObj.GetComponent<Button>();
                if (button != null)
                {
                    int capturedId = i;
                    button.onClick.AddListener(() => OnSlotClick(capturedId));
                }
                
                slots[i] = slot;
            }
        }
    }
    
    private void UpdateSlot(int gunId)
    {
        if (slots == null || gunId < 0 || gunId >= slots.Length) return;
        
        var slot = slots[gunId];
        var gun = gameDataAsset.guns[gunId];
        
        slot.SetUnlocked(true, gun.Name);
    }
    
    private void HighlightCurrentGun(int gunId)
    {
        if (slots == null) return;
        
        for (int i = 0; i < slots.Length; i++)
        {
            slots[i].SetHighlight(i == gunId);
        }
    }
    
    private void OnSlotClick(int gunId)
    {
        if (!gameManager.IsGunUnlocked(gunId)) return;
        
        EventBus<GunSwitchedEvent>.Publish(new GunSwitchedEvent { GunId = gunId });
    }
}