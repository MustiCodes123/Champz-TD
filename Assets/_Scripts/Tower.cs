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
    private const string ANIM_DIRECTION_X = "DirectionX";
    private const string ANIM_DIRECTION_Y = "DirectionY";
    private const string ANIM_IS_ATTACKING = "IsAttacking";

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

        // Set attacking state
        animator.SetBool(ANIM_IS_ATTACKING, hasTarget);

        if (hasTarget && currentTarget != null)
        {
            // Calculate direction to target
            Vector2 direction = (currentTarget.position - transform.position).normalized;
            
            // Set directional animation parameters
            animator.SetFloat(ANIM_DIRECTION_X, direction.x);
            animator.SetFloat(ANIM_DIRECTION_Y, direction.y);

            // Optional: Flip sprite based on direction
            if (spriteRenderer != null && Mathf.Abs(direction.x) > 0.1f)
            {
                spriteRenderer.flipX = direction.x < 0;
            }
        }
        else
        {
            // Idle state - face down by default
            animator.SetFloat(ANIM_DIRECTION_X, 0);
            animator.SetFloat(ANIM_DIRECTION_Y, -1);
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
