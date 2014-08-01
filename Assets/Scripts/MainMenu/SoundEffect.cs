using UnityEngine;
using System.Collections;

public class SoundEffect : MonoBehaviour
{

    //Attach this script as a component to any object with a SFX
    public AudioClip sfx;
    private AudioSource source;
    private int scene = -1;
    private bool muted;

    private GameUI gameUI;

    void OnLevelWasLoaded()
    {
        if (scene != -1 && Application.loadedLevel != scene)
        {
            Destroy(gameObject);
            return;
        }
    }

    public static void mute(bool shouldMute)
    {
        //SoundEffect sfx = FindObjectOfType<SoundEffect>();

        SoundEffect[] SFXs = FindObjectsOfType<SoundEffect>();
        foreach (SoundEffect sfx in SFXs)
        {
            sfx.GetComponent<AudioSource>().mute = shouldMute;
        }
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            mute(!source.mute);
        }
    }

    // Use this for initialization
    void Start()
    {
        gameUI = GameUI.getInstance();
        gameUI.GetComponent<PauseMenu>();

        DontDestroyOnLoad(gameObject);
        scene = Application.loadedLevel;
        source = gameObject.AddComponent<AudioSource>();
        source.clip = sfx;
        if (!gameUI.GetComponent<PauseMenu>().sfxMuted)
        {
            source.Play();
        }
    }
}