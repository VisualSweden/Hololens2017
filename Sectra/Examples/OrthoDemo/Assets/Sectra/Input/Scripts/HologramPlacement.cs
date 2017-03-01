
using UnityEngine;
using HoloToolkit.Unity.InputModule;

public class HologramPlacement : MonoBehaviour, IInputClickHandler
{
    
    public float LerpSpeed = 0.05f;
    public float distanceFromCamera = 0.90f;
    private bool rotEnabled;
    private bool movEnabled;

    public bool GotTransform { get; private set; }

    void Start()
    {

        startPlacement();
    }

    public void startPlacement()
    {
        //Make sure we get all events while placing hologram
        InputManager.Instance.AddGlobalListener(gameObject);

        gameObject.BroadcastMessage("OnHologramPlacementActivated", null, SendMessageOptions.DontRequireReceiver);

    }
    // Update is called once per frame
    void Update()
    {
        if (!GotTransform)
        {
            //The position to aim for
            Vector3 goalPosition = Camera.main.transform.position + Camera.main.transform.forward * distanceFromCamera;

            //Lerp toward that position
            transform.position = Vector3.Lerp(transform.position, goalPosition, LerpSpeed);
        }
    }
    
    public void OnInputClicked(InputEventData eventData)
    {
        
        //Execute function OnHologramPlaced on this and all child gameobjects
        gameObject.BroadcastMessage("OnHologramPlaced", null, SendMessageOptions.DontRequireReceiver);

        // Note that we have a transform.
        GotTransform = true;

        InputManager.Instance.RemoveGlobalListener(gameObject);
    }
    
}