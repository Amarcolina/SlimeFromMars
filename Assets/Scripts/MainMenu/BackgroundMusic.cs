using UnityEngine;
using System.Collections;

public class BackgroundMusic : MonoBehaviour
{
    public AudioClip music;
    private AudioSource source;
    private static BackgroundMusic musicInstance;
    private int scene = -1;
    public float volume;

    public static BackgroundMusic getInstance()
    {
        if (musicInstance == null)
        {
            musicInstance = FindObjectOfType<BackgroundMusic>();
            if (musicInstance == null)
            {
                GameObject obj = new GameObject("BackgroundMusic");
                musicInstance = obj.AddComponent<BackgroundMusic>();
            }
        }
        return musicInstance;
    }

    void Awake()
    {
        volume = 1f;
        DontDestroyOnLoad(this);
    }

    // Use this for initialization
    void Start()
    {
        BackgroundMusic[] musics = FindObjectsOfType<BackgroundMusic>();
        if (musics.Length != 1)
        {
            Destroy(gameObject);
        }

        //DontDestroyOnLoad(gameObject);
        scene = Application.loadedLevel;
        source = gameObject.AddComponent<AudioSource>();
        source.clip = music;
        source.loop = true;
        source.Play();
        source.volume = volume;
    }
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
        source.volume = volume;
    }

}