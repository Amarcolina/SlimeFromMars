using UnityEngine;
using System.Collections;

public class SoundManager : MonoBehaviour
{
    private GameUI gameUI;
    private AudioSource source;

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
        gameUI = GameUI.getInstance();
        //source = gameObject.AddComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        if (source != null)
        {
            if (gameUI.GetComponent<PauseMenu>().sfxMuted)
            {
                source.mute = true;
            }
            else
            {
                source.mute = false;
            }
        }
    }

    //Plays a sound and waits until it is finished to play the sound again
    public void PlaySoundAndWait(Transform emitter, AudioClip sfx)
    {
        //Create an empty game object
        GameObject go = new GameObject("Audio: " + sfx.name);
        go.transform.position = emitter.position;
        go.transform.parent = emitter;

        ////Create the source
        AudioSource source = go.AddComponent<AudioSource>();
        source.clip = sfx;
        //if (!source.isPlaying)
        //{
        //    source.Play();
        //    Destroy(go, sfx.length);
        //}
       // source = gameObject.AddComponent<AudioSource>();
        if (sfx != null /*&& !source.isPlaying*/)
        {
            if (!source.isPlaying)
            {
                source.Play();
                Destroy(go, sfx.length);
                //AudioSource.PlayClipAtPoint(source.clip, gameObj.transform.position);
            }
        }
    }
    //Override method to loop sound
    public void PlayLoop(AudioClip sfx)
    {
      //  source = gameObject.AddComponent<AudioSource>();
        source.clip = sfx;
        if (sfx != null)
        {
            if (!source.isPlaying)
            {
                source.Play();
                source.loop = true;
                //AudioSource.PlayClipAtPoint(source.clip, gameObj.transform.position);
            }
        }
    }
    //Plays a sound at position
    public void PlaySound(AudioClip sfx)
    {
        source = gameObject.AddComponent<AudioSource>();
        source.clip = sfx;
        if (sfx != null /*&& !source.isPlaying*/)
        {
            source.Play();
        }
        Destroy(gameObject.GetComponent<AudioSource>());
    }


    public bool isPlaying()
    {
        if (source.isPlaying)
        {
            return true;
        }
        else
            return false;
    }



}
