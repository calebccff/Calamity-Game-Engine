using Microsoft.Xna.Framework.Audio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestShader
{
    /// <summary>
    /// Represents an audio instance that wraps a <see cref="SoundEffectInstance"/>.<br/>
    /// Handles playing and stopping the associated audio track.
    /// </summary>
    public class AudioInstance
    {

        /// <summary>
        /// The instance of the sound effect.<br/>
        /// </summary>
        private SoundEffectInstance _instance;

        /// <summary>
        /// Gets or sets a value indicating whether to apply 3D positional audio.<br/>
        /// </summary>
        /// <remarks>
        /// If set to false, the sound will be played without any positional audio.
        /// </remarks>
        private bool _apply3d = true;

        /// <summary>
        /// Initializes a new instance of the AudioInstance class.<br/>
        /// </summary>
        /// <param name="instance">The SoundEffectInstance to wrap.</param>
        public AudioInstance(SoundEffectInstance instance)
        {
            _instance = instance;
        }

        /// <summary>
        /// Plays the audio.<br/>
        /// </summary>
        public void Play()
        {
            _instance.Play();
        }

        /// <summary>
        /// Stops the audio.<br/>
        /// </summary>
        public void Stop()
        {
            _instance.Stop();
        }

        /// <summary>
        /// Gets or sets whether the audio is looped.<br/>
        /// </summary>
        public virtual bool IsLooped
        {
            get { return _instance.IsLooped; }
            set { _instance.IsLooped = value; }
        }

        /// <summary>
        /// Gets or sets the pan of the audio.<br/>
        /// </summary>
        public float Pan
        {
            get { return _instance.Pan; }
            set { _instance.Pan = value; }
        }

        /// <summary>
        /// Gets or sets the pitch of the audio.<br/>
        /// </summary>
        public float Pitch
        {
            get { return _instance.Pitch; }
            set { _instance.Pitch = value; }
        }

        /// <summary>
        /// Gets or sets the volume of the audio.<br/>
        /// </summary>
        public float Volume
        {
            get { return _instance.Volume; }
            set { _instance.Volume = value; }
        }

        /// <summary>
        /// Gets the state of the audio.<br/>
        /// </summary>
        public virtual SoundState State
        {
            get { return _instance.State; }
        }

        /// <summary>
        /// Gets whether the audio is disposed.<br/>
        /// </summary>
        public bool IsDisposed
        {
            get { return _instance.IsDisposed; }
        }

        /// <summary>
        /// Applies 3D positioning to the audio using the specified listener and emitter.<br/>
        /// </summary>
        /// <param name="listener">The AudioListener to use.</param>
        /// <param name="emitter">The AudioEmitter to use.</param>
        public void Apply3D(AudioListener listener, AudioEmitter emitter)
        {
            if (listener._active)
                _instance.Apply3D(listener._listener, emitter);
        }

        /// <summary>
        /// Applies 3D positioning to the audio using the specified listeners and emitter.<br/>
        /// </summary>
        /// <param name="listeners">The AudioListeners to use.</param>
        /// <param name="emitter">The AudioEmitter to use.</param>
        public void Apply3D(AudioListener[] listeners, AudioEmitter emitter)
        {
            _instance.Apply3D(listeners.Where((listener) => (listener._active)).Select((listener, index) => (listener._listener)).ToArray(), emitter);
        }

        /// <summary>
        /// Disposes the audio instance.<br/>
        /// </summary>
        public void Dispose()
        {
            _instance.Dispose();
        }

        /// <summary>
        /// Pauses the audio.<br/>
        /// </summary>
        public virtual void Pause()
        {
            _instance.Pause();
        }

        /// <summary>
        /// Resumes the audio.<br/>
        /// </summary>
        public virtual void Resume()
        {
            _instance.Resume();
        }

        /// <summary>
        /// Stops the audio.<br/>
        /// </summary>
        /// <param name="immediate">Whether to stop immediately.</param>
        public virtual void Stop(bool immediate)
        {
            _instance.Stop(immediate);
        }

    }
}
