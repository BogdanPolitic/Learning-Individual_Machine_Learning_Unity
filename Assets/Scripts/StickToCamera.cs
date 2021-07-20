using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StickToCamera : MonoBehaviour
{
    GameObject mainCamera;
    void Start()
    {
        mainCamera = GameObject.Find("Main Camera");
        transform.rotation = mainCamera.transform.rotation;
        transform.SetParent(mainCamera.transform);
    }
}
