using DI;
using UnityEngine;

/// <summary>
/// 진화 시스템: 레벨 조건 충족 시 다음 총으로 진화
/// </summary>
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

        // 이벤트 구독
        EventBus<CheckEvolutionEvent>.Subscribe(OnCheckEvolution);

        initialized = true;
        Debug.Log("[EvolutionSystem] Initialized");
    }

    public void Dispose()
    {
        EventBus<CheckEvolutionEvent>.Unsubscribe(OnCheckEvolution);
    }

    /// <summary>
    /// 진화 조건 체크
    /// </summary>
    private void OnCheckEvolution(CheckEvolutionEvent e)
    {
        var gunData = gameDataAsset.guns[e.GunId];

        // 최종 형태인지 체크
        if (gunData.IsFinalForm || gunData.NextGunId < 0)
        {
            Debug.Log($"[EvolutionSystem] Gun {e.GunId} is already at final form");
            return;
        }

        // 진화 레벨에 도달했는지 체크
        if (e.CurrentLevel >= gunData.EvolveLevel)
        {
            EvolveGun(e.GunId, gunData.NextGunId);
        }
    }

    /// <summary>
    /// 총 진화 실행
    /// </summary>
    private void EvolveGun(int currentGunId, int nextGunId)
    {
        Debug.Log($"[EvolutionSystem] Evolving Gun {currentGunId} -> Gun {nextGunId}");

        // 다음 총 데이터 유효성 체크
        if (nextGunId < 0 || nextGunId >= gameDataAsset.guns.Count)
        {
            Debug.LogError($"[EvolutionSystem] Invalid next gun ID: {nextGunId}");
            return;
        }

        var nextGunData = gameDataAsset.guns[nextGunId];

        // 진화 이벤트 발행 (UI 이펙트용)
        EventBus<GunEvolvedEvent>.Publish(new GunEvolvedEvent
        {
            PreviousGunId = currentGunId,
            NewGunId = nextGunId,
            NewGunName = nextGunData.Name
        });

        // 현재 장착 총이 진화한 총이면 자동 교체
        if (gameData.CurrentGunIndex == currentGunId)
        {
            gameData.CurrentGunIndex = nextGunId;

            EventBus<GunSwitchedEvent>.Publish(new GunSwitchedEvent
            {
                GunId = nextGunId
            });
        }

        // 해금 처리 (진화된 총 해금)
        gameData.ClickCounts[nextGunId] = 1;

        // 저장
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
