using UnityEngine;
using System.Collections;

public class SFXSliderControl : MonoBehaviour
{

    public UISlider _Volumeslider;
    private GameUI gameUI;
    private PauseMenu pause;
    private SoundManager sound;

    // Use this for initialization
    void Start()
    {
        gameUI = GameUI.getInstance();
        sound = SoundManager.getInstance();
        pause = gameUI.GetComponent<PauseMenu>();
    }

    void Update()
    {
        if (!pause.sfxMuted)
        {
            _Volumeslider.sliderValue = sound.volume;
            _Volumeslider.onValueChange += OnValueChange;
        }
    }

    //Volume change for Sound Effects
    void OnValueChange(float val)
    {
        if (!pause.sfxMuted)
        {
            sound.volume = val;
        }
    }


}
