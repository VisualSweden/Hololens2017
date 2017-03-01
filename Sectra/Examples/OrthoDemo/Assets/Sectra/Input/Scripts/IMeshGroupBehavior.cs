using System;

namespace Sectra.Interfaces
{
    /// <summary>
    /// Interface for items in a MeshGroup.
    /// Required to relay input effects to other items in group.
    /// </summary>
    public interface IMeshGroupBehaviour
    {
        bool enabled { get; set; }
        void EnterForeground();
        void EnterBackground();
        void EnterNeutral();
        event EventHandler EnteringForeground;
        event EventHandler ExitingForeground;
        event EventHandler EnteringSoloInput;
        event EventHandler ExitingSoloInput;
    }
}
