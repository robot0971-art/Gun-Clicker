using UnityEngine;
using UnityEngine.InputSystem;
using DI;

public class ClickHandler : MonoBehaviour
{
    [SerializeField] private bool useRaycast = false;
    [Header("Attack Tuning")]
    [SerializeField] private bool useGunAttackSpeed = true;
    [SerializeField] private float manualAttackCooldown = 0.1f;
    [SerializeField] private float minimumAttackCooldown = 0.01f;

    private Camera mainCamera;
    private float lastAttackTime;
    private GameDataAsset gameDataAsset;
    private GameData gameData;

    private void Start()
    {
        mainCamera = Camera.main;
        lastAttackTime = 0f;
        gameDataAsset = DIContainer.Resolve<GameDataAsset>();
        gameData = DIContainer.Resolve<GameData>();
    }

    private void Update()
    {
        if (Mouse.current == null || !Mouse.current.leftButton.wasPressedThisFrame)
            return;

        if (Time.time - lastAttackTime < GetAttackCooldown())
            return;

        if (useRaycast)
        {
            HandleRaycastClick();
        }
        else
        {
            PublishClick();
        }
    }

    private void HandleRaycastClick()
    {
        if (mainCamera == null)
            return;

        var ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (Physics.Raycast(ray, out var hit))
        {
            if (hit.collider != null && hit.collider.gameObject == gameObject)
            {
                PublishClick();
            }
        }
    }

    private void PublishClick()
    {
        lastAttackTime = Time.time;

        EventBus<AttackEvent>.Publish(new AttackEvent
        {
            GunId = -1,
            Damage = 0,
            IsCritical = false
        });

        EventBus<ClickEvent>.Publish(new ClickEvent());
    }

    private float GetAttackCooldown()
    {
        if (!useGunAttackSpeed)
        {
            return Mathf.Max(minimumAttackCooldown, manualAttackCooldown);
        }

        if (gameDataAsset == null || gameData == null || gameDataAsset.guns == null)
        {
            return Mathf.Max(minimumAttackCooldown, manualAttackCooldown);
        }

        int gunIndex = gameData.CurrentGunIndex;
        if (gunIndex < 0 || gunIndex >= gameDataAsset.guns.Count)
        {
            return Mathf.Max(minimumAttackCooldown, manualAttackCooldown);
        }

        return Mathf.Max(minimumAttackCooldown, gameDataAsset.guns[gunIndex].AttackSpeed);
    }
}
