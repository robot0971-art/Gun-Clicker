using System;
using UnityEngine;

[Serializable]
public class MonsterData
{
    public int Id;
    public string Name;
    public int BaseHP;
    public int BaseDefense;        // 방어력 (데미지 감소)
    public int ExpReward;          // 처치 시 경험치
    public int GoldReward;         // 처치 시 골드
    public float HpScaling;        // 스테이지별 HP 증가율
    public string SpriteName;
}
