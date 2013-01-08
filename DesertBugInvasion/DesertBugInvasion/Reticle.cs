using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace DesertBugInvasion
{
    class Reticle : AnimatedSprite
    {
        public enum ReticleState { Idle, Firing, Reloading }

        ReticleState State { get; set; }

        MouseState _prevMouseState;

        public bool FiredThisFrame { get; private set; }
        int _lastFire;
        int _fireDelay;
        int _baseFireDelay;
        int _powerupFireDelay;

        public int MaxAmmo { get; private set; }
        public int CurrentAmmo { get; private set; }
        TimeSpan _reloadStartTime;
        TimeSpan _reloadDuration;

        double _recoilDistance = 5;

        public Vector2 Position { get { return _position; } }


        bool _powerupActive = false;
        TimeSpan _powerupDuration = TimeSpan.FromSeconds(10);
        TimeSpan _powerupEndTime = TimeSpan.FromSeconds(0);


        public Reticle(Game1 game, Texture2D textureImage, Vector2 position,
            Point frameSize, Rectangle collisionOffset, Point currentFrame,
            Point sheetSize, Point frameOffset, int millisecondsPerFrame = 16)
            : base(game, textureImage, position, frameSize, collisionOffset, currentFrame,
            sheetSize, frameOffset, null, millisecondsPerFrame)
        {
            _scale = new Vector2(0.5f);

            _lastFire = int.MinValue;
            _baseFireDelay = 150;
            _powerupFireDelay = 100;
            _fireDelay = _baseFireDelay;

            MaxAmmo = 60;
            CurrentAmmo = MaxAmmo;
            _reloadDuration = TimeSpan.FromSeconds(1.5);
            _origin = new Vector2(64, 64);
        }

        public override void Update(GameTime gameTime)
        {
            MouseState mouseState = Mouse.GetState();
            _position = new Vector2(mouseState.X, mouseState.Y);
            FiredThisFrame = false;

            int gameMs = (int)gameTime.TotalGameTime.TotalMilliseconds;


            if (_powerupActive)
            {
                _rotation = -(float)(Math.PI * 2 * (_powerupEndTime.TotalMilliseconds - gameMs) / _powerupDuration.TotalMilliseconds);

                if (gameTime.TotalGameTime > _powerupEndTime)
                {
                    EndPowerUp(gameTime);
                }
            }


            if (State == ReticleState.Reloading)
            {
                if (gameTime.TotalGameTime > _reloadStartTime + _reloadDuration)
                {
                    Reload();
                    _color = Color.White;
                    State = ReticleState.Idle;
                }
                else
                {
                    float reloadRatio = 
                        (float)((gameMs - _reloadStartTime.TotalMilliseconds) / _reloadDuration.TotalMilliseconds);

                    CurrentAmmo = (int)(MaxAmmo * reloadRatio);

                    _rotation = -(float)Math.PI * 2 * reloadRatio;
                }
            }
            else
            {

                if (mouseState.LeftButton == ButtonState.Pressed && 
                    (State == ReticleState.Firing || 
                    (State == ReticleState.Idle && _prevMouseState.LeftButton == ButtonState.Released))
                   )
                {
                    if (gameMs > _lastFire + _fireDelay)
                    {
                        _lastFire = gameMs;

                        if (CurrentAmmo <= 0)
                        {
                            _color = Color.Red;
                            _rotation = 0;

                            if (_prevMouseState.LeftButton == ButtonState.Released)
                            {
                                State = ReticleState.Reloading;
                                _reloadStartTime = gameTime.TotalGameTime;
                                // Play gun reloading sound
                                Game.PlayCue("gunreload");
                            }
                            else
                            {
                                // Play gun empty click sound
                                Game.PlayCue("metallicclick");
                            }
                        }
                        else
                        {
                            Game.PlayCue("m21shot");
                            State = ReticleState.Firing;
                            FiredThisFrame = true;


                            if (!_powerupActive)
                            {
                                _rotation = -(float)(Math.PI * 2 * CurrentAmmo / (float)MaxAmmo);
                                CurrentAmmo--;
                            }

                            double recoilAngle = Game.NextDouble() * Math.PI * 2;

                            Vector2 recoil;
                            recoil.X = (float)(Math.Sin(recoilAngle) * _recoilDistance);
                            recoil.Y = (float)(Math.Cos(recoilAngle) * _recoilDistance);

                            Mouse.SetPosition(mouseState.X + (int)Math.Round(recoil.X), 
                                              mouseState.Y + (int)Math.Round(recoil.Y));
                        }
                    }
                }
                else
                {
                    State = ReticleState.Idle;
                }
            }
            

            _prevMouseState = mouseState;
            base.Update(gameTime);
        }

        internal void Reload()
        {
            CurrentAmmo = MaxAmmo;
            // Play gun reloaded sound
            Game.PlayCue("riflereload");
            _rotation = 0;
        }

        public void StartPowerUp(GameTime gameTime)
        {
            _fireDelay = _powerupFireDelay;
            _powerupEndTime = gameTime.TotalGameTime + _powerupDuration;

            Reload();

            _color = Color.Green;

            _powerupActive = true;
        }

        public void EndPowerUp(GameTime gameTime)
        {
            _fireDelay = _baseFireDelay;

            _color = Color.White;

            _powerupActive = false;
        }
    }
}
