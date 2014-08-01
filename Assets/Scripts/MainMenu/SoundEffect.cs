using UnityEngine;
using System.Collections;

public class SoundEffect : MonoBehaviour
{

    //Attach this to any object with a SFX to allow it to be muted
    public AudioClip sfx;
    private AudioSource source;
    private int scene = -1;

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
        SoundEffect sfx = FindObjectOfType<SoundEffect>();
        sfx.GetComponent<AudioSource>().mute = shouldMute;
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
        SoundEffect[] SFXs = FindObjectsOfType<SoundEffect>();
        if (SFXs.Length != 1)
        {
            Destroy(gameObject);
        }

        DontDestroyOnLoad(gameObject);
        scene = Application.loadedLevel;

        source = gameObject.AddComponent<AudioSource>();
        source.clip = sfx;
    }
}