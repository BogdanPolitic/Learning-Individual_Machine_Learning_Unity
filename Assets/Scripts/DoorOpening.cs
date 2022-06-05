using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorOpening : MonoBehaviour
{
    public float rotSpeed = 50.0f;
    public float closedAngleDegrees;
    private float closeRange = 3.0f;

    private bool heldByCharacter = false;


    private Animator characterAnimator;

    [SerializeField] DoorOpeningCollisionChecker doorCheckerPozitive;
    [SerializeField] DoorOpeningCollisionChecker doorCheckerNegative;
    [SerializeField] public DoorSoundPlayer DoorSoundPlayer;

    // The canonical interval is [0 ... 360)
    float ToCanonicalInterval(float euler)
    {
        float _euler = euler;

        while (!(_euler >= 0.0f && _euler < 360.0f))
            _euler += (-Mathf.Sign(_euler)) * 360.0f;

        return _euler;
    }

    private void Awake()
    {
        characterAnimator = GameObject.Find("Character").GetComponent<Animator>();
    }

    void Update()
    {
        //Debug.Log("held = " + heldByCharacter);

        if (doorCheckerPozitive.heldByCharacter || doorCheckerNegative.heldByCharacter)
            return;

        // The door closing naturally:

        float currentAngleCanonical = ToCanonicalInterval(transform.localEulerAngles.y);
        float closedAngleCanonical = ToCanonicalInterval(closedAngleDegrees);

        currentAngleCanonical = ToCanonicalInterval(currentAngleCanonical - closedAngleCanonical);

        if (currentAngleCanonical <= 180.0f)
            currentAngleCanonical -= rotSpeed * Time.deltaTime;
        else
            currentAngleCanonical += rotSpeed * Time.deltaTime;

        // If angle is close to the door-closed-angle:
        if (    Mathf.Abs(360.0f - currentAngleCanonical) < closeRange
            ||  Mathf.Abs(currentAngleCanonical) < closeRange)
            transform.localEulerAngles = new Vector3(
                transform.localEulerAngles.x,
                closedAngleDegrees,
                transform.localEulerAngles.z
            );
        else
            transform.Rotate(0.0f, 0.0f, currentAngleCanonical + closedAngleCanonical - transform.localEulerAngles.y);
    }

    /*private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.name == "Character")
        {
            heldByCharacter = true;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.name == "Character")
        {
            heldByCharacter = false;
        }
    }*/

    /*private void OnTriggerEnter(Collider other)
    {
        if (UIScript.instance.currentMode == GameModes.Modes.Pathfinding && other.gameObject.name == "Character")
        {
            Debug.Log("entered at " + gameObject.name);
            heldByCharacter = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (UIScript.instance.currentMode == GameModes.Modes.Pathfinding && other.gameObject.name == "Character")
        {
            heldByCharacter = false;
        }
    }*/
}
