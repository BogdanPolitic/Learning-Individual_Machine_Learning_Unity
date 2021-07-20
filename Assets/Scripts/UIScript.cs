using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class UIScript : MonoBehaviour
{
    enum Option
    {
        TRAINING,
        TEST,
        PATHFINDING
    };

    public string objName;
    public Rect windowRect;

    Rect backToMenuRect;
    Rect searchObjectRect;

    GameModes gameStatus;
    public GameModes.Modes currentMode;
    public Rect menuWindowRect, menuWindow;
    int windowWidth = 150;
    int windowHeight = 225;
    int buttonWidth = 100;
    int buttonHeight = 40;

    // dimensions for the Continuous-Capturing-Button:
    int CCbuttonWidth = 2 * Screen.width / 7;
    int CCbuttonHeight = Screen.height / 15;
    public bool CC_ON;

    GameObject panel;
    float fade;
    float fadingSpeed = 0.4f;
    bool hideMenu;
    //bool lastChosenWasTraining = true;
    Option lastChosen = Option.TRAINING;
    GameObject showResultPanel;
    GameObject showResultText;

    public CaptureAndSave captureAndSave;
    public CustomPathfinding customPathfinding;

    void Start()
    {
        objName = "";

        gameStatus = GameObject.Find("GameModes").GetComponent<GameModes>();
        currentMode = GameModes.Modes.Menu;
        panel = GameObject.Find("Panel");

        panel.GetComponent<Image>().color = new Vector4(1, 1, 1, 1.0f);
        hideMenu = false;

        showResultPanel = GameObject.Find("ShowResultPanel");
        showResultText = GameObject.Find("ShowResultText");

        CC_ON = false;
    }

    void OnGUI()
    {
        backToMenuRect = new Rect(Screen.width - 200, 5, 150, 50);
        searchObjectRect = new Rect(Screen.width - 200, Screen.height - 325, 150, 50);
        windowRect = GUI.Window(0, new Rect(5, 5, 150, 50), MyWindow, "");
        menuWindowRect = new Rect(Screen.width / 2 - windowWidth / 2, Screen.height / 2 - windowHeight / 2, windowWidth, windowHeight);

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

        if (currentMode == GameModes.Modes.SceneToMenu && fade >= 1)
            currentMode = GameModes.Modes.Menu;

        if (currentMode == GameModes.Modes.MenuToScene && fade <= 0)
        {
            if (lastChosen == Option.TRAINING)
                currentMode = GameModes.Modes.Training;
            else if (lastChosen == Option.TEST)
                currentMode = GameModes.Modes.Test;
            else
                currentMode = GameModes.Modes.Pathfinding;
        }
        

        if ((currentMode == GameModes.Modes.Training ||
            currentMode == GameModes.Modes.Test ||
            currentMode == GameModes.Modes.Pathfinding) &&
            GUI.Button(backToMenuRect, "Back to menu"))
            currentMode = GameModes.Modes.SceneToMenu;

        if (currentMode == GameModes.Modes.Pathfinding &&
            captureAndSave.objs.ContainsKey(objName) &&
            GUI.Button(searchObjectRect, "Search object"))
            customPathfinding.target = captureAndSave.objs[objName].transform;

        if (currentMode == GameModes.Modes.SceneToMenu)
            customPathfinding.target = null;


        if (currentMode == GameModes.Modes.Training) { 
            string CCstatus = CC_ON
                ?   "ON"
                :   "OFF";
            if (GUI.Button(new Rect(
                    Screen.width / 2 - CCbuttonWidth / 2,
                    14 * Screen.height / 15 - CCbuttonHeight / 2,
                    CCbuttonWidth,
                    CCbuttonHeight
                ), "Continuous Capturing " + CCstatus)) {
                CC_ON = !CC_ON;
            }
        }

        if (currentMode == GameModes.Modes.Test) { 
            showResultPanel.GetComponent<CanvasRenderer>().cull = false;
            showResultText.GetComponent<CanvasRenderer>().cull = false;
        } else {
            showResultPanel.GetComponent<CanvasRenderer>().cull = true;
            showResultText.GetComponent<CanvasRenderer>().cull = true;
        }

        if (currentMode == GameModes.Modes.Menu) {
            showResultText.GetComponent<Text>().text = "";
        }

        panel.GetComponent<Image>().color = new Vector4(1, 1, 1, fade);

        if (currentMode == GameModes.Modes.Menu)
            hideMenu = false;

        if (!hideMenu)
            menuWindow = GUI.Window(0, menuWindowRect, MenuWindow, "Choose any mode");
    }

    void MyWindow(int windowId)
    {
        GUI.Label(new Rect(11, 0, 125, 20), "Name the object");
        objName = GUI.TextField(new Rect(10, 20, 125, 20), objName, 25);
    }

    void MenuWindow(int windowId)
    {
        if (GUI.Button(new Rect(windowWidth / 2 - buttonWidth / 2, windowHeight / 4 - buttonHeight / 2, buttonWidth, buttonHeight), "Training Mode"))
        {
            currentMode = GameModes.Modes.MenuToScene;
            hideMenu = true;
            lastChosen = Option.TRAINING;
        }

        if (GUI.Button(new Rect(windowWidth / 2 - buttonWidth / 2, 2 * windowHeight / 4 - buttonHeight / 2, buttonWidth, buttonHeight), "Test Mode"))
        {
            currentMode = GameModes.Modes.MenuToScene;
            hideMenu = true;
            lastChosen = Option.TEST;
        }

        if (GUI.Button(new Rect(windowWidth / 2 - buttonWidth / 2, 3 * windowHeight / 4 - buttonHeight / 2, buttonWidth, buttonHeight), "Pathfinding"))
        {
            currentMode = GameModes.Modes.MenuToScene;
            hideMenu = true;
            lastChosen = Option.PATHFINDING;
        }
    }
}
