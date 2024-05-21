using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace TestShader
{
    
    /// <summary>
    /// Class for loading and getting textures (to avoid loading textures multiple time)
    /// </summary>
    public static class TextureManager
    {
        static Dictionary<string, Texture2D> _textures = new Dictionary<string, Texture2D>();


        /// <summary>
        /// Called before LoadContent
        /// Doesn't do anything currently
        /// </summary>
        public static void Initialization()
        {

        }

        /// <summary>
        /// Gets a texture with <paramref name="src"/> and adds it to the dictionary with <paramref name="id"/>
        /// Only if it is not loaded already
        /// </summary>
        /// <param name="id"></param>
        /// <param name="src"></param>
        public static void LoadTexture(string id, string src)
        {

            //emitter = new AudioEmitter();
            if (!_textures.ContainsKey(id))
            {
                _textures.Add(id, Game.I.Content.Load<Texture2D>(src));
            }
        }

        /// <summary>
        /// Tries getting a <see cref="Texture2D"/> with the given <paramref name="id"/>
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentException"> Tried getting a texture not yet loaded</exception>
        public static Texture2D GetTexture(string id)
        {
            try
            {
                return _textures[id];
            }
            catch (KeyNotFoundException)
            {
                throw new System.ArgumentException("Tried getting a texture not yet loaded with id: " + id);
            }
        }


    }
}
