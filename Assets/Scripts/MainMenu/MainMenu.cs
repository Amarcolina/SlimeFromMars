using UnityEngine;
using System.Collections;

public class MainMenu : MonoBehaviour
{
	//
        void onclickStart ()
        {
                Application.LoadLevel ("defaultScene");
                AudioListener.pause = false;
        }

}
