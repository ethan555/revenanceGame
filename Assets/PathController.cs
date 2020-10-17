using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
    Units can move
*/
[RequireComponent(typeof(Unit))]
[RequireComponent(typeof(Target))]
public class PathController : MonoBehaviour
{
    #region Pathing
        public Stack<Vector3> path;
        private Vector3 position;
        private float distance;
        public float heightCost = 1f;
        public float heightCostMultiplier = 1f;
    #endregion

    // Start is called before the first frame update
    void Start() {
        path = new Stack<Vector3>();
        position = transform.position;
    }

    // Update heightCost
    public void updateHeightCost(float heightCostMultiplier_) {
        heightCostMultiplier = heightCostMultiplier_;
    }

    // Get New Path
    public void getPath(Vector3 end) {
        position = transform.position;
        path = MeshGenerator.navmesh.getSmartPath(position, end);
        Debug.Log("New Path: " + path.Count.ToString());
    }
}
