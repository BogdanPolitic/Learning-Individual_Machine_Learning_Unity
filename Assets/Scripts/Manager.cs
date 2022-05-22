using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Manager : MonoBehaviour
{
    public static Manager Instance;

    // Sub-managers:
    [SerializeField] public MySoundManager MySoundManager;

    void Awake()
    {
        Instance = this;
        MySoundManager.Initialize();
    }
}
