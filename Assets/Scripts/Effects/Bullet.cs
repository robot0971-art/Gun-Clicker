using UnityEngine;

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
    private BulletPool pool;

    public void Initialize(BulletPool bulletPool)
    {
        pool = bulletPool;
    }

    public void Fire(Vector3 startPosition, Vector3 targetDirection, int bulletDamage, bool critical = false)
    {
        transform.position = startPosition;
        direction = targetDirection.normalized;
        damage = bulletDamage;
        isCritical = critical;
        spawnTime = Time.time;
        isActive = true;

        if (direction.sqrMagnitude > 0.0001f)
        {
            transform.right = direction;
        }

        var rigidbody = GetComponent<Rigidbody>();
        if (rigidbody != null)
        {
            rigidbody.useGravity = false;
            rigidbody.linearVelocity = Vector3.zero;
            rigidbody.angularVelocity = Vector3.zero;
        }

        gameObject.SetActive(true);
        Debug.Log($"[Bullet] Fire pos={startPosition} dir={direction} damage={damage} crit={isCritical}");
    }

    private void Update()
    {
        if (!isActive) return;

        transform.position += direction * speed * Time.deltaTime;

        if (Time.time - spawnTime >= lifeTime)
        {
            ReturnToPool();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isActive) return;

        Debug.Log($"[Bullet] Trigger hit {other.name} tag={other.tag} layer={other.gameObject.layer}");

        if (other.CompareTag("Monster") || other.gameObject.layer == LayerMask.NameToLayer("Monster"))
        {
            Debug.Log($"[Bullet] Monster hit damage={damage} crit={isCritical}");
            var slime = other.GetComponentInParent<Slime>();
            if (slime != null)
            {
                slime.TakeDamage(damage, isCritical);
            }
            else
            {
                EventBus<MonsterHitEvent>.Publish(new MonsterHitEvent
                {
                    MonsterId = -1,
                    Damage = damage,
                    CurrentHP = 0,
                    IsCritical = isCritical
                });
            }

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

    public void ForceReturn()
    {
        ReturnToPool();
    }
}
