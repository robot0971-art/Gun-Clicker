# Unity Scene 구성 가이드

## 1. Prefab 생성

### 1.1 FloatingText Prefab
```
1. Hierarchy에서 Create > UI > Text - TextMeshPro
2. 이름: "FloatingText"
3. Inspector 설정:
   - TextMeshPro: Font Size = 24, Alignment = Center
   - Color = Yellow
   - FloatingText 컴포넌트 추가
4. Project > Assets/Prefabs 폴더로 드래그 (Prefab 생성)
5. Hierarchy에서 삭제
```

### 1.2 GunSlot Prefab
```
1. Hierarchy에서 Create > UI > Button
2. 이름: "GunSlot"
3. 자식 구조:
   GunSlot (Button)
   ├── GunImage (Image) - 총 스프라이트
   ├── NameText (TMP Text) - 총 이름
   ├── Highlight (Image) - 선택 표시
   │   └── Color = Yellow, Alpha = 100
   └── LockOverlay (Image)
       └── UnlockText (TMP Text) - 해금 조건

4. Inspector 설정:
   - Button: Transition = None
   - GunSlot 컴포넌트 추가
   - 각 필드에 자식 오브젝트 연결

5. Project > Assets/Prefabs 폴더로 드래그
6. Hierarchy에서 삭제
```

---

## 2. Scene 구성

### 2.1 GlobalInstaller GameObject
```
1. Hierarchy에서 Create Empty
2. 이름: "GlobalInstaller"
3. Inspector:
   - GlobalInstaller 컴포넌트 추가
   - Tag = "GameController"
```

### 2.2 Canvas 구성
```
Canvas
├── TopBar (Empty)
│   ├── MoneyText (TMP Text)
│   │   - Text: "$0"
│   │   - Font Size: 32
│   └── CurrentGunText (TMP Text)
│       - Text: "Revolver"
│       - Font Size: 24
│
├── GunDisplay (Empty)
│   ├── GunImage (Image)
│   │   - Color: White (임시)
│   ├── ClickHandler 컴포넌트
│   └── ClickEffect 컴포넌트
│       - FloatingText Prefab 연결
│       - FloatingTextParent = GunDisplay
│
├── TabBar (Empty)
│   ├── ShopTabButton (Button)
│   │   └── Text: "Shop"
│   └── CollectionTabButton (Button)
│       └── Text: "Collection"
│
├── ShopPanel (Empty)
│   ├── UpgradeLevelText (TMP Text)
│   ├── CostText (TMP Text)
│   ├── CurrentValueText (TMP Text)
│   ├── BuyButton (Button)
│   │   └── Text: "Buy"
│   └── ShopPanel 컴포넌트
│       - 각 UI 요소 연결
│
├── CollectionPanel (Empty)
│   ├── SlotContainer (Empty)
│   │   - Grid Layout Group 추가
│   └── CollectionPanel 컴포넌트
│       - Slot Container 연결
│       - GunSlot Prefab 연결
│
├── UnlockPopup (Empty)
│   ├── Background (Image)
│   ├── UnlockText (TMP Text)
│   │   - Text: "Unlocked!"
│   └── CloseButton (Button)
│       └── Text: "Close"
│
├── FlashImage (Image)
│   - Color: White, Alpha = 0
│   - Raycast Target = false
│
└── UIManager 컴포넌트 (Canvas에 추가)
    - 모든 UI 요소 연결
```

### 2.3 Effects GameObject
```
Effects (Empty)
├── UnlockEffect 컴포넌트
│   - UnlockPopup 연결
│   - FlashImage 연결
│   - AudioSource 연결
└── AudioSource
    - Play On Awake = false
```

---

## 3. UI 레이아웃 설정

### 3.1 Canvas Scaler
```
Canvas > Canvas Scaler
- UI Scale Mode: Scale With Screen Size
- Reference Resolution: 1920 x 1080
- Match: 0.5
```

### 3.2 TopBar Layout
```
TopBar
- RectTransform: Anchor = Top-Stretch
- Height: 80

MoneyText
- Anchor: Top-Left
- Position: (20, -40)

CurrentGunText
- Anchor: Top-Right
- Position: (-20, -40)
```

### 3.3 GunDisplay Layout
```
GunDisplay
- Anchor: Center
- Size: 300 x 300

GunImage
- Fill Center
```

### 3.4 TabBar Layout
```
TabBar
- Anchor: Bottom
- Height: 60
- Horizontal Layout Group 추가

ShopTabButton, CollectionTabButton
- Width: 150
```

### 3.5 CollectionPanel SlotContainer
```
SlotContainer
- Grid Layout Group 추가
  - Cell Size: 150 x 150
  - Spacing: 10
  - Constraint: Fixed Column Count = 4
```

---

## 4. 컴포넌트 연결

### 4.1 UIManager
```
Top Bar:
- MoneyText → 연결
- CurrentGunText → 연결

Gun Display:
- GunImage → 연결
- ClickHandler → 연결 (자동)

Tabs:
- ShopTabButton → 연결
- CollectionTabButton → 연결
- ShopPanel → 연결
- CollectionPanel → 연결
```

### 4.2 ClickEffect
```
Gun Bounce:
- Gun Transform → GunImage 연결

Floating Text:
- Floating Text Prefab → Prefab 연결
- Floating Text Parent → GunDisplay 연결
```

### 4.3 ShopPanel
```
UI Elements:
- UpgradeLevelText → 연결
- CostText → 연결
- CurrentValueText → 연결
- BuyButton → 연결
```

### 4.4 CollectionPanel
```
UI Elements:
- Slot Container → 연결
- Slot Prefab → GunSlot Prefab 연결
```

### 4.5 UnlockEffect
```
UI:
- Unlock Popup → UnlockPopup 연결
- Unlock Text → UnlockText 연결
- Close Button → CloseButton 연결
- Flash Image → FlashImage 연결

Audio:
- Audio Source → AudioSource 연결
```

---

## 5. 테스트

### 5.1 Play Mode 테스트
1. Play 버튼 클릭
2. Console에서 "[GlobalInstaller] All services registered and initialized" 확인
3. 화면 클릭 → 골드 증가 확인
4. Shop 탭 → 업그레이드 구매 테스트
5. Collection 탭 → 총 선택 테스트

### 5.2 저장 테스트
1. Play → 골드 획득 → Stop
2. 다시 Play → 골드 유지 확인

---

## 6. 빌드 전 체크리스트

- [ ] GameDataAsset.asset이 Resources 폴더에 있음
- [ ] 모든 UI 요소가 연결됨
- [ ] Prefab이 올바르게 연결됨
- [ ] Canvas Scaler 설정 확인
- [ ] Console에 에러 없음