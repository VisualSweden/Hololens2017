
using System;
using UnityEngine;
using HoloToolkit.Unity.InputModule;

namespace Sectra.Interaction
{
    /// <summary>
    /// Event args for a manipulation event.
    /// </summary>
    public class ExtendedManipulationEventArgs : ManipulationEventArgs
    {
        /// <summary>
        /// Total distance moved since the beginning of the manipulation gesture.
        /// </summary>
        public Vector3 InitialPosition { get; private set; }

        public ExtendedManipulationEventArgs(IInputSource inputSource, uint sourceId, Vector3 cumulativeDelta, Vector3 initialPosition)
            : base(inputSource, sourceId, cumulativeDelta)
        {
            InitialPosition = initialPosition;
        }
    }

    /// <summary>
    /// Component that allows manipulation an object with your hand on HoloLens.
    /// Manipulation is done by calculating the angular delta and z-delta between the current and previous hand positions,
    /// and then repositioning the object based on that.
    /// </summary>
    public class FakeWindowsHandsInput : BaseInputSource,
                                 IInputHandler,
                                 ISourceStateHandler
    {
        public bool IsManipulationEnabled = true;
        public bool IsHoldEnabled = true;
        public int Smoothing = 8;

        private bool isLoggedAsListener = false;

        private bool isDown;
        private bool hasChanged;
        private Vector3 downPos;
        private DateTime downTime;
        private DateTime upTime;
        private bool isHolding;
        private bool isManipulating;

        private const float LerpHandPosFactor = 0.2f;
        private const float FingerPressDelay = 0.05f;
        private const float MaxClickDuration = 0.5f;
        private const float ManipulationStartThreshold = 0.005f;

        private Vector3 startHandPos;
        private Vector3 lerpedHandPos;

        private IInputSource currentInputSource = null;
        private uint currentInputSourceId;

        
        #region BaseInputSource overrides
        public override SupportedInputEvents SupportedEvents
        {
            get { return SupportedInputEvents.Manipulation | 
                        SupportedInputEvents.Hold | 
                        SupportedInputEvents.SourceClicked; }
        }

        public override SupportedInputInfo GetSupportedInputInfo(uint sourceId)
        {
            return SupportedInputInfo.None;
        }

        public override bool TryGetPosition(uint sourceId, out Vector3 position)
        {
            if (!isManipulating)
            {
                position = Vector3.zero;
                return false;
            }

            position = startHandPos;
            return true;
        }

        public override bool TryGetOrientation(uint sourceId, out Quaternion orientation)
        {
            // Orientation is not supported by hands
            orientation = Quaternion.identity;
            return false;
        }
        #endregion


        #region Start/Stop
        protected override void Start()
        {
            base.Start();
            TryAddAsGlobalListener(gameObject);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (isManipulating)
                StopManipulation();
            if (isHolding)
                StopHold();
            TryRemoveAsGlobalListener(gameObject);
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            if (isManipulating)
                StopManipulation();
            if (isHolding)
                StopHold();
            TryRemoveAsGlobalListener(gameObject);
        }
        #endregion


        private void Update()
        {
            // Do nothing if no input
            if (!HasInput())
                return;

            // Finger is down 
            if (isDown)
            {
                hasChanged = false;
                // Early exit if we have not pressed long enough
                var elapsed = (DateTime.Now - downTime).TotalSeconds;
                if (elapsed < FingerPressDelay)
                    return;

                var currPos = GetCurrentInputPosition();
                var manipDist = (currPos - downPos).magnitude;

                // Check for manipulation first
                if (!isManipulating && !isHolding)
                {
                    // Check criteria for starting manipulation 
                    if (manipDist > ManipulationStartThreshold)
                    {
                        StartManipulation(downPos);
                        return;
                    }

                    // Check criteria for starting hold
                    if (elapsed > MaxClickDuration)
                    {
                        StartHold(downPos);
                        return;
                    }
                }

                // Take care of manipulation 
                if (isManipulating)
                {
                    UpdateManipulation(currPos);
                    return;
                }
            }
            else // Finger is up
            {
                // Finger went up just now
                if (hasChanged)
                {
                    hasChanged = false;
                    // Stop any manip
                    if (isManipulating)
                    {
                        StopManipulation();
                        return;
                    }
                    // Stop any holding
                    if (isHolding)
                    {
                        StopHold();
                        return;
                    }

                    // Early exit if we have not pressed long enough
                    var elapsed = (upTime - downTime).TotalSeconds;
                    if (elapsed < FingerPressDelay)
                    {
                        Debug.Log("Click too fast..");
                        return;
                    }

                    // Only click left (currently support only single clicks)
                    RaiseSourceClickedEvent(new SourceClickEventArgs(currentInputSource, currentInputSourceId, 1));
                    Debug.Log("Click event fired");
                    return;
                }
            }
        }


        #region Manipulation Start/Update/Stop
        /// <summary>
        /// Starts manipulation of the object
        /// </summary>
        public void StartManipulation(Vector3 downPos)
        {
            if (!IsManipulationEnabled)
                return;
            if (isManipulating || isHolding)
                return;
            
            isManipulating = true;

            this.startHandPos = downPos;
            this.lerpedHandPos = this.startHandPos;

            var ea = new ManipulationEventArgs(
                currentInputSource, 
                currentInputSourceId,
                Vector3.zero);
            RaiseManipulationStartedEvent(ea);
        }

        /// <summary>
        /// Update the position of the object being manipulated
        /// </summary>
        private void UpdateManipulation(Vector3 currentHandPos)
        {
            // Smoothing
            lerpedHandPos = Vector3.Lerp(lerpedHandPos, currentHandPos, LerpHandPosFactor);

            var ea = new ManipulationEventArgs(
                currentInputSource,
                currentInputSourceId,
                lerpedHandPos - startHandPos);
            RaiseManipulationUpdatedEvent(ea);
        }

        /// <summary>
        /// Stops manipulation of the object
        /// </summary>
        public void StopManipulation(bool exitOk = true)
        {
            if (!isManipulating)
                return;
            
            isManipulating = false;
            currentInputSource = null;

            var ea = new ExtendedManipulationEventArgs(
                currentInputSource,
                currentInputSourceId,
                Vector3.zero, 
                startHandPos);
            if (exitOk)
                RaiseManipulationCompletedEvent(ea);
            else
                RaiseManipulationCanceledEvent(ea);
            //Debug.Log(exitOk ? "Manip completed event fired" : "Manip calceled event fired");
        }
        #endregion


        #region Hold Start/Stop
        /// <summary>
        /// Starts manipulation of the object
        /// </summary>
        public void StartHold(Vector3 downPos)
        {
            if (!IsHoldEnabled)
                return;
            if (isHolding || isManipulating)
                return;
            
            isHolding = true;
            startHandPos = downPos; // Store this in case someone asks

            var ea = new HoldEventArgs(
                currentInputSource,
                currentInputSourceId);
            RaiseHoldStartedEvent(ea);
        }
        
        /// <summary>
        /// Stops manipulation of the object
        /// </summary>
        public void StopHold(bool exitOk = true)
        {
            if (!isHolding)
                return;
            
            isHolding = false;
            currentInputSource = null;

            var ea = new HoldEventArgs(currentInputSource, currentInputSourceId);
            if (exitOk)
                RaiseHoldCompletedEvent(ea);
            else
                RaiseHoldCanceledEvent(ea);
            //Debug.Log(exitOk ? "Hold completed event fired" : "Hold calceled event fired");
        }
        #endregion


        #region Input down/up + Source detected/lost
        public void OnInputUp(InputEventData eventData)
        {
            if (!OurInput(eventData.SourceId))
                return;

            isDown = false;
            hasChanged = true;
            upTime = DateTime.Now;
            Debug.Log("Finger up");
        }

        public void OnInputDown(InputEventData eventData)
        {
            if (isManipulating)
            {
                // We're already handling manip input, so we can't start a new manip operation.
                return;
            }

            currentInputSource = eventData.InputSource;
            currentInputSourceId = eventData.SourceId;

            Vector3 currentHandPos;
            currentInputSource.TryGetPosition(currentInputSourceId, out currentHandPos);

            isDown = true;
            hasChanged = true;
            downPos = currentHandPos;
            downTime = DateTime.Now;
            Debug.Log("Finger down");
        }

        public void OnSourceDetected(SourceStateEventData eventData)
        {
            // Nothing to do
        }

        public void OnSourceLost(SourceStateEventData eventData)
        {
            if (!OurInput(eventData.SourceId))
                return;

            if (isManipulating)
            {
                StopManipulation(false);
            }
            if (isHolding)
            {
                StopHold(false);
            }
        }
        #endregion


        #region Helpers
        private void TryAddAsGlobalListener(GameObject go)
        {
            if (isLoggedAsListener || !IsManipulationEnabled)
                return;
            
            var instance = InputManager.Instance;
            if (instance == null)
                return;

            InputManager.Instance.AddGlobalListener(gameObject);
            isLoggedAsListener = true;
        }

        private void TryRemoveAsGlobalListener(GameObject go)
        {
            if (isLoggedAsListener)
                InputManager.Instance.RemoveGlobalListener(gameObject);
        }

        private bool HasInput()
        {
            return (this.currentInputSource != null);
        }

        private bool OurInput(uint sid)
        {
            return HasInput() && sid == currentInputSourceId;
        }

        private Vector3 GetCurrentInputPosition()
        {
            Vector3 currentHandPos;
            currentInputSource.TryGetPosition(currentInputSourceId, out currentHandPos);
            return currentHandPos;
        }
        #endregion
    }
}
