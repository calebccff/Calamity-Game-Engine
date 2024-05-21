using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestShader
{
    /// <summary>
    /// An example of a camera that focuses on the average position of the players if no focus is set<br/>
    /// Otherwise it focuses on the focused component or player<br/>
    /// </summary>
    public class DemoCamera : TargetCamera
    {
        public override Vector2 GetTarget()
        {
            Vector2 target = PlayerAverage(_scene._players);
            if (focus != null) target = focus._position.ToVector2();
            if (focusPlayerID != -1 && focusPlayerID < _scene.numPlayers) target = _scene._players[focusPlayerID]._position.ToVector2() + (_scene._players[focusPlayerID]._colliderSensor._hitboxes[0].Center.ToVector2() * _scale);
            return target;
        }
    }
}
