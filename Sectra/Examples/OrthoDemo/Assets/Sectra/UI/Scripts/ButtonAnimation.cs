using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HoloToolkit.Unity.InputModule;
using System;

/// <summary>
/// The ButtonAnimation deals with changing the appearance of the Button mesh. 
/// The Script should be put on the top level part of the button (with the collider), and
/// hold a reference to the actual mesh (usually a cylinder)
/// </summary>
public class ButtonAnimation : MonoBehaviour,
                                IInputHandler,
                                IFocusable,
                                ISourceStateHandler
{

    [Tooltip("Determines if the button should be a toggle button or an action button.")]
    public bool ToggleBehaviour = false;

    [Tooltip("The mesh that should be animated. Defaults to self.")]
    public GameObject ButtonMesh;

    public Vector3 focusOffset = new Vector3(0.0f, 0.0f, 0.1f);

    public Color PressedColor = new Color(0.6f,0.6f,0.6f);

    public Color DefaultColor = new Color(1.0f, 1.0f, 1.0f);

    public Color ToggleColor = new Color(1.0f,0.0f,0.0f);

    private Material buttonMaterial;
    private Material textMaterial;

    private Color currentColor;

    private bool currentlyInModal = false;
    private bool focused = false;
    private bool pressed = false;
	// Use this for initialization
	void Start () {
		
        if (ButtonMesh == null)
        {
            ButtonMesh = gameObject;
        }

        Renderer rend = ButtonMesh.GetComponent<Renderer>();

        if (rend != null)
        {
            buttonMaterial = rend.material;
            buttonMaterial.color = DefaultColor;
        }
        
        currentColor = DefaultColor;
        
    }

    public void OnInputUp(InputEventData eventData)
    {

        pressed = false;
        if (ToggleBehaviour)
        {
            currentColor = (currentColor == DefaultColor ? ToggleColor : DefaultColor);
        }


        setCurrentColor();

        if (currentlyInModal)
        {
            InputManager.Instance.PopModalInputHandler();
            currentlyInModal = false;
        }
            

    }

    public void OnInputDown(InputEventData eventData)
    {
        pressed = true;

        setCurrentColor();
        InputManager.Instance.PushModalInputHandler(gameObject);
        currentlyInModal = true;
    }

    public void OnSourceDetected(SourceStateEventData eventData)
    {
        
    }

    public void OnSourceLost(SourceStateEventData eventData)
    {
        focused = false;
        pressed = false;
        if (currentlyInModal)
        {
            InputManager.Instance.PopModalInputHandler();
            currentlyInModal = false;
        }

        setCurrentColor();

    }

    public void OnFocusEnter()
    {
        transform.position += focusOffset;
        focused = true;

        setCurrentColor();
    }

    public void OnFocusExit()
    {
        transform.position -= focusOffset;
        focused = false;

        setCurrentColor();
    }



    private void setCurrentColor()
    {
        if (buttonMaterial)
        {
            buttonMaterial.color = (pressed ? PressedColor : currentColor) * (focused ? 1.5f : 1.0f);
        }
    }
}
