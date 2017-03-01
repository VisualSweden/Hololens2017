using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HoloToolkit.Unity.InputModule;
using System;

public class ResetTibia : MonoBehaviour,   
                          IInputClickHandler{

    public GameObject objectToReset;

    public void OnInputClicked(InputEventData eventData)
    {
        if (objectToReset != null)
        {
            foreach (Transform child in objectToReset.transform)
            {
                child.localPosition = Vector3.zero;
                child.localRotation = Quaternion.identity;
            }
            objectToReset.transform.localPosition = Vector3.zero;
            objectToReset.transform.localRotation = Quaternion.identity;
        }
    }
}
