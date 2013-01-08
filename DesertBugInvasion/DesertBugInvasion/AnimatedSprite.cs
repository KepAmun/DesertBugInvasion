using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;

namespace DesertBugInvasion
{
    abstract class AnimatedSprite : Sprite
    {
        // Stuff needed to draw the sprite
        protected Point _frameSize;
        protected Point _currentFrame;
        protected Point _sheetSize;

        // Collision data
        protected Rectangle _collisionOffset;

        // Framerate stuff
        int _timeSinceLastFrame = 0;
        int _millisecondsPerFrame;
        protected Point _frameOffset;

        Vector2 _lastPosition;
        int _lastDirection;

        string _cueName;

        protected bool _looping = true;

        public string CueName
        {
            get { return _cueName; }
        }


        public AnimatedSprite(Game1 game, Texture2D textureImage, Vector2 position, Point frameSize,
            Rectangle collisionOffset, Point currentFrame, Point sheetSize, Point frameOffset, string cueName,
            int millisecondsPerFrame = 16)
            : base(game, textureImage, position)
        {
            _frameSize = frameSize;
            _lastPosition = position;
            _lastDirection = 0;
            _collisionOffset = collisionOffset;
            _currentFrame = currentFrame;
            _sheetSize = sheetSize;
            _frameOffset = frameOffset;
            _cueName = cueName;
            _millisecondsPerFrame = millisecondsPerFrame;
        }

        public override void Update(GameTime gameTime)
        {
            int direction = _lastDirection;

            if (_lastPosition != _position)
            {
                // Determining which row of the sprite sheet to use by examining 
                // the angle of the vector between the last position and the current one.
                Vector2 posDelta = (_position - _lastPosition);
                posDelta.Normalize();

                double theta = Math.Atan2(-posDelta.Y, -posDelta.X);

                direction = (int)Math.Round(theta / (Math.PI / 4)) % 8;

                if (direction < 0)
                    direction += 8;

                if (_sheetSize.X != 1)
                    _frameOffset.Y = direction;

                _lastPosition = _position;
                _lastDirection = direction;
            }


            // Update frame if time to do so based on framerate
            _timeSinceLastFrame += gameTime.ElapsedGameTime.Milliseconds;
            if (_timeSinceLastFrame > _millisecondsPerFrame)
            {
                _timeSinceLastFrame = 0;

                if (_currentFrame.X + 1 >= _sheetSize.X)
                {
                    if (_looping)
                    {
                        _currentFrame.X = 0;
                        ++_currentFrame.Y;
                        if (_currentFrame.Y >= _sheetSize.Y)
                            _currentFrame.Y = 0;
                    }
                }
                else
                {
                    // Increment to next frame
                    ++_currentFrame.X;
                }
            }
        }

        public virtual void Draw(GameTime gameTime)
        {
            _sourceRectangle = new Rectangle((_currentFrame.X + _frameOffset.X) * _frameSize.X,
                    (_currentFrame.Y + _frameOffset.Y) * _frameSize.Y,
                    _frameSize.X, _frameSize.Y);

            base.Draw(gameTime);
        }

        // Gets the collision rect based on position, framesize and collision offset
        public Rectangle CollisionRect
        {
            get
            {
                Rectangle r = new Rectangle((int)(_collisionOffset.X * _scale.X), (int)(_collisionOffset.Y * _scale.Y), (int)(_collisionOffset.Width * _scale.X), (int)(_collisionOffset.Height * _scale.Y));

                r.Offset((int)_position.X, (int)_position.Y);

                return r;
            }
        }

        // Detect if this sprite is off the screen and irrelevant
        public bool IsOutOfBounds(Rectangle clientRect)
        {
            if (_position.X < -_frameSize.X ||
                _position.X > clientRect.Width ||
                _position.Y < -_frameSize.Y ||
                _position.Y > clientRect.Height)
            {
                return true;
            }

            return false;
        }
    }
}
