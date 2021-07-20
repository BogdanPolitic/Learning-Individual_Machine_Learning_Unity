using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameModes : MonoBehaviour
{
    public enum Modes
    {
        Training,
        Test,
        Pathfinding,
        Menu,
        MenuToScene,
        SceneToMenu
    }

    Modes currentMode;

    public void SetMode(Modes mode)
    {
        currentMode = mode;
    }

    public Modes GetMode()
    {
        return currentMode;
    }

    void Update() {

        //Debug.Log("current Mode = " + currentMode);
    }
}
