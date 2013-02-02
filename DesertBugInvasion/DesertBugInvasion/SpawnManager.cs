using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System.Collections.ObjectModel;


namespace DesertBugInvasion
{
    /// <summary>
    /// This is a game component that implements IUpdateable.
    /// </summary>
    public class SpawnManager : DrawableGameComponent
    {
        //A sprite for the player and a list of automated sprites
        public GameComponentCollection Components { get; private set; }

        // Variables for spawning new enemies
        float _minSpeed = 1.2f;
        float _maxSpeed = 2.0f;

        TimeSpan _lastSpawnTime;
        public TimeSpan LevelStartTime { get; set; }
        public int LevelNumber { get; set; }

        public new Game1 Game { get { return (Game1)base.Game; } }

        public SpawnManager(Game1 game)
            : base(game)
        {
            Components = new GameComponentCollection();

            //for (int i = 0; i <= 30000; i += 1000)
            //{
            //    Console.WriteLine(i + ": " + CalcSpawnDelay(i));
            //}

            _lastSpawnTime = TimeSpan.FromSeconds(0);

            LevelNumber = 1;
        }

        /// <summary>
        /// Allows the game component to perform any initialization it needs to before starting
        /// to run.  This is where it can query for any required services and load content.
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();
        }

        protected override void LoadContent()
        {

            base.LoadContent();
        }

        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {
            MouseState mouseState = Mouse.GetState();

            double spawnsPerSec =
                CalcSpawnRate(gameTime.TotalGameTime.TotalMilliseconds - LevelStartTime.TotalMilliseconds);

            double secSinceLastSpawn = gameTime.TotalGameTime.TotalSeconds - _lastSpawnTime.TotalSeconds;

            int numToSpawn = (int)(spawnsPerSec * secSinceLastSpawn);

            for (int i = 0; i < numToSpawn; i++)
            {
                SpawnEnemy();
            }

            _lastSpawnTime += TimeSpan.FromSeconds(numToSpawn / spawnsPerSec);


            // Update all sprites
            Collection<IGameComponent> componentsToRemove = new Collection<IGameComponent>();
            foreach (AutomatedSprite s in Components)
            {
                // Make the sprite check the surrounding area for prey to chase or predators to flee from.
                s.CheckProximity(Components);

                s.Update(gameTime);

                // Remove object if it is out of bounds
                if (s.IsOutOfBounds(Game.Window.ClientBounds))
                {
                    componentsToRemove.Add(s);
                }

            }

            foreach (IGameComponent component in componentsToRemove)
            {
                Components.Remove(component);
            }

            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            //Game.SpriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend);

            // Draw all sprites
            foreach (AnimatedSprite s in Components)
                s.Draw(gameTime);

            //Game.SpriteBatch.End();
            base.Draw(gameTime);
        }


        private void SpawnEnemy()
        {
            Vector2 velocity = Vector2.Zero;
            Vector2 position = Vector2.Zero;

            // Default frame size
            Point frameSize = new Point(128, 128);

            // Randomly choose which side of the screen to place enemy,
            // then randomly create a position along that side of the screen
            // and randomly choose a speed for the enemy
            
            float speed = ((float)Game.NextDouble() * (_maxSpeed - _minSpeed)) + _minSpeed;

            switch (Game.NextRandomInt(4))
            {
                case 0: // LEFT to RIGHT
                    position = new Vector2(
                        -frameSize.X, Game.NextRandomInt(
                        GraphicsDevice.PresentationParameters.BackBufferHeight
                        - frameSize.Y));

                    velocity = new Vector2(speed, 0);
                    break;

                case 1: // RIGHT to LEFT
                    position = new
                        Vector2(
                        GraphicsDevice.PresentationParameters.BackBufferWidth,
                        Game.NextRandomInt(
                        GraphicsDevice.PresentationParameters.BackBufferHeight
                        - frameSize.Y));

                    velocity = new Vector2(-speed, 0);
                    break;

                case 2: // BOTTOM to TOP
                    position = new Vector2(Game.NextRandomInt(
                        GraphicsDevice.PresentationParameters.BackBufferWidth
                        - frameSize.X),
                        GraphicsDevice.PresentationParameters.BackBufferHeight);

                    velocity = new Vector2(0, -speed);
                    break;

                case 3: // TOP to BOTTOM
                    position = new Vector2(Game.NextRandomInt(
                        GraphicsDevice.PresentationParameters.BackBufferWidth
                        - frameSize.X), -frameSize.Y);

                    velocity = new Vector2(0, speed);
                    break;
            }

            // Create the sprite
            if (Game.NextDouble() < 0.20) // 20% chance to spawn a predator.
            {
                Components.Add(new AutomatedSprite(Game,
                    Game.Content.Load<Texture2D>(@"Images/fire_ant"),
                    position, new Point(128, 128), new Rectangle(48, 72, 34, 24), new Point(0, 0),
                    new Point(8, 1), velocity, new Point(4, 0), "deaths", 200, 64, true));
            }
            else
            {
                Components.Add(new AutomatedSprite(Game,
                    Game.Content.Load<Texture2D>(@"Images/spider_0"),
                    position, new Point(128, 128), new Rectangle(48, 72, 34, 22), new Point(0, 0),
                    new Point(8, 1), velocity, new Point(4, 0), "death3", 100, 64));
            }

        }

        /// <summary>
        /// Calculates the number of milliseconds between enemy spawns given level time.
        /// </summary>
        /// <param name="gameTime"></param>
        /// <returns></returns>
        double CalcSpawnRate(double levelMs)
        {
            double pulsePeriod = 15000.0;
            double x = levelMs / pulsePeriod + LevelNumber + 2;
            double rampRate = 0.2 + 0.05 * (LevelNumber + 2);
            double spawnsPerSec = (Math.Cos(x * (2 * Math.PI)) + 1) * rampRate * x;
            
            //int spawnDelayMs = 0;
            //if (spawnsPerSec > 0)
            //    spawnDelayMs = (int)Math.Round((1 / spawnsPerSec) * 1000);

            return spawnsPerSec;
        }
    }
}
