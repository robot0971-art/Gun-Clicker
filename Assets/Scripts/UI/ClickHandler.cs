using UnityEngine;

/// <summary>
/// 클릭 감지 및 ClickEvent 발행
/// </summary>
public class ClickHandler : MonoBehaviour
{
    [SerializeField] private bool useRaycast = false;
    
    private Camera mainCamera;
    private float lastAttackTime;
    private float attackCooldown = 0.1f; // 최소 공격 간격
    
    private void Start()
    {
        mainCamera = Camera.main;
        lastAttackTime = 0f;
    }
    
    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            // 연사 쿨타임 체크
            if (Time.time - lastAttackTime < attackCooldown)
                return;
            
            if (useRaycast)
            {
                HandleRaycastClick();
            }
            else
            {
                // Simple: 화면 어디든 클릭하면 이벤트 발행
                PublishClick();
            }
        }
    }
    
    private void HandleRaycastClick()
    {
        var ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        
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
        
        // AttackEvent 발행 (CombatManager가 처리)
        EventBus<AttackEvent>.Publish(new AttackEvent 
        { 
            GunId = -1, // CombatManager가 현재 총 가져옴
            Damage = 0, // CombatManager가 계산
            IsCritical = false // CombatManager가 계산
        });
        
        // 기존 ClickEvent도 유지 (하위 호환성)
        EventBus<ClickEvent>.Publish(new ClickEvent());
    }
}