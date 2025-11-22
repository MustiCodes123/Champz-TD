using UnityEngine;
using System.Collections;

/// <summary>
/// Status Effect System - Manages all debuffs and buffs
/// </summary>
public enum StatusEffectType
{
    None,
    Stun,
    Poison,
    Bleeding,
    Freeze,
    Slow,
    Confusion,
    Curse
}

[System.Serializable]
public class StatusEffectData
{
    public StatusEffectType type;
    public float duration;
    public float damagePerSecond; // For DOT effects
    public float slowAmount; // 0.5 = 50% speed reduction
    
    public StatusEffectData(StatusEffectType type, float duration, float damagePerSecond = 0f, float slowAmount = 0f)
    {
        this.type = type;
        this.duration = duration;
        this.damagePerSecond = damagePerSecond;
        this.slowAmount = slowAmount;
    }
}

/// <summary>
/// Component that manages active status effects on an enemy
/// </summary>
public class StatusEffectManager : MonoBehaviour
{
    private Enemy enemy;
    private WaypointFollower waypointFollower;
    private SpriteRenderer spriteRenderer;

    // Active effects
    private bool isStunned = false;
    private bool isPoisoned = false;
    private bool isBleeding = false;
    private bool isFrozen = false;
    private bool isSlowed = false;
    private bool isConfused = false;
    private bool isCursed = false;

    // Effect timers
    private float stunTimer = 0f;
    private float poisonTimer = 0f;
    private float bleedingTimer = 0f;
    private float freezeTimer = 0f;
    private float slowTimer = 0f;
    private float confusionTimer = 0f;
    private float curseTimer = 0f;

    // DOT damage values
    private float poisonDPS = 0f;
    private float bleedingDPS = 0f;

    // Slow/Speed modifiers
    private float slowModifier = 1f;
    private float curseSlowModifier = 1f;
    private float freezeSlowModifier = 1f;

    // Curse damage amplification
    private float curseDamageMultiplier = 1f;

    // Visual effect timers
    private float dotDamageTickRate = 0.5f;
    private float nextDotDamageTick = 0f;

    void Awake()
    {
        enemy = GetComponent<Enemy>();
        waypointFollower = GetComponent<WaypointFollower>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        if (enemy == null || enemy.IsDead()) return;

        UpdateEffectTimers();
        ApplyDOTDamage();
        UpdateVisualEffects();
    }

    private void UpdateEffectTimers()
    {
        // Stun
        if (isStunned)
        {
            stunTimer -= Time.deltaTime;
            if (stunTimer <= 0f)
            {
                RemoveStun();
            }
        }

        // Poison
        if (isPoisoned)
        {
            poisonTimer -= Time.deltaTime;
            if (poisonTimer <= 0f)
            {
                RemovePoison();
            }
        }

        // Bleeding
        if (isBleeding)
        {
            bleedingTimer -= Time.deltaTime;
            if (bleedingTimer <= 0f)
            {
                RemoveBleeding();
            }
        }

        // Freeze
        if (isFrozen)
        {
            freezeTimer -= Time.deltaTime;
            if (freezeTimer <= 0f)
            {
                RemoveFreeze();
            }
        }

        // Slow
        if (isSlowed)
        {
            slowTimer -= Time.deltaTime;
            if (slowTimer <= 0f)
            {
                RemoveSlow();
            }
        }

        // Confusion
        if (isConfused)
        {
            confusionTimer -= Time.deltaTime;
            if (confusionTimer <= 0f)
            {
                RemoveConfusion();
            }
        }

        // Curse
        if (isCursed)
        {
            curseTimer -= Time.deltaTime;
            if (curseTimer <= 0f)
            {
                RemoveCurse();
            }
        }
    }

    private void ApplyDOTDamage()
    {
        if (Time.time >= nextDotDamageTick)
        {
            nextDotDamageTick = Time.time + dotDamageTickRate;

            float totalDOT = 0f;

            if (isPoisoned)
                totalDOT += poisonDPS * dotDamageTickRate;

            if (isBleeding)
                totalDOT += bleedingDPS * dotDamageTickRate;

            if (totalDOT > 0f && enemy != null)
            {
                enemy.TakeDamage(totalDOT);
            }
        }
    }

    private void UpdateVisualEffects()
    {
        if (spriteRenderer == null) return;

        // Priority: Freeze > Poison > Bleeding > Curse > Normal
        if (isFrozen)
        {
            spriteRenderer.color = new Color(0.5f, 0.8f, 1f, 1f); // Light blue tint
        }
        else if (isPoisoned)
        {
            spriteRenderer.color = new Color(0.6f, 1f, 0.6f, 1f); // Green tint
        }
        else if (isBleeding)
        {
            spriteRenderer.color = new Color(1f, 0.6f, 0.6f, 1f); // Red tint
        }
        else if (isCursed)
        {
            spriteRenderer.color = new Color(0.7f, 0.5f, 0.9f, 1f); // Purple tint
        }
        else
        {
            spriteRenderer.color = Color.white; // Normal
        }
    }

    #region Apply Effects

    public void ApplyEffect(StatusEffectData effectData)
    {
        switch (effectData.type)
        {
            case StatusEffectType.Stun:
                ApplyStun(effectData.duration);
                break;
            case StatusEffectType.Poison:
                ApplyPoison(effectData.duration, effectData.damagePerSecond);
                break;
            case StatusEffectType.Bleeding:
                ApplyBleeding(effectData.duration, effectData.damagePerSecond);
                break;
            case StatusEffectType.Freeze:
                ApplyFreeze(effectData.duration, effectData.slowAmount);
                break;
            case StatusEffectType.Slow:
                ApplySlow(effectData.duration, effectData.slowAmount);
                break;
            case StatusEffectType.Confusion:
                ApplyConfusion(effectData.duration);
                break;
            case StatusEffectType.Curse:
                ApplyCurse(effectData.duration, effectData.slowAmount, effectData.damagePerSecond);
                break;
        }
    }

    public void ApplyStun(float duration)
    {
        isStunned = true;
        stunTimer = duration;
        
        if (waypointFollower != null)
            waypointFollower.SetStunned(true);
    }

    public void ApplyPoison(float duration, float damagePerSecond)
    {
        isPoisoned = true;
        poisonTimer = Mathf.Max(poisonTimer, duration); // Refresh duration
        poisonDPS = damagePerSecond;
    }

    public void ApplyBleeding(float duration, float damagePerSecond)
    {
        isBleeding = true;
        bleedingTimer = Mathf.Max(bleedingTimer, duration);
        bleedingDPS = damagePerSecond;
    }

    public void ApplyFreeze(float duration, float slowAmount)
    {
        isFrozen = true;
        freezeTimer = duration;
        freezeSlowModifier = 1f - slowAmount;
        UpdateSpeedModifiers();
        
        if (waypointFollower != null)
            waypointFollower.SetStunned(true); // Freeze acts like stun
    }

    public void ApplySlow(float duration, float slowAmount)
    {
        isSlowed = true;
        slowTimer = Mathf.Max(slowTimer, duration);
        slowModifier = 1f - slowAmount;
        UpdateSpeedModifiers();
    }

    public void ApplyConfusion(float duration)
    {
        isConfused = true;
        confusionTimer = duration;
        
        // Confused enemies attack other enemies
        enemy.SetConfused(true);
    }

    public void ApplyCurse(float duration, float slowAmount, float damageMultiplier)
    {
        isCursed = true;
        curseTimer = duration;
        curseSlowModifier = 1f - slowAmount;
        curseDamageMultiplier = 1f + damageMultiplier; // e.g., 1.5 = 50% more damage
        UpdateSpeedModifiers();
    }

    #endregion

    #region Remove Effects

    private void RemoveStun()
    {
        isStunned = false;
        stunTimer = 0f;
        
        if (waypointFollower != null && !isFrozen)
            waypointFollower.SetStunned(false);
    }

    private void RemovePoison()
    {
        isPoisoned = false;
        poisonTimer = 0f;
        poisonDPS = 0f;
    }

    private void RemoveBleeding()
    {
        isBleeding = false;
        bleedingTimer = 0f;
        bleedingDPS = 0f;
    }

    private void RemoveFreeze()
    {
        isFrozen = false;
        freezeTimer = 0f;
        freezeSlowModifier = 1f;
        UpdateSpeedModifiers();
        
        if (waypointFollower != null && !isStunned)
            waypointFollower.SetStunned(false);
    }

    private void RemoveSlow()
    {
        isSlowed = false;
        slowTimer = 0f;
        slowModifier = 1f;
        UpdateSpeedModifiers();
    }

    private void RemoveConfusion()
    {
        isConfused = false;
        confusionTimer = 0f;
        enemy.SetConfused(false);
    }

    private void RemoveCurse()
    {
        isCursed = false;
        curseTimer = 0f;
        curseSlowModifier = 1f;
        curseDamageMultiplier = 1f;
        UpdateSpeedModifiers();
    }

    #endregion

    private void UpdateSpeedModifiers()
    {
        // Combine all speed modifiers (multiplicative)
        float finalSpeedModifier = slowModifier * curseSlowModifier * freezeSlowModifier;
        
        if (waypointFollower != null)
        {
            waypointFollower.SetSpeedModifier(finalSpeedModifier);
        }
    }

    public bool IsStunned() => isStunned || isFrozen;
    public bool IsConfused() => isConfused;
    public float GetDamageMultiplier() => curseDamageMultiplier;

    public void ClearAllEffects()
    {
        if (isStunned) RemoveStun();
        if (isPoisoned) RemovePoison();
        if (isBleeding) RemoveBleeding();
        if (isFrozen) RemoveFreeze();
        if (isSlowed) RemoveSlow();
        if (isConfused) RemoveConfusion();
        if (isCursed) RemoveCurse();
    }
}
