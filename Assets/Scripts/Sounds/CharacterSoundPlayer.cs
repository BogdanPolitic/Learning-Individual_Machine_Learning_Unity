using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterSoundPlayer : MonoBehaviour
{
    private Sound _walkingSound;
    public Sound WalkingSound => _walkingSound;

    public void Initialize(Sound walkingSound)
    {
        _walkingSound = walkingSound;
    }

    public void PlayWalkingSound()
    {
        Manager.Instance.MySoundManager.PlaySound(_walkingSound, true, Vector3.zero);
    }

    public void StopPlayingWalkingSound()
    {
        Manager.Instance.MySoundManager.StopSound(_walkingSound, false);
    }
}
