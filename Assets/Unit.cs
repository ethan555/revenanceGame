using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour
{
    // private int health;
    // private float sightRadius;
    protected int faction {get; set;}
    protected bool focused {get; set;} = false;
    // private Vector3 targetLocation;
    // private Focus target;
    // private Stack<Vector3> path;

    public Unit(int faction_) {
        // health = health_;
        // sightRadius = sightRadius_;
        faction = faction_;
    }

    public void setFocused(bool focused_) {
        focused = focused_;
    }
}
