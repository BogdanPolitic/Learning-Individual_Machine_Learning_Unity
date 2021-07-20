using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class ObjectVisibilityChecker : MonoBehaviour
{
    GameObject[] allGOs;
    public int status;
    List<GameObject> allGOsL;
    public GameObject playerCamera;
    public CaptureAndSave captureAndSaveScript;
    float proximity = 3.0f;

    const int TRAINABLE_OBJECT_LAYER = 6;

    // Start is called before the first frame update
    void Start()
    {
        allGOs = (GameObject[])GameObject.FindObjectsOfType(typeof(GameObject));    // toate GameObject-urile
        allGOsL = new List<GameObject>();                                           // toate GameObject-urile care sunt la radacina ierarhiei

        foreach (GameObject o in allGOs)
        {
            if (o.transform.parent == null)
                allGOsL.Add(o);
        }
    }

    // Update is called once per frame
    void Update()
    {
        float distance;
        int ct = 0;
        GameObject onlyVisible = null;
        foreach (GameObject o in allGOsL)
        {
            if (o.layer == TRAINABLE_OBJECT_LAYER)
            {
                MeshRenderer mr = o.GetComponent<MeshRenderer>();
                distance = (playerCamera.transform.position - o.transform.position).magnitude;

                if (distance < proximity && mr != null && mr.isVisible)
                {
                    onlyVisible = o;
                    ct++;
                } else
                {
                    foreach (Transform child in o.transform) {
                        mr = child.GetComponent<MeshRenderer>();
                        distance = (playerCamera.transform.position - child.position).magnitude;
                        if (distance < proximity && mr != null && mr.isVisible)
                        {
                            onlyVisible = o;
                            ct++;
                            break;
                        }
                    }
                }
            }
        }

        //Debug.Log("it = " + ct);

        if (ct == 0)
        {
            //Debug.Log("Kinda empty here ...");
            status = 0;
            captureAndSaveScript.captureTarget = null;
            return;
        }

        if (ct > 1)
        {
            status = 2;
            captureAndSaveScript.captureTarget = null;
            return;
        }

        //Debug.Log("You can only see " + onlyVisible.name);
        status = 1;

        captureAndSaveScript.captureTarget = onlyVisible;
    }
}