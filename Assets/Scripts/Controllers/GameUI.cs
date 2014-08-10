using UnityEngine;
using System.Collections;

public class GameUI : MonoBehaviour {

    //UI Label References
    public UILabel ResourceCounter_Label;
    public UILabel LightningLevel_Label;
    public UILabel BioLevel_Label;
    public UILabel RadiationLevel_Label;
    public UILabel AbilityText_Label;
    public UILabel WarningText_Label;
	public UILabel SpecialText_Label;
    public UISprite ResourceWindow_Sprite;
    public TweenScale ResourceCountTweener;
    public TweenScale ResourceSpriteTweener;
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
    public GameObject RadiationUnlocked_GameObject;
    public GameObject ElectricityUnlocked_GameObject;
    public GameObject BioUnlocked_GameObject;
    //Private bools for only turning on the shadowed elemental UI once.
    private bool ElectricityActivated = false;
    private bool BioActivated = false;
    private bool RadiationActivated = false;
    private SlimeController _slimeControllerInstance;
    private float warningtime = 0;
    private float specialtime = 0;

    private static GameUI _gameuiInstance;
    public static GameUI getInstance() {
        if (_gameuiInstance == null) {
            _gameuiInstance = FindObjectOfType<GameUI>();
        }
        return _gameuiInstance;
    }

    public void Awake() {
        _slimeControllerInstance = SlimeController.getInstance();
    }

    public void Update() {

        if (Input.GetKeyDown(HotkeyType.BIO_OFFENSIVE_KEY.getKeyCode())) {
            BioOffense();
        }
        if (Input.GetKeyDown(HotkeyType.ELECTRIC_OFFENSIVE_KEY.getKeyCode())) {
            LightningOffense();
        }
        if (Input.GetKeyDown(HotkeyType.RADIATION_OFFENSIVE_KEY.getKeyCode())) {
            RadiationOffense();
        }
        if (Input.GetKeyDown(HotkeyType.BIO_DEFENSIVE_KEY.getKeyCode())) {
            BioDefense();
        }
        if (Input.GetKeyDown(HotkeyType.ELECTRIC_DEFENSIVE_KEY.getKeyCode())) {
            LightningDefense();
        }
        if (Input.GetKeyDown(HotkeyType.RADIATION_DEFENSIVE_KEY.getKeyCode())) {
            RadiationDefense();
        }

        if (specialtime > 0) {
            specialtime -= Time.deltaTime;
        } else {
            SpecialText_Label.enabled = false;   
        }
        
        if (warningtime > 0) {
            warningtime -= Time.deltaTime;
            WarningText_Label.alpha -= .005f;
        } else {
            WarningText_Label.enabled = false;
            RadiationUnlocked_GameObject.SetActive(false);
            ElectricityUnlocked_GameObject.SetActive(false);
            BioUnlocked_GameObject.SetActive(false);
        }
    }

    //Updates the resource counter. Takes in the current amount of resources. Pass the new final amount, not the amount being added.
    // Color of Box is based on how close to 0 resources you are
   public void ResourceUpdate(float ResourceUpdate) {
        ResourceCounter_Label.text = "Resources: " + ResourceUpdate;
        ResourceCountTweener.Reset();
        ResourceCountTweener.Play(true);
        ResourceSpriteTweener.Reset();
        ResourceSpriteTweener.Play(true);
        
        if (ResourceUpdate <= 10) {
            ResourceWindow_Sprite.color = Color.red;
        } else if (ResourceUpdate > 10 && ResourceUpdate <= 20) {
            ResourceWindow_Sprite.color = Color.yellow;   
        } else {
            ResourceWindow_Sprite.color = Color.green;   
        }
    }

    //Updates the lightning level by taking the current level as a float. Will show it when first called.
    //Takes in the current amount of resources. Pass the new final amount, not the amount being added.
    public void LightningUpdate(float CurrentLevel) {
        if (!ElectricityActivated) {
            LightningContainer_GameObject.SetActive(true);
            ElectricityActive_GameObject.SetActive(true);
            ElectricityNonActive_GameObject.SetActive(false);
            ElectricityActivated = true;
            WarningText_Label.enabled = true;
            WarningText_Label.text = "Electricity Mutation Acquired!";
            WarningText_Label.alpha = 1;
            warningtime = 3;
            ElectricityUnlocked_GameObject.SetActive(true);
        }
        LightningLevel_Label.text = "Electricity Level: " + CurrentLevel;
    }

    //Updates the bio level by taking the current level as a float. Will show it when first called.
    public void BioUpdate(float CurrentLevel) {
        if (!BioActivated) {
            BioContainer_GameObject.SetActive(true);
            BioActive_GameObject.SetActive(true);
            BioNonActive_GameObject.SetActive(false);
            BioActivated = true;
            WarningText_Label.enabled = true;
            WarningText_Label.text = "Bio Mutation Acquired!";
            WarningText_Label.alpha = 1;
            warningtime = 3;
            BioUnlocked_GameObject.SetActive(true);
        }
        BioLevel_Label.text = "Bio Level: " + CurrentLevel;
    }

    //Updates the radiation level by taking the current level as a float. Will show it when first called.
    public void RadiationUpdate(float CurrentLevel) {
        if (!RadiationActivated) {
            RadiationContainer_GameObject.SetActive(true);
            RadiationActive_GameObject.SetActive(true);
            RadiationNonActive_GameObject.SetActive(false);
            RadiationActivated = true;
            WarningText_Label.enabled = true;
            WarningText_Label.text = "Radiation Mutation Acquired!";
            WarningText_Label.alpha = 1;
            warningtime = 3;
            RadiationUnlocked_GameObject.SetActive(true);
        }
        RadiationLevel_Label.text = "Radiation Level: " + CurrentLevel;
    }

    //Two functions called upon clicking either skills or close, which open and close the skills panel.
    public void MenuActivated() {
        SkillsToggleButton_GameObject.SetActive(false);
        PopoutMenu_GameObject.SetActive(true);

    }

    public void MenuDeactivated() {
        PopoutMenu_GameObject.SetActive(false);
        SkillsToggleButton_GameObject.SetActive(true);

    }

	public void SlimeTankPopUp(){
        SpecialText_Label.text = "It looks like it could shatter under the right conditions...";
        SpecialText_Label.enabled = true;
        specialtime = 3;
	}


    // A set of functions set out to be used upon clicking the abilities in the skills panel. Each one corresponds to the icon
    // or ability on the panel.
    public void RadiationOffense() {
		if (_slimeControllerInstance.getRadiationLevel() > 0) {
            _slimeControllerInstance.skipNextFrame();
			//SetAbilityRadius(_slimeControllerInstance.getRadiationLevel, _slimeControllerInstance.RADIATION_BASE_RANGE);
            if (checkCanCastAbility(SlimeController.RADIATION_OFFENSE_COST)) {
                _slimeControllerInstance.beginCast(ElementalCastType.RADIATION_OFFENSIVE);
            }
        }

    }

    public void RadiationDefense() {
        if (_slimeControllerInstance.getRadiationLevel() > 0) {
            _slimeControllerInstance.skipNextFrame();
            if (checkCanCastAbility(SlimeController.RADIATION_DEFENSE_COST)) {
                SlimeController.getInstance().beginCast(ElementalCastType.RADIATION_DEFENSIVE);
            }
        }

    }

    public void LightningOffense() {
        if (_slimeControllerInstance.getElectricityLevel() > 0) {
            _slimeControllerInstance.skipNextFrame();
            if (checkCanCastAbility(SlimeController.ELECTRICITY_OFFENSE_COST)) {
                SlimeController.getInstance().beginCast(ElementalCastType.ELECTRICITY_OFFENSIVE);
            }
        }
    }

    public void LightningDefense() {
        if (_slimeControllerInstance.getElectricityLevel() > 0) {
            _slimeControllerInstance.skipNextFrame();
            if (checkCanCastAbility(SlimeController.ELECTRICITY_DEFENSE_COST)) {
                SlimeController.getInstance().useElectricityDefense();
            }
        }
    }

    public void BioOffense() {
        if (_slimeControllerInstance.getBioLevel() > 0) {
            _slimeControllerInstance.skipNextFrame();
            if (checkCanCastAbility(SlimeController.BIO_OFFENSE_COST)) {
                SlimeController.getInstance().beginCast(ElementalCastType.BIO_OFFENSIVE);
            }
        }   
    }

    public void BioDefense() {
        if (_slimeControllerInstance.getBioLevel() > 0) {
            _slimeControllerInstance.skipNextFrame();
            if (checkCanCastAbility(SlimeController.BIO_DEFENSE_COST)) {
                SlimeController.getInstance().useBioDefense();
            }
        }
       
    }

    public bool checkCanCastAbility(int resourcesRequired) {
        if (_slimeControllerInstance.getSelectedSlime() == null) {
            //todo: Must select a slime first alert!
            WarningText_Label.enabled = true;
            WarningText_Label.text = "Must Select A Slime First!";
            WarningText_Label.alpha = 1;
            warningtime = 2;
            return false;
        } else if (_slimeControllerInstance.getEnergyAmount() < resourcesRequired) {
            //todo: Must have enough energy alert!
            WarningText_Label.enabled = true;
            WarningText_Label.text = "Not Enough Energy!";
            WarningText_Label.alpha = 1;
            warningtime = 2;
            return false;
        }
        return true;
    }

    //Functions used for text being added by hovering over buttons
    //************************************************************
    public void LightningOffenseHover() {
        AbilityText_GameObject.SetActive(true);
        AbilityText_Label.text = "Chain Lightning: Deals damage to a target, then arcs to targets in a short radius.";
    }

    public void LightningOffenseHoverOut() {
        AbilityText_GameObject.SetActive(false);
    }

    public void LightningDefenseHover() {
        AbilityText_GameObject.SetActive(true);
        AbilityText_Label.text = "Electric Shield: Electrifies a tile of slime, causing an electric burst that damages enemies";
    }

    public void LightningDefenseHoverOut() {
        AbilityText_GameObject.SetActive(false);
    }

    public void BioOffenseHover() {
        AbilityText_GameObject.SetActive(true);
        AbilityText_Label.text = "Lethal Mutation: Gives the slime the ability to use deadly tentacle attacks.";
    }

    public void BioOffenseHoverOut() {
        AbilityText_GameObject.SetActive(false);
    }

    public void BioDefenseHover() {
        AbilityText_GameObject.SetActive(true);
        AbilityText_Label.text = "Natural Defenses: Fortifies one area of the slime, increasing defenses in that area";
    }

    public void BioDefenseHoverOut() {
        AbilityText_GameObject.SetActive(false);
    }

    public void RadiationOffenseHover() {
        AbilityText_GameObject.SetActive(true);
        AbilityText_Label.text = "Fallout: Covers an area in deadly radiation, harming enemies in the radius.";
    }

    public void RadiationOffenseHoverOut() {
        AbilityText_GameObject.SetActive(false);
    }

    public void RadiationDefenseHover() {
        AbilityText_GameObject.SetActive(true);
        AbilityText_Label.text = "Toxic Barrier: Irradiates a tile, stunning enemies who come into contact with it briefly";
    }

    public void RadiationDefenseHoverOut() {
        AbilityText_GameObject.SetActive(false);
    }
    //*********************************************
    //*********************************************
}
