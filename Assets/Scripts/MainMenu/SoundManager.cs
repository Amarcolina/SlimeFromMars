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
        source = gameObject.AddComponent<AudioSource>();
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
    public void PlaySoundAndWait(GameObject gameObj, AudioClip sfx)
    {
        source.clip = sfx;
        if (sfx != null /*&& !source.isPlaying*/)
        {
            if (!source.isPlaying)
            {
                source.Play();
                //AudioSource.PlayClipAtPoint(source.clip, gameObj.transform.position);
            }
        }
    }
    public void PlaySoundAndWait(GameObject gameObj, AudioClip sfx, bool loop)
    {
        source.clip = sfx;
        if (sfx != null)
        {
            if (!source.isPlaying)
            {
                source.Play();
                //AudioSource.PlayClipAtPoint(source.clip, gameObj.transform.position);
            }
        }
        if (loop)
        {
            source.loop = true;
        }
    }

    public void PlaySound(GameObject gameObj, AudioClip sfx)
    {
        source.clip = sfx;
        if (sfx != null /*&& !source.isPlaying*/)
        {
                AudioSource.PlayClipAtPoint(source.clip, gameObj.transform.position);
            
        }
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
