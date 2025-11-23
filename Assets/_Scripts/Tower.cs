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
            Debug.LogError("Tower: Missing TowerData component!");
            enabled = false;
            return;
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

        // Spawn projectile
        GameObject projectileObj = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
        
        Projectile projectile = projectileObj.GetComponent<Projectile>();
        if (projectile != null)
        {
            projectile.Initialize(currentTarget, damage, towerData);
        }
    }

    private void UpdateAnimations(bool hasTarget)
    {
        if (animator == null) return;

        if (hasTarget && currentTarget != null)
        {
            // Calculate direction to target
            Vector2 direction = (currentTarget.position - transform.position).normalized;
            
            // Determine which direction animation to play based on angle
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            
            // Reset all triggers
            animator.ResetTrigger(ANIM_ATTACK_LEFT);
            animator.ResetTrigger(ANIM_ATTACK_RIGHT);
            animator.ResetTrigger(ANIM_ATTACK_UP);
            animator.ResetTrigger(ANIM_ATTACK_DOWN);
            
            // Set appropriate attack trigger based on angle
            // Right: -45 to 45 degrees
            // Up: 45 to 135 degrees
            // Left: 135 to -135 degrees (or 135 to 225)
            // Down: -135 to -45 degrees (or 225 to 315)
            
            if (angle >= -45f && angle < 45f)
            {
                animator.SetTrigger(ANIM_ATTACK_RIGHT);
            }
            else if (angle >= 45f && angle < 135f)
            {
                animator.SetTrigger(ANIM_ATTACK_UP);
            }
            else if (angle >= 135f || angle < -135f)
            {
                animator.SetTrigger(ANIM_ATTACK_LEFT);
            }
            else // angle >= -135f && angle < -45f
            {
                animator.SetTrigger(ANIM_ATTACK_DOWN);
            }
        }
        else
        {
            // Idle state - animator will return to idle automatically
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
