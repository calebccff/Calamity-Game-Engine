using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;
using TestShader;

namespace TestShader
{
    /// <summary>
    /// CharacterComponent that manages audio for a character<br/>
    /// </summary>
    public class AudioCharacterComponent : CharacterComponent
    {
        /// <summary>
        /// Dictionary of list of sounds currently playing by name<br/>
        /// Contains a list of both the instance and the emitter for each sound<br/>
        /// null for the emitter means the sound is using the default emitter<br/>
        /// </summary>
        public Dictionary<string, List<(AudioInstance instance, AudioEmitter emitter)>> _currentlyPlaying = new Dictionary<string, List<(AudioInstance instance, AudioEmitter emitter)>>();

        /// <summary>
        /// Dictionary of sound names to sound IDs<br/>
        /// </summary>
        public Dictionary<string, string> _soundNameToID = new Dictionary<string, string>();
        /// <summary>
        /// Dictionary of sound names to sources<br/>
        /// </summary>
        public Dictionary<string, string> _soundNameToSrc = new Dictionary<string, string>();

        /// <summary>
        /// Default emitter for the component<br/>
        /// It's position is updated to the position of the component<br/>
        /// </summary>
        AudioEmitter emitter;

        /// <summary>
        /// Constructor,<br/>
        /// Creates a default emitter for the component<br/>
        /// </summary>
        /// <param name="character"></param>
        public AudioCharacterComponent(Character character) : base(character)
        {
            emitter = new AudioEmitter();
            emitter.Position = new Vector3(_position.X, _position.Y, 0);
        }

        /// <summary>
        /// Called when the corresponding component's position changes<br/>
        /// Updates the position of the default emitter<br/>
        /// </summary>
        /// <param name="Old"> Old position of the component</param>
        /// <param name="New"> New position of the component</param>
        public override void OnPositionChange(Point Old, Point New)
        {
            emitter.Position = new Vector3(New.X, New.Y, 0);
            base.OnPositionChange(Old, New);
        }

        /// <summary>
        /// LoadContent,<br/>
        /// Loads all sounds in the dictionaries (_soundNameToID and _soundNameToSrc)<br/>
        /// Initializes _currentlyPlaying for each sound<br/>
        /// </summary>
        public override void LoadContent()
        {
            foreach (string key in _soundNameToID.Keys)
            {
                AudioManager.LoadSound(_soundNameToID[key], _soundNameToSrc[key]);
                _currentlyPlaying[key] = new List<(AudioInstance instance, AudioEmitter emitter)>();
            }
        }

        //TODO: Implement Play sound with emitter passed as a param

        /// <summary>
        /// Plays a sound<br/>
        /// Creates a new sound instance from the name <paramref name="name"/> and adds it to _currentlyPlaying<br/>
        /// The default emitter is used<br/>
        /// Adds the instance to the currently playing list<br/>
        /// </summary>
        /// <param name="name"> Name of the sound</param>
        public void Play(string name)
        {
            AudioInstance newSound = AudioManager.GetAudioInstance(_soundNameToID[name]);
            newSound.IsLooped = false;
            newSound.Apply3D(character._scene._listeners.ToArray(), emitter);
            newSound.Play();
            if (_currentlyPlaying[name] == null) { _currentlyPlaying[name] = new List<(AudioInstance instance, AudioEmitter emitter)>(); }
            _currentlyPlaying[name].Add((newSound, null));
        }

        /// <summary>
        /// Plays a sound at a specific position<br/>
        /// Creates a new sound instance from the name <paramref name="name"/> and adds it to _currentlyPlaying<br/>
        /// A new emitter is created at the specified position <paramref name="at"/><br/>
        /// Adds the instance to the currently playing list<br/>
        /// </summary>
        /// <param name="name"> Name of the sound</param>
        /// <param name="at"> Position of the emitter</param>
        public void PlayAt(string name, Point at)
        {
            AudioInstance newSound = AudioManager.GetAudioInstance(_soundNameToID[name]);
            newSound.IsLooped = false;
            newSound.Apply3D(character._scene._listeners.ToArray(), emitter);
            newSound.Play();
            if (_currentlyPlaying[name] == null) { _currentlyPlaying[name] = new List<(AudioInstance instance, AudioEmitter emitter)>(); }
            _currentlyPlaying[name].Add((newSound, new AudioEmitter() { Position = new Vector3(at.X, at.Y, 0) }));
        }

        /// <summary>
        /// Update,<br/>
        /// Updates the 3D effects of all currently playing sounds<br/>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            foreach (List<(AudioInstance instance, AudioEmitter emitter)> soundType in _currentlyPlaying.Values)
            {
                foreach ((AudioInstance instance, AudioEmitter emitter) sound in soundType)
                {
                    if (emitter == null)
                        sound.instance.Apply3D(character._scene._listeners.ToArray(), emitter);
                    else
                        sound.instance.Apply3D(character._scene._listeners.ToArray(), sound.emitter);
                }
            }

            base.Update(gameTime);
        }





    }
}
