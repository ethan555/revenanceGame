using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[RequireComponent(typeof(Unit))]
public class Target : MonoBehaviour
{
    private int maxHealth;
    private int health;
    private int maxShields;
    private int shields;
    private int maxArmor;
    private int armor;

    public Target(int health_, int shields_ = 0, int armor_ = 0) {
        maxHealth = health_;
        maxShields = shields_;
        maxArmor = armor_;
        health = health_;
        shields = shields_;
        armor = armor_;
    }

    public void damage(int damage_, int damageType) {
        int remainingDamage = damage_;
        if (shields > 0) {
            int shieldDamage = DamageTypes.damageShields(damage_, damageType);
            remainingDamage -= shieldDamage;
            shields = Math.Max(0, shields - shieldDamage);
            if (shields == 0) {
                // Shield Gate
                if (damageType != DamageTypes.WARPING) {
                    remainingDamage -= maxShields;
                }
            }
        }
        if (remainingDamage <= 0) return;

        if (armor > 0) {
            int armorDamage = DamageTypes.damageArmor(damage_, damageType);
            remainingDamage -= armorDamage;
            armor = Math.Max(0, armor - armorDamage);
        }
        if (remainingDamage <= 0) return;
        
        if (health > 0) {
            int healthDamage = DamageTypes.damageHealth(damage_, damageType);
            health = Math.Max(0, health - healthDamage);
        }
        if (health == 0) {
            // Kill unit
            Destroy(gameObject);
        }
    }

    public void heal(int heal_, int healType) {
        switch(healType) {
            case DamageTypes.SHIELDS:
                shields = Math.Max(shields + heal_, maxShields);
                break;
            case DamageTypes.ARMOR:
                armor = Math.Max(armor + heal_, maxArmor);
                break;
            case DamageTypes.HEALTH:
                health = Math.Max(health + heal_, maxHealth);
                break;
        }
    }

    protected void OnDestroy() {
        // Particles
        onDestroyParticles();
        // Update Factions?
    }

    protected void onDestroyParticles() {
        // Particles, blow up!
    }
}
