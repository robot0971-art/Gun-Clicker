using DI;
using UnityEngine;

public class BulletSpawner : MonoBehaviour
{
    [SerializeField] private Transform firePoint;
    [SerializeField] private Transform targetPoint;
    [SerializeField] private BulletPool bulletPool;

    private GameData gameData;
    private GameDataAsset gameDataAsset;

    private void Start()
    {
        gameData = DIContainer.Resolve<GameData>();
        gameDataAsset = DIContainer.Resolve<GameDataAsset>();

        if (gameData == null || gameDataAsset == null)
        {
            Debug.LogError("[BulletSpawner] Missing GameData/GameDataAsset. Add GlobalInstaller to the scene.");
            enabled = false;
            return;
        }

        if (bulletPool == null)
        {
            bulletPool = GetComponent<BulletPool>();
        }

        if (bulletPool == null)
        {
            bulletPool = FindObjectOfType<BulletPool>();
        }

        if (bulletPool == null)
        {
            Debug.LogError("[BulletSpawner] Missing BulletPool reference. Assign the BulletPool object in the Inspector.");
            enabled = false;
            return;
        }

        EventBus<AttackEvent>.Subscribe(OnAttack);
    }

    private void OnDestroy()
    {
        EventBus<AttackEvent>.Unsubscribe(OnAttack);
    }

    private void OnAttack(AttackEvent e)
    {
        Debug.Log("[BulletSpawner] Attack received");
        SpawnBullet();
    }

    private void SpawnBullet()
    {
        if (bulletPool == null || gameData == null || gameDataAsset == null) return;

        var gunData = gameDataAsset.guns[gameData.CurrentGunIndex];
        var gunExp = gameData.GunExperiences[gameData.CurrentGunIndex];

        Bullet bullet = bulletPool.GetBullet();
        if (bullet == null) return;

        Vector3 spawnPosition = firePoint != null ? firePoint.position : transform.position;
        Vector3 direction = firePoint != null ? firePoint.right : transform.right;

        if (direction.sqrMagnitude < 0.0001f)
        {
            direction = Vector3.right;
        }

        spawnPosition += direction.normalized * 0.2f;

        int baseDamage = CalculateDamage(gunData, gunExp);
        bool isCritical = Random.value < gunData.CriticalChance;
        int damage = isCritical ? baseDamage * 2 : baseDamage;

        Debug.Log($"[BulletSpawner] Spawn bullet at {spawnPosition} dir {direction} crit={isCritical} dmg={damage}");
        bullet.Fire(spawnPosition, direction, damage, isCritical);
    }

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
