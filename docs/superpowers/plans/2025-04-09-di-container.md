# DI Container Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Unity용 경량 DI Container 구현 - [Inject] 어트리뷰트 자동 주입, Installer 패턴, 씬별 컨테이너 관리

**Architecture:** Static DIContainer가 Container 리스트를 관리. MonoInstaller가 씬별 컨테이너 생성/제거. 최신 컨테이너부터 역순 검색으로 의존성 해결.

**Tech Stack:** Unity, C#

---

## Files to Create/Modify

```
Assets/Scripts/DI/
├── InjectAttribute.cs     # 어트리뷰트 정의
└── DIContainer.cs         # Container, DIContainer, MonoInstaller 통합
```

---

## Task 1: InjectAttribute 정의

**Files:**
- Create: `Assets/Scripts/DI/InjectAttribute.cs`

- [ ] **Step 1: InjectAttribute 구현**

```csharp
using System;

namespace DI
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class InjectAttribute : Attribute
    {
    }
}
```

- [ ] **Step 2: Unity에서 인식되는지 확인**

Unity 에디터에서 Assets > Create > C# Script로 파일이 보이는지 확인

---

## Task 2: DIContainer 핵심 구현

**Files:**
- Create: `Assets/Scripts/DI/DIContainer.cs`

- [ ] **Step 1: Container 클래스 구현**

```csharp
using System;
using System.Collections.Generic;
using UnityEngine;

namespace DI
{
    public class Container
    {
        private readonly Dictionary<string, object> _bindings = new Dictionary<string, object>();

        public void Bind<T>(T instance, string key = null)
        {
            string bindingKey = GetKey(typeof(T), key);
            _bindings[bindingKey] = instance;
        }

        public void Bind<TInterface, TImplementation>(TImplementation instance, string key = null)
            where TImplementation : TInterface
        {
            string bindingKey = GetKey(typeof(TInterface), key);
            _bindings[bindingKey] = instance;
        }

        public bool TryResolve<T>(out T instance, string key = null)
        {
            string bindingKey = GetKey(typeof(T), key);
            if (_bindings.TryGetValue(bindingKey, out var obj))
            {
                instance = (T)obj;
                return true;
            }
            instance = default;
            return false;
        }

        private string GetKey(Type type, string key)
        {
            return string.IsNullOrEmpty(key) ? type.FullName : $"{type.FullName}:{key}";
        }
    }
}
```

- [ ] **Step 2: DIContainer static 클래스 구현**

```csharp
public static class DIContainer
{
    private static readonly List<Container> _containers = new List<Container>();

    public static void RegisterContainer(Container container)
    {
        _containers.Add(container);
    }

    public static void UnregisterContainer(Container container)
    {
        _containers.Remove(container);
    }

    public static void Register<T>(T instance, string key = null)
    {
        if (_containers.Count == 0)
        {
            Debug.LogError("[DIContainer] No container registered. Create an Installer first.");
            return;
        }
        _containers[_containers.Count - 1].Bind(instance, key);
    }

    public static void Register<TInterface, TImplementation>(TImplementation instance, string key = null)
        where TImplementation : TInterface
    {
        if (_containers.Count == 0)
        {
            Debug.LogError("[DIContainer] No container registered. Create an Installer first.");
            return;
        }
        _containers[_containers.Count - 1].Bind<TInterface, TImplementation>(instance, key);
    }

    public static T Resolve<T>(string key = null)
    {
        for (int i = _containers.Count - 1; i >= 0; i--)
        {
            if (_containers[i].TryResolve<T>(out var instance, key))
            {
                return instance;
            }
        }
        Debug.LogWarning($"[DIContainer] Could not resolve {typeof(T).FullName}{(string.IsNullOrEmpty(key) ? "" : ":" + key)}");
        return default;
    }

    public static void Inject(GameObject gameObject)
    {
        var monoBehaviours = gameObject.GetComponents<MonoBehaviour>();
        foreach (var mb in monoBehaviours)
        {
            if (mb == null) continue;
            InjectIntoObject(mb);
        }
    }

    private static void InjectIntoObject(object obj)
    {
        var type = obj.GetType();
        var fields = type.GetFields(System.Reflection.BindingFlags.Instance | 
                                     System.Reflection.BindingFlags.Public | 
                                     System.Reflection.BindingFlags.NonPublic);
        
        foreach (var field in fields)
        {
            if (field.GetCustomAttributes(typeof(InjectAttribute), true).Length > 0)
            {
                var value = ResolveField(field.FieldType, field.Name);
                if (value != null)
                {
                    field.SetValue(obj, value);
                }
            }
        }
    }

    private static object ResolveField(Type fieldType, string fieldName)
    {
        for (int i = _containers.Count - 1; i >= 0; i--)
        {
            if (_containers[i].TryResolve(fieldType, out var instance, null))
            {
                return instance;
            }
        }
        Debug.LogWarning($"[DIContainer] Could not resolve {fieldType.FullName} for field {fieldName}");
        return null;
    }

    public static void Clear()
    {
        _containers.Clear();
    }
}
```

- [ ] **Step 3: MonoInstaller 추상 클래스 구현**

```csharp
public abstract class MonoInstaller : MonoBehaviour
{
    protected Container Container { get; private set; }

    public abstract void InstallBindings();

    protected virtual void Awake()
    {
        Container = new Container();
        DIContainer.RegisterContainer(Container);
        InstallBindings();
    }

    protected virtual void OnDestroy()
    {
        DIContainer.UnregisterContainer(Container);
    }

    protected void Bind<T>(T instance, string key = null)
    {
        Container.Bind(instance, key);
    }

    protected void Bind<TInterface, TImplementation>(TImplementation instance, string key = null)
        where TImplementation : TInterface
    {
        Container.Bind<TInterface, TImplementation>(instance, key);
    }
}
```

---

## Task 3: 전체 DIContainer.cs 통합

**Files:**
- Modify: `Assets/Scripts/DI/DIContainer.cs`

- [ ] **Step 1: 모든 코드를 하나의 파일로 통합**

```csharp
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace DI
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class InjectAttribute : Attribute
    {
    }

    public class Container
    {
        private readonly Dictionary<string, object> _bindings = new Dictionary<string, object>();

        public void Bind<T>(T instance, string key = null)
        {
            string bindingKey = GetKey(typeof(T), key);
            _bindings[bindingKey] = instance;
        }

        public void Bind<TInterface, TImplementation>(TImplementation instance, string key = null)
            where TImplementation : TInterface
        {
            string bindingKey = GetKey(typeof(TInterface), key);
            _bindings[bindingKey] = instance;
        }

        public bool TryResolve<T>(out T instance, string key = null)
        {
            string bindingKey = GetKey(typeof(T), key);
            if (_bindings.TryGetValue(bindingKey, out var obj))
            {
                instance = (T)obj;
                return true;
            }
            instance = default;
            return false;
        }

        public bool TryResolve(Type type, out object instance, string key = null)
        {
            string bindingKey = GetKey(type, key);
            return _bindings.TryGetValue(bindingKey, out instance);
        }

        private string GetKey(Type type, string key)
        {
            return string.IsNullOrEmpty(key) ? type.FullName : $"{type.FullName}:{key}";
        }
    }

    public static class DIContainer
    {
        private static readonly List<Container> _containers = new List<Container>();
        private static readonly Dictionary<Type, FieldInfo[]> _fieldCache = new Dictionary<Type, FieldInfo[]>();

        public static int ContainerCount => _containers.Count;

        public static void RegisterContainer(Container container)
        {
            if (container == null)
            {
                Debug.LogError("[DIContainer] Cannot register null container.");
                return;
            }
            _containers.Add(container);
        }

        public static void UnregisterContainer(Container container)
        {
            _containers.Remove(container);
        }

        public static void Register<T>(T instance, string key = null)
        {
            if (_containers.Count == 0)
            {
                Debug.LogError("[DIContainer] No container registered. Create an Installer first.");
                return;
            }
            _containers[_containers.Count - 1].Bind(instance, key);
        }

        public static void Register<TInterface, TImplementation>(TImplementation instance, string key = null)
            where TImplementation : TInterface
        {
            if (_containers.Count == 0)
            {
                Debug.LogError("[DIContainer] No container registered. Create an Installer first.");
                return;
            }
            _containers[_containers.Count - 1].Bind<TInterface, TImplementation>(instance, key);
        }

        public static T Resolve<T>(string key = null)
        {
            for (int i = _containers.Count - 1; i >= 0; i--)
            {
                if (_containers[i].TryResolve<T>(out var instance, key))
                {
                    return instance;
                }
            }
            Debug.LogWarning($"[DIContainer] Could not resolve {typeof(T).FullName}{(string.IsNullOrEmpty(key) ? "" : ":" + key)}");
            return default;
        }

        public static void Inject(GameObject gameObject)
        {
            if (gameObject == null)
            {
                Debug.LogError("[DIContainer] Cannot inject into null GameObject.");
                return;
            }

            var monoBehaviours = gameObject.GetComponents<MonoBehaviour>();
            foreach (var mb in monoBehaviours)
            {
                if (mb == null) continue;
                InjectIntoObject(mb);
            }
        }

        private static void InjectIntoObject(object obj)
        {
            var type = obj.GetType();
            
            if (!_fieldCache.TryGetValue(type, out var fields))
            {
                var allFields = type.GetFields(BindingFlags.Instance | 
                                                BindingFlags.Public | 
                                                BindingFlags.NonPublic);
                
                var injectFields = new List<FieldInfo>();
                foreach (var field in allFields)
                {
                    if (field.GetCustomAttributes(typeof(InjectAttribute), true).Length > 0)
                    {
                        injectFields.Add(field);
                    }
                }
                fields = injectFields.ToArray();
                _fieldCache[type] = fields;
            }
            
            foreach (var field in fields)
            {
                if (TryResolveField(field.FieldType, out var value, field.Name))
                {
                    field.SetValue(obj, value);
                }
            }
        }

        private static bool TryResolveField(Type fieldType, out object value, string fieldName)
        {
            for (int i = _containers.Count - 1; i >= 0; i--)
            {
                if (_containers[i].TryResolve(fieldType, out value, null))
                {
                    return true;
                }
            }
            Debug.LogWarning($"[DIContainer] Could not resolve {fieldType.FullName} for field '{fieldName}'");
            value = null;
            return false;
        }

        public static void Clear()
        {
            _containers.Clear();
            _fieldCache.Clear();
        }
    }

    public abstract class MonoInstaller : MonoBehaviour
    {
        protected Container Container { get; private set; }

        public abstract void InstallBindings();

        protected virtual void Awake()
        {
            Container = new Container();
            DIContainer.RegisterContainer(Container);
            InstallBindings();
        }

        protected virtual void OnDestroy()
        {
            DIContainer.UnregisterContainer(Container);
        }

        protected void Bind<T>(T instance, string key = null)
        {
            Container.Bind(instance, key);
        }

        protected void Bind<TInterface, TImplementation>(TImplementation instance, string key = null)
            where TImplementation : TInterface
        {
            Container.Bind<TInterface, TImplementation>(instance, key);
        }
    }
}
```

- [ ] **Step 2: InjectAttribute.cs 삭제 (통합됨)**

InjectAttribute.cs는 DIContainer.cs에 통합되었으므로 별도 파일이 필요 없음

---

## Task 4: 예제 Installer 생성

**Files:**
- Create: `Assets/Scripts/Installers/GameInstaller.cs`
- Create: `Assets/Scripts/Installers/GlobalInstaller.cs`

- [ ] **Step 1: GlobalInstaller 생성 (DontDestroyOnLoad)**

```csharp
using DI;
using UnityEngine;

public class GlobalInstaller : MonoInstaller
{
    public override void InstallBindings()
    {
        // 전역으로 사용할 데이터 등록
        Bind(GameData.Instance);
    }

    protected override void Awake()
    {
        DontDestroyOnLoad(gameObject);
        base.Awake();
    }
}
```

- [ ] **Step 2: GameInstaller 생성 (씬별)**

```csharp
using DI;
using UnityEngine;

public class GameInstaller : MonoInstaller
{
    [SerializeField] private GameManager gameManager;

    public override void InstallBindings()
    {
        Bind(gameManager);
    }
}
```

---

## Task 5: GameData 예제 (전역 데이터)

**Files:**
- Create: `Assets/Scripts/GameData.cs`

- [ ] **Step 1: GameData Singleton**

```csharp
using UnityEngine;

public class GameData : MonoBehaviour
{
    public static GameData Instance { get; private set; }
    
    public long TotalGold { get; set; }
    public int ClickPower { get; set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
}
```

---

## 주입 시점 규칙

**중요:** `[Inject]` 필드를 사용하는 MonoBehaviour는 `Start()`에서 `DIContainer.Inject(gameObject)`를 호출해야 합니다.

```csharp
public class UIManager : MonoBehaviour
{
    [Inject] private GameData gameData;
    
    void Start()
    {
        DIContainer.Inject(gameObject); // Start에서 주입
    }
}
```

MonoInstaller는 Awake에서 컨테이너를 생성하므로, 주입받는 쪽은 Start에서 호출하여 순서를 보장합니다.

---

## Testing in Unity

1. **기본 등록/조회 테스트**
   - 빈 게임오브젝트에 GameInstaller 추가
   - Play 모드에서 DIContainer.Resolve<타입>() 호출 확인

2. **자동 주입 테스트**
   - [Inject] 필드가 있는 MonoBehaviour 생성
   - DIContainer.Inject(gameObject) 호출 후 필드 값 확인

3. **씬 전환 테스트**
   - 씬 A에 Installer A, 씬 B에 Installer B
   - 씬 전환 후 A의 컨테이너가 제거되는지 확인

---

## Notes

- InjectAttribute는 DIContainer.cs에 통합되어 하나의 파일로 관리
- MonoInstaller의 Awake/OnDestroy에서 자동으로 컨테이너 등록/해제
- 최신 컨테이너부터 역순 검색으로 씬별 오버라이드 지원
- Resolve 실패 시 null 반환 + Warning 로그