using System;
using System.Collections.Generic;
using ExcelConverter;
using UnityEngine;

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
    public int ClickValue;
    public int UnlockClicks;
    public string SpriteName;
    public Sprite GunSprite;
    public Vector3 FirePointOffset;
    public Vector3 MuzzleFlashOffset;

    public int BaseDamage;
    public float AttackSpeed;
    public float CriticalChance;
    public float CriticalMultiplier;

    public int EvolveLevel;
    public int NextGunId;
    public bool IsFinalForm;
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
