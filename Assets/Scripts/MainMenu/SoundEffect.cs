using UnityEngine;
using System.Collections;

public class SoundEffect : MonoBehaviour {

    //Attach this script as a component to any object with a SFX
    public AudioClip sfx;
    private AudioSource source;
    public bool loop;
    private GameUI gameUI;

    public void Update() {
        if (gameUI.GetComponent<PauseMenu>().sfxMuted) {
            source.mute = true;
        } else {
            source.mute = false;
        }
    }

    // Use this for initialization
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
}