using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class ObjectVisibilityChecker : MonoBehaviour
{
    public int status;
    public GameObject playerCamera;
    public CaptureAndSave captureAndSaveScript;
    float proximity = 3.0f;

    [SerializeField] GameObject trainableObjects;

    void Update()
    {
        float distance;
        int objectSightCount = 0;
        GameObject onlyVisible = null;
        foreach (Transform o in trainableObjects.transform)
        {
            MeshRenderer mr = o.GetComponent<MeshRenderer>();
            distance = (playerCamera.transform.position - o.position).magnitude;

            if (distance < proximity && mr != null && mr.isVisible)
            {
                onlyVisible = o.gameObject;
                objectSightCount++;
            } else
            {
                foreach (Transform child in o.transform) {
                    mr = child.GetComponent<MeshRenderer>();
                    distance = (playerCamera.transform.position - child.position).magnitude;
                    if (distance < proximity && mr != null && mr.isVisible)
                    {
                        onlyVisible = o.gameObject;
                        objectSightCount++;
                        break;
                    }
                }
            }
        }

        //Debug.Log("it = " + ct);

        if (objectSightCount == 0)
        {
            //Debug.Log("There's no trainable object visible right now.");
            status = 0;
            captureAndSaveScript.captureTarget = null;
            return;
        }

        if (objectSightCount > 1)
        {
            status = 2;
            captureAndSaveScript.captureTarget = null;
            return;
        }

        //Debug.Log("The only trainable object visible is " + onlyVisible.name);
        status = 1;

        captureAndSaveScript.captureTarget = onlyVisible;
    }
}