using UnityEngine;
using System.Collections;

public class SoundEffect : MonoBehaviour
{

    //Attach this script as a component to any object with a SFX
    public AudioClip sfx;
    private AudioSource source;

    private GameUI gameUI;

    public static void mute(bool shouldMute)
    {
        //SoundEffect sfx = FindObjectOfType<SoundEffect>();

        SoundEffect[] SFXs = FindObjectsOfType<SoundEffect>();
        foreach (SoundEffect sfx in SFXs)
        {
            //sfx.GetComponent<AudioSource>().mute = shouldMute;
        }
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            mute(!source.mute);
        }
        if (gameUI.GetComponent<PauseMenu>().sfxMuted)
        {
            source.mute = true;
        }
        else
            source.mute = false;
    }

    // Use this for initialization
    void Start()
    {
        gameUI = GameUI.getInstance();
        //DontDestroyOnLoad(gameObject);
        source = gameObject.AddComponent<AudioSource>();
        source.clip = sfx;
       // if (!gameUI.GetComponent<PauseMenu>().sfxMuted)
       // {
            source.Play();
       // }
    }
}