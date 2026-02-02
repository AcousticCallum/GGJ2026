using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class SoundManager : MonoBehaviour
{
    public static SoundManager instance;

    public AudioMixer audioMixer;

    public Sound[] sounds;
    public float blacklistClearCooldown;

    private List<string> blacklist = new List<string>();

    private void Awake()
    {
        // Another instance exits
        if (instance && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        // Singleton
        instance = this;
        DontDestroyOnLoad(gameObject);

        // Initialize sounds
        foreach (Sound s in sounds)
        {
            s.source = gameObject.AddComponent<AudioSource>();
            
            s.source.clip = s.clip;
            s.source.outputAudioMixerGroup = s.mixerGroup;

            s.source.loop = s.loop;
            s.source.volume = s.volume;

            s.source.pitch = s.pitch;
        }

        // Start blacklist clear loop
        StartCoroutine(BlacklistClearLoop());
    }

    private void Start()
    {
        PlaySound("Music");
    }

    private IEnumerator BlacklistClearLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(blacklistClearCooldown);

            blacklist.Clear();
        }
    }

    public void PlaySound(string soundName, bool overrideBlacklist = false)
    {
        if (!overrideBlacklist && blacklist.Contains(soundName))
        {
            return;
        }

        blacklist.Add(soundName);

        foreach(Sound s in sounds)
        {
            if (s.name.Equals(soundName))
            {
                s.source.pitch = s.pitch + Random.value * s.pitchVariation;
                s.source.Play();
                return;
            }
        }

        //Debug.Log("Couldn't find sound named '" + soundName + "'.");
    }
}
