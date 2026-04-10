using UnityEngine;

/// <summary>
/// 총알 동작 및 충돌 처리
/// </summary>
public class Bullet : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float speed = 20f;
    [SerializeField] private float lifeTime = 3f;
    [SerializeField] private ParticleSystem hitParticlePrefab;
    
    private int damage;
    private bool isCritical;
    private Vector3 direction;
    private float spawnTime;
    private bool isActive = false;
    
    // 풀링을 위한 참조
    private BulletPool pool;
    
    public void Initialize(BulletPool bulletPool)
    {
        pool = bulletPool;
    }
    
    /// <summary>
    /// 총알 발사 설정
    /// </summary>
    public void Fire(Vector3 startPosition, Vector3 targetDirection, int bulletDamage, bool critical = false)
    {
        transform.position = startPosition;
        direction = targetDirection.normalized;
        damage = bulletDamage;
        isCritical = critical;
        spawnTime = Time.time;
        isActive = true;
        
        // 총알 회전 (방향에 맞게)
        transform.rotation = Quaternion.LookRotation(direction);
        
        gameObject.SetActive(true);
    }
    
    private void Update()
    {
        if (!isActive) return;
        
        // 이동
        transform.position += direction * speed * Time.deltaTime;
        
        // 생명 시간 체크
        if (Time.time - spawnTime >= lifeTime)
        {
            ReturnToPool();
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (!isActive) return;
        
        // 몬스터 레이어 체크 (Monster 레이어 필요)
        if (other.CompareTag("Monster") || other.gameObject.layer == LayerMask.NameToLayer("Monster"))
        {
            // 몬스터 피격 이벤트 발생
            EventBus<MonsterHitEvent>.Publish(new MonsterHitEvent 
            { 
                MonsterId = -1, // CombatManager에서 처리
                Damage = damage,
                CurrentHP = 0,
                IsCritical = isCritical
            });
            
            // 타격 이펙트
            SpawnHitEffect();
            
            // 풀에 반환
            ReturnToPool();
        }
        
        // 벽이나 지형 체크
        if (other.CompareTag("Wall") || other.CompareTag("Ground"))
        {
            SpawnHitEffect();
            ReturnToPool();
        }
    }
    
    private void SpawnHitEffect()
    {
        if (hitParticlePrefab != null)
        {
            var particle = Instantiate(hitParticlePrefab, transform.position, Quaternion.identity);
            Destroy(particle.gameObject, 1f);
        }
    }
    
    private void ReturnToPool()
    {
        isActive = false;
        gameObject.SetActive(false);
        
        if (pool != null)
        {
            pool.ReturnBullet(this);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    /// <summary>
    /// 강제로 풀에 반환 (외부에서 호출)
    /// </summary>
    public void ForceReturn()
    {
        ReturnToPool();
    }
}
