# Phase 4: Effects Implementation

## Overview
이펙트 시스템 구현 - ClickEffect, UnlockEffect

## Dependencies
- EventBus (완료)
- Core System (Phase 1)
- GunsPack Sprites (asset)

---

## Files to Create

```
Assets/Scripts/Effects/
├── ClickEffect.cs
└── UnlockEffect.cs
```

---

## 1. ClickEffect

**Purpose:** 클릭 시각적 피드백

**Effects:**
- Gun scale bounce animation
- Floating money text (+$1)
- Bullet sprite animation
- Particle effect

**Events to Subscribe:**
- ClickEvent → Trigger effect
- MoneyChangedEvent → Show floating text

**Implementation:**
```csharp
public class ClickEffect : MonoBehaviour
{
    [SerializeField] private GameObject floatingTextPrefab;
    [SerializeField] private ParticleSystem clickParticle;
    [SerializeField] private Transform gunTransform;
    
    private Vector3 originalScale;
    
    void Start()
    {
        originalScale = gunTransform.localScale;
        EventBus<ClickEvent>.Subscribe(OnClick);
        EventBus<MoneyChangedEvent>.Subscribe(OnMoneyChanged);
    }
    
    void OnClick(ClickEvent e)
    {
        // Gun bounce animation
        StartCoroutine(BounceAnimation());
        
        // Particle effect
        clickParticle.Play();
    }
    
    void OnMoneyChanged(MoneyChangedEvent e)
    {
        // Floating text
        ShowFloatingText(e.Delta);
    }
    
    IEnumerator BounceAnimation()
    {
        gunTransform.localScale = originalScale * 1.1f;
        yield return new WaitForSeconds(0.1f);
        gunTransform.localScale = originalScale;
    }
    
    void ShowFloatingText(long amount)
    {
        // Instantiate floating text at gun position
    }
}
```

---

## 2. UnlockEffect

**Purpose:** 총 해금 시각적 피드백

**Effects:**
- Screen flash
- Dramatic gun reveal
- Sound effect
- Notification popup

**Events to Subscribe:**
- GunUnlockedEvent → Trigger unlock animation

**Implementation:**
```csharp
public class UnlockEffect : MonoBehaviour
{
    [SerializeField] private GameObject unlockPopupPrefab;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip unlockSound;
    
    void Start()
    {
        EventBus<GunUnlockedEvent>.Subscribe(OnGunUnlocked);
    }
    
    void OnGunUnlocked(GunUnlockedEvent e)
    {
        StartCoroutine(PlayUnlockAnimation(e.GunId));
    }
    
    IEnumerator PlayUnlockAnimation(int gunId)
    {
        // Screen flash
        // Sound effect
        audioSource.PlayOneShot(unlockSound);
        
        // Wait
        yield return new WaitForSeconds(0.5f);
        
        // Show popup
        ShowUnlockPopup(gunId);
    }
    
    void ShowUnlockPopup(int gunId)
    {
        // Instantiate popup with gun name
    }
}
```

---

## Prefabs to Create

```
Assets/Prefabs/
├── FloatingText.prefab
│   └── TextMeshPro (floating +$1)
└── UnlockPopup.prefab
    └── Background + GunNameText + CloseButton
```

---

## Implementation Order

1. ClickEffect.cs
2. UnlockEffect.cs
3. Create FloatingText prefab
4. Create UnlockPopup prefab
5. Attach effects to scene
6. Test click feedback
7. Test unlock animation

---

## Testing Checklist

- [ ] ClickEffect 컴포넌트 생성
- [ ] 클릭 시 gun bounce 테스트
- [ ] Floating text 테스트
- [ ] Particle effect 테스트
- [ ] UnlockEffect 컴포넌트 생성
- [ ] 해금 시 popup 테스트
- [ ] Sound effect 테스트