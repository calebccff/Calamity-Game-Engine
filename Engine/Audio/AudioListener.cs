using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;


namespace TestShader
{
    /// <summary>
    /// The AudioListener class represents an audio listener in the game.<br/>
    /// An audio listener is responsible for applying 3D positioning to audio sources.<br/>
    /// </summary>
    public class AudioListener : Component
    {
        /// <summary>
        /// The instance of the XNA AudioListener.<br/>
        /// </summary>
        public Microsoft.Xna.Framework.Audio.AudioListener _listener = new Microsoft.Xna.Framework.Audio.AudioListener();

        /// <summary>
        /// Indicates whether this audio listener is active or not.<br/>
        /// </summary>
        public bool _active;

        /// <summary>
        /// The default distance at which the listener is positioned from the audio source.<br/>
        /// </summary>
        public static float ListenerDistance = 10;

        /// <summary>
        /// Initializes a new instance of the AudioListener class.<br/>
        /// </summary>
        public AudioListener() : base()
        {
            // No additional logic needed here
        }

        /// <summary>
        /// Updates the position of the audio listener based on the position of the game object.<br/>
        /// </summary>
        /// <param name="gameTime">Snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {
            // Set the XNA AudioListener's position to match the game object's position
            _listener.Position = new Vector3(_position.X, _position.Y, ListenerDistance);
            base.Update(gameTime);
        }
    }
}
