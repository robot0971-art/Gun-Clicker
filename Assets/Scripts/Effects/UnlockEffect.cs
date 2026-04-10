using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DI;

/// <summary>
/// 총 해금 시각적 피드백
/// </summary>
public class UnlockEffect : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject unlockPopup;
    [SerializeField] private TMP_Text unlockText;
    [SerializeField] private Button closeButton;
    [SerializeField] private Image flashImage;
    
    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip unlockSound;
    
    [Header("Settings")]
    [SerializeField] private float flashDuration = 0.2f;
    [SerializeField] private float popupDelay = 0.3f;
    
    private GameDataAsset gameDataAsset;
    
    private void Start()
    {
        gameDataAsset = DIContainer.Resolve<GameDataAsset>();
        
        EventBus<GunUnlockedEvent>.Subscribe(OnGunUnlocked);
        
        if (unlockPopup != null)
        {
            unlockPopup.SetActive(false);
        }
        
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(ClosePopup);
        }
    }
    
    private void OnDestroy()
    {
        EventBus<GunUnlockedEvent>.Unsubscribe(OnGunUnlocked);
    }
    
    private void OnGunUnlocked(GunUnlockedEvent e)
    {
        StartCoroutine(PlayUnlockAnimation(e.GunId));
    }
    
    private IEnumerator PlayUnlockAnimation(int gunId)
    {
        var gun = gameDataAsset.guns[gunId];
        
        // 화면 플래시
        if (flashImage != null)
        {
            yield return StartCoroutine(FlashScreen());
        }
        
        // 사운드
        if (audioSource != null && unlockSound != null)
        {
            audioSource.PlayOneShot(unlockSound);
        }
        
        // 팝업 지연
        yield return new WaitForSeconds(popupDelay);
        
        // 팝업 표시
        if (unlockPopup != null && unlockText != null)
        {
            unlockText.text = $"Unlocked: {gun.Name}!";
            unlockPopup.SetActive(true);
        }
    }
    
    private IEnumerator FlashScreen()
    {
        flashImage.gameObject.SetActive(true);
        
        var color = flashImage.color;
        color.a = 0.5f;
        flashImage.color = color;
        
        float elapsed = 0f;
        while (elapsed < flashDuration)
        {
            elapsed += Time.deltaTime;
            color.a = Mathf.Lerp(0.5f, 0f, elapsed / flashDuration);
            flashImage.color = color;
            yield return null;
        }
        
        flashImage.gameObject.SetActive(false);
    }
    
    private void ClosePopup()
    {
        if (unlockPopup != null)
        {
            unlockPopup.SetActive(false);
        }
    }
}