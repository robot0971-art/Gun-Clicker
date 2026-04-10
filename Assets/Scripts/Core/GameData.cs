using System;

/// <summary>
/// 총별 경험치/레벨 데이터
/// </summary>
[Serializable]
public class GunExperienceData
{
    public int GunId;
    public int Level;              // 현재 레벨 (1부터 시작)
    public int CurrentExp;         // 현재 경험치
    public int ExpToNextLevel;     // 다음 레벨까지 필요한 경험치
    
    public GunExperienceData(int gunId)
    {
        GunId = gunId;
        Level = 1;
        CurrentExp = 0;
        ExpToNextLevel = CalculateExpForLevel(2);
    }
    
    private int CalculateExpForLevel(int level)
    {
        // 레벨업 필요 경험치: 레벨^2 * 100
        return level * level * 100;
    }
    
    public void AddExp(int exp)
    {
        CurrentExp += exp;
        while (CurrentExp >= ExpToNextLevel && Level < 100)
        {
            CurrentExp -= ExpToNextLevel;
            Level++;
            ExpToNextLevel = CalculateExpForLevel(Level + 1);
        }
    }
}

/// <summary>
/// 런타임 게임 상태 (DI Container로 관리)
/// </summary>
public class GameData
{
    public long TotalGold { get; set; }
    public int CurrentGunIndex { get; set; }
    public int[] ClickCounts { get; set; }
    public int[] UpgradeLevels { get; set; }
    public GunExperienceData[] GunExperiences { get; set; }
    public int CurrentMonsterId { get; set; }
    public int CurrentMonsterHP { get; set; }
    public int CurrentStage { get; set; }
    
    public GameData()
    {
        TotalGold = 0;
        CurrentGunIndex = 0;
        ClickCounts = new int[8];
        UpgradeLevels = new int[8];
        GunExperiences = new GunExperienceData[8];
        for (int i = 0; i < 8; i++)
        {
            GunExperiences[i] = new GunExperienceData(i);
        }
        CurrentMonsterId = 0;
        CurrentMonsterHP = 0;
        CurrentStage = 1;
    }
    
    public void Reset()
    {
        TotalGold = 0;
        CurrentGunIndex = 0;
        ClickCounts = new int[8];
        UpgradeLevels = new int[8];
        GunExperiences = new GunExperienceData[8];
        for (int i = 0; i < 8; i++)
        {
            GunExperiences[i] = new GunExperienceData(i);
        }
        CurrentMonsterId = 0;
        CurrentMonsterHP = 0;
        CurrentStage = 1;
    }
}