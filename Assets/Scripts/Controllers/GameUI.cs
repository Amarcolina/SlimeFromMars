using UnityEngine;
using System.Collections;

public class GameUI : MonoBehaviour {

	//UI Label References
	public UILabel ResourceCounter_Label;
	public UILabel LightningLevel_Label;
	public UILabel BioLevel_Label;
	public UILabel RadiationLevel_Label;
	public UILabel AbilityText_Label;
	//GameObject References, used primarily to activate or deactivate elemental UI
	public GameObject LightningContainer_GameObject;
	public GameObject BioContainer_GameObject;
	public GameObject RadiationContainer_GameObject;
	public GameObject PopoutMenu_GameObject;
	public GameObject SkillsToggleButton_GameObject;
	public GameObject AbilityText_GameObject;
	//GameObject References, for toggling shadows on non-unlocked abilities
	public GameObject ElectricityActive_GameObject;
	public GameObject BioActive_GameObject;
	public GameObject RadiationActive_GameObject;
	public GameObject ElectricityNonActive_GameObject;
	public GameObject BioNonActive_GameObject;
	public GameObject RadiationNonActive_GameObject;
	//Private bools for only turning on the shadowed elemental UI once.
	private bool ElectricityActivated = false;
	private bool BioActivated = false;
	private bool RadiationActivated = false;

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
		if (!ElectricityActivated) {
			LightningContainer_GameObject.SetActive (true);
			ElectricityActive_GameObject.SetActive(true);
			ElectricityNonActive_GameObject.SetActive(false);
			ElectricityActivated = true;
		}
		LightningLevel_Label.text = "Lightning Level: " + CurrentLevel;
	}

	//Updates the bio level by taking the current level as a float. Will show it when first called.
    public void BioUpdate(float CurrentLevel){
		if (!BioActivated) {
			BioContainer_GameObject.SetActive (true);
			BioActive_GameObject.SetActive(true);
			BioNonActive_GameObject.SetActive(false);
			BioActivated = true;
		}
		BioLevel_Label.text = "Bio Level: " + CurrentLevel;
	}

	//Updates the radiation level by taking the current level as a float. Will show it when first called.
	public void RadiationUpdate(float CurrentLevel){
		if (!RadiationActivated) {
			RadiationContainer_GameObject.SetActive (true);
			RadiationActive_GameObject.SetActive(true);
			RadiationNonActive_GameObject.SetActive(false);
			RadiationActivated = true;
		}
		RadiationLevel_Label.text = "Radiation Level: " + CurrentLevel;
	}

	//Two functions called upon clicking either skills or close, which open and close the skills panel.
	public void MenuActivated(){
		SkillsToggleButton_GameObject.SetActive(false);
		PopoutMenu_GameObject.SetActive (true);
	}

	public void MenuDeactivated(){
		PopoutMenu_GameObject.SetActive (false);
		SkillsToggleButton_GameObject.SetActive (true);
	}


	// A set of functions set out to be used upon clicking the abilities in the skills panel. Each one corresponds to the icon
	// or ability on the panel.

	public void RadiationOffense(){

	}

	public void RadiationDefense(){

	}

	public void LightningOffense(){

	}

	public void LightningDefense(){

	}

	public void BioOffense(){

	}

	public void BioDefense(){

	}

	//Functions used for text being added by hovering over buttons
	//************************************************************
	public void LightningOffenseHover(){
		AbilityText_GameObject.SetActive (true);
		AbilityText_Label.text = "Chain Lightning: Deals damage to a target, then bounces to targets in a short radius.";
	}

	public void LightningOffenseHoverOut(){
		AbilityText_GameObject.SetActive (false);
	}

	public void LightningDefenseHover(){
		AbilityText_GameObject.SetActive (true);
		AbilityText_Label.text = "Electricity Shield: Provides the slime with a deadly electric barrier around its body.";
	}
	
	public void LightningDefenseHoverOut(){
		AbilityText_GameObject.SetActive (false);
	}

	public void BioOffenseHover(){
		AbilityText_GameObject.SetActive (true);
		AbilityText_Label.text = "Lethal Mutation: Gives the slime the ability to use deadly tentacle attacks.";
	}
	
	public void BioOffenseHoverOut(){
		AbilityText_GameObject.SetActive (false);
	}

	public void BioDefenseHover(){
		AbilityText_GameObject.SetActive (true);
		AbilityText_Label.text = "Natural Defenses: Increases the slimes natural armor, giving it more durability";
	}
	
	public void BioDefenseHoverOut(){
		AbilityText_GameObject.SetActive (false);
	}

	public void RadiationOffenseHover(){
		AbilityText_GameObject.SetActive (true);
		AbilityText_Label.text = "Fallout: Covers an area in deadly radiation, harming enemies in the radius.";
	}
	
	public void RadiationOffenseHoverOut(){
		AbilityText_GameObject.SetActive (false);
	}

	public void RadiationDefenseHover(){
		AbilityText_GameObject.SetActive (true);
		AbilityText_Label.text = "Toxicity: Gives the slime a deadly irradiated body that deals damage to nearby enemy units";
	}
	
	public void RadiationDefenseHoverOut(){
		AbilityText_GameObject.SetActive (false);
	}
	//*********************************************
	//*********************************************


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
