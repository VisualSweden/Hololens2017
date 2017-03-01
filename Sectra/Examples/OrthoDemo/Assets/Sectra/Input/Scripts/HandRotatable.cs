
using UnityEngine;
using System;
using HoloToolkit.Unity.InputModule;
using HoloToolkit.Unity;

/// <summary>
/// Component that allows rotating an object with your hand on HoloLens.
/// Rotating is done by rotating based on your starting and current position of your hand 
/// in relation to the object rotated. 
/// </summary>
public class HandRotatable : MonoBehaviour,
                                 IFocusable,
                                 IInputHandler,
                                 ISourceStateHandler
{
    /// <summary>
    /// Event triggered when dragging starts.
    /// </summary>
    public event Action StartedRotating;

    /// <summary>
    /// Event triggered when dragging stops.
    /// </summary>
    public event Action StoppedRotating;

    [Tooltip("Transform that will be dragged. Defaults to the object of the component.")]
    public Transform HostTransform;

    [Tooltip("Scale by which rotation will be scaled.")]
    public float RotationScale = 5f;

    public bool IsRotatingEnabled = true;

    private bool isRotating;
    private bool isGazed;

    private Quaternion startRotation;
    private Vector3 startHandPosition;
    private Vector3 startDir;

    private IInputSource currentInputSource = null;
    private uint currentInputSourceId;


    private void Start()
    {
        if (GetComponent<HandDraggable>() && GetComponent<HandDraggable>().isActiveAndEnabled)
        {
            Debug.LogWarning("Both HandDraggable and HandRotatable on same object can give strange behaviour");
        }
        if (HostTransform == null)
        {
            HostTransform = transform;
        }
        
        
    }

    private void OnDestroy()
    {
        if (isRotating)
        {
            StopRotating();
        }

        if (isGazed)
        {
            OnFocusExit();
        }
    }

    private void Update()
    {
        if (IsRotatingEnabled && isRotating)
        {
            UpdateRotate();
        }
    }

    /// <summary>
    /// Starts dragging the object.
    /// </summary>
    public void StartRotating()
    {
        if (!IsRotatingEnabled)
        {
            return;
        }

        if (isRotating)
        {
            return;
        }

        // Add self as a modal input handler, to get all inputs during the manipulation
        InputManager.Instance.PushModalInputHandler(gameObject);

        isRotating = true;

        Vector3 handPosition;
        currentInputSource.TryGetPosition(currentInputSourceId, out handPosition);

        // Store the initial offset between the hand and the object, so that we can consider it when dragging
        startRotation = HostTransform.rotation;
        startHandPosition = handPosition;
        startDir = Vector3.Normalize(HostTransform.position - handPosition);
        StartedRotating.RaiseEvent();
    }


    /// <summary>
    /// Enables or disables dragging.
    /// </summary>
    /// <param name="isEnabled">Indicates whether dragging shoudl be enabled or disabled.</param>
    public void SetRotating(bool isEnabled)
    {
        if (IsRotatingEnabled == isEnabled)
        {
            return;
        }

        IsRotatingEnabled = isEnabled;

        if (isRotating)
        {
            StopRotating();
        }
    }

    /// <summary>
    /// Update the rotation of the object being dragged.
    /// </summary>
    private void UpdateRotate()
    {
        //Get the current hand position
        Vector3 newHandPosition;
        if (currentInputSource.TryGetPosition(currentInputSourceId, out newHandPosition))
        {

            Vector3 deltaPos = startHandPosition - newHandPosition;

            Vector3 currentDir = Vector3.Normalize(startDir + deltaPos * RotationScale);

            HostTransform.rotation = Quaternion.FromToRotation(startDir, currentDir) * startRotation;
            
        }

    }

    /// <summary>
    /// Stops rotating the object.
    /// </summary>
    public void StopRotating()
    {
        if (!isRotating)
        {
            return;
        }

        // Remove self as a modal input handler
        InputManager.Instance.PopModalInputHandler();

        isRotating = false;
        currentInputSource = null;
        StoppedRotating.RaiseEvent();
    }

    public void OnFocusEnter()
    {
        if (!IsRotatingEnabled)
        {
            return;
        }

        if (isGazed)
        {
            return;
        }

        isGazed = true;
    }

    public void OnFocusExit()
    {
        if (!IsRotatingEnabled)
        {
            return;
        }

        if (!isGazed)
        {
            return;
        }

        isGazed = false;
    }

    
    public void OnInputUp(InputEventData eventData)
    {
        if (currentInputSource != null &&
                eventData.SourceId == currentInputSourceId)
        {
            StopRotating();
        }
    }

    public void OnInputDown(InputEventData eventData)
    {
        if (isRotating)
        {
            // We're already handling drag input, so we can't start a new rotate operation.
            return;
        }

        if (!eventData.InputSource.SupportsInputInfo(eventData.SourceId, SupportedInputInfo.Position))
        {
            // The input source must provide positional data for this script to be usable
            return;
        }

        currentInputSource = eventData.InputSource;
        currentInputSourceId = eventData.SourceId;
        StartRotating();
    }

    public void OnSourceDetected(SourceStateEventData eventData)
    {
        // Nothing to do
    }

    public void OnSourceLost(SourceStateEventData eventData)
    {
        if (currentInputSource != null && eventData.SourceId == currentInputSourceId)
        {
            StopRotating();
        }
    }
}

