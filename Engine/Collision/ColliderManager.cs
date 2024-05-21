using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace TestShader
{
    /// <summary>
    /// Static class for managing colliders<br/>
    /// </summary>
    static public class ColliderManager
    {

        /// <summary>
        /// Calculates all collisions<br/>
        /// Later might be optimised<br/>
        /// </summary>
        /// <param name="gameTime"> Game time</param>
        static public void UpdateCollision(GameTime gameTime)
        {
            //UpdateHitbox(gameTime);
            foreach (var element in Game.I._colliderSensors._list)
            {
                (element as ColliderSensor).Collide();
            }
        }

        /// <summary>
        /// Draws all hitboxes for debugging<br/>
        /// </summary>
        /// <param name="gameTime"></param>
        static public void DrawHitboxes(GameTime gameTime)
        {
            foreach (Collider element in Game.I._colliders._list)
            {
                Game.I._spriteRenderer.HollowRect(element, Color.Red,0f);
            }

        }

        
        /*
        // Function to calculate all hitboxes
        // Might reintroduce later
        public void  UpdateHitbox(GameTime gameTime) { 
             foreach (var element in _colliders._list)
             {
                 (element as Collider).UpdateHitboxes();
             }
         }

         */
    }
}
