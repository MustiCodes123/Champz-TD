using UnityEngine;

/// <summary>
/// Projectile - Moves towards target and deals damage with status effects
/// </summary>
public class Projectile : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float speed = 10f;
    [SerializeField] private float rotateSpeed = 200f;

    private Transform target;
    private float damage;
    private TowerData towerData;

    public void Initialize(Transform target, float damage, TowerData towerData)
    {
        this.target = target;
        this.damage = damage;
        this.towerData = towerData;
    }

    void Update()
    {
        if (target == null)
        {
            Destroy(gameObject);
            return;
        }

        // Check if target is dead
        Enemy enemy = target.GetComponent<Enemy>();
        if (enemy != null && enemy.IsDead())
        {
            Destroy(gameObject);
            return;
        }

        // Move towards target
        Vector2 direction = (target.position - transform.position).normalized;
        transform.position += (Vector3)direction * speed * Time.deltaTime;

        // Rotate towards target
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        Quaternion targetRotation = Quaternion.Euler(0, 0, angle);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotateSpeed * Time.deltaTime);

        // Check if reached target
        float distanceToTarget = Vector2.Distance(transform.position, target.position);
        if (distanceToTarget < 0.2f)
        {
            HitTarget();
        }
    }

    private void HitTarget()
    {
        Enemy enemy = target.GetComponent<Enemy>();
        if (enemy != null && !enemy.IsDead())
        {
            // Deal damage
            enemy.TakeDamage(damage);

            // Apply special effect if available
            if (towerData != null && towerData.HasSpecialAbility())
            {
                float chance = towerData.specialAbilityChance;
                if (Random.value <= chance)
                {
                    StatusEffectData effect = towerData.GetSpecialEffect();
                    if (effect != null)
                    {
                        StatusEffectManager statusManager = enemy.GetComponent<StatusEffectManager>();
                        if (statusManager != null)
                        {
                            statusManager.ApplyEffect(effect);
                        }
                    }
                }
            }
        }

        Destroy(gameObject);
    }
}
