using System.Collections.Generic;
using UnityEngine;

public class Pool<T> : MonoBehaviour where T : MonoBehaviour
{
    [Header("Pool Settings")]
    [SerializeField] protected GameObject prefab;
    [SerializeField] protected int poolSize = 10;
    [SerializeField] protected Transform poolParent;

    protected readonly Queue<T> pooledItems = new Queue<T>();
    protected readonly List<T> allItems = new List<T>();
    protected bool isInitialized;

    protected virtual void Awake()
    {
        if (poolParent == null)
        {
            poolParent = transform;
        }
    }

    public virtual void Prewarm(int count)
    {
        if (prefab == null)
        {
            Debug.LogError($"[{GetType().Name}] Prefab is not assigned.");
            return;
        }

        for (int i = 0; i < count; i++)
        {
            CreateItem();
        }

        isInitialized = true;
    }

    public virtual T Get()
    {
        if (prefab == null)
        {
            Debug.LogError($"[{GetType().Name}] Prefab is not assigned.");
            return null;
        }

        if (!isInitialized)
        {
            Prewarm(poolSize);
        }

        T item;
        if (pooledItems.Count > 0)
        {
            item = pooledItems.Dequeue();
        }
        else
        {
            item = CreateItem();
        }

        if (item != null)
        {
            item.gameObject.SetActive(true);
        }

        return item;
    }

    public virtual void Release(T item)
    {
        if (item == null)
        {
            Debug.LogWarning($"[{GetType().Name}] Trying to release null item.");
            return;
        }

        if (!allItems.Contains(item))
        {
            Debug.LogWarning($"[{GetType().Name}] Trying to release item not from this pool: {item.name}");
            return;
        }

        if (!item.gameObject.activeSelf)
        {
            Debug.LogWarning($"[{GetType().Name}] Item already deactivated: {item.name}");
            return;
        }

        item.gameObject.SetActive(false);
        item.transform.SetParent(poolParent);
        pooledItems.Enqueue(item);
    }

    protected virtual T CreateItem()
    {
        var instance = Instantiate(prefab, poolParent);
        var component = instance.GetComponent<T>();

        if (component == null)
        {
            Debug.LogError($"[{GetType().Name}] Prefab does not contain component of type {typeof(T).Name}");
            Destroy(instance);
            return null;
        }

        instance.gameObject.SetActive(false);
        allItems.Add(component);
        pooledItems.Enqueue(component);
        return component;
    }

    protected virtual void OnDestroy()
    {
        foreach (var item in allItems)
        {
            if (item != null)
            {
                Destroy(item.gameObject);
            }
        }

        pooledItems.Clear();
        allItems.Clear();
    }
}