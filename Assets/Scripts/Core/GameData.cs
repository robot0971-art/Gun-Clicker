using System;

[Serializable]
public class GunExperienceData
{
    public int GunId;
    public int Level;
    public int CurrentExp;
    public int ExpToNextLevel;

    public GunExperienceData(int gunId)
    {
        GunId = gunId;
        Level = 1;
        CurrentExp = 0;
        ExpToNextLevel = CalculateExpForLevel(2);
    }

    private int CalculateExpForLevel(int level)
    {
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

[Serializable]
public class GameData
{
    public int CurrentGunIndex { get; set; }
    public int[] ClickCounts { get; set; }
    public int[] UpgradeLevels { get; set; }
    public GunExperienceData[] GunExperiences { get; set; }
    public int CurrentMonsterId { get; set; }
    public int CurrentMonsterHP { get; set; }
    public int CurrentStage { get; set; }

    public GameData()
    {
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
