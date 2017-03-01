using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HoloToolkit.Unity.InputModule;
using System;

/// <summary>
/// HandDirectionTransform modifies the object's transform so that it has origin 
/// at the eyes position and direction towards the hand tracked. If no hand 
/// can be tracked it will default to straight forward. 
/// Currently only tracks the first hand to be detected.
/// </summary>
public class HandDirectionTransform : MonoBehaviour,
                                      ISourceStateHandler
{

    private IInputSource currentInputSource = null;
    private uint currentInputSourceId;

    public bool IsRightHanded = true;

    [Tooltip("Pointer offset form hand position (cm).")]
    public Vector2 PointerOffsetFromHand = new Vector2(-8, 8);

    private Camera mainCamera;
    

    void Start () {
        mainCamera = Camera.main;

        InputManager.Instance.AddGlobalListener(gameObject);

	}
	
	// Update is called once per frame
	void Update () {
		
            UpdateTransform();
	}


    private void UpdateTransform()
    {
        //set position to Camera
        transform.position = mainCamera.transform.position;
        transform.forward = mainCamera.transform.forward;

        if (currentInputSource != null)
        {
            Vector3 handPosition;
            
            if (currentInputSource.TryGetPosition(currentInputSourceId, out handPosition)) 
            {
                var camUp = mainCamera.transform.up;
                var camRight = Vector3.Normalize(Vector3.Cross(mainCamera.transform.up, mainCamera.transform.forward));
                var LR = IsRightHanded ? 1 : -1;
            
                handPosition = handPosition
                     + 0.01f * PointerOffsetFromHand.x * camRight * LR
                     + 0.01f * PointerOffsetFromHand.y * camUp;

                transform.forward = Vector3.Normalize(handPosition - transform.position);
            }
        }
        
    }

    public void OnSourceDetected(SourceStateEventData eventData)
    {
        currentInputSource = eventData.InputSource;
        currentInputSourceId = eventData.SourceId;
        
    }


    public void OnSourceLost(SourceStateEventData eventData)
    {
        if (eventData.SourceId == currentInputSourceId)
        {
            currentInputSource = null;
            currentInputSourceId = 0;
        }
    }

}
