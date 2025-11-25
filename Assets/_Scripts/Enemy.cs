using UnityEngine;
using System.Collections;

/// <summary>
/// Simplified Enemy - Health, waypoint following, status effects, confusion
/// </summary>
public class Enemy : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private float maxHealth = 100f;
    private float currentHealth;

    [Header("Health Bar")]
    [SerializeField] private UnityEngine.UI.Slider healthBarSlider;

    [Header("Combat (Confusion)")]
    [SerializeField] private float confusionDamage = 10f;
    [SerializeField] private float confusionAttackRange = 1.5f;
    [SerializeField] private float confusionAttackCooldown = 1f;

    private bool isDead = false;
    private bool isConfused = false;
    private float confusionAttackTimer = 0f;

    // Components
    private StatusEffectManager statusEffectManager;
    private WaypointFollower waypointFollower;
    private Animator animator;
    private SpriteRenderer spriteRenderer;

    void Awake()
    {
        currentHealth = maxHealth;
        
        statusEffectManager = GetComponent<StatusEffectManager>();
        if (statusEffectManager == null)
            statusEffectManager = gameObject.AddComponent<StatusEffectManager>();

        waypointFollower = GetComponent<WaypointFollower>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Initialize health bar
        UpdateHealthBar();
    }

    void Update()
    {
        if (isDead) return;

        // Confused enemies attack other enemies
        if (isConfused)
        {
            confusionAttackTimer -= Time.deltaTime;
            if (confusionAttackTimer <= 0f)
            {
                AttackNearbyEnemies();
                confusionAttackTimer = confusionAttackCooldown;
            }
        }

        UpdateAnimations();
    }

    public void TakeDamage(float damage)
    {
        if (isDead) return;

        // Apply curse damage multiplier if present
        if (statusEffectManager != null)
        {
            damage *= statusEffectManager.GetDamageMultiplier();
        }

        currentHealth -= damage;

        // Show damage floating text
        FloatingText.Create(transform.position + Vector3.up * 0.5f, 
            Mathf.RoundToInt(damage).ToString(), 
            Color.red);

        // Update health bar
        UpdateHealthBar();

        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;

        // Give gold reward
        if (GameManager.Instance != null)
        {
            int goldReward = GameManager.Instance.GetRandomEnemyGold();
            GameManager.Instance.AddGold(goldReward);

            // Show gold floating text
            FloatingText.Create(transform.position + Vector3.up * 0.5f,
                $"+{goldReward}G",
                Color.yellow);
        }

        // Play death animation if available
        if (animator != null && animator.GetCurrentAnimatorStateInfo(0).IsName("Death"))
        {
            animator.SetTrigger("Die");
            StartCoroutine(DestroyAfterAnimation(1f));
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private IEnumerator DestroyAfterAnimation(float delay)
    {
        // Stop movement
        if (waypointFollower != null)
            waypointFollower.StopMoving();

        // Disable collider
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
            col.enabled = false;

        yield return new WaitForSeconds(delay);
        Destroy(gameObject);
    }

    private void AttackNearbyEnemies()
    {
        Collider2D[] nearbyEnemies = Physics2D.OverlapCircleAll(transform.position, confusionAttackRange);
        
        foreach (Collider2D col in nearbyEnemies)
        {
            if (col.gameObject != gameObject && col.CompareTag("Enemy"))
            {
                Enemy otherEnemy = col.GetComponent<Enemy>();
                if (otherEnemy != null && !otherEnemy.IsDead())
                {
                    otherEnemy.TakeDamage(confusionDamage);
                    break; // Attack only one enemy per tick
                }
            }
        }
    }

    private void UpdateAnimations()
    {
        if (animator == null) return;

        // Get movement direction from waypoint follower
        Vector2 moveDirection = Vector2.zero;
        bool isMoving = false;
        
        if (waypointFollower != null && waypointFollower.IsMoving())
        {
            moveDirection = waypointFollower.GetMovementDirection();
            isMoving = moveDirection != Vector2.zero;
        }

        // Set animator parameter for movement
        animator.SetBool("IsMoving", isMoving);

        // Flip sprite based on horizontal direction
        // Moving left: flip sprite to face left
        // Moving right: don't flip (default rightward animation)
        if (spriteRenderer != null && moveDirection.x != 0)
        {
            spriteRenderer.flipX = moveDirection.x < 0;
        }
    }

    #region Public Getters/Setters

    public bool IsDead() => isDead;

    public void SetConfused(bool confused)
    {
        isConfused = confused;
    }

    public void SetMaxHealth(float health)
    {
        maxHealth = health;
        currentHealth = maxHealth;
        UpdateHealthBar();
    }

    public float GetHealthPercentage()
    {
        return currentHealth / maxHealth;
    }

    private void UpdateHealthBar()
    {
        if (healthBarSlider != null)
        {
            healthBarSlider.value = GetHealthPercentage();
        }
    }

    #endregion

    void OnDrawGizmosSelected()
    {
        // Visualize confusion attack range
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, confusionAttackRange);
    }
}
