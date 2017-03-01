using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HoloToolkit.Unity.InputModule;
using System;

public class LeftHandToggleButton : MonoBehaviour,
                                     IInputHandler
{
    
    public HandDirectionTransform handDirectionTransform;
    
    public void OnInputUp(InputEventData eventData)
    {
        if (handDirectionTransform != null)
        {
            handDirectionTransform.IsRightHanded = !handDirectionTransform.IsRightHanded;
        }
    }

    public void OnInputDown(InputEventData eventData)
    {

    }
    
}
