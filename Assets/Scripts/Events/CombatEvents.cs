using System;

// 공격 이벤트 (클릭 시 발생)
public struct AttackEvent 
{ 
    public int GunId;
    public int Damage;
    public bool IsCritical;
}

// 몬스터 피격 이벤트
public struct MonsterHitEvent 
{ 
    public int MonsterId;
    public int Damage;
    public int CurrentHP;
    public bool IsCritical;
}

// 몬스터 사망 이벤트
public struct MonsterKilledEvent 
{ 
    public int MonsterId;
    public int ExpReward;
    public int GoldReward;
}

// 새 몬스터 스폰 이벤트
public struct MonsterSpawnedEvent 
{ 
    public int MonsterId;
    public int MaxHP;
    public string MonsterName;
}

// 크리티컬 발생 이벤트 (UI 이펙트용)
public struct CriticalHitEvent 
{ 
    public int Damage;
    public int MonsterId;
}

// 레벨업 이벤트
public struct GunLevelUpEvent 
{ 
    public int GunId;
    public int NewLevel;
}

// 진화 체크 이벤트
public struct CheckEvolutionEvent 
{ 
    public int GunId;
    public int CurrentLevel;
}

// 진화 완료 이벤트
public struct GunEvolvedEvent 
{ 
    public int PreviousGunId;
    public int NewGunId;
    public string NewGunName;
}
