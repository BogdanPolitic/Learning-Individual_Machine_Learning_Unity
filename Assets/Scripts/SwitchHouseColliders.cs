using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwitchHouseColliders : MonoBehaviour
{
    public static SwitchHouseColliders instance;
    [SerializeField] GameObject apartmentStructure;
    [SerializeField] GameObject pathfindingWalls;
    [SerializeField] GameObject doors;

    /*private void SetColliderActive(GameObject gO, bool active)
    {
        if (gO.GetComponent<Collider>)
    }*/

    public void SetCollidersActiveRecursively(Transform gO, bool active)
    {
        if (gO.GetComponent<Collider>() != null)
            gO.GetComponent<Collider>().enabled = active;

        foreach (Transform child in gO.transform)
        {
            SetCollidersActiveRecursively(child, active);
        }
    }

    private void SetTriggersRecursive(GameObject gO, bool trigger)
    {
        if (gO.GetComponent<MeshCollider>() != null)
            gO.GetComponent<MeshCollider>().isTrigger = trigger;

        foreach (Transform child in gO.transform)
        {
            SetTriggersRecursive(child.gameObject, trigger);
        }
    }

    // If the trigger is true, that means the colliders deactivate. If the trigger is false, that means the colliders activate. In short, (isTrigger == true) <=> (GetComponent<Collider> == inactive).
    // They're still active, but only to trigger, not to collide.
    // So if you want the colliders to become unavailable, set trigger to TRUE.
    public void SetTriggers(bool trigger)
    {
        apartmentStructure.GetComponent<MeshCollider>().enabled = !trigger;
        SetTriggersRecursive(gameObject, trigger);                              // The house objects
        //SetTriggersRecursive(doors, trigger);
        
        /*foreach (Transform pathfindingWall in pathfindingWalls.transform)
        {
            pathfindingWall.GetComponent<BoxCollider>().enabled = !trigger;
        }*/
    }

    void Awake()
    {
        instance = this;
    }
}
