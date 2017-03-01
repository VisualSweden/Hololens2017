
using System;
using UnityEngine;
using HoloToolkit.Unity.InputModule;

namespace Sectra.Interaction
{
    public class MeshHandInteraction : MonoBehaviour,
                                        Sectra.Interfaces.IMeshGroupBehaviour,
                                        IFocusable, 
                                        IHoldHandler,
                                        IManipulationHandler
    {
        [Tooltip("Transform that will be dragged. Defaults to the object of the component.")]
        public Transform HostTransform;

        public bool isDraggingEnabled = true;

        public event EventHandler EnteringForeground;
        public event EventHandler ExitingForeground;
        public event EventHandler EnteringSoloInput;
        public event EventHandler ExitingSoloInput;

        private Renderer render;
        public Shader shaderBg;
        public Shader shaderFg;
        private Color neutralColor;
        private Color focusColor;

        private Vector3 startPosObj;
        private Vector3 startPosHand;
        private bool isRegistered = false;
        
        private bool hologramPlaced = true;

        private void Start()
        {
            if (HostTransform == null)
                HostTransform = transform;
            // Store quicklinks
            render = GetComponent<Renderer>();

            if (shaderBg == null)
            {
                shaderBg = Shader.Find("SimpleAlpha");
            }
            if (shaderFg == null)
            {
                shaderFg = Shader.Find("SimpleLao");
            }

            neutralColor = render.material.color;
            focusColor = Color.Lerp(neutralColor, Color.white, 0.25f);
        }
        

        #region On Focus
        public void OnFocusEnter()
        {
            if (hologramPlaced)
                SetColor(focusColor);
        }

        public void OnFocusExit()
        {
            SetColor(neutralColor);
        }
        #endregion
        

        #region On Hold
        public void OnHoldStarted(HoldEventData eventData)
        {

            InputManager.Instance.PushModalInputHandler(gameObject);
            EnterForeground();
            EnteringForeground.Fire(this.gameObject);
        }

        public void OnHoldCompleted(HoldEventData eventData)
        {
            EnterNeutral();
            ExitingForeground.Fire(this.gameObject);
            InputManager.Instance.PopModalInputHandler();
        }

        public void OnHoldCanceled(HoldEventData eventData)
        {
            EnterNeutral();
            ExitingForeground.Fire(this.gameObject);
            InputManager.Instance.PopModalInputHandler();
        }
        #endregion


        #region On Manipulation
        public void OnManipulationStarted(ManipulationEventData eventData)
        {
            if (isDraggingEnabled && hologramPlaced)
            {
                Debug.Log("Manipulation Started");
                if (isRegistered)
                    return; // Should not happen
                InputManager.Instance.PushModalInputHandler(gameObject);
                isRegistered = true;
                startPosObj = HostTransform.position;
                EnteringSoloInput.Fire(this.gameObject);
            }
        }

        public void OnManipulationUpdated(ManipulationEventData eventData)
        {
            if (isRegistered)
            {
                var delta = eventData.CumulativeDelta; // Incremental delta, not cummulative!
                HostTransform.position = startPosObj + delta;
            }
            
        }

        public void OnManipulationCompleted(ManipulationEventData eventData)
         {
            if (isRegistered)
            { 
                InputManager.Instance.PopModalInputHandler();
                isRegistered = false;
                ExitingSoloInput.Fire(this.gameObject);
            }
            
        }

        public void OnManipulationCanceled(ManipulationEventData eventData)
        {
            if (isRegistered) { 
                InputManager.Instance.PopModalInputHandler();
                isRegistered = false;
                ExitingSoloInput.Fire(this.gameObject);
            }
        }
        #endregion


        #region Foreground / Background
        public void EnterForeground()
        {
            SetShader(shaderFg);
        }

        public void EnterBackground()
        {
            SetShader(shaderBg);
        }

        public void EnterNeutral()
        {
            SetShader(shaderFg);
        }
        #endregion

        #region Hologram Placement

        private void OnHologramPlacementActivated()
        {
            hologramPlaced = false;
        }


        private void OnHologramPlaced()
        {
            hologramPlaced = true;
        }

        #endregion

        #region Helpers
        private void SetShader(Shader shader)
        {
            if (render != null && render.material != null)
                render.material.shader = shader;
        }

        private void SetColor(Color color)
        {
            if (render != null && render.material != null)
                render.material.color = color;
        }
        #endregion
    }
}
