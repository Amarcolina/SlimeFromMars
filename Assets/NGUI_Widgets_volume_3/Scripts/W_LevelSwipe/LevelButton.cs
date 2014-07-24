using UnityEngine;
using System.Collections;

public class LevelButton : MonoBehaviour 
{
	public string levelName = "noname";
	public bool loadOnClick = false;
	public bool levelIsUnlocked = true;
	
	
	private UISprite lockSprite;
	
	void Start()
	{
		this.lockSprite = (UISprite)this.GetComponentInChildren<UISprite>();
		NGUITools.SetActive(this.lockSprite.gameObject,!this.levelIsUnlocked);
	}
	
	void OnClick () 
	{
		Debug.Log("Loading level stages : "+this.levelName);
		if ((this.loadOnClick) && (this.levelIsUnlocked)) Application.LoadLevel(this.levelName);
	}
	
	
	public void IsUnlocked(bool state)
	{
		this.levelIsUnlocked = state;
		
		NGUITools.SetActive(this.lockSprite.gameObject,!this.levelIsUnlocked);
		
	}
}
