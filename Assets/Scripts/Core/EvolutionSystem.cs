using DI;
using UnityEngine;

public class EvolutionSystem
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

        EventBus<CheckEvolutionEvent>.Subscribe(OnCheckEvolution);

        initialized = true;
        Debug.Log("[EvolutionSystem] Initialized");
    }

    public void Dispose()
    {
        EventBus<CheckEvolutionEvent>.Unsubscribe(OnCheckEvolution);
    }

    private void OnCheckEvolution(CheckEvolutionEvent e)
    {
        var gunData = gameDataAsset.guns[e.GunId];

        if (gunData.IsFinalForm || gunData.NextGunId < 0)
        {
            Debug.Log($"[EvolutionSystem] Gun {e.GunId} is already at final form");
            return;
        }

        if (e.CurrentLevel >= gunData.EvolveLevel)
        {
            EvolveGun(e.GunId, gunData.NextGunId);
        }
    }

    private void EvolveGun(int currentGunId, int nextGunId)
    {
        Debug.Log($"[EvolutionSystem] Evolving Gun {currentGunId} -> Gun {nextGunId}");

        if (nextGunId < 0 || nextGunId >= gameDataAsset.guns.Count)
        {
            Debug.LogError($"[EvolutionSystem] Invalid next gun ID: {nextGunId}");
            return;
        }

        if (gameData.CurrentGunIndex == currentGunId)
        {
            gameData.CurrentGunIndex = nextGunId;
        }

        gameData.ClickCounts[nextGunId] = 1;
        var nextGunData = gameDataAsset.guns[nextGunId];

        EventBus<GunEvolvedEvent>.Publish(new GunEvolvedEvent
        {
            PreviousGunId = currentGunId,
            NewGunId = nextGunId,
            NewGunName = nextGunData.Name
        });

        saveManager.Save(gameData);

        Debug.Log($"[EvolutionSystem] Evolution complete! New gun: {nextGunData.Name}");
    }

    public bool CanEvolve(int gunId)
    {
        if (gunId < 0 || gunId >= gameDataAsset.guns.Count)
            return false;

        var gunData = gameDataAsset.guns[gunId];
        if (gunData.IsFinalForm || gunData.NextGunId < 0)
            return false;

        var gunExp = gameData.GunExperiences[gunId];
        return gunExp.Level >= gunData.EvolveLevel;
    }
}
