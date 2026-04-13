using System;

public struct AttackEvent
{
    public int GunId;
    public int Damage;
    public bool IsCritical;
}

public struct MonsterHitEvent
{
    public int MonsterId;
    public int Damage;
    public int CurrentHP;
    public bool IsCritical;
}

public struct MonsterKilledEvent
{
    public int MonsterId;
    public int ExpReward;
}

public struct MonsterSpawnedEvent
{
    public int MonsterId;
    public int MaxHP;
    public string MonsterName;
}

public struct CriticalHitEvent
{
    public int Damage;
    public int MonsterId;
}

public struct GunLevelUpEvent
{
    public int GunId;
    public int NewLevel;
}

public struct CheckEvolutionEvent
{
    public int GunId;
    public int CurrentLevel;
}

public struct GunEvolvedEvent
{
    public int PreviousGunId;
    public int NewGunId;
    public string NewGunName;
}
