using System.Collections;
using UnityEngine;

public class MuzzleFlashPool : Pool<MuzzleFlashItem>
{
    [SerializeField] private float fallbackDuration = 2f;

    public void Play(Vector3 position, Quaternion rotation, float scale, float duration = -1f)
    {
        var item = Get();
        if (item == null)
        {
            return;
        }

        item.transform.SetPositionAndRotation(position, rotation);
        item.transform.localScale = Vector3.one * scale;
        item.Play();

        StartCoroutine(ReturnAfterDelay(item, duration > 0f ? duration : fallbackDuration));
    }

    private IEnumerator ReturnAfterDelay(MuzzleFlashItem item, float duration)
    {
        yield return new WaitForSeconds(duration);

        if (item != null && item.gameObject.activeSelf)
        {
            item.Stop();
            Release(item);
        }
    }

    protected override MuzzleFlashItem CreateItem()
    {
        var instance = Instantiate(prefab, poolParent);
        var item = instance.GetComponent<MuzzleFlashItem>();

        if (item == null)
        {
            item = instance.AddComponent<MuzzleFlashItem>();
        }

        item.Initialize();
        instance.gameObject.SetActive(false);
        allItems.Add(item);
        pooledItems.Enqueue(item);
        return item;
    }
}

public class MuzzleFlashItem : MonoBehaviour
{
    [SerializeField] private ParticleSystem particleSystemComponent;

    public void Initialize()
    {
        if (particleSystemComponent == null)
        {
            particleSystemComponent = GetComponent<ParticleSystem>();
        }

        if (particleSystemComponent == null)
        {
            particleSystemComponent = GetComponentInChildren<ParticleSystem>(true);
        }
    }

    public void Play()
    {
        if (particleSystemComponent == null)
        {
            return;
        }

        particleSystemComponent.Clear(true);
        particleSystemComponent.Play(true);
    }

    public void Stop()
    {
        if (particleSystemComponent == null)
        {
            return;
        }

        particleSystemComponent.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }
}
