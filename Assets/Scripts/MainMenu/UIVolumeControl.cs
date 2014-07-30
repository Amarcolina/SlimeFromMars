using UnityEngine;
using System.Collections;

public class UIVolumeControl : MonoBehaviour {

        private UISlider _Volumeslider;
        private GameUI gameUI;
        private PauseMenu pause;
        
        // Use this for initialization
        void Start ()
        {
            gameUI = GameUI.getInstance();
            pause = gameUI.GetComponent<PauseMenu>();
        }

        void Update(){
                if(!pause.muted){
                        _Volumeslider = gameObject.GetComponent<UISlider>();
                        _Volumeslider.sliderValue = AudioListener.volume;
                        _Volumeslider.onValueChange += OnValueChange;
                }
        }
        
        void OnValueChange(float val)
        {   
                if (!pause.muted)
                {
                AudioListener.volume = val;
                }
        }
}
