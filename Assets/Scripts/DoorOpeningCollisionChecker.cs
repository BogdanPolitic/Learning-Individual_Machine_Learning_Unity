using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorOpeningCollisionChecker : MonoBehaviour
{
    public enum CheckerType
    {
        EULER_NEGATIVE,
        EULER_POZITIVE
    }

    DoorOpening doorParameters;
    public CheckerType checkerType;
    public bool heldByCharacter = false;
    private Animator characterAnimator;

    private void Awake()
    {
        doorParameters = transform.parent.GetComponent<DoorOpening>();
        characterAnimator = GameObject.Find("Character").GetComponent<Animator>();
    }

    void Update()
    {
        if (heldByCharacter)
        {
            transform.parent.Rotate(
                0.0f, 
                0.0f, 
                (
                    checkerType == CheckerType.EULER_POZITIVE
                    ? doorParameters.rotSpeed * Time.deltaTime
                    : -doorParameters.rotSpeed * Time.deltaTime
                )
            );
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name == "Character")
        {
            heldByCharacter = true;
            if (checkerType == CheckerType.EULER_NEGATIVE)
                characterAnimator.SetBool("PushingWithRightHand", true);
            else
                characterAnimator.SetBool("PushingWithLeftHand", true);

            doorParameters.DoorSoundPlayer.PlayCreakingSound();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.name == "Character")
        {
            heldByCharacter = false;
            if (checkerType == CheckerType.EULER_NEGATIVE)
                characterAnimator.SetBool("PushingWithRightHand", false);
            else
                characterAnimator.SetBool("PushingWithLeftHand", false);
        }
    }
}
