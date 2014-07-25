﻿using UnityEngine;
using System.Collections;

public class PauseMenu : MonoBehaviour
{


    private float startTime = 0.1f;
    public bool muted;


    //Main Pause Menu
    public GameObject continuelabel;
    public GameObject optionslabel;
    public GameObject exitlabel;
    public GameObject pauselabel;

    //Options Pause menu
    public GameObject optionstitle;
    public GameObject volumetitle;
    public GameObject backlabel;
    public GameObject volumeslider;
    public GameObject fpsbox;

    public GameObject fpslabel;
    public UILabel fpstitle;
    public GameObject mutebox;

    void Start()
    {
        //Screen.lockCursor = true;
        Time.timeScale = 1;
    }

    void Update()
    {
        //When user hits esc key, go to pausemenu
            if (Input.GetKeyDown("escape"))
            {
                Paused();
            }
        
    }
    //Pause game and activate pausemenu gameobjects
    public void Paused()
    {
        if (!IsGamePaused())
        {
            //Display main pause labels
            pauselabel.SetActive(true);
            continuelabel.SetActive(true);
            optionslabel.SetActive(true);
            exitlabel.SetActive(true);
        }
        PauseGame();
    }
    //Resume game on continue
    public void ContinueClicked()
    {
        //Remove main pause labels
        pauselabel.SetActive(false);
        continuelabel.SetActive(false);
        optionslabel.SetActive(false);
        exitlabel.SetActive(false);

        UnPauseGame();

        //lock player controls
       /* GameObject gameObject = GameObject.FindGameObjectWithTag("Player");
        PlayerController control = gameObject.GetComponent<PlayerController>();
        control.playerLocked = false;*/
    }
    //Remove main pause menu and display options settings
    public void OptionsClicked()
    {
        //If options are clicked, remove the main pause labels
        pauselabel.SetActive(false);
        continuelabel.SetActive(false);
        optionslabel.SetActive(false);
        exitlabel.SetActive(false);

        //And display the new options labels
        optionstitle.SetActive(true);
        volumetitle.SetActive(true);
        backlabel.SetActive(true);
        volumeslider.SetActive(true);
        mutebox.SetActive(true);
        fpsbox.SetActive(true);
    }
    //Go back to Main Pause Menu and remove options
    public void BackClicked()
    {
        //Display main pause labels
        pauselabel.SetActive(true);
        continuelabel.SetActive(true);
        optionslabel.SetActive(true);
        exitlabel.SetActive(true);

        //Remove option labels
        optionstitle.SetActive(false);
        volumetitle.SetActive(false);
        backlabel.SetActive(false);
        volumeslider.SetActive(false);
        mutebox.SetActive(false);
        fpsbox.SetActive(false);
    }

    bool IsBeginning()
    {
        return (Time.time < startTime);
    }

    void PauseGame()
    {

        //Pause gametime
        Time.timeScale = 0;
        AudioListener.pause = true;
        Screen.lockCursor = false;
        Screen.showCursor = true;
    }

    public void UnPauseGame()
    {
        //Resume gametime
        Time.timeScale = 1;
        AudioListener.pause = false;
        Screen.lockCursor = false;
    }
    //Exit game on button click
    public void ExitGame()
    {
        Application.LoadLevel("MainMenu");
    }

    bool IsGamePaused()
    {
        return (Time.timeScale == 0);
    }

    void OnApplicationPause(bool pause)
    {
        if (IsGamePaused())
        {
            AudioListener.pause = true;
        }
    }

    public void showFPS(bool isActive)
    {
        if (isActive)
        {
            fpslabel.SetActive(true);
        }
        else
            fpslabel.SetActive(false);
    }

    public void MuteButton(bool isActive)
    {
        muted = isActive;
        if (isActive)
        {
            AudioListener.volume = 0;
        }
        else
            AudioListener.volume = 1;

    }
}