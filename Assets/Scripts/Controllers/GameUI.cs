using UnityEngine;
using System.Collections;

public class GameUI : MonoBehaviour {

	public UILabel ResourceCounter_Label;
	public UILabel LightningLevel_Label;
	public GameObject LightningContainer_GameObject;
    
	private static GameUI _gameuiInstance;
	public static GameUI getInstance() {
		if (_gameuiInstance == null) {
			_gameuiInstance = FindObjectOfType<GameUI>();
		}
		return _gameuiInstance;
	}
	
	//Updates the resource counter. Takes in the current amount of resources. Pass the new final amount, not the amount being added.
	public static void ResourceUpdate(float ResourceUpdate){
		ResourceCounter_Label.text = "Resources: " + ResourceUpdate;
	}
	
	//Updates the lightning level by taking the current level as a float. Will show it when first called.
	//Takes in the current amount of resources. Pass the new final amount, not the amount being added.
	public static void LightningUpdate(float CurrentLevel){
		LightningContainer_GameObject.SetActive(true);
		LightningLevel_Label.text = "Lightning Level: " + CurrentLevel;
	}
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	/*
		if(Input.GetKeyDown(KeyCode.W)){
		LightningUpdate(2);
		}
   */
	}
}
