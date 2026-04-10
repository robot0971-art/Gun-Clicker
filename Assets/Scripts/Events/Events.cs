using System;

// 클릭 이벤트
public struct ClickEvent { }

// 골드 변경 이벤트
public struct MoneyChangedEvent 
{ 
    public long Amount; 
    public long Delta; 
}

// 총 해금 이벤트
public struct GunUnlockedEvent 
{ 
    public int GunId; 
}

// 총 변경 이벤트
public struct GunSwitchedEvent 
{ 
    public int GunId; 
}

// 업그레이드 구매 이벤트
public struct UpgradePurchasedEvent 
{ 
    public int GunId; 
    public int Level; 
}

// 게임 초기화 완료 이벤트
public struct GameInitializedEvent { }

// 저장 완료 이벤트
public struct SaveCompletedEvent { }

// 로드 완료 이벤트
public struct LoadCompletedEvent { }