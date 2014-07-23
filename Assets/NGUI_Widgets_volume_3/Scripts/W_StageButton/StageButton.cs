using UnityEngine;
using System.Collections;

public class StageButton : MonoBehaviour 
{
	public string levelName = "noname";
	public bool loadOnClick = false;
	
	void OnClick () 
	{
		Debug.Log("Loading level: "+this.levelName);
		if (this.loadOnClick) Application.LoadLevel(this.levelName);
	}
	
}
