using DI;
using UnityEngine;

public class GameManager
{
    private GameDataAsset gameDataAsset;
    private GameData gameData;
    private SaveManager saveManager;

    private bool initialized = false;

    public void Initialize()
    {
        if (initialized) return;

        gameDataAsset = DIContainer.Resolve<GameDataAsset>();
        gameData = DIContainer.Resolve<GameData>();
        saveManager = DIContainer.Resolve<SaveManager>();

        if (saveManager.HasSaveData())
        {
            var savedData = saveManager.Load();
            gameData.CurrentGunIndex = savedData.CurrentGunIndex;
            gameData.ClickCounts = savedData.ClickCounts;
            gameData.UpgradeLevels = savedData.UpgradeLevels;
            gameData.GunExperiences = savedData.GunExperiences;
            gameData.CurrentStage = savedData.CurrentStage;
            gameData.CurrentMonsterId = savedData.CurrentMonsterId;
            gameData.CurrentMonsterHP = savedData.CurrentMonsterHP;
        }

        EventBus<MonsterKilledEvent>.Subscribe(OnMonsterKilled);

        initialized = true;
        EventBus<GameInitializedEvent>.Publish(new GameInitializedEvent());

        Debug.Log("[GameManager] Initialized");
    }

    public void Dispose()
    {
        EventBus<MonsterKilledEvent>.Unsubscribe(OnMonsterKilled);

        saveManager.Save(gameData);
    }

    private void OnMonsterKilled(MonsterKilledEvent e)
    {
        saveManager.Save(gameData);
    }

    public int GetCurrentGunIndex()
    {
        return gameData.CurrentGunIndex;
    }
}
