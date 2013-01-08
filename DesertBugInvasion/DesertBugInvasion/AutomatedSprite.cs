using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DesertBugInvasion
{
    class AutomatedSprite : AnimatedSprite
    {

        enum SpriteState { Standing, Walking, Attacking, Dieing }
        SpriteState _currentState;

        protected Vector2 _velocity;
        float _direction;
        float _speed;
        TimeSpan _lastTurnTime;
        TimeSpan _minTurnDelay = TimeSpan.FromMilliseconds(200);

        float _cogDistSquared;

        bool _predator;
        public bool Predator
        {
            get { return _predator; }
        }

        int _maxHealth;
        int _currentHealth;

        public AutomatedSprite(Game1 game, Texture2D textureImage, Vector2 position, Point frameSize,
            Rectangle collisionOffset, Point currentFrame, Point sheetSize, Vector2 velocity, Point frameOffset, string cueName,
            float cogDist, int millisecondsPerFrame = 16, bool predator = false)
            : base(game, textureImage, position, frameSize, collisionOffset, currentFrame,
            sheetSize, frameOffset, cueName, millisecondsPerFrame)
        {
            _velocity = velocity;

            _cogDistSquared = cogDist * cogDist;

            _predator = predator;
            if (_predator)
                _maxHealth = 6;
            else
                _maxHealth = 2;

            _currentHealth = _maxHealth;

            GotoState(SpriteState.Walking);
        }

        public override void Update(GameTime gameTime)
        {
            if (_currentState != SpriteState.Dieing)
            {
                if (_currentState == SpriteState.Walking && Game.NextDouble() > 0.99)
                {
                    GotoState(SpriteState.Standing);
                }
                else if (_currentState == SpriteState.Standing && Game.NextDouble() > 0.96)
                {
                    GotoState(SpriteState.Walking);
                }
            }

            // Move sprite based on direction
            if (_currentState == SpriteState.Walking)
                _position += _velocity;

            base.Update(gameTime);
        }


        // Check the surrounding area for prey to chase or predators to flee from.
        public void CheckProximity(GameComponentCollection spriteList)
        {
            if (_currentState != SpriteState.Dieing)
            {
                AutomatedSprite closestSprite = null;
                float closestTarget = _cogDistSquared;
                Vector2 totalPosition = Vector2.Zero;
                int n = 0;

                foreach (AutomatedSprite s in spriteList)
                {
                    Vector2 posDiff = s._position - _position;
                    if (s._predator != _predator &&
                        s._currentState != SpriteState.Dieing &&
                        posDiff.LengthSquared() < closestTarget)
                    {
                        closestSprite = s;
                        closestTarget = posDiff.LengthSquared();
                        totalPosition += s._position;
                        n++;
                    }
                }


                if (closestSprite != null)
                {
                    GotoState(SpriteState.Walking);

                    Vector2 pointOfInterest;

                    if (_predator && _currentHealth < _maxHealth)
                    {
                        pointOfInterest = closestSprite._position;
                        if (CollisionRect.Intersects(closestSprite.CollisionRect))
                        {
                            closestSprite.Kill();

                            _currentHealth = _maxHealth;
                        }
                    }
                    else
                    {
                        pointOfInterest = totalPosition / n;
                    }


                    Vector2 posDiff = pointOfInterest - _position;
                    float speed = _velocity.Length();
                    Vector2 unitDiff = posDiff;
                    unitDiff.Normalize();

                    if (_predator)
                    {
                        // Seek if hurt
                        if (_currentHealth < _maxHealth)
                        {
                            _velocity = unitDiff * speed;
                        }
                    }
                    else
                    {
                        // Flee
                        _velocity = unitDiff * -speed;
                    }
                }
            }
        }

        void GotoState(SpriteState state)
        {
            if (_currentState != state)
            {
                _currentState = state;

                switch (state)
                {
                    case SpriteState.Standing:
                        _frameOffset.X = 0;
                        _sheetSize = new Point(4, 1);
                        break;
                    case SpriteState.Walking:
                        _frameOffset.X = 4;
                        _sheetSize = new Point(8, 1);
                        break;
                    case SpriteState.Attacking:
                        _frameOffset.X = 12;
                        _sheetSize = new Point(4, 1);
                        break;
                    case SpriteState.Dieing:
                        _frameOffset.X = 16;
                        _sheetSize = new Point(8, 1);
                        _looping = false;
                        break;

                    default:
                        break;
                }

                _currentFrame.X = 0;
            }
        }


        public void Hurt(int damage = 1)
        {
            _currentHealth -= damage;
            if (_currentHealth <= 0)
            {
                Kill();
            }
            else
            {
                // Play bug hit noise
                Game.PlayCue(CueName); // TODO: Change to NOT death sound.
            }
        }


        public void Kill()
        {
            GotoState(SpriteState.Dieing);
            Game.PlayCue(CueName);
        }


        public bool IsDead()
        {
            return _currentState == SpriteState.Dieing;
        }


        void Rescale(float scale)
        {
            if (_currentState != SpriteState.Dieing)
            {
                Point preCenter = CollisionRect.Center;
                _scale *= scale;
                Point postCenter = CollisionRect.Center;

                Vector2 offset = new Vector2(preCenter.X - postCenter.X, preCenter.Y - postCenter.Y);
                _position += offset;
            }

        }
    }
}
