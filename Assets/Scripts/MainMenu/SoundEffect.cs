using UnityEngine;
using System.Collections;

public class SoundEffect : MonoBehaviour {

    //Attach this script as a component to any object with a SFX
    public AudioClip sfx;
    private AudioSource source;
    public bool loop;
    private GameUI gameUI;

    //Check for mute settings
    public void Update() {
        if (gameUI.GetComponent<PauseMenu>().sfxMuted) {
            source.mute = true;
        } else {
            source.mute = false;
        }
    }

    //Should play a sound on start of the script
    void Start() {
        gameUI = GameUI.getInstance();
        //DontDestroyOnLoad(gameObject);
        source = gameObject.AddComponent<AudioSource>();
        source.clip = sfx;
        // if (!gameUI.GetComponent<PauseMenu>().sfxMuted)
        // {
        //AudioSource.PlayClipAtPoint(sfx, gameObject.transform.position);
            source.Play();
        
        if (loop)
        {
            source.loop = true;
        }
        // }
    }

    //Use this to force play a sound
    public void PlaySound(AudioClip sfx)
    {
        if(source!=null)
        source.clip = sfx;
        source.Play();
        
    }
}