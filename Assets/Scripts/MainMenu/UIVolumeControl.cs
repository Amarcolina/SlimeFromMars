using UnityEngine;
using System.Collections;

public class UIVolumeControl : MonoBehaviour {

        private UISlider _Volumeslider;
        
        // Use this for initialization
        void Start ()
        {

        }

        void Update(){
                GameObject root = GameObject.FindGameObjectWithTag ("UI");
                PauseMenu pauseMenu = root.GetComponent<PauseMenu> ();
                if(!pauseMenu.muted){
                        _Volumeslider = gameObject.GetComponent<UISlider>();
                        _Volumeslider.sliderValue = AudioListener.volume;
                        _Volumeslider.onValueChange += OnValueChange;
                }
        }
        
        void OnValueChange(float val)
        {
                GameObject root = GameObject.FindGameObjectWithTag ("UI");
                PauseMenu pauseMenu = root.GetComponent<PauseMenu>();
                if (!pauseMenu.muted)
                {
                AudioListener.volume = val;
                }
        }
}
