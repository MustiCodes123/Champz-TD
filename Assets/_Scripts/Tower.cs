using UnityEngine;
using DG.Tweening;

/// <summary>
/// Tower - Handles targeting, shooting, and directional animations
/// Works with TowerData for stats
/// </summary>
public class Tower : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private Transform firePoint;
    [SerializeField] private GameObject projectilePrefab;

    [Header("Animation")]
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Header("Priest Healing Aura")]
    [SerializeField] private float healingAuraRange = 3f;
    [SerializeField] private float healingInterval = 2f;
    [SerializeField] private float healingAmount = 0.1f; // 10% health per tick
    private float healingTimer = 0f;

    // Tower stats (from TowerData)
    private TowerData towerData;
    private float range;
    private float fireRate;
    private float damage;

    // Targeting
    private Transform currentTarget;
    private float fireCountdown = 0f;
    private string lastAttackTrigger = "";

    // Animation parameters
    private const string ANIM_ATTACK_LEFT = "AttackLeft";
    private const string ANIM_ATTACK_RIGHT = "AttackRight";
    private const string ANIM_ATTACK_UP = "AttackUp";
    private const string ANIM_ATTACK_DOWN = "AttackDown";

    void Start()
    {
        towerData = GetComponent<TowerData>();
        if (towerData == null)
        {
            Debug.LogError($"Tower ({gameObject.name}): Missing TowerData component!");
            enabled = false;
            return;
        }

        // Validate animator setup
        if (animator == null)
        {
            Debug.LogWarning($"Tower ({gameObject.name}): Animator reference not assigned in inspector!");
            animator = GetComponent<Animator>();
            if (animator == null)
            {
                Debug.LogError($"Tower ({gameObject.name}): No Animator component found!");
            }
        }

        if (animator != null && animator.runtimeAnimatorController == null)
        {
            Debug.LogError($"Tower ({gameObject.name}): Animator Controller not assigned to Animator component!");
        }

        if (spriteRenderer == null)
        {
            Debug.LogWarning($"Tower ({gameObject.name}): SpriteRenderer reference not assigned in inspector!");
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        ApplyStats();
    }

    void Update()
    {
        // Priest healing aura
        if (towerData.towerType == TowerType.Priest)
        {
            UpdateHealingAura();
        }

        // Find target
        currentTarget = FindClosestEnemy();

        // Update animations
        bool hasTarget = currentTarget != null;
        UpdateAnimations(hasTarget);

        // Attack logic
        if (hasTarget)
        {
            if (fireCountdown <= 0f)
            {
                Shoot();
                fireCountdown = 1f / fireRate;
            }
        }

        fireCountdown -= Time.deltaTime;
    }

    public void ApplyStats()
    {
        if (towerData == null) return;

        range = towerData.GetCurrentRange();
        fireRate = towerData.GetCurrentFireRate();
        damage = towerData.GetCurrentDamage();
    }

    private Transform FindClosestEnemy()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        Transform closest = null;
        float minDistance = Mathf.Infinity;

        foreach (GameObject enemy in enemies)
        {
            float distance = Vector2.Distance(transform.position, enemy.transform.position);
            if (distance <= range && distance < minDistance)
            {
                // Check if enemy is alive
                Enemy enemyScript = enemy.GetComponent<Enemy>();
                if (enemyScript != null && !enemyScript.IsDead())
                {
                    minDistance = distance;
                    closest = enemy.transform;
                }
            }
        }

        return closest;
    }

    private void Shoot()
    {
        if (projectilePrefab == null || firePoint == null || currentTarget == null)
            return;

        // Play attack sound
        AudioManager.Instance.PlaySound(GetAttackSoundType(towerData.towerType));

        // Spawn projectile
        GameObject projectileObj = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
        
        Projectile projectile = projectileObj.GetComponent<Projectile>();
        if (projectile != null)
        {
            projectile.Initialize(currentTarget, damage, towerData);
        }
    }

    private SoundType GetAttackSoundType(TowerType towerType)
    {
        return towerType switch
        {
            TowerType.Hunter => SoundType.TowerAttack_Hunter,
            TowerType.Jester => SoundType.TowerAttack_Jester,
            TowerType.Lumberjack => SoundType.TowerAttack_Lumberjack,
            TowerType.Priest => SoundType.TowerAttack_Priest,
            TowerType.Alchemist => SoundType.TowerAttack_Alchemist,
            TowerType.Warrior => SoundType.TowerAttack_Warrior,
            _ => SoundType.TowerAttack_Hunter // Default fallback
        };
    }

    private void UpdateAnimations(bool hasTarget)
    {
        if (animator == null) return;

        Debug.Log($"[{gameObject.name}] UpdateAnimations - hasTarget: {hasTarget}, currentTarget: {(currentTarget != null ? currentTarget.name : "null")}, currentState: {animator.GetCurrentAnimatorStateInfo(0).IsName("Idle")}");

        if (hasTarget && currentTarget != null)
        {
            // Calculate direction to target
            Vector2 direction = (currentTarget.position - transform.position).normalized;
            
            // Determine which direction animation to play based on angle
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            
            // Determine which attack trigger to use based on angle
            string attackTrigger = "";
            
            // Improved angle ranges for better directional detection
            if (angle >= -30f && angle < 30f)
            {
                attackTrigger = ANIM_ATTACK_RIGHT;
            }
            else if (angle >= 30f && angle < 150f)
            {
                attackTrigger = ANIM_ATTACK_UP;
            }
            else if (angle >= 150f || angle < -150f)
            {
                attackTrigger = ANIM_ATTACK_LEFT;
            }
            else // angle >= -150f && angle < -30f
            {
                attackTrigger = ANIM_ATTACK_DOWN;
            }

            Debug.Log($"[{gameObject.name}] Angle: {angle:F1}°, Selected Trigger: {attackTrigger}, Last Trigger: {lastAttackTrigger}");

            // Determine which animation state to play based on the trigger
            string stateName = "";
            if (attackTrigger == ANIM_ATTACK_LEFT) stateName = "AttackLeft";
            else if (attackTrigger == ANIM_ATTACK_RIGHT) stateName = "AttackRight";
            else if (attackTrigger == ANIM_ATTACK_UP) stateName = "AttackUp";
            else if (attackTrigger == ANIM_ATTACK_DOWN) stateName = "AttackDown";

            // Play or continue playing the attack animation
            if (attackTrigger != lastAttackTrigger)
            {
                // Direction changed - play new attack animation
                animator.Play(stateName, 0, 0f);
                lastAttackTrigger = attackTrigger;
                Debug.Log($"[{gameObject.name}] <color=green>Playing: {stateName}</color>");
            }
            else
            {
                // Same direction - ensure we're still in the attack state (in case animation finished)
                AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
                if (!stateInfo.IsName(stateName))
                {
                    animator.Play(stateName, 0, 0f);
                    Debug.Log($"[{gameObject.name}] <color=cyan>Re-entering: {stateName}</color>");
                }
            }
        }
        else
        {
            // No target - force return to idle
            AnimatorStateInfo currentState = animator.GetCurrentAnimatorStateInfo(0);
            
            // If we're not already in Idle state, force transition to Idle
            if (!currentState.IsName("Idle"))
            {
                Debug.Log($"[{gameObject.name}] <color=yellow>No target - forcing return to Idle from {currentState.shortNameHash}</color>");
                
                // Force animator to play Idle state immediately
                animator.Play("Idle", 0, 0f);
            }
            
            // Clear last trigger when not attacking
            if (!string.IsNullOrEmpty(lastAttackTrigger))
            {
                lastAttackTrigger = "";
                Debug.Log($"[{gameObject.name}] <color=yellow>Cleared last attack trigger</color>");
            }
        }
    }

    private void UpdateHealingAura()
    {
        healingTimer -= Time.deltaTime;
        if (healingTimer <= 0f)
        {
            healingTimer = healingInterval;
            HealNearbyTowers();
        }
    }

    private void HealNearbyTowers()
    {
        // Find all towers in range
        Collider2D[] nearbyColliders = Physics2D.OverlapCircleAll(transform.position, healingAuraRange);
        
        foreach (Collider2D col in nearbyColliders)
        {
            if (col.gameObject == gameObject) continue; // Skip self

            Tower otherTower = col.GetComponent<Tower>();
            if (otherTower != null)
            {
                // Visual healing effect using DOTween
                ShowHealingEffect(otherTower.transform);
            }
        }
    }

    private void ShowHealingEffect(Transform target)
    {
        // Scale pulse effect
        if (target != null)
        {
            target.DOScale(target.localScale * 1.1f, 0.2f).SetLoops(2, DG.Tweening.LoopType.Yoyo);
        }
    }

    void OnDrawGizmosSelected()
    {
        // Draw attack range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, towerData != null ? towerData.GetCurrentRange() : 3f);

        // Draw healing aura for Priest
        if (towerData != null && towerData.towerType == TowerType.Priest)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, healingAuraRange);
        }
    }
}
