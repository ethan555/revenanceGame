using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PathController))]
public class MovementController : MonoBehaviour
{
    public PathController pathController;
    #region Movement
        private Vector3 position;
        private Vector3 destination;
        public float movementSpeed = .01f;
        public float movementSpeedMultiplier = 1f;
        private Vector3 velocity;
        private float moveRate;
    #endregion
    // Start is called before the first frame update
    void Start() {
        position = transform.position;
        destination = transform.position;
        Vector3 newPosition = position;

        // Make sure we are on top of the terrain
        float meshY = 0f;
        RaycastHit hit;
        Ray ray = new Ray(new Vector3(position.x,MeshGenerator.instance.maxHeight,position.z), Vector3.down);
        if (MeshGenerator.instance.meshCollider.Raycast(ray, out hit, 2.0f * MeshGenerator.instance.maxHeight)) {
            meshY = hit.point.y;
        } else {
            meshY = position.y;
        }
        newPosition.y = meshY;

        transform.position = newPosition;
    }

    // Update is called once per frame
    void Update() {
        // Move
        float distance = Vector3.Distance(position, destination);
        Vector3 direction = (destination - position).normalized;
        if (direction.magnitude > .9f) {
            Vector3 delta = direction * movementSpeed * movementSpeedMultiplier;
            if (delta.magnitude > distance) {
                float leftover = delta.magnitude - distance;
                position = destination;
                updateDestination();
                position = position + (destination - position).normalized * leftover;
            } else {
                position += delta;
            }

            // Make sure we are on top of the terrain
            float meshY = 0f;
            RaycastHit hit;
            Ray ray = new Ray(new Vector3(position.x,MeshGenerator.instance.maxHeight,position.z), Vector3.down);
            if (MeshGenerator.instance.meshCollider.Raycast(ray, out hit, 2.0f * MeshGenerator.instance.maxHeight)) {
                meshY = hit.point.y;
            } else {
                meshY = position.y;
            }
            position.y = meshY;

            transform.position = position;
        } else {
            updateDestination();
        }
        
    }

    // Update movementSpeed
    public void updatemovementSpeed(float movementSpeedMultiplier_) {
        movementSpeedMultiplier = movementSpeedMultiplier_;
    }

    private void updateDestination() {
        if (pathController.path.Count > 0) {
            Vector3 oldDestination = destination;
            destination = pathController.path.Pop();
            if (oldDestination == destination) {
                Debug.Log("FUCK RIGHT OFF");
                while (destination == oldDestination && pathController.path.Count > 0) {
                    destination = pathController.path.Pop();
                    Debug.Log("FUCK AGAIN");
                }
            }
        }
    }
}
