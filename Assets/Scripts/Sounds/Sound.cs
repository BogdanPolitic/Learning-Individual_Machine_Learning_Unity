using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Sound : System.ICloneable
{
    public AudioClip clip;

    [HideInInspector]
    public string name;

    [Range(0f, 1f)]
    public float volume;

    [Range(.1f, 100f)]
    public float pitch;

    public bool loop;

    [HideInInspector]
    public AudioSource audioSource;

    [HideInInspector]
    public GameObject gameObject;

    public object Clone()
    {
        var sound = (Sound)MemberwiseClone();
        sound.clip = clip;
        sound.name = name;
        sound.volume = volume;
        sound.pitch = pitch;
        sound.loop = loop;
        return sound;
    }

    public Sound Clone(GameObject gameObject)
    {
        Sound clonedSound = (Sound)Clone();
        clonedSound.gameObject = gameObject;
        clonedSound.audioSource = gameObject.AddComponent<AudioSource>();
        clonedSound.audioSource.clip = clonedSound.clip;
        clonedSound.name = clonedSound.clip.name;
        clonedSound.audioSource.volume = clonedSound.volume;
        clonedSound.audioSource.pitch = clonedSound.pitch;
        clonedSound.audioSource.loop = clonedSound.loop;
        return clonedSound;
    }
}
