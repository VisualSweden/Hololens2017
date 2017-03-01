using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HoloToolkit.Unity.InputModule;

public class HandGazeToggle : MonoBehaviour, 
                              IInputHandler
{

    public HandDirectionTransform HandDirectionTransform;
    public GazeManager GazeManager;

    private bool useHandGaze = true;

    void Start()
    {

    }

    public void OnInputUp(InputEventData eventData)
    {
        if (HandDirectionTransform != null && GazeManager != null)
        {
            
            useHandGaze = !useHandGaze;
            GazeManager.GazeTransform = useHandGaze ? HandDirectionTransform.transform : Camera.main.transform;
            
        }
    }

    public void OnInputDown(InputEventData eventData)
    {

    }
}
