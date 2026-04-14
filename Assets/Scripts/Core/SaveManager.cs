using System;
using UnityEngine;

[Serializable]
public class SaveData
{
    public int CurrentGunIndex;
    public int[] ClickCounts;
    public int[] UpgradeLevels;
    public GunExperienceSaveData[] GunExperiences;
    public int CurrentStage;
    public int CurrentMonsterId;
    public int CurrentMonsterHP;
}

[Serializable]
public class GunExperienceSaveData
{
    public int GunId;
    public int Level;
    public int CurrentExp;
    public int ExpToNextLevel;
}

public class SaveManager
{
    private const string SAVE_KEY = "GunClicker_SaveData";

    public void Save(GameData gameData)
    {
        var saveData = new SaveData
        {
            CurrentGunIndex = gameData.CurrentGunIndex,
            ClickCounts = gameData.ClickCounts,
            UpgradeLevels = gameData.UpgradeLevels,
            CurrentStage = gameData.CurrentStage,
            CurrentMonsterId = gameData.CurrentMonsterId,
            CurrentMonsterHP = gameData.CurrentMonsterHP
        };

        saveData.GunExperiences = new GunExperienceSaveData[gameData.GunExperiences.Length];
        for (int i = 0; i < gameData.GunExperiences.Length; i++)
        {
            var exp = gameData.GunExperiences[i];
            saveData.GunExperiences[i] = new GunExperienceSaveData
            {
                GunId = exp.GunId,
                Level = exp.Level,
                CurrentExp = exp.CurrentExp,
                ExpToNextLevel = exp.ExpToNextLevel
            };
        }

        string json = JsonUtility.ToJson(saveData);
        PlayerPrefs.SetString(SAVE_KEY, json);
        PlayerPrefs.Save();

        Debug.Log("[SaveManager] Game saved");
        EventBus<SaveCompletedEvent>.Publish(new SaveCompletedEvent());
    }

    public GameData Load()
    {
        var data = new GameData();

        if (!PlayerPrefs.HasKey(SAVE_KEY))
        {
            EventBus<LoadCompletedEvent>.Publish(new LoadCompletedEvent());
            return data;
        }

        string json = PlayerPrefs.GetString(SAVE_KEY);
        SaveData saveData = JsonUtility.FromJson<SaveData>(json);

        if (saveData != null)
        {
            data.CurrentGunIndex = saveData.CurrentGunIndex;
            data.ClickCounts = saveData.ClickCounts ?? new int[8];
            data.UpgradeLevels = saveData.UpgradeLevels ?? new int[8];
            data.CurrentStage = saveData.CurrentStage;
            data.CurrentMonsterId = saveData.CurrentMonsterId;
            data.CurrentMonsterHP = saveData.CurrentMonsterHP;

            if (saveData.GunExperiences != null)
            {
                data.GunExperiences = new GunExperienceData[saveData.GunExperiences.Length];
                for (int i = 0; i < saveData.GunExperiences.Length; i++)
                {
                    var exp = saveData.GunExperiences[i];
                    var loadedExp = new GunExperienceData(exp.GunId)
                    {
                        Level = exp.Level,
                        CurrentExp = exp.CurrentExp,
                        ExpToNextLevel = exp.ExpToNextLevel
                    };
                    loadedExp.Normalize();
                    data.GunExperiences[i] = loadedExp;
                }
            }
        }

        EventBus<LoadCompletedEvent>.Publish(new LoadCompletedEvent());
        return data;
    }

    public void Clear()
    {
        PlayerPrefs.DeleteKey(SAVE_KEY);
        PlayerPrefs.Save();
    }

    public bool HasSaveData()
    {
        return PlayerPrefs.HasKey(SAVE_KEY);
    }
}
