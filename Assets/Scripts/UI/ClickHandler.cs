using UnityEngine;
using UnityEngine.InputSystem;

public class ClickHandler : MonoBehaviour
{
    [SerializeField] private bool useRaycast = false;

    private Camera mainCamera;
    private float lastAttackTime;
    private float attackCooldown = 0.1f;

    private void Start()
    {
        mainCamera = Camera.main;
        lastAttackTime = 0f;
    }

    private void Update()
    {
        if (Mouse.current == null || !Mouse.current.leftButton.wasPressedThisFrame)
            return;

        if (Time.time - lastAttackTime < attackCooldown)
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
}
