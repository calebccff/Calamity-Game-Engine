using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Diagnostics;

namespace TestShader
{
    /// <summary>
    /// A class representing a single particle<br/>
    /// Adapted from Monocle Engine from Maddy Thorson<br/>
    /// </summary>
    public class Particle
    {
        /// <summary>
        /// The sprite that this particle follows (moves relative to)<br/>
        /// </summary>
        public SpriteGameComponent _track;
        
        /// <summary>
        /// The type of particle<br/>
        /// </summary>
        public ParticleType _type;

        /// <summary>
        /// The texture that this particle uses<br/>
        /// </summary>
        public Texture2D _source;
        /// <summary>
        /// The rectangle in the source texture that this particle uses<br/>
        /// </summary>
        public Rectangle _rect;

        /// <summary>
        /// Whether or not this particle is visible<br/>
        /// </summary>
        public bool _visible;

        /// <summary>
        /// The color of this particle currently<br/>
        /// </summary>
        public Color _color;

        /// <summary>
        /// The color of this particle when it starts<br/>
        /// </summary>
        public Color StartColor;

        /// <summary>
        /// The position of this particle<br/>
        /// </summary>
        public Vector2 _position;

        /// <summary>
        /// The speed of this particle<br/>
        /// </summary>
        public Vector2 _speed;

        /// <summary>
        /// The current scale of this particle for rendering<br/>
        /// </summary>
        public float _scale;

        /// <summary>
        /// The scale of this particle when it starts<br/>
        /// </summary>
        public float StartScale;

        /// <summary>
        /// The currently remaining lifespan of this particle<br/>
        /// </summary>
        public float _life;

        /// <summary>
        /// The lifespan of this particle when it starts<br/>
        /// </summary>
        public float StartLife;

        /// <summary>
        /// The current rotation of this particle<br/>
        /// </summary>
        public float _rotation;

        /// <summary>
        /// The current spin of this particle (rotation speed)<br/>
        /// </summary>
        public float _spin;

        /// <summary>
        /// The depth of this particle for rendering<br/>
        /// </summary>
        public int _depth;

        /// <summary>
        /// The gravity multiplier of this particle currently<br/>
        /// </summary>
        public float _gravity;

        /// <summary>
        /// The max fall speed of this particle<br/>
        /// </summary>
        public float _maxFall;

        /// <summary>
        /// Simulate this particle for the given duration<br/>
        /// </summary>
        public bool SimulateFor(float duration)
        {
            if (duration > _life)
            {
                _life = 0;
                _visible = false;
                return false;
            }
            else
            {
                float dt = (float)Game.I.TargetElapsedTime.TotalSeconds;
                if (dt > 0)
                    for (var t = 0f; t < duration; t += dt)
                        Update(dt);

                return true;
            }
        }

        /// <summary>
        /// Update this particle<br/>
        /// </summary>
        /// <param name="delta">Time since last update</param>
        public virtual void Update(float delta)
        {
            var dt = 0f;

            dt = delta;

            //Calculate the fraction of the particle's lifespan left
            var ease = _life / StartLife;

            //Update lifespan
            _life -= dt;
            if (_life <= 0)
            {
                _visible = false; //not visible particles are not updated

                return;
            }



            //Update Spin and rotation depending on rotation mode
            if (_type.RotationMode == ParticleType.RotationModes.SameAsDirection)
            {
                if (_speed != Vector2.Zero)
                    _rotation = _speed.Angle();
            }
            else
                _rotation += _spin * dt;

            //Update alpha depending on FadeMode
            float alpha;
            if (_type.FadeMode == ParticleType.FadeModes.Linear)
                alpha = ease;
            else if (_type.FadeMode == ParticleType.FadeModes.Late)
                alpha = Math.Min(1f, ease / .25f);
            else if (_type.FadeMode == ParticleType.FadeModes.InAndOut)
            {
                if (ease > .75f)
                    alpha = 1 - ((ease - .75f) / .25f);
                else if (ease < .25f)
                    alpha = ease / .25f;
                else
                    alpha = 1f;
            }
            else
                alpha = 1f;


            //Update color based on ColorMode
            if (alpha == 0)
                _color = Color.Transparent;
            else
            {
                if (_type.ColorMode == ParticleType.ColorModes.Static)
                    _color = StartColor;
                else if (_type.ColorMode == ParticleType.ColorModes.Fade)
                    _color = Color.Lerp(_type._color2, StartColor, ease);
                else if (_type.ColorMode == ParticleType.ColorModes.Blink)
                    _color = (Calc.BetweenInterval(_life, .1f) ? StartColor : _type._color2);
                else if (_type.ColorMode == ParticleType.ColorModes.Choose)
                    _color = StartColor;

                if (alpha < 1f)
                    _color *= alpha;
            }


            //Update speed and position with friction (_type.Friction)
            _position += _speed * dt;
            _speed += _type.Acceleration * dt;
            _speed = Calc.Approach(_speed, Vector2.Zero, _type.Friction * dt);
            _speed.Y = Calc.Approach(_speed.Y, _maxFall, _gravity * Game.I._deltaTime);
            if (_type.SpeedMultiplier != 1)
                _speed *= (float)Math.Pow(_type.SpeedMultiplier, dt);

            //Update scale
            if (_type.ScaleOut)
                _scale = StartScale * Ease.CubeOut(ease);
        }

        /// <summary>
        /// Draw this particle<br/>
        /// </summary>
        public void Draw()
        {
            //Check if the particle is visible
            if (!_visible) return;

            //Calculate render position
            var renderAt = new Vector2((int)_position.X, (int)_position.Y);
            
            //If we have a sprite to track, add its position
            if (_track != null)
                renderAt += _track._position.ToVector2();

            Vector2 scaledPosition = renderAt;

            //draw the particle
            Game.I._spriteRenderer.Draw(_source, scaledPosition, _rect, _color, _rotation, new Vector2(_rect.Width / 2, _rect.Height / 2), new Vector2(_scale, _scale), SpriteEffects.None, _depth);
        }
    }
}
