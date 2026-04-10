using System.Collections;
using UnityEngine;

/// <summary>
/// 클릭 시각적 피드백
/// </summary>
public class ClickEffect : MonoBehaviour
{
    [Header("Gun Bounce")]
    [SerializeField] private Transform gunTransform;
    [SerializeField] private float bounceScale = 1.1f;
    [SerializeField] private float bounceDuration = 0.1f;
    
    [Header("Floating Text")]
    [SerializeField] private GameObject floatingTextPrefab;
    [SerializeField] private Transform floatingTextParent;
    
    [Header("Particle")]
    [SerializeField] private ParticleSystem clickParticle;
    
    private Vector3 originalScale;
    
    private void Start()
    {
        if (gunTransform != null)
        {
            originalScale = gunTransform.localScale;
        }
        
        EventBus<ClickEvent>.Subscribe(OnClick);
        EventBus<MoneyChangedEvent>.Subscribe(OnMoneyChanged);
    }
    
    private void OnDestroy()
    {
        EventBus<ClickEvent>.Unsubscribe(OnClick);
        EventBus<MoneyChangedEvent>.Unsubscribe(OnMoneyChanged);
    }
    
    private void OnClick(ClickEvent e)
    {
        if (gunTransform != null)
        {
            StartCoroutine(BounceAnimation());
        }
        
        if (clickParticle != null)
        {
            clickParticle.Play();
        }
    }
    
    private void OnMoneyChanged(MoneyChangedEvent e)
    {
        if (floatingTextPrefab != null && floatingTextParent != null)
        {
            ShowFloatingText(e.Delta);
        }
    }
    
    private IEnumerator BounceAnimation()
    {
        gunTransform.localScale = originalScale * bounceScale;
        yield return new WaitForSeconds(bounceDuration);
        gunTransform.localScale = originalScale;
    }
    
    private void ShowFloatingText(long amount)
    {
        var textObj = Instantiate(floatingTextPrefab, floatingTextParent);
        var floatingText = textObj.GetComponent<FloatingText>();
        
        if (floatingText != null)
        {
            floatingText.Setup($"+${FormatNumber(amount)}", Color.yellow);
        }
    }
    
    private string FormatNumber(long num)
    {
        if (num >= 1000000) return $"{num / 1000000f:F1}M";
        if (num >= 1000) return $"{num / 1000f:F1}K";
        return num.ToString();
    }
}