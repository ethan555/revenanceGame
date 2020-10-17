using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageTypes
{
    public const int PHYSICAL = 0;
    public const int FIRE = 1;
    public const int ELECTRIC = 2;
    public const int RENDING = 3;
    public const int WARPING = 4;

    public const int SHIELDS = 0;
    public const int ARMOR = 1;
    public const int HEALTH = 2;

    private const int ARMOR_FLAT = 5;
    private const float ARMOR_RATING = .9f;

    public static int damageShields(int damage_, int damageType) {
        int totalDamage = shieldsFormula((int)Mathf.Round(damage_ * GetDamageMultiplier(damageType, SHIELDS)));
        return totalDamage;
    }

    private static int shieldsFormula(int damage) {
        int result = (int)Mathf.Round(damage);
        return result >= 1 ? result : 1;
    }

    public static int damageArmor(int damage_, int damageType) {
        int totalDamage = armorFormula(
            (int)Mathf.Round(damage_ * GetDamageMultiplier(damageType, ARMOR))
        );
        return totalDamage;
    }

    private static int armorFormula(int damage) {
        int result = (int)Mathf.Round(damage * ARMOR_RATING) - ARMOR_FLAT;
        return result >= 1 ? result : 1;
    }

    public static int damageHealth(int damage_, int damageType) {
        int totalDamage = (int)Mathf.Round(damage_ * GetDamageMultiplier(damageType, HEALTH));
        return totalDamage;
    }

    public static float GetDamageMultiplier(int damageType, int healthType) {
        switch (damageType) {
            case PHYSICAL:
                switch (healthType) {
                    case SHIELDS:
                        return 1f;
                    case ARMOR:
                        return 1f;
                    case HEALTH:
                        return 1f;
                };
                break;
            case FIRE:
                switch (healthType) {
                    case SHIELDS:
                        return .25f;
                    case ARMOR:
                        return 1.25f;
                    case HEALTH:
                        return 1f;
                };
                break;
            case ELECTRIC:
                switch (healthType) {
                    case SHIELDS:
                        return 1.5f;
                    case ARMOR:
                        return .1f;
                    case HEALTH:
                        return .5f;
                };
                break;
            case RENDING:
                switch (healthType) {
                    case SHIELDS:
                        return .9f;
                    case ARMOR:
                        return 2f;
                    case HEALTH:
                        return .9f;
                };
                break;
            case WARPING:
                switch (healthType) {
                    case SHIELDS:
                        return 2f;
                    case ARMOR:
                        return 1f;
                    case HEALTH:
                        return 1f;
                };
                break;
            default: return 0f;
        }
        return 0f;
    }
}
