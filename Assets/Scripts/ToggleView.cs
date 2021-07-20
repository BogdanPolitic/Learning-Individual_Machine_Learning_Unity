using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToggleView : MonoBehaviour
{

    Vector3 thirdPersonPosition = new Vector3(0, 2, -1.8f);
    Quaternion thirdPersonRotation = Quaternion.Euler(20.0f, 0, 0);
    Vector3 firstPersonPosition = new Vector3(0, 1.1f, 0.15f);
    Quaternion firstPersonRotation = Quaternion.Euler(0, 0, 0);

    bool isFirstPerson = false;

    public void SetThirdPersonView() {
        isFirstPerson = false;
    }

    // Start is called before the first frame update
    void Start()
    {
        //0; 1,5; 0
        //rotation 0; 0; 0
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            isFirstPerson = !isFirstPerson;
        }

        if (isFirstPerson)
        {
            transform.localPosition = firstPersonPosition;
            transform.localRotation = firstPersonRotation;
        } else
        {
            transform.localPosition = thirdPersonPosition;
            transform.localRotation = thirdPersonRotation;
        }
    }
}
