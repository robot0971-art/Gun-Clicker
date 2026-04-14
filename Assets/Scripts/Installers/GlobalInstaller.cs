using DI;
using UnityEngine;

public class GlobalInstaller : MonoInstaller
{
    public override void InstallBindings()
    {
        // Static data (from Excel)
        var gameDataAsset = Resources.Load<GameDataAsset>("GameDataAsset");
        Bind(gameDataAsset);
        
        // Runtime state
        Bind(new GameData());
        
        // Services
        Bind(new SaveManager());
        Bind(new GameManager());
        
        // Core Managers
        Bind(new CombatManager());        // 전투 시스템
        Bind(new ExperienceSystem());     // 경험치 시스템
        Bind(new EvolutionSystem());      // 진화 시스템
        
        // Initialize all managers
        var gameManager = DIContainer.Resolve<GameManager>();
        gameManager.Initialize();
        
        var combatManager = DIContainer.Resolve<CombatManager>();
        combatManager.Initialize();
        
        var expSystem = DIContainer.Resolve<ExperienceSystem>();
        expSystem.Initialize();
        
        var evoSystem = DIContainer.Resolve<EvolutionSystem>();
        evoSystem.Initialize();
        
        Debug.Log("[GlobalInstaller] All services registered and initialized");
    }

    protected override void Awake()
    {
        DontDestroyOnLoad(gameObject);
        base.Awake();
    }
    
    protected override void OnDestroy()
    {
        // Dispose in reverse order
        var evoSystem = DIContainer.Resolve<EvolutionSystem>();
        evoSystem?.Dispose();
        
        var expSystem = DIContainer.Resolve<ExperienceSystem>();
        expSystem?.Dispose();
        
        var combatManager = DIContainer.Resolve<CombatManager>();
        combatManager?.Dispose();
        
        var gameManager = DIContainer.Resolve<GameManager>();
        gameManager?.Dispose();
        
        base.OnDestroy();
    }
}
