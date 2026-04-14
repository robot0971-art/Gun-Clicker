using System.Collections;
using DI;
using UnityEngine;

public class Slime : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target;
    [SerializeField] private string targetObjectName = "Pistol";
    [SerializeField] private Transform spawnPoint;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 1.5f;
    [SerializeField] private float stopDistance = 0.8f;
    [SerializeField] private bool followTarget = true;
    [SerializeField] private bool faceTarget = true;
    [SerializeField] private bool invertFacing;
    [SerializeField] private bool lockYAxis = true;

    [Header("Animation")]
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private string movingBool = "IsMoving";
    [SerializeField] private string hitTrigger = "Hit";
    [SerializeField] private string deadTrigger = "Dead";
    [SerializeField] private string attackTrigger = "Attack";
    [SerializeField] private string idleStateName = "Idle";
    [SerializeField] private string walkStateName = "Walk";
    [SerializeField] private string hitStateName = "Hurt";
    [SerializeField] private string deadStateName = "Death";
    [SerializeField] private string attackStateName = "Attack1";
    [SerializeField] private RuntimeAnimatorController animatorControllerOverride;

    [Header("Hit Flash")]
    [SerializeField] private Color hitColor = new Color(1f, 0.6f, 0.6f, 1f);
    [SerializeField] private float hitFlashDuration = 0.08f;

    [Header("HP Bar")]
    [SerializeField] private Transform hpBarFill;
    [SerializeField] private int slimeMaxHP = 16;
    [SerializeField] private HitTextPool hitTextPool;
    [SerializeField] private int expReward = 5;

    [Header("Attack")]
    [SerializeField] private float attackRange = 0.9f;
    [SerializeField] private float attackCooldown = 0.8f;
    [SerializeField] private float destroyDelay = 0.25f;

    private int currentMonsterId = -1;
    private int currentHP;
    private int maxHP;
    private bool isAlive;
    private bool isSpawned;
    private Vector3 initialPosition;
    private SlimePool ownerPool;
    private Color originalColor = Color.white;
    private float hpBarOriginalScaleX = 1f;
    private SpriteRenderer hpBarRenderer;
    private float hpBarSpriteWidth = 1f;
    private Vector3 hpBarOriginalLocalPosition;
    private Coroutine hitFlashRoutine;
    private float lastAttackTime;
    private Collider[] colliders;
    private string currentAnimationState;

    private void Awake()
    {
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }

        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            }
        }

        colliders = GetComponentsInChildren<Collider>(true);

        initialPosition = transform.position;

        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }

        if (hpBarFill != null)
        {
            hpBarOriginalScaleX = hpBarFill.localScale.x;
            hpBarOriginalLocalPosition = hpBarFill.localPosition;
            hpBarRenderer = hpBarFill.GetComponent<SpriteRenderer>();
            if (hpBarRenderer != null)
            {
                hpBarSpriteWidth = hpBarRenderer.sprite != null ? hpBarRenderer.sprite.bounds.size.x : 1f;
            }
        }
        else
        {
            hpBarFill = FindChildByName(transform, "Front");
            if (hpBarFill != null)
            {
                hpBarOriginalScaleX = hpBarFill.localScale.x;
                hpBarOriginalLocalPosition = hpBarFill.localPosition;
                hpBarRenderer = hpBarFill.GetComponent<SpriteRenderer>();
                if (hpBarRenderer != null)
                {
                    hpBarSpriteWidth = hpBarRenderer.sprite != null ? hpBarRenderer.sprite.bounds.size.x : 1f;
                }
            }
        }
    }

    private void Start()
    {
        if (target == null)
        {
            var targetObject = GameObject.Find(targetObjectName);
            if (targetObject != null)
            {
                target = targetObject.transform;
            }
        }

        if (hitTextPool == null)
        {
            hitTextPool = FindObjectOfType<HitTextPool>();
        }
    }

    private void OnEnable()
    {
    }

    private void OnDisable()
    {
    }

    private void Update()
    {
        if (!isAlive)
        {
            return;
        }

        if (!followTarget || target == null)
        {
            UpdateAnimationState(false);
            return;
        }

        var targetPosition = target.position;
        var currentPosition = transform.position;
        if (lockYAxis)
        {
            targetPosition.y = currentPosition.y;
        }

        var distance = Vector3.Distance(currentPosition, targetPosition);

        UpdateFacing(targetPosition);
        UpdateAnimationState(distance > stopDistance);

        if (distance <= stopDistance)
        {
            TryAttack(distance);
            return;
        }

        transform.position = Vector3.MoveTowards(
            currentPosition,
            targetPosition,
            moveSpeed * Time.deltaTime
        );
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    public void InitializeSpawn(Vector3 spawnPosition, int monsterId = 0, int monsterHP = 16)
    {
        currentMonsterId = monsterId;
        slimeMaxHP = monsterHP;
        currentHP = monsterHP;
        maxHP = monsterHP;
        isAlive = true;
        isSpawned = true;

        transform.position = spawnPosition;

        if (animator != null)
        {
            BindAnimatorController();
            currentAnimationState = null;
            PlayAnimation(walkStateName, true);
        }

        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = true;
            spriteRenderer.color = originalColor;
        }

        SetCollidersEnabled(true);
        UpdateHPBar();
    }

    public void TakeDamage(int damage, bool critical = false)
    {
        if (!isAlive)
        {
            return;
        }

        currentHP = Mathf.Max(0, currentHP - damage);
        UpdateHPBar();

        EventBus<MonsterHitEvent>.Publish(new MonsterHitEvent
        {
            MonsterId = currentMonsterId,
            Damage = damage,
            CurrentHP = currentHP,
            IsCritical = critical
        });

        if (hitTextPool != null)
        {
            hitTextPool.ShowHitText(transform.position + Vector3.up * 0.6f, critical);
        }

        if (animator != null)
        {
            PlayAnimation(hitStateName, true);
        }

        if (hitFlashRoutine != null)
        {
            StopCoroutine(hitFlashRoutine);
        }

        hitFlashRoutine = StartCoroutine(HitFlash());

        if (currentHP <= 0)
        {
            StartDeathSequence();
        }
    }

    private void OnMonsterSpawned(MonsterSpawnedEvent e)
    {
        currentMonsterId = e.MonsterId;
        currentHP = slimeMaxHP;
        maxHP = slimeMaxHP;
        isAlive = true;
        isSpawned = true;

        if (spawnPoint != null)
        {
            transform.position = spawnPoint.position;
        }
        else if (!followTarget)
        {
            transform.position = initialPosition;
        }

        if (animator != null)
        {
            currentAnimationState = null;
            PlayAnimation(walkStateName, true);
        }

        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = true;
            spriteRenderer.color = originalColor;
        }

        SetCollidersEnabled(true);

        UpdateHPBar();
    }

    private void OnMonsterHit(MonsterHitEvent e)
    {
        if (!isSpawned || !isAlive)
        {
            return;
        }

        if (e.MonsterId != -1)
        {
            return;
        }

        currentHP = Mathf.Max(0, currentHP - e.Damage);
        UpdateHPBar();

        if (hitTextPool != null)
        {
            hitTextPool.ShowHitText(transform.position + Vector3.up * 0.6f, e.IsCritical);
        }

        if (animator != null)
        {
            PlayAnimation(hitStateName, true);
        }

        if (hitFlashRoutine != null)
        {
            StopCoroutine(hitFlashRoutine);
        }

        hitFlashRoutine = StartCoroutine(HitFlash());

        if (currentHP <= 0)
        {
            StartDeathSequence();
        }
    }

    private void OnMonsterKilled(MonsterKilledEvent e)
    {
        if (!isSpawned || !isAlive)
        {
            return;
        }

        if (e.MonsterId != -1 && currentMonsterId != -1 && e.MonsterId != currentMonsterId)
        {
            return;
        }

        StartDeathSequence();
    }

    private void UpdateFacing(Vector3 targetPosition)
    {
        if (!faceTarget || spriteRenderer == null)
        {
            return;
        }

        bool shouldFlip = targetPosition.x < transform.position.x;
        spriteRenderer.flipX = invertFacing ? !shouldFlip : shouldFlip;
    }

    private void UpdateAnimationState(bool moving)
    {
        if (animator == null)
        {
            return;
        }

        PlayAnimation(moving ? walkStateName : idleStateName, false);
    }

    private void TryAttack(float distance)
    {
        if (animator == null)
        {
            return;
        }

        if (distance > attackRange)
        {
            return;
        }

        if (Time.time - lastAttackTime < attackCooldown)
        {
            return;
        }

        lastAttackTime = Time.time;
        PlayAnimation(attackStateName, true);
    }

    private IEnumerator HitFlash()
    {
        if (spriteRenderer == null)
        {
            yield break;
        }

        spriteRenderer.color = hitColor;
        yield return new WaitForSeconds(hitFlashDuration);
        spriteRenderer.color = originalColor;
    }

    private void UpdateHPBar()
    {
        if (maxHP <= 0)
        {
            Debug.LogWarning("[Slime] HP bar fill is not assigned.");
            return;
        }

        float ratio = Mathf.Clamp01((float)currentHP / maxHP);
        if (hpBarRenderer != null)
        {
            var localScale = hpBarFill.localScale;
            float targetScaleX = hpBarOriginalScaleX * ratio;
            localScale.x = targetScaleX;
            hpBarFill.localScale = localScale;

            var localPosition = hpBarFill.localPosition;
            localPosition.x = hpBarOriginalLocalPosition.x + ((hpBarOriginalScaleX - targetScaleX) * hpBarSpriteWidth * 0.5f);
            hpBarFill.localPosition = localPosition;
            return;
        }

        if (hpBarFill != null)
        {
            var localScale = hpBarFill.localScale;
            localScale.x = hpBarOriginalScaleX * ratio;
            hpBarFill.localScale = localScale;

            var localPosition = hpBarFill.localPosition;
            localPosition.x = hpBarOriginalLocalPosition.x + ((hpBarOriginalScaleX - localScale.x) * 0.5f);
            hpBarFill.localPosition = localPosition;
        }
    }

    private void HandleDeathVisual()
    {
        if (animator != null)
        {
            PlayAnimation(deadStateName, true);
        }
    }

    private void PlayAnimation(string stateName, bool forceRestart)
    {
        if (animator == null || string.IsNullOrWhiteSpace(stateName))
        {
            return;
        }

        if (animator.runtimeAnimatorController == null)
        {
            return;
        }

        animator.enabled = true;

        if (!forceRestart && currentAnimationState == stateName)
        {
            return;
        }

        currentAnimationState = stateName;
        animator.Play(stateName, 0, 0f);
        animator.Update(0f);
    }

    private void BindAnimatorController()
    {
        if (animator == null)
        {
            return;
        }

        // Prefer the controller already assigned on the Animator.
        // Only fall back to the override slot when the Animator has no controller yet.
        if (animator.runtimeAnimatorController == null && animatorControllerOverride != null)
        {
            animator.runtimeAnimatorController = animatorControllerOverride;
        }

        if (animator.runtimeAnimatorController == null)
        {
            animator.enabled = false;
            return;
        }

        animator.enabled = true;
        animator.Rebind();
        animator.Update(0f);
    }

    private void StartDeathSequence()
    {
        if (!isAlive)
        {
            return;
        }

        isAlive = false;
        isSpawned = true;
        currentHP = 0;
        UpdateHPBar();
        HandleDeathVisual();

        if (spriteRenderer != null)
        {
            spriteRenderer.color = originalColor;
        }

        SetCollidersEnabled(false);

        EventBus<MonsterKilledEvent>.Publish(new MonsterKilledEvent
        {
            MonsterId = currentMonsterId,
            ExpReward = expReward
        });

        if (ownerPool != null)
        {
            StartCoroutine(ReturnToPoolAfterDelay());
            return;
        }

        Debug.LogWarning("[Slime] No owner pool assigned, object will remain deactivated.");

    }

    public void SetOwnerPool(SlimePool pool)
    {
        ownerPool = pool;
    }

    private IEnumerator ReturnToPoolAfterDelay()
    {
        yield return new WaitForSeconds(destroyDelay);
        if (ownerPool != null)
        {
            ownerPool.Release(this);
        }
        else
        {
            Debug.LogWarning("[Slime] Cannot return to pool - ownerPool is null.");
        }
    }

    private void SetCollidersEnabled(bool enabled)
    {
        if (colliders == null)
        {
            return;
        }

        foreach (var collider in colliders)
        {
            if (collider != null)
            {
                collider.enabled = enabled;
            }
        }
    }

    private Transform FindChildByName(Transform root, string childName)
    {
        foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
        {
            if (child.name == childName)
            {
                return child;
            }
        }

        return null;
    }
}
