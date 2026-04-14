using System;

[Serializable]
public class GunExperienceData
{
    public const int MaxGunLevel = 5;
    private static int expPerLevelStep = 100;

    public int GunId;
    public int Level;
    public int CurrentExp;
    public int ExpToNextLevel;

    public GunExperienceData(int gunId)
    {
        GunId = gunId;
        Level = 1;
        CurrentExp = 0;
        ExpToNextLevel = CalculateRequiredExpForLevel(2);
    }

    public static int CalculateRequiredExpForLevel(int level)
    {
        return (level - 1) * expPerLevelStep;
    }

    public static void ConfigureExperience(int newExpPerLevelStep)
    {
        expPerLevelStep = Math.Max(1, newExpPerLevelStep);
    }

    public void AddExp(int exp)
    {
        if (Level >= MaxGunLevel)
        {
            Level = MaxGunLevel;
            CurrentExp = 0;
            ExpToNextLevel = 0;
            return;
        }

        CurrentExp += exp;
        while (CurrentExp >= ExpToNextLevel && Level < MaxGunLevel)
        {
            CurrentExp -= ExpToNextLevel;
            Level++;
            ExpToNextLevel = Level >= MaxGunLevel ? 0 : CalculateRequiredExpForLevel(Level + 1);
        }

        if (Level >= MaxGunLevel)
        {
            Level = MaxGunLevel;
            CurrentExp = 0;
            ExpToNextLevel = 0;
        }
    }

    public void Normalize()
    {
        Level = Math.Clamp(Level, 1, MaxGunLevel);

        if (Level >= MaxGunLevel)
        {
            CurrentExp = 0;
            ExpToNextLevel = 0;
            return;
        }

        ExpToNextLevel = CalculateRequiredExpForLevel(Level + 1);
        CurrentExp = Math.Clamp(CurrentExp, 0, Math.Max(0, ExpToNextLevel - 1));
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
