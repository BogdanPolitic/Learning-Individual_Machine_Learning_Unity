using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NameTheObjectField : MonoBehaviour
{
    public string objName;
    public Rect windowRect;

    void Start()
    {
        objName = "";
    }

    void OnGUI()
    {
        windowRect = GUI.Window(0, new Rect(5, 5, 250, 50), MyWindow, "");
    }

    void MyWindow(int windowId)
    {
        GUI.Label(new Rect(11, 0, 200, 20), "Name the object");
        objName = GUI.TextField(new Rect(10, 20, 200, 20), objName, 25);
    }
}
