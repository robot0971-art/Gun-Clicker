using DI;
using UnityEngine;

public class BulletSpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform firePoint;
    [SerializeField] private Transform targetPoint;
    [SerializeField] private BulletPool bulletPool;
    [SerializeField] private MuzzleFlashPool muzzleFlashPool;

    [Header("Muzzle Flash")]
    [SerializeField] private ParticleSystem muzzleFlashPrefab;
    [SerializeField] private Vector3 muzzleFlashPositionOffset = Vector3.zero;
    [SerializeField] private Vector3 muzzleFlashRotationOffset = Vector3.zero;
    [SerializeField] private float muzzleFlashScale = 1f;
    [SerializeField] private float muzzleFlashLifetime = 2f;

    [Header("Level Tuning")]
    [SerializeField, Range(0f, 0.5f)] private float levelDamageBonusPerLevel = 0.05f;

    private GameData gameData;
    private GameDataAsset gameDataAsset;
    private Vector3 defaultFirePointLocalPosition;

    private void Start()
    {
        gameData = DIContainer.Resolve<GameData>();
        gameDataAsset = DIContainer.Resolve<GameDataAsset>();

        if (firePoint != null)
        {
            defaultFirePointLocalPosition = firePoint.localPosition;
        }

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

        ApplyCurrentGunOffsets();
        EventBus<AttackEvent>.Subscribe(OnAttack);
        EventBus<GunEvolvedEvent>.Subscribe(OnGunEvolved);
    }

    private void OnDestroy()
    {
        EventBus<AttackEvent>.Unsubscribe(OnAttack);
        EventBus<GunEvolvedEvent>.Unsubscribe(OnGunEvolved);
    }

    private void OnAttack(AttackEvent e)
    {
        Debug.Log("[BulletSpawner] Attack received");
        SpawnBullet();
    }

    private void OnGunEvolved(GunEvolvedEvent e)
    {
        ApplyCurrentGunOffsets();
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

        SpawnMuzzleFlash(spawnPosition, direction);
        spawnPosition += direction.normalized * 0.2f;

        int baseDamage = CalculateDamage(gunData, gunExp);
        bool isCritical = Random.value < gunData.CriticalChance;
        int damage = isCritical ? baseDamage * 2 : baseDamage;

        Debug.Log($"[BulletSpawner] Spawn bullet at {spawnPosition} dir {direction} crit={isCritical} dmg={damage}");
        bullet.Fire(spawnPosition, direction, damage, isCritical);
    }

    private void SpawnMuzzleFlash(Vector3 spawnPosition, Vector3 direction)
    {
        if (muzzleFlashPrefab == null) return;

        Vector3 gunMuzzleFlashOffset = GetCurrentGunMuzzleFlashOffset();
        Quaternion baseRotation = Quaternion.FromToRotation(Vector3.right, direction.normalized);
        Vector3 worldOffset = baseRotation * (muzzleFlashPositionOffset + gunMuzzleFlashOffset);
        Quaternion finalRotation = baseRotation * Quaternion.Euler(muzzleFlashRotationOffset);
        Vector3 finalPosition = spawnPosition + worldOffset;

        if (muzzleFlashPool != null)
        {
            muzzleFlashPool.Play(finalPosition, finalRotation, muzzleFlashScale, muzzleFlashLifetime);
            return;
        }

        ParticleSystem muzzleFlash = Instantiate(muzzleFlashPrefab, finalPosition, finalRotation);
        muzzleFlash.transform.localScale = Vector3.one * muzzleFlashScale;
        muzzleFlash.Play();
        Destroy(muzzleFlash.gameObject, muzzleFlashLifetime);
    }

    private int CalculateDamage(GunData gun, GunExperienceData exp)
    {
        float damage = gun.BaseDamage;
        damage *= 1 + (exp.Level - 1) * levelDamageBonusPerLevel;

        var upgradeData = gameDataAsset.upgrades[gameData.CurrentGunIndex];
        var upgradeLevel = gameData.UpgradeLevels[gameData.CurrentGunIndex];
        damage *= Mathf.Pow(upgradeData.ValueMultiplier, upgradeLevel);

        return Mathf.RoundToInt(damage);
    }

    private void ApplyCurrentGunOffsets()
    {
        if (firePoint == null || gameData == null || gameDataAsset == null || gameDataAsset.guns == null)
        {
            return;
        }

        int gunIndex = gameData.CurrentGunIndex;
        if (gunIndex < 0 || gunIndex >= gameDataAsset.guns.Count)
        {
            firePoint.localPosition = defaultFirePointLocalPosition;
            return;
        }

        firePoint.localPosition = defaultFirePointLocalPosition + gameDataAsset.guns[gunIndex].FirePointOffset;
    }

    private Vector3 GetCurrentGunMuzzleFlashOffset()
    {
        if (gameData == null || gameDataAsset == null || gameDataAsset.guns == null)
        {
            return Vector3.zero;
        }

        int gunIndex = gameData.CurrentGunIndex;
        if (gunIndex < 0 || gunIndex >= gameDataAsset.guns.Count)
        {
            return Vector3.zero;
        }

        return gameDataAsset.guns[gunIndex].MuzzleFlashOffset;
    }
}
