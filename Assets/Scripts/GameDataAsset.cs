using System;
using System.Collections.Generic;
using UnityEngine;
using ExcelConverter;

[CreateAssetMenu(fileName = "GameDataAsset", menuName = "Game/DataAsset")]
public class GameDataAsset : ScriptableObject
{
    [Sheet("Guns")]
    public List<GunData> guns;
    
    [Sheet("Upgrades")]
    public List<UpgradeData> upgrades;
    
    [Sheet("Config")]
    public List<ConfigData> config;
    
    [Sheet("Monsters")]
    public List<MonsterData> monsters;
}

[Serializable]
public class GunData
{
    public int Id;
    public string Name;
    
    // 기존 필드 (하위 호환성 유지)
    public int ClickValue;         // [Deprecated] 기존 클릭 가치
    public int UnlockClicks;       // 해금에 필요한 클릭 수
    public string SpriteName;
    
    // 새로운 전투 필드
    public int BaseDamage;         // 기본 공격력
    public float AttackSpeed;      // 연사력 (쿨타임 감소)
    public float CriticalChance;   // 크리티컬 확률 (0.0 ~ 1.0)
    public float CriticalMultiplier; // 크리티컬 배율 (예: 2.0 = 2배)
    
    // 새로운 진화 필드
    public int EvolveLevel;        // 진화에 필요한 레벨
    public int NextGunId;          // 진화 후 변경될 총 ID (-1이면 최종)
    public bool IsFinalForm;       // 최종 형태 여부
}

[Serializable]
public class UpgradeData
{
    public int Id;
    public int GunId;
    public int BaseCost;
    public float CostMultiplier;
    public float ValueMultiplier;
    public int MaxLevel;
}

[Serializable]
public class ConfigData
{
    public int Id;
    public string Key;
    public int ValueInt;
    public float ValueFloat;
    public string ValueString;
}