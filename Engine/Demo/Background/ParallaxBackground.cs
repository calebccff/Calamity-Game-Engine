using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Microsoft.Xna.Framework.Audio;

namespace TestShader
{
    class ParallaxBackground : SpriteGameComponent
    {
        //TODO: manage this class, so the background wraps
        float _layer;
        bool fixX = false;
        bool fixY = false;

        public ParallaxBackground(Texture2D texture, float layer) : base(texture)
        {
            _origin.X = texture.Width / 2;
            _origin.Y = texture.Height / 2;
            //0 normal 1 infinetly far (1-1/distance))
            _layerDepth = 0.7f + layer / 100;//-layer+10;
            _layer = layer;
            //Debug.WriteLine(texture.Bounds);
        }


        public override void Update(GameTime gameTime)
        {

             ;
            //Debug.Write(_position);
            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            if (_visible)
            {
                Vector2 origin = _origin;
                if (_spriteEffect == SpriteEffects.FlipVertically) { origin.Y = _spriteFrame.Height - origin.Y; }
                if (_spriteEffect == SpriteEffects.FlipHorizontally) { origin.X = _spriteFrame.Width - origin.X; }
                Func<Camera, Vector2> dynamicPosition = (camera) =>
                {
                    Point position = new Point();
                    position = (camera._position.ToVector2() * _layer + _scale*new Vector2(0, _height/10)/_layer).ToPoint();
                    if (fixX) { position -= new Point(position.X, 0); }
                    if (fixY) { position -= new Point(0, position.Y); };
                    return position.ToVector2();
                };

                _spriteRenderer.Draw(_texture, dynamicPosition, _spriteFrame, _color, _rotation, origin, new Vector2(_scale.X, _scale.Y), _spriteEffect, _layerDepth, _shader);
            }
        }
    }
}

