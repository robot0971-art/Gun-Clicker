# Phase 2: Click System Implementation

## Overview
클릭 처리 시스템 - ClickHandler, 클릭 감지 및 이벤트 발행

## Dependencies
- EventBus (완료)
- Core System (Phase 1)

---

## Files to Create

```
Assets/Scripts/UI/
└── ClickHandler.cs
```

---

## 1. ClickHandler

**Purpose:** 화면 클릭 감지, ClickEvent 발행

**Responsibilities:**
1. Detect click on gun sprite
2. Publish ClickEvent
3. Trigger visual feedback (optional - can be in separate effect)

**Implementation:**
```csharp
public class ClickHandler : MonoBehaviour
{
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            // Check if click is on gun area
            EventBus<ClickEvent>.Publish(new ClickEvent());
        }
    }
}
```

---

## Click Area Detection

**Option A: Simple (Recommended for KISS)**
- Entire screen click (no area check)
- Just publish ClickEvent

**Option B: Area-specific**
- Raycast to check if clicked on gun GameObject
- Only publish if hit gun

---

## Implementation Order

1. ClickHandler.cs
2. Attach to Gun GameObject in scene
3. Test click detection
4. Verify ClickEvent → GameManager → MoneyChangedEvent flow

---

## Testing Checklist

- [ ] ClickHandler 컴포넌트 생성
- [ ] 클릭 감지 테스트
- [ ] ClickEvent 발행 테스트
- [ ] GameManager에서 ClickEvent 수신 테스트
- [ ] MoneyChangedEvent 발행 테스트