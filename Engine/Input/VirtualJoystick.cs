using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;

namespace TestShader
{
    // Input Manager class from the Monocle Engine from Maddy Thorson
    /// <summary>
    /// A virtual input that is represented as a Vector2, with both X and Y as values between -1 and 1<br/>
    /// </summary>
    public class VirtualJoystick : VirtualInput
    {
        public List<Node> Nodes;
        public bool Normalized;
        public float? SnapSlices;

        public Vector2 Value { get; private set; }
        public Vector2 PreviousValue { get; private set; }

        public VirtualJoystick(bool normalized)
            : base()
        {
            Nodes = new List<Node>();
            Normalized = normalized;
        }

        public VirtualJoystick(bool normalized, params Node[] nodes)
            : base()
        {
            Nodes = new List<Node>(nodes);
            Normalized = normalized;
        }

        public override void Update()
        {
            foreach (var node in Nodes)
                node.Update();

            PreviousValue = Value;
            Value = Vector2.Zero;
            foreach (var node in Nodes)
            {
                Vector2 value = node.Value;
                if (value != Vector2.Zero)
                {
                    if (Normalized)
                    {
                        if (SnapSlices.HasValue)
                            value = value.SnappedNormal(SnapSlices.Value);
                        else
                            value.Normalize();
                    }
                    else if (SnapSlices.HasValue)
                        value = value.Snapped(SnapSlices.Value);

                    Value = value;
                    break;
                }
            }
        }

        public static implicit operator Vector2(VirtualJoystick joystick)
        {
            return joystick.Value;
        }

        public abstract class Node : VirtualInputNode
        {
            public abstract Vector2 Value { get; }
        }

        public class PadLeftStick : Node
        {
            public int GamepadIndex;
            public float Deadzone;

            public PadLeftStick(int gamepadIndex, float deadzone)
            {
                GamepadIndex = gamepadIndex;
                Deadzone = deadzone;
            }

            public override Vector2 Value
            {
                get
                {
                    return InputManager.GamePads[GamepadIndex].GetLeftStick(Deadzone);
                }
            }
        }

        public class PadRightStick : Node
        {
            public int GamepadIndex;
            public float Deadzone;

            public PadRightStick(int gamepadIndex, float deadzone)
            {
                GamepadIndex = gamepadIndex;
                Deadzone = deadzone;
            }

            public override Vector2 Value
            {
                get
                {
                    return InputManager.GamePads[GamepadIndex].GetRightStick(Deadzone);
                }
            }
        }

        public class PadDpad : Node
        {
            public int GamepadIndex;

            public PadDpad(int gamepadIndex)
            {
                GamepadIndex = gamepadIndex;
            }

            public override Vector2 Value
            {
                get
                {
                    Vector2 value = Vector2.Zero;

                    if (InputManager.GamePads[GamepadIndex].DPadRightCheck)
                        value.X = 1f;
                    else if (InputManager.GamePads[GamepadIndex].DPadLeftCheck)
                        value.X = -1f;

                    if (InputManager.GamePads[GamepadIndex].DPadDownCheck)
                        value.Y = 1f;
                    else if (InputManager.GamePads[GamepadIndex].DPadUpCheck)
                        value.Y = -1f;

                    return value;
                }
            }
        }

        public class KeyboardKeys : Node
        {
            public OverlapBehaviors OverlapBehavior;
            public Keys Left;
            public Keys Right;
            public Keys Up;
            public Keys Down;

            private bool turnedX;
            private bool turnedY;
            private Vector2 value;

            public KeyboardKeys(OverlapBehaviors overlapBehavior, Keys left, Keys right, Keys up, Keys down)
            {
                OverlapBehavior = overlapBehavior;
                Left = left;
                Right = right;
                Up = up;
                Down = down;
            }

            public override void Update()
            {
                //X Axis
                if (InputManager.Keyboard.Check(Left))
                {
                    if (InputManager.Keyboard.Check(Right))
                    {
                        switch (OverlapBehavior)
                        {
                            default:
                            case OverlapBehaviors.CancelOut:
                                value.X = 0;
                                break;

                            case OverlapBehaviors.TakeNewer:
                                if (!turnedX)
                                {
                                    value.X *= -1;
                                    turnedX = true;
                                }
                                break;

                            case OverlapBehaviors.TakeOlder:
                                //X stays the same
                                break;
                        }
                    }
                    else
                    {
                        turnedX = false;
                        value.X = -1;
                    }
                }
                else if (InputManager.Keyboard.Check(Right))
                {
                    turnedX = false;
                    value.X = 1;
                }
                else
                {
                    turnedX = false;
                    value.X = 0;
                }

                //Y Axis
                if (InputManager.Keyboard.Check(Up))
                {
                    if (InputManager.Keyboard.Check(Down))
                    {
                        switch (OverlapBehavior)
                        {
                            default:
                            case OverlapBehaviors.CancelOut:
                                value.Y = 0;
                                break;

                            case OverlapBehaviors.TakeNewer:
                                if (!turnedY)
                                {
                                    value.Y *= -1;
                                    turnedY = true;
                                }
                                break;

                            case OverlapBehaviors.TakeOlder:
                                //Y stays the same
                                break;
                        }
                    }
                    else
                    {
                        turnedY = false;
                        value.Y = -1;
                    }
                }
                else if (InputManager.Keyboard.Check(Down))
                {
                    turnedY = false;
                    value.Y = 1;
                }
                else
                {
                    turnedY = false;
                    value.Y = 0;
                }
            }

            public override Vector2 Value
            {
                get { return value; }
            }
        }
    }


}
