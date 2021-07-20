using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class SwitchGameMode : MonoBehaviour
{
    GameModes gameStatus;
    GameModes.Modes currentMode;
    public Rect menuWindowRect, menuWindow;
    int windowWidth = 150;
    int windowHeight = 150;
    int buttonWidth = 100;
    int buttonHeight = 40;

    GameObject panel;
    float fade;
    float fadingSpeed = 0.2f;
    bool hideMenu;

    void Start()
    {
        gameStatus = GameObject.Find("GameModes").GetComponent<GameModes>();
        currentMode = GameModes.Modes.Menu;
        menuWindowRect = new Rect(Screen.width / 2 - windowWidth / 2, Screen.height / 2 - windowHeight / 2, windowWidth, windowHeight);
        panel = GameObject.Find("Panel");

        panel.GetComponent<Image>().color = new Vector4(1, 1, 1, 1.0f);
        hideMenu = false;
    }

    void OnGUI()
    {
        switch (currentMode)
        {
            case GameModes.Modes.Training:
            case GameModes.Modes.Test:
                fade = 0;
                break;
            case GameModes.Modes.Menu:
                fade = 1;
                break;
            case GameModes.Modes.MenuToScene:
                fade -= fadingSpeed * Time.deltaTime;
                break;
            case GameModes.Modes.SceneToMenu:
                fade += fadingSpeed * Time.deltaTime;
                break;
            default:
                break;
        }

        panel.GetComponent<Image>().color = new Vector4(1, 1, 1, fade);

        if (!hideMenu)
            menuWindow = GUI.Window(0, menuWindowRect, MenuWindow, "Choose any mode");
    }

    void MenuWindow(int windowId)
    {
        if (GUI.Button(new Rect(windowWidth / 2 - buttonWidth / 2, windowHeight / 3 - buttonHeight / 2, buttonWidth, buttonHeight), "Training Mode"))
        {
            currentMode = GameModes.Modes.MenuToScene;
            hideMenu = true;
        }

        if (GUI.Button(new Rect(windowWidth / 2 - buttonWidth / 2, 2 * windowHeight / 3 - buttonHeight / 2, buttonWidth, buttonHeight), "Test Mode"))
        {
            currentMode = GameModes.Modes.MenuToScene;
            hideMenu = true;
        }
    }
}
