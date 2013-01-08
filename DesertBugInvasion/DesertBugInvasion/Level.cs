using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace DesertBugInvasion
{
    class Level : GameMode
    {
        TimeSpan _startTime;
        int _levelNumber;

        public Level(Game1 game)
            : base(game)
        {

        }

        public void Start(GameTime gameTime)
        {
            _startTime = gameTime.TotalGameTime;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);
        }
    }
}
