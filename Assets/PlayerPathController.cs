using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PathController))]
public class PlayerPathController : MonoBehaviour
{
    public PathController pathController;

    // Start is called before the first frame update
    void Start() {
        PlayerController.pathingEvent.AddListener(updatePath);
    }

    private void OnDestroy() {
        PlayerController.pathingEvent.RemoveListener(updatePath);
    }

    private void updatePath(Vector3 destination) {
        Debug.Log("Got Event " + destination.ToString());
        pathController.getPath(destination);
    }
}
