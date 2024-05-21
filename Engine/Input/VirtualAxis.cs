using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System;

namespace TestShader
{

    // Input Manager class from the Monocle Engine from Maddy Thorson
    /// <summary>
    /// A virtual input represented as a float between -1 and 1<br/>
    /// </summary>
    public class VirtualAxis : VirtualInput
    {
        public List<Node> Nodes;

        public float Value { get; private set; }
        public float PreviousValue { get; private set; }

        public VirtualAxis()
            : base()
        {
            Nodes = new List<Node>();
        }

        public VirtualAxis(params Node[] nodes)
            : base()
        {
            Nodes = new List<Node>(nodes);
        }

        public override void Update()
        {
            foreach (var node in Nodes)
                node.Update();

            PreviousValue = Value;
            Value = 0;
            foreach (var node in Nodes)
            {
                float value = node.Value;
                if (value != 0)
                {
                    Value = value;
                    break;
                }
            }
        }

        public static implicit operator float(VirtualAxis axis)
        {
            return axis.Value;
        }

        public abstract class Node : VirtualInputNode
        {
            public abstract float Value { get; }
        }

        public class PadLeftStickX : Node
        {
            public int GamepadIndex;
            public float Deadzone;

            public PadLeftStickX(int gamepadIndex, float deadzone)
            {
                GamepadIndex = gamepadIndex;
                Deadzone = deadzone;
            }

            public override float Value
            {
                get
                {
                    return Calc.SignThreshold(InputManager.GamePads[GamepadIndex].GetLeftStick().X, Deadzone);
                }
            }
        }

        public class PadLeftStickY : Node
        {
            public int GamepadIndex;
            public float Deadzone;

            public PadLeftStickY(int gamepadIndex, float deadzone)
            {
                GamepadIndex = gamepadIndex;
                Deadzone = deadzone;
            }

            public override float Value
            {
                get
                {
                    return Calc.SignThreshold(InputManager.GamePads[GamepadIndex].GetLeftStick().Y, Deadzone);
                }
            }
        }

        public class PadRightStickX : Node
        {
            public int GamepadIndex;
            public float Deadzone;

            public PadRightStickX(int gamepadIndex, float deadzone)
            {
                GamepadIndex = gamepadIndex;
                Deadzone = deadzone;
            }

            public override float Value
            {
                get
                {
                    return Calc.SignThreshold(InputManager.GamePads[GamepadIndex].GetRightStick().X, Deadzone);
                }
            }
        }

        public class PadRightStickY : Node
        {
            public int GamepadIndex;
            public float Deadzone;

            public PadRightStickY(int gamepadIndex, float deadzone)
            {
                GamepadIndex = gamepadIndex;
                Deadzone = deadzone;
            }

            public override float Value
            {
                get
                {
                    return Calc.SignThreshold(InputManager.GamePads[GamepadIndex].GetRightStick().Y, Deadzone);
                }
            }
        }

        public class PadDpadLeftRight : Node
        {
            public int GamepadIndex;

            public PadDpadLeftRight(int gamepadIndex)
            {
                GamepadIndex = gamepadIndex;
            }

            public override float Value
            {
                get
                {
                    if (InputManager.GamePads[GamepadIndex].DPadRightCheck)
                        return 1f;
                    else if (InputManager.GamePads[GamepadIndex].DPadLeftCheck)
                        return -1f;
                    else
                        return 0;
                }
            }
        }

        public class PadDpadUpDown : Node
        {
            public int GamepadIndex;

            public PadDpadUpDown(int gamepadIndex)
            {
                GamepadIndex = gamepadIndex;
            }

            public override float Value
            {
                get
                {
                    if (InputManager.GamePads[GamepadIndex].DPadDownCheck)
                        return 1f;
                    else if (InputManager.GamePads[GamepadIndex].DPadUpCheck)
                        return -1f;
                    else
                        return 0;
                }
            }
        }

        public class KeyboardKeys : Node
        {
            public OverlapBehaviors OverlapBehavior;
            public Keys Positive;
            public Keys Negative;

            private float value;
            private bool turned;

            public KeyboardKeys(OverlapBehaviors overlapBehavior, Keys negative, Keys positive)
            {
                OverlapBehavior = overlapBehavior;
                Negative = negative;
                Positive = positive;
            }

            public override void Update()
            {
                if (InputManager.Keyboard.Check(Positive))
                {
                    if (InputManager.Keyboard.Check(Negative))
                    {
                        switch (OverlapBehavior)
                        {
                            default:
                            case OverlapBehaviors.CancelOut:
                                value = 0;
                                break;

                            case OverlapBehaviors.TakeNewer:
                                if (!turned)
                                {
                                    value *= -1;
                                    turned = true;
                                }
                                break;

                            case OverlapBehaviors.TakeOlder:
                                //value stays the same
                                break;
                        }
                    }
                    else
                    {
                        turned = false;
                        value = 1;
                    }
                }
                else if (InputManager.Keyboard.Check(Negative))
                {
                    turned = false;
                    value = -1;
                }
                else
                {
                    turned = false;
                    value = 0;
                }
            }

            public override float Value
            {
                get { return value; }
            }
        }
    }
}
