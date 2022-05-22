using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MySoundManager : MonoBehaviour
{
    [SerializeField] List<Sound> MySounds;
    [SerializeField] CharacterSoundPlayer MainCharacter;

    public void Initialize()
    {
        foreach (Sound s in MySounds)
        {
            s.audioSource = gameObject.AddComponent<AudioSource>();
            s.audioSource.clip = s.clip;
            s.name = s.clip.name;
            s.audioSource.volume = s.volume;
            s.audioSource.pitch = s.pitch;
            s.audioSource.loop = s.loop;
        }

        MainCharacter.Initialize(FindSound("Walking").Clone(MainCharacter.gameObject));
    }

    Sound FindSound(string name)
    {
        return MySounds.Find(s => s.name == name);
    }

    public void PlaySound(string soundName, bool loop, Vector3 position)
    {
        Sound s = FindSound(soundName);
        if (s == null)
            return;
        PlaySound(s, loop, position);
    }

    public void PlaySound(Sound sound, bool loop, Vector3 position)
    {
        if (sound == null) return;
        sound.audioSource.loop = loop;
        sound.audioSource.Play();
    }

    public void StopSound(string soundName, bool finishLoop)
    {
        Sound s = FindSound(soundName);
        if (s == null)
            return;
        StopSound(s, finishLoop);
    }

    public void StopSound(Sound sound, bool finishLoop)
    {
        if (sound == null)
            return;

        if (finishLoop)
            sound.audioSource.loop = false;
        else
            sound.audioSource.Stop();
    }
}
