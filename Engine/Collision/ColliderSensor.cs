using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace TestShader
{
    /// <summary>
    /// Collider that is used to detect collisions<br/>
    /// </summary>
    public class ColliderSensor : Collider//NO UPDATE
    {
        /// <summary>
        /// List of colliders that are colliding with the sensor<br/>
        /// </summary>
        public List<Collider> _colliding = new List<Collider>();

        /// <summary>
        /// Bool if the sensor is colliding<br/>
        /// </summary>
        public bool _isColliding = false;

        /// <summary>
        /// Delegate for checking if two colliders should collide<br/>
        /// </summary>
        public delegate bool ShouldCollideCheck(ColliderSensor self, Collider other);
        public ShouldCollideCheck _sCC = null;

        /// <summary>
        /// Struct for the data about a manual collision check<br/>
        /// </summary>
        public struct CollideData
        {
            /// <summary>
            /// Bool if the sensor is colliding<br/>
            /// </summary>
            public bool _isColliding;

            /// <summary>
            /// List of colliders that are colliding with the sensor<br/>
            /// </summary>
            public List<Collider> _colliding;

            /// <summary>
            /// Constructor<br/>
            /// </summary>
            /// <param name="isColliding"> Bool if the sensor is colliding</param>
            /// <param name="colliding"> List of colliders that are colliding with the sensor</param>
            public CollideData(bool isColliding, List<Collider> colliding)
            {
                _isColliding = isColliding;
                _colliding = new List<Collider>(colliding);
            }
        }

        /// <summary>
        /// Constructor<br/>
        /// </summary>
        /// <param name="comp"> Object that this collider is attached to</param>
        /// <param name="sCC"> Delegate for checking if two colliders should collide</param>
        public ColliderSensor(object comp, ShouldCollideCheck sCC = null) : base(comp)
        {
            Game.I._colliderSensors.Add(this);
            _sCC = sCC;
        }



        /// <summary>
        /// Checks if the sensor is colliding at its current position<br/>
        /// </summary>
        public void Collide()
        {
            // Reset variables
            _isColliding = false;
            _colliding.Clear();

            // Iterate through all colliders
            foreach (Collider collider in Game.I._colliders._list)
            {
                // Check if the collider is active and it is a different collider
                if (collider._id == _id) continue;
                if (collider._active == false) continue;

                // Check if it should collide
                if (_sCC != null && _sCC(this, collider) == false) continue;
                
                // Check if there are any intersections between the hitboxes
                bool intersect = false;
                foreach (Rectangle rect in collider._hitboxes)
                {
                    Rectangle newRect1 = rect;
                    newRect1.Offset(collider._position);
                    foreach (Rectangle rect2 in _hitboxes)
                    {
                        Rectangle newRect2 = rect2;
                        newRect2.Offset(_position);

                        if (newRect1.Intersects(newRect2))
                        {
                            intersect = true;
                            break;
                        }
                    }
                    // If there is an intersection, the two sensors are colliding
                    if (intersect) break;
                }
                if (intersect)
                {
                    // Add the collider to the list
                    _isColliding = true;
                    _colliding.Add(collider);
                }
            }
        }

        /// <summary>
        /// Checks if the sensor is colliding at a specific position with a specific delegate filter<br/>
        /// </summary>
        /// <param name="position"> Position to check</param>
        /// <param name="sCC"> Delegate for checking if two colliders should collide</param>
        /// <returns> Struct with bool if the sensor is colliding and list of colliding colliders</returns>
        public CollideData CollideAt(Point position, ShouldCollideCheck sCC = null)
        {
            //Create new variables to store the collision data
            bool newIsColliding = false;
            List<Collider> newColliding = new List<Collider>();

            // Iterate through all colliders
            foreach (Collider collider in Game.I._colliders._list)
            {
                // Check if the collider is active and it is a different collider
                if (collider._id == _id || collider._active == false) continue;
                if (sCC != null && sCC(this, collider) == false) continue;

                // Check if there are any intersections between the hitboxes
                bool intersect = false;
                foreach (Rectangle rect2 in _hitboxes)
                {

                    Rectangle newRect2 = rect2;
                    newRect2.Offset(position);


                    foreach (Rectangle rect in collider._hitboxes)
                    {
                        Rectangle newRect1 = rect;
                        newRect1.Offset(collider._position);
                        if (newRect1.Intersects(newRect2))
                        {
                            intersect = true;
                            break;
                        }
                    }
                    // If there is an intersection, the two sensors are colliding
                    if (intersect) break;
                }
                if (intersect)
                {
                    // Add the collider to the list
                    newIsColliding = true;
                    newColliding.Add(collider);
                }
            }
            // Return the collision data
            return new CollideData(newIsColliding, newColliding);
        }

        //TODO: Redundant

        /// <summary>
        /// Checks if the sensor is colliding at a specific position with a specific delegate filter<br/>
        /// </summary>
        /// <param name="position"> Position to check</param>
        /// <param name="sCC"> Delegate for checking if two colliders should collide</param>
        /// <returns> Struct with bool if the sensor is colliding and list of colliding colliders</returns>
        public CollideData CollideAt(Vector2 position, ShouldCollideCheck sCC = null)
        {
            //Create new variables to store the collision data
            bool newIsColliding = false;
            List<Collider> newColliding = new List<Collider>();
            
            // Iterate through all colliders
            foreach (Collider collider in Game.I._colliders._list)
            {
                // Check if the collider is active and it is a different collider
                if (collider._id == _id || collider._active == false) continue;
                if (sCC != null && sCC(this, collider) == false) continue;

                // Check if there are any intersections between the hitboxes
                bool intersect = false;
                foreach (Rectangle rect2 in _hitboxes)
                {

                    Rectangle newRect2 = rect2;
                    newRect2.Offset(position);


                    foreach (Rectangle rect in collider._hitboxes)
                    {
                        Rectangle newRect1 = rect;
                        newRect1.Offset(collider._position);
                        //TODO: Save time on collision somewhere
                        if (newRect1.Intersects(newRect2))
                        {
                            intersect = true;
                            break;
                        }
                    }
                    // If there is an intersection, the two sensors are colliding
                    if (intersect) break;
                }
                if (intersect)
                {
                    // Add the collider to the list
                    newIsColliding = true;
                    newColliding.Add(collider);
                }
            }

            // Return the collision data
            return new CollideData(newIsColliding, newColliding);
        }

        //TODO: Implement this function for isRiding

        /// <summary>
        /// Find the colliders that do not collide with the sensor but would collide if the sensor was at <paramref name="position"/><br/>
        ///Uses the delegate filter <paramref name="sCC"/><br/>
        /// </summary>
        /// <example> One could use this to find if a player is stalling on a platform</example>
        /// <param name="position"> Position to check</param>
        /// <param name="sCC"> Delegate for checking if two colliders should collide</param>
        /// <returns> Struct with bool if there are any such colliders and list of such colliders</returns>
        public CollideData CollideOutsideAt(Point position, ShouldCollideCheck sCC = null)
        {
            // Find the colliders that collide with the sensor at its current position
            CollideData original = CollideAt(_position, sCC);

            // Find the colliders that collide with the sensor at the specified position
            CollideData atPoint = CollideAt(position, sCC);

            // Calculate the colliders that do not collide with the sensor but would collide if the sensor was at the specified position
            List<Collider> outsideColliders = new List<Collider>(atPoint._colliding);
            for (int i = outsideColliders.Count - 1; i >= 0; i--)
            {
                foreach (Collider element in original._colliding)
                {
                    if (element._id == outsideColliders[i]._id)
                    {   
                        outsideColliders.RemoveAt(i);
                        break;
                    }
                }
            }

            // Return the collision data
            return new CollideData(outsideColliders.Count != 0, outsideColliders);
        }

        //TODO: Redundant 

        /// <summary>
        /// Find the colliders that do not collide with the sensor but would collide if the sensor was at <paramref name="position"/><br/>
        /// Uses the delegate filter <paramref name="sCC"/><br/>
        /// </summary>
        /// <param name="position"> Position to check</param>
        /// <param name="sCC"> Delegate for checking if two colliders should collide</param>
        /// <returns> Struct with bool if there are any such colliders and list of such colliders</returns>
        public CollideData CollideOutsideAt(Vector2 position, ShouldCollideCheck sCC = null)
        {
            // Find the colliders that collide with the sensor at its current position
            CollideData original = CollideAt(_position, sCC);

            // Find the colliders that collide with the sensor at the specified position
            CollideData atPoint = CollideAt(position, sCC);

            // Calculate the colliders that do not collide with the sensor but would collide if the sensor was at the specified position
            List<Collider> outsideColliders = new List<Collider>(atPoint._colliding);
            for (int i = outsideColliders.Count - 1; i >= 0; i--)
            {
                foreach (Collider element in original._colliding)
                {
                    if (element._id == outsideColliders[i]._id)
                    {
                        outsideColliders.RemoveAt(i);
                        break;
                    }
                }
            }

            // Return the collision data
            return new CollideData(outsideColliders.Count != 0, outsideColliders);
        }

        //TODO: Implement this function using Colliding(Rectangle)

        /// <summary>
        /// Checks if the sensor is colliding with a specific collider<br/>
        /// </summary>
        /// <param name="collider"> The collider to check</param>
        /// <returns> Bool if the sensor is colliding </returns>
        public bool Colliding(Collider collider)
        {
            // Check if the collider is active and it is a different collider
            if (collider._id == _id || collider._active == false) return false;

            // Check if there are any intersections between the hitboxes
            bool intersect = false;
            foreach (Rectangle rect2 in _hitboxes)
            {
                Rectangle newRect2 = rect2;
                newRect2.Offset(_position);

                foreach (Rectangle rect in collider._hitboxes)
                {
                    Rectangle newRect1 = rect;
                    newRect1.Offset(collider._position);

                    if (newRect1.Intersects(newRect2))
                    {
                        intersect = true;
                        break;
                    }
                }
                // If any intersection is found the colliders are colliding
                if (intersect) break;
            }

            return intersect;
        }

        /// <summary>
        /// Checks if the sensor is colliding with a specific rectangle<br/>
        /// </summary>
        /// <param name="rect"> The rectangle to check</param>
        /// <returns> Bool if the sensor is colliding </returns>
        public bool Colliding(Rectangle rect)
        {
            // Check if there are any intersections between the hitboxes and the rectangle
            bool intersect = false;
            foreach (Rectangle rect2 in _hitboxes)
            {

                Rectangle newRect2 = rect2;
                newRect2.Offset(_position);

                if (rect.Intersects(newRect2))
                {
                    intersect = true;
                }

                if (intersect) break;
            }
            return intersect;
        }

        /// <summary>
        /// Dispose<br/>
        /// </summary>
        public override void Dispose()
        {
            base.Dispose();
        }

        /// <summary>
        /// UnsafeDispose<br/>
        /// Removes the sensor from the list of sensors<br/>
        /// </summary>
        public override void UnsafeDispose()
        {
            Game.I._colliderSensors.Remove(this);
            base.UnsafeDispose();
        }

    }
}
