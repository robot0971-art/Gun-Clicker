using UnityEngine;
using System;

/// <summary>
/// 저장용 데이터 구조체
/// </summary>
[Serializable]
public class SaveData
{
    public long TotalGold;
    public int CurrentGunIndex;
    public int[] ClickCounts;
    public int[] UpgradeLevels;
    
    // 새로운 필드
    public GunExperienceSaveData[] GunExperiences;
    public int CurrentStage;
    public int CurrentMonsterId;
    public int CurrentMonsterHP;
}

/// <summary>
/// 총 경험치 저장용 데이터
/// </summary>
[Serializable]
public class GunExperienceSaveData
{
    public int GunId;
    public int Level;
    public int CurrentExp;
    public int ExpToNextLevel;
}

/// <summary>
/// 게임 데이터 저장/로드 (PlayerPrefs)
/// </summary>
public class SaveManager
{
    private const string SAVE_KEY = "GunClicker_SaveData";
    
    public void Save(GameData gameData)
    {
        var saveData = new SaveData
        {
            TotalGold = gameData.TotalGold,
            CurrentGunIndex = gameData.CurrentGunIndex,
            ClickCounts = gameData.ClickCounts,
            UpgradeLevels = gameData.UpgradeLevels,
            CurrentStage = gameData.CurrentStage,
            CurrentMonsterId = gameData.CurrentMonsterId,
            CurrentMonsterHP = gameData.CurrentMonsterHP
        };
        
        // GunExperiences 변환
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
            data.TotalGold = saveData.TotalGold;
            data.CurrentGunIndex = saveData.CurrentGunIndex;
            data.ClickCounts = saveData.ClickCounts ?? new int[8];
            data.UpgradeLevels = saveData.UpgradeLevels ?? new int[8];
            data.CurrentStage = saveData.CurrentStage;
            data.CurrentMonsterId = saveData.CurrentMonsterId;
            data.CurrentMonsterHP = saveData.CurrentMonsterHP;
            
            // GunExperiences 변환
            if (saveData.GunExperiences != null)
            {
                data.GunExperiences = new GunExperienceData[saveData.GunExperiences.Length];
                for (int i = 0; i < saveData.GunExperiences.Length; i++)
                {
                    var exp = saveData.GunExperiences[i];
                    data.GunExperiences[i] = new GunExperienceData(exp.GunId)
                    {
                        Level = exp.Level,
                        CurrentExp = exp.CurrentExp,
                        ExpToNextLevel = exp.ExpToNextLevel
                    };
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