using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class EventVector3 : UnityEvent<Vector3> {

}

public class PlayerController : MonoBehaviour
{
    private static PlayerController playerController;
    public static PlayerController instance { get { return playerController; } }
    public Dictionary<int, GameObject> units;
    public HashSet<int> focusedUnits;

    public static UnityEvent<Vector3> pathingEvent;
    public static UnityEvent targetingEvent;

    private void Awake() {
        playerController = this;
        if (pathingEvent == null)
            pathingEvent = new EventVector3();
        if (targetingEvent == null)
            targetingEvent = new UnityEvent();
        units = new Dictionary<int, GameObject>();
    }

    Ray FuckYou;
    // Update is called once per frame
    void Update() {
        // Debug.DrawRay(FuckYou.origin, FuckYou.direction * 100f, Color.red);
        if (Input.GetMouseButtonDown(0)) {
            RaycastHit hit;
    
            // FuckYou = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (MeshGenerator.instance.meshCollider.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 100f)) {
                // foreach (int unitId in focusedUnits) {
                //     units[unitId].PathController.getPath(hit.point);
                // }
                pathingEvent.Invoke(hit.point);
                Debug.Log("Sent Event");
            }
        }
    }

    public void addUnit(int unitId, GameObject unit) {
        if (!units.ContainsKey(unitId)) {
            units.Add(unitId, unit);
        }
    }

    public void removeUnit(int unitId, GameObject unit) {
        if (units.ContainsKey(unitId)) {
            units.Remove(unitId);
        }
    }
}
