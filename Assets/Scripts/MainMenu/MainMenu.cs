using UnityEngine;
using System.Collections;

public class MainMenu : MonoBehaviour
{

        void OnClickStart ()
        {
                Application.LoadLevel ("MainLevel");
                AudioListener.pause = false;
        }

}
