using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterSoundPlayer : MonoBehaviour
{
    private Sound _walkingSound;
    private Sound _photoShootingSound;
    public Sound WalkingSound => _walkingSound;
    public Sound PhotoShootingSound => _photoShootingSound;

    public void Initialize(Sound walkingSound, Sound photoShootingSound)
    {
        _walkingSound = walkingSound;
        _photoShootingSound = photoShootingSound;
    }

    public void PlayWalkingSound()
    {
        Manager.Instance.MySoundManager.PlaySound(_walkingSound, true, Vector3.zero);
    }

    public void StopPlayingWalkingSound()
    {
        Manager.Instance.MySoundManager.StopSound(_walkingSound, false);
    }

    public void PlayPhotoShootingSound()
    {
        Manager.Instance.MySoundManager.PlaySound(_photoShootingSound, false, Vector3.zero);
    }
}
