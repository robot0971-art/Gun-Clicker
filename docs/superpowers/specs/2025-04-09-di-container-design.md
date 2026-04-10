# DI Container Design Spec

**Goal:** Unity 프로젝트용 경량 DI Container 구현 - Installer 패턴으로 씬별 의존성 관리

**Architecture:** 
- Static DIContainer가 Container 리스트를 관리 (최신 컨테이너가 뒤에 추가)
- MonoInstaller가 씬별 컨테이너 생성/제거 담당
- [Inject] 어트리뷰트로 자동 주입, 최신 컨테이너부터 역순 검색

**Tech Stack:** Unity, C#

---

## Core Components

### 1. DIContainer (static class)
- `containers: List<Container>` - 등록된 컨테이너들 (최신이 뒤)
- `Register<T>(instance, key?)` - 타입(옵션:키)으로 인스턴스 등록
- `Register<TInterface, TImplementation>(instance, key?)` - 인터페이스로 구현체 등록
- `Resolve<T>(key?)` - 타입(옵션:키)으로 인스턴스 조회 (최신 컨테이너부터)
- `Inject(GameObject)` - 게임오브젝트의 [Inject] 필드에 자동 주입

### 2. Container (inner class)
- `bindings: Dictionary<string, object>` - 키-인스턴스 매핑
- 키 생성 규칙: `typeof(T).FullName` 또는 `typeof(T).FullName + ":" + key`

### 3. MonoInstaller (abstract class, MonoBehaviour)
- `container: Container` - 이 Installer가 관리하는 컨테이너
- `abstract void InstallBindings()` - 서브클래스에서 바인딩 구현
- `Register<T>(instance, key?)` - 컨테이너에 등록 + DIContainer에 컨테이너 등록
- `Awake()` - 컨테이너 생성, InstallBindings() 호출
- `OnDestroy()` - DIContainer에서 컨테이너 제거

### 4. InjectAttribute
- 필드에 적용하여 자동 주입 표시

---

## Key Generation Rules

| 호출 | 키 |
|------|-----|
| `Register<PlayerController>(player)` | `"PlayerController"` |
| `Register<PlayerController>(player, "main")` | `"PlayerController:main"` |
| `Register<IInventory, Inventory>(inventory)` | `"IInventory"` |

---

## Resolution Flow

1. `Resolve<T>()` 호출
2. `containers` 리스트를 뒤에서 앞으로 순회
3. 각 컨테이너에서 키로 조회
4. 첫 번째로 찾은 인스턴스 반환
5. 없으면 `null` 반환 (또는 예외)

---

## Injection Flow

1. `DIContainer.Inject(gameObject)` 호출
2. 게임오브젝트의 모든 MonoBehaviour 컴포넌트 가져오기
3. 각 컴포넌트의 `[Inject]` 어트리뷰트가 붙은 private/public 필드 찾기
4. 각 필드에 대해 `Resolve<필드타입>()` 호출
5. 결과를 필드에 할당 (리플렉션 사용)

---

## Usage Examples

### Installer 정의
```csharp
public class GameplayInstaller : MonoInstaller {
    [SerializeField] private PlayerController player;
    
    public override void InstallBindings() {
        Register(player);
        Register<IEnemySpawner, EnemySpawner>(new EnemySpawner());
        Register<IGameManager, GameManager>(FindObjectOfType<GameManager>(), "main");
    }
}
```

### 의존성 주입받기
```csharp
public class UIManager : MonoBehaviour {
    [Inject] private PlayerController player;
    [Inject] private IEnemySpawner spawner;
    
    void Start() {
        DIContainer.Inject(gameObject);
        // 이제 player, spawner 사용 가능
    }
}
```

### 전역 Installer (DontDestroyOnLoad)
```csharp
public class GlobalInstaller : MonoInstaller {
    void Awake() {
        DontDestroyOnLoad(gameObject);
    }
    
    public override void InstallBindings() {
        Register<IGameManager, GameManager>(new GameManager());
    }
}
```

---

## Edge Cases

### 중복 등록
- 동일 키로 등록 시 기존 것 덮어씌움 (경고 로그 없이)

### 씬 전환
- 씬 언로드 시 Installer의 `OnDestroy()` 호출
- 해당 컨테이너가 `DIContainer.containers`에서 제거됨
- DontDestroyOnLoad Installer는 씬 전환에도 유지됨

### 주입 순서
- 최신 컨테이너부터 검색하므로, 씬 Installer → 전역 Installer 순으로 조회
- 씬에서 오버라이드 가능

### null 주입
- Resolve 실패 시 필드는 null로 유지됨 (경고 로그 출력)

---

## File Structure

```
Assets/Scripts/DI/
├── InjectAttribute.cs (~10줄)
└── DIContainer.cs (~150줄)
    ├── InjectAttribute
    ├── Container class
    ├── DIContainer static class
    └── MonoInstaller abstract class
```

---

## Testing Checklist

- [ ] 기본 타입 등록/조회
- [ ] 인터페이스 타입으로 구현체 등록/조회
- [ ] 키가 있는 등록/조회
- [ ] 최신 컨테이너부터 역순 검색
- [ ] 씬 전환 시 컨테이너 제거
- [ ] [Inject] 필드 자동 주입
- [ ] Resolve 실패 시 null 반환 + 경고 로그