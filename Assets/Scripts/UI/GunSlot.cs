using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 총 슬롯 UI (CollectionPanel용)
/// </summary>
public class GunSlot : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image gunImage;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text unlockText;
    [SerializeField] private GameObject lockOverlay;
    [SerializeField] private GameObject highlight;
    [SerializeField] private Button button;
    
    public int GunId { get; private set; }
    
    public void Setup(int gunId, string gunName, int unlockClicks, bool isUnlocked, bool isCurrent)
    {
        GunId = gunId;
        
        if (nameText != null)
        {
            nameText.text = isUnlocked ? gunName : "???";
        }
        
        if (unlockText != null)
        {
            unlockText.text = $"{unlockClicks} clicks";
        }
        
        if (lockOverlay != null)
        {
            lockOverlay.SetActive(!isUnlocked);
        }
        
        if (highlight != null)
        {
            highlight.SetActive(isCurrent);
        }
        
        if (button != null)
        {
            button.interactable = isUnlocked;
        }
    }
    
    public void SetHighlight(bool active)
    {
        if (highlight != null)
        {
            highlight.SetActive(active);
        }
    }
    
    public void SetUnlocked(bool isUnlocked, string gunName = "")
    {
        if (nameText != null && isUnlocked)
        {
            nameText.text = gunName;
        }
        
        if (lockOverlay != null)
        {
            lockOverlay.SetActive(!isUnlocked);
        }
        
        if (button != null)
        {
            button.interactable = isUnlocked;
        }
    }
}