using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SoundManager : MonoBehaviour
{
    private GameUI gameUI;
    private static SoundManager soundInstance;
    private List<AudioSource> sounds;
    private AudioSource[] sources;

    public static SoundManager getInstance(){
        if (soundInstance == null){
            GameObject obj = new GameObject("Sound Manager");
            soundInstance = obj.AddComponent<SoundManager>();
        }
        return soundInstance;
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
        for (int i = sounds.Count - 1; i >= 0; i--)
        {
            if (sounds[i] != null)
            {
                if (gameUI.GetComponent<PauseMenu>().sfxMuted)
                {
                    sounds[i].mute = true;
                }
                else
                {
                    sounds[i].mute = false;
                }
            }
            else
            {
                sounds.RemoveAt(i);
            }
        }
    }

    public void PlaySound(Transform emitter, AudioClip sfx, bool wait = false, bool loop = false)
    {
        //If the provided sfx clip is null, we don't do anything
        if (sfx == null)
        {
            return;
        }

        //Check to see if we wait, if so then check if the clip is the same as the one playing
        //If it is then simply return, this ensures we do not play a sound if there is one playing
        if (wait)
        {
            //Grab all audiosources from the transforms children
            sources = emitter.GetComponentsInChildren<AudioSource>();
            foreach (AudioSource s in sources)
            {
                //If the audiosource is the same one that we want to play
                if (s.clip == sfx && s.isPlaying)
                {
                    return;
                }
            }
        }
        //Create an empty game object - for tracking audio position
        GameObject go = new GameObject("Audio: " + sfx.name);
        go.transform.position = emitter.position;
        go.transform.parent = emitter;
        Destroy(go, sfx.length);

        AudioSource source = go.AddComponent<AudioSource>();
        //Set clip to the specified sfx
        source.clip = sfx;
        //Add source sound to list of AudioSources                             
        sounds.Add(source);
        //Mute the source if the game ui specifies it should be muted                       
        source.mute = gameUI.GetComponent<PauseMenu>().sfxMuted;
        //Loop the source if the user wants to loop      
        source.loop = loop;
        //Finish by playing the audio source                        
        source.Play();
    }
}