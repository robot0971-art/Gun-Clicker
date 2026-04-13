using UnityEngine;

public class SlimePool : Pool<Slime>
{
    [Header("Slime Pool Settings")]
    [SerializeField] private int slimePoolSize = 8;

    protected override void Awake()
    {
        poolSize = slimePoolSize;
        base.Awake();
    }

    public Slime GetSlime(Vector3 position, int monsterId, int monsterHP)
    {
        var slime = Get();
        if (slime != null)
        {
            slime.SetOwnerPool(this);
            slime.transform.position = position;
            slime.InitializeSpawn(position, monsterId, monsterHP);
        }
        return slime;
    }

    public void PrewarmSlimes(int count)
    {
        Prewarm(count);
    }
}