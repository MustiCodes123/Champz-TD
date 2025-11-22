using UnityEngine;

/// <summary>
/// Tower Types and their special abilities
/// </summary>
public enum TowerType
{
    Hunter,      // Bow - Stun ability
    Jester,      // Cards - Confusion ability
    Lumberjack,  // Saw - Bleeding ability
    Priest,      // Staff - Healing/Curse ability
    Alchemist,   // Poison vial - Poison/Freeze ability
    Warrior      // Halberd - High damage
}

/// <summary>
/// Tower Data - Manages tower stats and upgrades
/// </summary>
public class TowerData : MonoBehaviour
{
    [Header("Tower Identity")]
    public TowerType towerType = TowerType.Hunter;
    public int currentLevel = 1;
    public int maxLevel = 5;

    [Header("Base Stats")]
    public float baseRange = 3f;
    public float baseDamage = 10f;
    public float baseFireRate = 1f;

    [Header("Upgrade Costs")]
    public int[] upgradeCosts = { 50, 100, 150, 200 }; // Cost for levels 2, 3, 4, 5

    [Header("Stat Scaling Per Level")]
    [Tooltip("Range increase per level")]
    public float rangePerLevel = 0.5f;
    [Tooltip("Damage increase per level")]
    public float damagePerLevel = 5f;
    [Tooltip("Fire rate increase per level")]
    public float fireRatePerLevel = 0.2f;

    [Header("Special Ability (Level 3+)")]
    [Tooltip("Chance to apply special effect (0-1)")]
    public float specialAbilityChance = 0.3f;
    [Tooltip("Special ability unlock level")]
    public int specialAbilityUnlockLevel = 3;

    public float GetCurrentRange()
    {
        return baseRange + (rangePerLevel * (currentLevel - 1));
    }

    public float GetCurrentDamage()
    {
        return baseDamage + (damagePerLevel * (currentLevel - 1));
    }

    public float GetCurrentFireRate()
    {
        return baseFireRate + (fireRatePerLevel * (currentLevel - 1));
    }

    public int GetUpgradeCost()
    {
        if (currentLevel >= maxLevel) return 0;
        
        int costIndex = currentLevel - 1; // Level 2 = index 0
        if (costIndex >= 0 && costIndex < upgradeCosts.Length)
            return upgradeCosts[costIndex];
        
        return 999; // Fallback
    }

    public bool CanUpgrade()
    {
        return currentLevel < maxLevel;
    }

    public void Upgrade()
    {
        if (CanUpgrade())
        {
            currentLevel++;
            
            // Apply upgraded stats to tower component
            Tower tower = GetComponent<Tower>();
            if (tower != null)
            {
                tower.ApplyStats();
            }

            Debug.Log($"{towerType} upgraded to level {currentLevel}");
        }
    }

    public bool HasSpecialAbility()
    {
        return currentLevel >= specialAbilityUnlockLevel;
    }

    public StatusEffectData GetSpecialEffect()
    {
        if (!HasSpecialAbility()) return null;

        // Return appropriate effect based on tower type
        switch (towerType)
        {
            case TowerType.Hunter:
                return new StatusEffectData(StatusEffectType.Stun, 1.5f);
            
            case TowerType.Jester:
                return new StatusEffectData(StatusEffectType.Confusion, 3f);
            
            case TowerType.Lumberjack:
                return new StatusEffectData(StatusEffectType.Bleeding, 4f, 5f); // 5 DPS for 4 seconds
            
            case TowerType.Priest:
                return new StatusEffectData(StatusEffectType.Curse, 3f, 0.5f, 0.3f); // 30% slow, 50% more damage
            
            case TowerType.Alchemist:
                if (currentLevel >= 4)
                    return new StatusEffectData(StatusEffectType.Freeze, 2f, 0f, 0.5f); // 50% slow
                else
                    return new StatusEffectData(StatusEffectType.Poison, 5f, 8f); // 8 DPS for 5 seconds
            
            case TowerType.Warrior:
                return null; // Warrior doesn't have status effect, just high damage
            
            default:
                return null;
        }
    }
}
