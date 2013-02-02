using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;

namespace DesertBugInvasion
{
    public class Game1 : Game
    {
        enum GameState { mainMenu, activeLevel, gameOver };
        enum GameSubstate { uninitilized, loading, active, unloading, done };

        GameState _currectGameState = GameState.mainMenu;
        GameSubstate _currectGameSubstate = GameSubstate.uninitilized;

        TimeSpan _stateStartTime;

        FadeBox _fadeBox;

        GraphicsDeviceManager _graphics;
        SpriteBatch _spriteBatch;

        public SpriteBatch SpriteBatch { get { return _spriteBatch; } }

        SpawnManager _spawnManager;

        List<Sprite> _bulletCraters;
        Texture2D _bulletCraterImage;

        AudioEngine _audioEngine;
        WaveBank _waveBank;
        SoundBank _soundBank;
        public SoundBank SoundBank
        {
            get { return _soundBank; }
        }

        Sprite _menuBackground;
        Sprite _levelBackground;

        SpriteFont _titleFont;
        SpriteFont _menuFont;
        SpriteFont _menuFontSmall;
        SpriteFont _scoreFont;

        int _spidersKilled = 0;
        int _fireAntsKilled = 0;
        int _spiderKillsRequired = 30;
        int _fireAntKillsRequired = 8;
        public void SpiderKilled() { _spidersKilled++; }
        public void FireAntKilled() { _fireAntsKilled++; }

        Random _rnd;

        MouseState _lastMouseState;

        Cue _menuMusic;
        Cue _levelMusic;
        Cue _endWinMusic;
        Cue _endLoseMusic;


        Reticle _reticle;

        int _levelNumber;
        int _maxLevel = 5;
        TimeSpan _levelDuration;


        Powerup _powerup;
        TimeSpan _nextPowerupSpawn;
        int _powerupSpawnTimeMin = 30; // In seconds
        int _powerupSpawnTimeRange = 10;


        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            _rnd = new Random();

            _bulletCraters = new List<Sprite>();

            _spawnManager = new SpawnManager(this);
            _fadeBox = new FadeBox(this);

            _levelDuration = TimeSpan.FromMinutes(2);
        }

        protected override void Initialize()
        {
            _spawnManager.Initialize();
            _fadeBox.Initialize();

            _audioEngine = new AudioEngine("Content\\Audio\\GameAudio.xgs");
            _waveBank = new WaveBank(_audioEngine, "Content\\Audio\\Wave Bank.xwb");
            _soundBank = new SoundBank(_audioEngine, "Content\\Audio\\Sound Bank.xsb");

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _reticle = new Reticle(this,
                Content.Load<Texture2D>(@"Images/Crosshair"),
                Vector2.Zero, new Point(128, 128), new Rectangle(63, 63, 1, 1), new Point(0, 0),
                new Point(1, 1), new Point(0, 0));

            _powerup = new Powerup(this, Content.Load<Texture2D>(@"Images/Army-Box-64"));
            _powerup.Initialize();

            // Create a new SpriteBatch, which can be used to draw textures.
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            _menuBackground = new Sprite(this, Content.Load<Texture2D>(@"Images/DesertTown"));
            _levelBackground = new Sprite(this, Content.Load<Texture2D>(@"Images/Desert"));

            _titleFont = Content.Load<SpriteFont>(@"Fonts/TitleFont");
            _menuFont = Content.Load<SpriteFont>(@"Fonts/MenuFont");
            _menuFontSmall = Content.Load<SpriteFont>(@"Fonts/MenuFontSmall");
            _scoreFont = Content.Load<SpriteFont>(@"Fonts/ScoreFont");

            _bulletCraterImage = Content.Load<Texture2D>(@"Images/BulletCrater");

            _levelMusic = _soundBank.GetCue("Last Minute_2");
            _menuMusic = _soundBank.GetCue("The Dark Amulet_0");
            _endWinMusic = _soundBank.GetCue("win music 1-3");
            _endLoseMusic = _soundBank.GetCue("Snow May Never End");

        }

        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        protected override void Update(GameTime gameTime)
        {
            MouseState mouseState = Mouse.GetState();

            switch (_currectGameState)
            {
                case GameState.mainMenu:

                    switch (_currectGameSubstate)
                    {
                        case GameSubstate.uninitilized:
                            _stateStartTime = gameTime.TotalGameTime;
                            _currectGameSubstate = GameSubstate.loading;

                            IsMouseVisible = true;

                            PlayMenuMusic();
                            _fadeBox.FadeIn();

                            _levelNumber = 1;
                            break;

                        case GameSubstate.loading:
                            if (_fadeBox.State == FadeBox.FadeState.Idle)
                            {
                                _currectGameSubstate = GameSubstate.active;
                            }
                            break;

                        case GameSubstate.active:
                            if(MousePressedOverViewport(mouseState))
                            {
                                _currectGameSubstate = GameSubstate.unloading;

                                _menuMusic.Stop(AudioStopOptions.Immediate);

                                _fadeBox.FadeOut();
                            }
                            break;
                        case GameSubstate.unloading:
                            if (_fadeBox.State == FadeBox.FadeState.Idle)
                            {
                                _currectGameState = GameState.activeLevel;
                                _currectGameSubstate = GameSubstate.uninitilized;
                            }
                            break;
                        default:
                            break;
                    }

                    break;

                case GameState.activeLevel:

                    _spawnManager.Update(gameTime);
                    _powerup.Update(gameTime);

                    switch (_currectGameSubstate)
                    {
                        case GameSubstate.uninitilized:
                            _stateStartTime = gameTime.TotalGameTime;
                            _currectGameSubstate = GameSubstate.loading;
                            _fadeBox.FadeIn();
                            IsMouseVisible = false;

                            _spidersKilled = 0;
                            _fireAntsKilled = 0;
                            _reticle.Reload();
                            _spawnManager.Components.Clear();
                            _bulletCraters.Clear();
                            _spawnManager.LevelStartTime = gameTime.TotalGameTime;

                            _spawnManager.LevelNumber = _levelNumber;

                            ResetPowerupSpawn(gameTime);

                            _soundBank.PlayCue("76405__dsp9000__old-church-bell");
                            PlayLevelMusic();

                            break;

                        case GameSubstate.loading:
                            if (_fadeBox.State == FadeBox.FadeState.Idle)
                            {
                                _currectGameSubstate = GameSubstate.active;
                            }
                            break;
                        case GameSubstate.active:
                            
                            _reticle.Update(gameTime);

                            if (_reticle.FiredThisFrame)
                            {
                                Point shotPos = new Point((int)_reticle.Position.X, (int)_reticle.Position.Y);

                                // Add bullet impact crater sprite.
                                _bulletCraters.Add(
                                    new Sprite(this,
                                        _bulletCraterImage,
                                        new Vector2(shotPos.X - (_bulletCraterImage.Width / 2),
                                                    shotPos.Y - (_bulletCraterImage.Height / 2) + 3)));


                                if (_powerup.ContainsPoint(shotPos))
                                {
                                    _reticle.StartPowerUp(gameTime);
                                    _powerup.Remove();
                                }

                                foreach (AutomatedSprite bug in _spawnManager.Components)
                                {
                                    // Check for reticle collisions
                                    if (!bug.IsDead() && bug.CollisionRect.Contains(shotPos))
                                    {
                                        bug.Hurt();

                                        if (bug.IsDead())
                                        {
                                            if (bug.Predator)
                                            {
                                                FireAntKilled();
                                            }
                                            else
                                            {
                                                SpiderKilled();
                                            }
                                        }

                                    }
                                }
                            }

                            if (gameTime.TotalGameTime > _nextPowerupSpawn)
                            {
                                SpawnPowerup(gameTime);
                            }


                            if (_levelDuration < (gameTime.TotalGameTime - _stateStartTime))
                            {
                                _currectGameSubstate = GameSubstate.unloading;

                                _levelMusic.Stop(AudioStopOptions.Immediate);

                                _fadeBox.FadeOut();
                            }
                            break;
                        case GameSubstate.unloading:
                            if (_fadeBox.State == FadeBox.FadeState.Idle)
                            {
                                _currectGameState = GameState.gameOver;
                                _currectGameSubstate = GameSubstate.uninitilized;
                            }
                            break;
                        default:
                            break;
                    }
                    break;

                case GameState.gameOver:

                    switch (_currectGameSubstate)
                    {
                        case GameSubstate.uninitilized:
                            _stateStartTime = gameTime.TotalGameTime;
                            _currectGameSubstate = GameSubstate.loading;
                            _fadeBox.FadeIn();

                            if (_spidersKilled >= _spiderKillsRequired &&
                                _fireAntsKilled >= _fireAntKillsRequired)
                            {
                                PlayEndWinMusic();
                            }
                            else
                            {
                                PlayEndLoseMusic();
                            }

                            IsMouseVisible = true;
                            break;

                        case GameSubstate.loading:
                            if (_fadeBox.State == FadeBox.FadeState.Idle)
                            {
                                _currectGameSubstate = GameSubstate.active;
                            }
                            break;
                        case GameSubstate.active:
                            if (gameTime.TotalGameTime - _stateStartTime > TimeSpan.FromSeconds(2) &&
                                MousePressedOverViewport(mouseState) &&
                                _lastMouseState.LeftButton != ButtonState.Pressed)
                            {
                                _fadeBox.FadeOut();
                                _currectGameSubstate = GameSubstate.unloading;
                            }
                            break;
                        case GameSubstate.unloading:
                            if (_fadeBox.State == FadeBox.FadeState.Idle)
                            {
                                if (_levelNumber <= _maxLevel)
                                {
                                    _currectGameState = GameState.activeLevel;
                                }
                                else
                                {
                                    _currectGameState = GameState.mainMenu;
                                }

                                _currectGameSubstate = GameSubstate.uninitilized;


                                if (_spidersKilled >= _spiderKillsRequired &&
                                    _fireAntsKilled >= _fireAntKillsRequired)
                                {
                                    _levelNumber++;
                                    _spiderKillsRequired = (int)(_spiderKillsRequired * 1.1f + 1);
                                    _fireAntKillsRequired = (int)(_fireAntKillsRequired * 1.1f + 1);
                                }

                            }
                            break;
                        default:
                            break;
                    }
                    break;

                default:
                    break;
            }

            _lastMouseState = mouseState;

            base.Update(gameTime);


            _fadeBox.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {

            _spriteBatch.Begin();

            switch (_currectGameState)
            {
                case GameState.mainMenu:

                    _menuBackground.Draw(gameTime);

                    _spriteBatch.DrawString(_titleFont,
                        "Desert Bug Invasion",
                        new Vector2(10, 20),
                        Color.DarkRed);

                    if (gameTime.TotalGameTime - _stateStartTime > TimeSpan.FromSeconds(1))
                        _spriteBatch.DrawString(_menuFont,
                            "Click to Play",
                            new Vector2(160, 200),
                            Color.DarkRed);

                    if (gameTime.TotalGameTime - _stateStartTime > TimeSpan.FromSeconds(2))
                        _spriteBatch.DrawString(_menuFontSmall,
                            "Kep Amun",
                            new Vector2(240, 240),
                            Color.DarkGoldenrod);
                    break;

                case GameState.activeLevel:

                    _levelBackground.Draw(gameTime);

                    foreach (Sprite crater in _bulletCraters)
                    {
                        crater.Draw(gameTime);
                    }

                    _powerup.Draw(gameTime);

                    _spawnManager.Draw(gameTime);

                    if (_currectGameSubstate == GameSubstate.active)
                        _reticle.Draw(gameTime);

                    TimeSpan timeLeft = _levelDuration - (gameTime.TotalGameTime - _stateStartTime);

                    if (gameTime.TotalGameTime - _stateStartTime < TimeSpan.FromSeconds(3))
                    {
                        Color c = Color.DarkRed;
                        if (gameTime.TotalGameTime - _stateStartTime > TimeSpan.FromSeconds(2))
                            c.A = (byte)(0xFF * (3000 - (gameTime.TotalGameTime - _stateStartTime).TotalMilliseconds) / 1000f);

                        _spriteBatch.DrawString(_titleFont,
                            string.Format("Level {0}", _levelNumber),
                            new Vector2(250, 10),
                            c);

                        _spriteBatch.DrawString(_titleFont,
                            string.Format("Spiders: {0}/{1}\nFire Ants: {2}/{3}",
                            _spidersKilled, _spiderKillsRequired,
                            _fireAntsKilled, _fireAntKillsRequired),
                            new Vector2(160, 120),
                            c);

                        _spriteBatch.DrawString(_titleFont,
                            timeLeft.ToString(@"m\:ss"),
                            new Vector2(300, 320),
                            c);
                    }
                    else
                    {
                        _spriteBatch.DrawString(_scoreFont,
                            string.Format("Level {0} - Spiders: {1}/{2}  Fire Ants: {3}/{4}",
                            _levelNumber,
                            _spidersKilled, _spiderKillsRequired,
                            _fireAntsKilled, _fireAntKillsRequired),
                            new Vector2(200, 0),
                            Color.DarkRed);

                        _spriteBatch.DrawString(_scoreFont,
                            string.Format("Ammo: {0}/{1}",
                            _reticle.CurrentAmmo, _reticle.MaxAmmo),
                            new Vector2(350, 450),
                            Color.DarkRed);

                        _spriteBatch.DrawString(_scoreFont,
                            timeLeft.ToString(@"m\:ss"),
                            new Vector2(750, 450),
                            Color.DarkRed);
                    }


                    break;

                case GameState.gameOver:

                    _menuBackground.Draw(gameTime);

                    if (_spidersKilled >= _spiderKillsRequired &&
                        _fireAntsKilled >= _fireAntKillsRequired)
                    {
                        if (_levelNumber < _maxLevel)
                        {
                            _spriteBatch.DrawString(_titleFont,
                                "Congratulations!",
                                new Vector2(40, 20),
                                Color.DarkRed);

                            _spriteBatch.DrawString(_menuFont,
                                string.Format("Level {0} Complete!",
                                    _levelNumber),
                                new Vector2(155, 90),
                                Color.DarkGoldenrod);
                        }
                        else
                        {
                            _spriteBatch.DrawString(_titleFont,
                                "The Town Is Saved!",
                                new Vector2(35, 20),
                                Color.DarkRed);

                            _spriteBatch.DrawString(_menuFont,
                                "Game Complete!",
                                new Vector2(155, 90),
                                Color.DarkGoldenrod);
                        }


                    }
                    else
                    {
                        _spriteBatch.DrawString(_titleFont,
                            "Try Again...",
                            new Vector2(155, 20),
                            Color.DarkRed);

                        _spriteBatch.DrawString(_menuFont,
                            string.Format("Level {0} unsuccessful",
                                _levelNumber),
                            new Vector2(155, 90),
                            Color.DarkGoldenrod);
                    }

                    if (gameTime.TotalGameTime - _stateStartTime > TimeSpan.FromSeconds(1))
                    {
                        _spriteBatch.DrawString(_menuFont,
                            string.Format("Spiders: {0}/{1}\nFire Ants:{2}/{3}",
                                _spidersKilled, _spiderKillsRequired,
                                _fireAntsKilled, _fireAntKillsRequired),
                            new Vector2(155, 170),
                            Color.DarkRed);
                    }

                    if (gameTime.TotalGameTime - _stateStartTime > TimeSpan.FromSeconds(2))
                    {
                        _spriteBatch.DrawString(_menuFontSmall,
                            "Click to continue",
                            new Vector2(180, 250),
                            Color.DarkGoldenrod);
                    }
                    break;

                default:
                    break;
            }

            base.Draw(gameTime);

            _spriteBatch.End();

            _fadeBox.Draw(gameTime);
        }


        void SpawnPowerup(GameTime gameTime)
        {
            _powerup.Spawn(gameTime);

            ResetPowerupSpawn(gameTime);
        }

        void ResetPowerupSpawn(GameTime gameTime)
        {
            _nextPowerupSpawn = gameTime.TotalGameTime +
                TimeSpan.FromSeconds(NextDouble() * _powerupSpawnTimeRange + _powerupSpawnTimeMin);
        }


        void PlayMenuMusic()
        {
            PlayCue(ref _menuMusic);
        }


        void PlayLevelMusic()
        {
            PlayCue(ref _levelMusic);
        }


        void PlayEndWinMusic()
        {
            PlayCue(ref _endWinMusic);
        }

        void PlayEndLoseMusic()
        {
            PlayCue(ref _endLoseMusic);
        }


        void PlayCue(ref Cue cue)
        {
            if (cue.IsStopped || cue.IsStopping)
            {
                string name = cue.Name;

                cue.Dispose();
                cue = _soundBank.GetCue(name);
            }

            if (!cue.IsPlaying)
                cue.Play();
        }


        public void PlayCue(string name)
        {
            _soundBank.PlayCue(name);
        }

        public double NextDouble()
        {
            return _rnd.NextDouble();
        }

        public int NextRandomInt(int max)
        {
            return _rnd.Next(max);
        }

        bool MousePressedOverViewport(MouseState mouseState)
        {
            bool result = false;

            if (mouseState.LeftButton == ButtonState.Pressed &&
                mouseState.X >= 0 && mouseState.X <= GraphicsDevice.Viewport.Width &&
                mouseState.Y >= 0 && mouseState.Y <= GraphicsDevice.Viewport.Height)
            {
                result = true;
            }

            return result;
        }
    }
}