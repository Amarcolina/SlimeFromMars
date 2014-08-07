using UnityEngine;
using System.Collections;

public class SoundManager : MonoBehaviour
{
    private GameUI gameUI;
    private AudioSource audioSource;
    private static SoundManager soundInstance;

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
    }

    // Update is called once per frame
    void Update()
    {
        if (audioSource != null)
        {
            if (gameUI.GetComponent<PauseMenu>().sfxMuted)
            {
                audioSource.mute = true;
            }
            else
            {
                audioSource.mute = false;
            }
        }
    }

    //Plays a sound and waits until it is finished to play the sound again
    public void PlaySoundAndWait(Transform emitter, AudioClip sfx)
    {
        //Only add 1 child gameobject for waiting sounds to prevent multiple of the same sounds playing(i.e slime expansion)
        if (emitter.childCount < 1)
        {
            //Create an empty game object - for tracking audio position
            GameObject go = new GameObject("Audio: " + sfx.name);
            go.transform.position = emitter.position;
            go.transform.parent = emitter;

            AudioSource source = go.AddComponent<AudioSource>();
            //Allow it to mute
            audioSource = source;
            //Set clip to the specified sfx
            source.clip = sfx;
            if (sfx != null)
            {
                if (!go.GetComponent<AudioSource>().isPlaying)
                {
                    source.Play();
                    //Destroy gameobject after sound has finished playing
                    Destroy(go, sfx.length);
                }
            }
        }
        
    }
    //Play a looping sound
    public void PlayLoop(Transform emitter, AudioClip sfx)
    {
        //Only add 1 child gameobject for waiting loop sounds
        if (emitter.childCount < 1)
        {
            //Create an empty game object - for tracking audio position
            GameObject go = new GameObject("Audio: " + sfx.name);
            go.transform.position = emitter.position;
            go.transform.parent = emitter;

            AudioSource source = go.AddComponent<AudioSource>();
            //Allow it to mute
            audioSource = source;
            //Set clip to the specified sfx
            source.clip = sfx;
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
    //Plays a sound at position
    public void PlaySound(Transform emitter, AudioClip sfx)
    {
            //Create an empty game object - for tracking audio position
            GameObject go = new GameObject("Audio: " + sfx.name);
            go.transform.position = emitter.position;
            go.transform.parent = emitter;

            AudioSource source = go.AddComponent<AudioSource>();
            //Allow it to mute
            audioSource = source;
            //Set clip to the specified sfx
            source.clip = sfx;
            if (sfx != null)
            {
                if (!go.GetComponent<AudioSource>().isPlaying)
                {
                    source.Play();
                    //Destroy gameobject after sound has finished playing
                    Destroy(go, sfx.length);
                }
            }
    }



}
