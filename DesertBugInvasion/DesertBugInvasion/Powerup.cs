using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace DesertBugInvasion
{
    class Powerup : Sprite
    {
        TimeSpan _lastSpawn;
        TimeSpan _lifespan = TimeSpan.FromSeconds(3);


        public Powerup(Game1 game, Texture2D texture)
            : base(game, texture)
        {
            Remove();

        }

        public override void Update(GameTime gameTime)
        {
            if (gameTime.TotalGameTime > _lastSpawn + _lifespan)
            {
                Remove();
            }

            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            int msLeft = (int)(((_lastSpawn + _lifespan) - gameTime.TotalGameTime).TotalMilliseconds);

            bool render = true;

            if (msLeft > 0 && msLeft < 1000)
            {
                render = (msLeft / 100) % 2 == 1;
            }

            if (render)
            {
                base.Draw(gameTime);
            }
        }

        public void Spawn(GameTime gameTime)
        {
            int w = GraphicsDevice.PresentationParameters.BackBufferWidth - _texture.Width;
            int h = GraphicsDevice.PresentationParameters.BackBufferHeight - _texture.Height;
            _position.X = (float)Game.NextDouble() * w;
            _position.Y = (float)Game.NextDouble() * h;

            _lastSpawn = gameTime.TotalGameTime;

            _color = Color.White;
        }

        public bool ContainsPoint(Point point)
        {
            bool result = false;

            
            Rectangle bounds = new Rectangle((int)_position.X, (int)_position.Y, _texture.Width, _texture.Height);


            result = bounds.Contains(point);

            return result;

        }

        public void Remove()
        {
            // Move off screen
            _position.X = -_texture.Width;
            _position.Y = -_texture.Height;
        }
    }
}
