using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HoloToolkit.Unity.InputModule;
using Sectra.Interaction;

public class ToggleRotation : MonoBehaviour,
                              IInputHandler
{
    private bool isRotation = false;

    public List<HandRotatable> ObjectsWithRotation;

    public List<MeshHandInteraction> ObjectsWithDragging;

    public void OnInputUp(InputEventData eventData)
    {
        isRotation = !isRotation;

        foreach (HandRotatable rotatable in ObjectsWithRotation) {
            rotatable.enabled = isRotation;
        }

        foreach (MeshHandInteraction draggable in ObjectsWithDragging) {

            draggable.isDraggingEnabled = !isRotation;
        }
    }

    public void OnInputDown(InputEventData eventData)
    {

    }
}
