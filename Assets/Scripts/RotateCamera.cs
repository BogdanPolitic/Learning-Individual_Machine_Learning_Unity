using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateCamera : MonoBehaviour
{
    public static RotateCamera instance;
    public bool isFirstPerson;

    float yaw, pitch;
    public Transform player;
    public float distToTarget;
    public Vector3 cameraOffset;
    public float minYaw, maxYaw;

    private void Awake()
    {
        instance = this;
        isFirstPerson = false;
    }

    void Start()
    {
        yaw = pitch = 0f;
        distToTarget = 4f;
        minYaw = -10f;
        maxYaw = 45f;
    }

    private void LateUpdate()
    {
        if (isFirstPerson)
            return;

        yaw -= Input.GetAxis("Mouse Y");
        pitch += Input.GetAxis("Mouse X");

        yaw = Mathf.Clamp(yaw, minYaw, maxYaw);

        transform.rotation = Quaternion.Euler(yaw, pitch, 0f);
        transform.position = player.position + transform.TransformVector(cameraOffset);
    }
}
