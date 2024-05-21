
namespace TestShader
{
    // Input Manager class from the Monocle Engine from Maddy Thorson
    /// <summary>
    /// Represents a virtual button, axis or joystick whose state is determined by the state of its VirtualInputNodes<br/>
    /// </summary>
    public abstract class VirtualInput
    {
        public enum OverlapBehaviors { CancelOut, TakeOlder, TakeNewer };
        public enum ThresholdModes { LargerThan, LessThan, EqualTo };

        public VirtualInput()
        {
            InputManager._virtualInputs.Add(this);
        }

        public void Deregister()
        {
            InputManager._virtualInputs.Remove(this);
        }

        public abstract void Update();
    }

    /// <summary>
    /// Add these to your VirtualInput to define how it determines its current input state.<br/>
    /// For example, if you want to check whether a keyboard key is pressed, create a VirtualButton and add to it a VirtualButton.KeyboardKey<br/>
    /// </summary>
    public abstract class VirtualInputNode
    {
        public virtual void Update()
        {

        }
    }
}
