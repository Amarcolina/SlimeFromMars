using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SoundManager : MonoBehaviour
{
    private GameUI gameUI;
    private static SoundManager soundInstance;
    private List<AudioSource> sounds;

    public static SoundManager getInstance()
    {
        if (soundInstance == null)
        {
            if (soundInstance == null)
                soundInstance = FindObjectOfType<SoundManager>();
        }
        return soundInstance;
        ;
    }

    // Use this for initialization
    void Start()
    {
        //Grab gameUI for menu settings
        gameUI = GameUI.getInstance();
        sounds = new List<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {

        foreach (AudioSource s in sounds)
        {
            if (s != null)
            {
                if (gameUI.GetComponent<PauseMenu>().sfxMuted)
                {
                    s.mute = true;
                }
                else
                    s.mute = false;
            }
        }
    }

    //Plays a sound at position
    public void PlaySound(Transform emitter, AudioClip sfx)
    {
        //Create an empty game object - for tracking audio position
        GameObject go = new GameObject("Audio: " + sfx.name);
        go.transform.position = emitter.position;
        go.transform.parent = emitter;

        AudioSource source = go.AddComponent<AudioSource>();
        //Set clip to the specified sfx
        source.clip = sfx;
        //Add source sound to list of AudioSources
        sounds.Add(source);
        if (sfx != null)
        {
            source.Play();
            //Destroy gameobject after sound has finished playing
            Destroy(go, sfx.length);
        }
    }

    //Overload method for waiting till sound finishes playing
    //Plays a sound and waits to play the sound again if we tell it to
    public void PlaySound(Transform emitter, AudioClip sfx, bool wait)
    {
        //Only add 1 child gameobject for waiting sounds to prevent multiple of the same sounds playing(i.e slime expansion)
        if (emitter.childCount < 1)
        {
            //Create an empty game object - for tracking audio position
            GameObject go = new GameObject("Audio: " + sfx.name);
            go.transform.position = emitter.position;
            go.transform.parent = emitter;

            AudioSource source = go.AddComponent<AudioSource>();
            //Set clip to the specified sfx
            source.clip = sfx;
            //Add source sound to list of AudioSources
            sounds.Add(source);
            if (sfx != null)
            {
                //If we choose to wait
                if (wait)
                {
                    //If sound is not playing
                    if (!go.GetComponent<AudioSource>().isPlaying)
                    {
                        //Play Sound
                        source.Play();
                        //Destroy gameobject after sound has finished playing
                        Destroy(go, sfx.length);
                    }
                }
                else
                    source.Play();
                Destroy(go, sfx.length);
            }
        }

    }

    //Play a looping sound
    public void PlaySoundLoop(Transform emitter, AudioClip sfx)
    {
        //Only add 1 child gameobject for waiting loop sounds
        if (emitter.childCount < 1)
        {
            //Create an empty game object - for tracking audio position
            GameObject go = new GameObject("Audio: " + sfx.name);
            go.transform.position = emitter.position;
            go.transform.parent = emitter;

            AudioSource source = go.AddComponent<AudioSource>();
            //Set clip to the specified sfx
            source.clip = sfx;
            //Add source sound to list of AudioSources
            sounds.Add(source);
            if (sfx != null)
            {
                if (!go.GetComponent<AudioSource>().isPlaying)
                {
                    source.Play();
                    source.loop = true;
                }
            }
        }
    }



}
