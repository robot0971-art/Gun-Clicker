using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 총알 오브젝트 풀링 관리
/// </summary>
public class BulletPool : MonoBehaviour
{
    [Header("Pool Settings")]
    [SerializeField] private Bullet bulletPrefab;
    [SerializeField] private int poolSize = 50;
    [SerializeField] private Transform poolParent;
    
    private Queue<Bullet> bulletPool = new Queue<Bullet>();
    private List<Bullet> activeBullets = new List<Bullet>();
    
    private void Start()
    {
        InitializePool();
    }
    
    /// <summary>
    /// 풀 초기화
    /// </summary>
    private void InitializePool()
    {
        if (bulletPrefab == null)
        {
            Debug.LogError("[BulletPool] Bullet prefab is not assigned!");
            return;
        }
        
        if (poolParent == null)
        {
            poolParent = transform;
        }
        
        for (int i = 0; i < poolSize; i++)
        {
            CreateBullet();
        }
        
        Debug.Log($"[BulletPool] Initialized with {poolSize} bullets");
    }
    
    /// <summary>
    /// 새 총알 생성
    /// </summary>
    private Bullet CreateBullet()
    {
        var bullet = poolParent != null
            ? Instantiate(bulletPrefab, poolParent)
            : Instantiate(bulletPrefab);

        bullet.transform.localPosition = Vector3.zero;
        bullet.transform.localRotation = Quaternion.identity;
        bullet.Initialize(this);
        bullet.gameObject.SetActive(false);
        bulletPool.Enqueue(bullet);
        return bullet;
    }
    
    /// <summary>
    /// 총알 가져오기
    /// </summary>
    public Bullet GetBullet()
    {
        Bullet bullet;
        
        if (bulletPool.Count > 0)
        {
            bullet = bulletPool.Dequeue();
        }
        else
        {
            // 풀이 비었으면 새로 생성
            bullet = CreateBullet();
            Debug.LogWarning("[BulletPool] Pool exhausted, creating new bullet");
        }
        
        activeBullets.Add(bullet);
        return bullet;
    }
    
    /// <summary>
    /// 총알 반환
    /// </summary>
    public void ReturnBullet(Bullet bullet)
    {
        if (bullet == null) return;
        
        if (activeBullets.Contains(bullet))
        {
            activeBullets.Remove(bullet);
        }
        
        bulletPool.Enqueue(bullet);
    }
    
    /// <summary>
    /// 모든 활성 총알 반환 (씬 전환 등)
    /// </summary>
    public void ReturnAllBullets()
    {
        foreach (var bullet in activeBullets)
        {
            if (bullet != null)
            {
                bullet.gameObject.SetActive(false);
                bulletPool.Enqueue(bullet);
            }
        }
        
        activeBullets.Clear();
        Debug.Log("[BulletPool] All bullets returned to pool");
    }
    
    /// <summary>
    /// 현재 사용 가능한 총알 수
    /// </summary>
    public int AvailableCount => bulletPool.Count;
    
    /// <summary>
    /// 현재 활성화된 총알 수
    /// </summary>
    public int ActiveCount => activeBullets.Count;
}
