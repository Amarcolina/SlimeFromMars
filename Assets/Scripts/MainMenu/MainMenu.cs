using UnityEngine;
using System.Collections;

public class MainMenu : MonoBehaviour
{
	//
        void onclickStart ()
        {
                Application.LoadLevel ("NewMainLevel");
                AudioListener.pause = false;
        }

    void onExitClicked()
    {
        //This is ignored in the editor and web player
        Application.Quit();
    }

}
