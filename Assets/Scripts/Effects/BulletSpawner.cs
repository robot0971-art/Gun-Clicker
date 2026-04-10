using DI;
using UnityEngine;

/// <summary>
/// 총알 발사 관리 - CombatManager와 연동
/// </summary>
public class BulletSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private Transform firePoint;
    [SerializeField] private Transform targetPoint;
    
    private BulletPool bulletPool;
    private GameData gameData;
    private GameDataAsset gameDataAsset;
    
    private void Start()
    {
        // DI 주입
        gameData = DIContainer.Resolve<GameData>();
        gameDataAsset = DIContainer.Resolve<GameDataAsset>();
        
        // BulletPool 찾기 또는 생성
        bulletPool = GetComponent<BulletPool>();
        if (bulletPool == null)
        {
            bulletPool = gameObject.AddComponent<BulletPool>();
        }
        
        // 이벤트 구독
        EventBus<AttackEvent>.Subscribe(OnAttack);
    }
    
    private void OnDestroy()
    {
        EventBus<AttackEvent>.Unsubscribe(OnAttack);
    }
    
    /// <summary>
    /// 공격 이벤트 수신 시 총알 발사
    /// </summary>
    private void OnAttack(AttackEvent e)
    {
        SpawnBullet();
    }
    
    /// <summary>
    /// 총알 생성 및 발사
    /// </summary>
    private void SpawnBullet()
    {
        if (bulletPool == null || gameData == null) return;
        
        var gunData = gameDataAsset.guns[gameData.CurrentGunIndex];
        var gunExp = gameData.GunExperiences[gameData.CurrentGunIndex];
        
        // 풀에서 총알 가져오기
        Bullet bullet = bulletPool.GetBullet();
        if (bullet == null) return;
        
        // 발사 위치와 방향 계산
        Vector3 spawnPosition = firePoint != null ? firePoint.position : transform.position;
        Vector3 direction = targetPoint != null 
            ? (targetPoint.position - spawnPosition).normalized 
            : transform.forward;
        
        // 데미지 계산 (CombatManager와 동일한 로직)
        int damage = CalculateDamage(gunData, gunExp);
        bool isCritical = Random.value < gunData.CriticalChance;
        
        // 총알 발사
        bullet.Fire(spawnPosition, direction, damage, isCritical);
    }
    
    /// <summary>
    /// 데미지 계산 (CombatManager와 동일한 공식)
    /// </summary>
    private int CalculateDamage(GunData gun, GunExperienceData exp)
    {
        float damage = gun.BaseDamage;
        damage *= (1 + (exp.Level - 1) * 0.1f);
        
        var upgradeData = gameDataAsset.upgrades[gameData.CurrentGunIndex];
        var upgradeLevel = gameData.UpgradeLevels[gameData.CurrentGunIndex];
        damage *= Mathf.Pow(upgradeData.ValueMultiplier, upgradeLevel);
        
        return Mathf.RoundToInt(damage);
    }
}
