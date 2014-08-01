using UnityEngine;
using System.Collections;

public class BackgroundMusic : MonoBehaviour
{
    public AudioClip music;
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
        BackgroundMusic music = FindObjectOfType<BackgroundMusic>();
        music.GetComponent<AudioSource>().mute = shouldMute;
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
        BackgroundMusic[] musics = FindObjectsOfType<BackgroundMusic>();
        if (musics.Length != 1)
        {
            Destroy(gameObject);
        }

        DontDestroyOnLoad(gameObject);
        scene = Application.loadedLevel;

        source = gameObject.AddComponent<AudioSource>();
        source.clip = music;
        source.loop = true;
        source.Play();
    }
}