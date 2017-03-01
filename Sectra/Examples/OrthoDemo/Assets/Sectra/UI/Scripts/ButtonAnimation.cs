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
                                ISourceStateHandler
{

    [Tooltip("Determines if the button should be a toggle button or an action button.")]
    public bool ToggleBehaviour = false;

    [Tooltip("The mesh that should be animated. Defaults to self.")]
    public GameObject ButtonMesh;

    [Tooltip("The text mesh that should be animated. Defaults to self.")]
    public GameObject TextMesh;

    public Color PressedColor = new Color(0.6f,0.6f,0.6f);

    public Color DefaultColor = new Color(1.0f, 1.0f, 1.0f);

    public Color ToggleColor = new Color(1.0f,0.0f,0.0f);

    private Material buttonMaterial;
    private Material textMaterial;

    private Color currentColor;

    private bool currentlyInModal = false;

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

        if (TextMesh != null)
        {
            Renderer textRenderer = TextMesh.GetComponent<Renderer>();

            if (textRenderer != null)
            {
                textMaterial = textRenderer.material;
                textMaterial.color = DefaultColor;
            }
        }
        
        currentColor = DefaultColor;
        
    }

    public void OnInputUp(InputEventData eventData)
    {
        if (ToggleBehaviour)
        {
            currentColor = (currentColor == DefaultColor ? ToggleColor : DefaultColor);
        }

        if (buttonMaterial)
        {
            buttonMaterial.color = currentColor;
        }

        if (textMaterial)
        {
            textMaterial.color = currentColor;
        }

        if (currentlyInModal)
        {
            InputManager.Instance.PopModalInputHandler();
            currentlyInModal = false;
        }
            

    }

    public void OnInputDown(InputEventData eventData)
    {
        if (buttonMaterial)
        {
            buttonMaterial.color = PressedColor;
        }

        if (textMaterial)
        {
            textMaterial.color = PressedColor;
        }

        InputManager.Instance.PushModalInputHandler(gameObject);
        currentlyInModal = true;
    }

    public void OnSourceDetected(SourceStateEventData eventData)
    {
        
    }

    public void OnSourceLost(SourceStateEventData eventData)
    {

        if (currentlyInModal)
        {
            InputManager.Instance.PopModalInputHandler();
            currentlyInModal = false;
        }

        if (buttonMaterial)
        {
            buttonMaterial.color = currentColor;
        }

        if (textMaterial)
        {
            textMaterial.color = currentColor;
        }

    }
}
