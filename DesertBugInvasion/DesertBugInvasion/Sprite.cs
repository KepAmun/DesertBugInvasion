using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace DesertBugInvasion
{
    class Sprite : DrawableGameComponent
    {
        protected Texture2D _texture;
        protected Vector2 _position;
        protected Rectangle _sourceRectangle;
        protected Color _color;
        protected Vector2 _scale;
        protected float _rotation;
        protected Vector2 _origin;

        public new Game1 Game { get { return (Game1)base.Game; } }

        public Sprite(Game1 game, Texture2D texture)
            : this(game, texture, Vector2.Zero)
        {
        }

        public Sprite(Game1 game, Texture2D texture, Vector2 position)
            : this(game, texture, position, new Vector2(1, 1))
        {
        }

        public Sprite(Game1 game, Texture2D texture, Vector2 position, Vector2 scale)
            : base(game)
        {
            _texture = texture;
            _position = position;
            _sourceRectangle = new Rectangle(0, 0, _texture.Width, _texture.Height);
            _color = Color.White;
            _scale = scale;
            _rotation = 0;
            _origin = Vector2.Zero;
        }

        public override void Draw(GameTime gameTime)
        {
            Game.SpriteBatch.Draw(
                _texture,
                _position,
                _sourceRectangle,
                _color,
                _rotation,
                _origin,
                _scale,
                SpriteEffects.None,
                0);
        }
    }
}
