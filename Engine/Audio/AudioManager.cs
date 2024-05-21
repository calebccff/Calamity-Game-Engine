using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using MonoGame;
using Microsoft.Xna.Framework.Audio;

namespace TestShader
{
    /// <summary>
    /// The AudioManager class is responsible for managing audio in the game.<br/>
    /// It provides methods to load and play sound effects, as well as update the audio listener.<br/>
    /// </summary>
    public static class AudioManager
    {
        /// <summary>
        /// The dictionary of loaded sound effects<br/>
        /// </summary>
        static Dictionary<string, SoundEffect> _soundEffects = new Dictionary<string, SoundEffect>();
        //float masterVolume = 1.0f;

        /// <summary>
        /// The audio listener for 3D positioning of audio sources<br/>
        /// </summary>
        static public AudioListener _listener;
        //AudioEmitter emitter;

        /// <summary>
        /// Initializes the AudioManager, creating an AudioListener instance.<br/>
        /// </summary>
        static public void Initialization()
        {
            //_soundEffect = new SoundEffect(,, AudioChannels.Stereo); 
            _listener = new AudioListener();
        }

        /// <summary>
        /// Loads a sound effect<br/>
        /// </summary>
        /// <param name="id">Identifier for the sound effect in the dictionary</param>
        /// <param name="src">Source of the sound</param>
        static public void LoadSound(string id, string src)
        {
            //emitter = new AudioEmitter();
            if (!_soundEffects.ContainsKey(id))
            {
                _soundEffects.Add(id, Game.I.Content.Load<SoundEffect>(src));
            }
        }

        /// <summary>
        /// Updates the position of the audio listener<br/>
        /// </summary>
        /// <param name="gameTime"> Snapshot of timing values </param>
        static public void Update(GameTime gameTime)
        {
            //_listener.Position = new Vector3(TestGame.game.Cameras[0]._position.X, TestGame.game.Cameras[0]._position.Y, 0);
        }

        /// <summary>
        /// Plays a sound effect<br/>
        /// </summary>
        /// <param name="id"> Identifier for the sound effect in the dictionary</param>
        /// <exception cref="System.ArgumentException"> Tried playing non-existent SoundEffect with id </exception>
        static public void Play(string id)
        {
            try
            {
                _soundEffects[id].Play();
            }
            catch (KeyNotFoundException)
            {
                throw new System.ArgumentException("Tried playing non-existent SoundEffect with id: " + id);
            }
        }

        /// <summary>
        /// Plays a sound effect<br/>
        /// </summary>
        /// <param name="id"> Identifier for the sound effect in the dictionary</param>
        /// <param name="volume"> Volume of the sound</param>
        /// <param name="pitch"> Pitch of the sound</param>
        /// <param name="pan"> Pan of the sound</param>
        /// <exception cref="System.ArgumentException"> Tried playing non-existent SoundEffect with id </exception>
        static public void Play(string id, float volume = 1.0f, float pitch = 0.0f, float pan = 0.0f)
        {
            try
            {
                _soundEffects[id].Play(volume, pitch, pan);
            }
            catch (KeyNotFoundException)
            {
                throw new System.ArgumentException("Tried playing non-existent SoundEffect with id: " + id);
            }
        }

        /// <summary>
        /// Gets the audio listener<br/>
        /// </summary>
        /// <returns> The audio listener </returns>
        static public AudioListener getListener()
        {
            return _listener;
        }

        /// <summary>
        /// Gets a sound effect<br/>
        /// </summary>
        /// <param name="id"> Identifier for the sound effect in the dictionary</param>
        /// <returns> The sound effect </returns>
        /// <exception cref="System.ArgumentException"> Tried accesing non-existent SoundEffect with id </exception>
        static public SoundEffect GetSoundEffect(string id)
        {
            try
            {
                return _soundEffects[id];
            }
            catch (KeyNotFoundException)
            {
                throw new System.ArgumentException("Tried accesing non-existent SoundEffect with id: " + id);
            }
        }

        /// <summary>
        /// Gets an audio instance for a SoundEffect<br/>
        /// </summary>
        /// <param name="id"> Identifier for the sound effect in the dictionary</param>
        /// <returns> The audio instance </returns>
        /// <exception cref="System.ArgumentException"> Tried creating instance for non-existent SoundEffect with id </exception>
        static public AudioInstance GetAudioInstance(string id)
        {
            try
            {
                return new AudioInstance(_soundEffects[id].CreateInstance());
            }
            catch (KeyNotFoundException)
            {
                throw new System.ArgumentException("Tried creating instance for non-existent SoundEffect with id: " + id);
            }
        }

    }
}
