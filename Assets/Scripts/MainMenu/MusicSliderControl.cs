using UnityEngine;
using System.Collections;

public class MusicSliderControl : MonoBehaviour
{

    public UISlider _Volumeslider;
    private GameUI gameUI;
    private PauseMenu pause;
    private BackgroundMusic music;

    // Use this for initialization
    void Start()
    {
        gameUI = GameUI.getInstance();
        music = BackgroundMusic.getInstance();
        pause = gameUI.GetComponent<PauseMenu>();
    }

    void Update()
    {
        if (!pause.musicMuted)
        {
            _Volumeslider.sliderValue = music.volume;
            _Volumeslider.onValueChange += OnMusicValueChange;
        }
    }

    //Volume change for Music
    void OnMusicValueChange(float val)
    {
        if (!pause.musicMuted)
        {
            music.volume = val;
        }
    }


}
