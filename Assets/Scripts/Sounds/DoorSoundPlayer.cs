using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorSoundPlayer : MonoBehaviour
{
    private Sound _creakingSound;
    public Sound CreakingSound => _creakingSound;

    public void Initialize(Sound creakingSound)
    {
        _creakingSound = creakingSound;
    }

    public void PlayCreakingSound()
    {
        Manager.Instance.MySoundManager.PlaySound(_creakingSound, false, Vector3.zero);
    }
}
