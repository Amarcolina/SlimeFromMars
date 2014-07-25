using UnityEngine;
using System.Collections;

public class GameUI : MonoBehaviour {

	//UI Label References
	public UILabel ResourceCounter_Label;
	public UILabel LightningLevel_Label;
	public UILabel BioLevel_Label;
	public UILabel RadiationLevel_Label;
	//GameObject References, used primarily to activate or deactivate elemental UI
	public GameObject LightningContainer_GameObject;
	public GameObject BioContainer_GameObject;
	public GameObject RadiationContainer_GameObject;
	public GameObject PopoutMenu_GameObject;

	private static GameUI _gameuiInstance;
	public static GameUI getInstance() {
		if (_gameuiInstance == null) {
			_gameuiInstance = FindObjectOfType<GameUI>();
		}
		return _gameuiInstance;
	}
	
	//Updates the resource counter. Takes in the current amount of resources. Pass the new final amount, not the amount being added.
	public void ResourceUpdate(float ResourceUpdate){
		ResourceCounter_Label.text = "Resources: " + ResourceUpdate;
	}
	
	//Updates the lightning level by taking the current level as a float. Will show it when first called.
	//Takes in the current amount of resources. Pass the new final amount, not the amount being added.
	public void LightningUpdate(float CurrentLevel){
		LightningContainer_GameObject.SetActive(true);
		LightningLevel_Label.text = "Lightning Level: " + CurrentLevel;
	}

	//Updates the bio level by taking the current level as a float. Will show it when first called.
    public void BioUpdate(float CurrentLevel){
		BioContainer_GameObject.SetActive(true);
		BioLevel_Label.text = "Bio Level: " + CurrentLevel;
	}

	//Updates the radiation level by taking the current level as a float. Will show it when first called.
	public void RadiationUpdate(float CurrentLevel){
		RadiationContainer_GameObject.SetActive(true);
		RadiationLevel_Label.text = "Radiation Level: " + CurrentLevel;
	}

	//Two functions called upon clicking either skills or close, which open and close the skills panel.
	public void MenuActivated(){
		PopoutMenu_GameObject.SetActive (true);
	}

	public void MenuDeactivated(){
		PopoutMenu_GameObject.SetActive (false);
	}


	// A set of functions set out to be used upon clicking the abilities in the skills panel. Each one corresponds to the icon
	// or ability on the panel.

    // if (Input.GetKeyDown(KeyCode.F6) && radiationLevel > 0 && energy >= RADIATION_OFFENSE_COST)
	public void RadiationOffense(){
        SlimeController.getInstance().useRadiationOffense();
	}

    //if (Input.GetKeyDown(KeyCode.F5) && radiationLevel > 0 && energy >= RADIATION_DEFENSE_COST) {
	public void RadiationDefense(){
        SlimeController.getInstance().useRadiationOffense();
	}

    //if (Input.GetKeyDown(KeyCode.F2) && energy >= ELECTRICITY_OFFENSE_COST) { 
	public void LightningOffense(){
        SlimeController.getInstance().useElectricityOffense();
	}

    //if (Input.GetKeyDown(KeyCode.F1) && electricityLevel > 0 && energy >= ELECTRICITY_DEFENSE_COST)
	public void LightningDefense(){
        SlimeController.getInstance().useElectricityDefense();
	}

    //if (Input.GetKeyDown(KeyCode.F4) && bioLevel > 0 && energy >= BIO_OFFENSE_COST)
	public void BioOffense(){
        SlimeController.getInstance().useBioOffense();
	}

    //if (Input.GetKeyDown(KeyCode.F3) && bioLevel > 0 && energy >= BIO_DEFENSE_COST)
	public void BioDefense(){
        SlimeController.getInstance().useBioDefense();
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
