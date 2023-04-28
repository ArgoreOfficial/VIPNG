using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using System;
using System.Diagnostics;
using System.Reflection;
using VIPNG.Physics;

namespace VIPNG
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        MMDevice _mic;

        VIPNGModel _model = new VIPNGModel();
        bool _isInEditMode = false;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            MMDeviceEnumerator enm = new MMDeviceEnumerator();
            var devices = enm.EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active);

            Debug.WriteLine("  ----");
            foreach (var device in devices)
            {
                Debug.WriteLine(device.FriendlyName);
                if (device.FriendlyName.Contains("CABLE"))
                {
                    Debug.WriteLine("   Connected to " + device.FriendlyName);

                    _mic = device;
                    break;
                }
                
            }
            Debug.WriteLine("  ----");

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _model.Load(
                    Content.Load<Texture2D>("lower"),
                    Content.Load<Texture2D>("hand")
                );

            _model.AddBone(new Vector2(400, 400), new Vector2(400, 230));
            _model.Bones[0].SetConstraints(170, 170, 1f, MathHelper.ToRadians(-90), MathHelper.ToRadians(-60), 1f);
        }

        protected override void Update(GameTime gameTime)
        {
            PiInput.Update();

            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            if(PiInput.IsKeyPressed(Keys.Tab, true))
            {
                _isInEditMode = !_isInEditMode;
            }

            if (_isInEditMode)
            {
                EditMode();
            }

            _model.Update(gameTime, _isInEditMode, _isInEditMode ? 0 : MicResponse(gameTime));

            base.Update(gameTime);
        }

        float MicResponse(GameTime gameTime)
        {
            float micVolume = 0;
            if (_mic != null) micVolume = MathF.Sin((float)gameTime.TotalGameTime.TotalSeconds * 20) * _mic.AudioMeterInformation.MasterPeakValue * 10;

            //Debug.WriteLine(micVolume);

            return micVolume;
        }

        void EditMode()
        {
            // select bone
            if (PiInput.LeftJustPressed())
            {
                _model.TrySelect(PiInput.CurrentMouseState.Position.ToVector2());
            }

            // add bone
            if (PiInput.RightJustPressed())
            {
                _model.AddBoneToSelected(PiInput.CurrentMouseState.Position.ToVector2());
            }

            // move
            if (PiInput.CurrentMouseState.MiddleButton == ButtonState.Pressed)
            {
                if (PiInput.IsKeyPressed(Keys.LeftShift))
                {
                    _model.MoveSelectedRoot(PiInput.GetMouseDelta());
                }
                else
                {
                    if (PiInput.IsKeyPressed(Keys.LeftControl))
                    {
                        _model.MoveSelectedTip(PiInput.GetMouseDelta(), true, false);
                    }
                    else if (PiInput.IsKeyPressed(Keys.LeftAlt))
                    {
                        _model.MoveSelectedTip(PiInput.GetMouseDelta(), false, true);
                    }
                    else _model.MoveSelectedTip(PiInput.GetMouseDelta());
                }
            }

            if (PiInput.IsKeyPressed(Keys.Delete, true))
            {
                _model.RemoveSelected();
            }

        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            _spriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, null, null, null);

            if(_isInEditMode)
            {
                _model.Draw(_spriteBatch, 0.3f);
                _model.DrawWire(_spriteBatch);
            }
            else
            {
                //_model.Draw(_spriteBatch, 1f);
                _model.DrawWire(_spriteBatch);
            }

            _spriteBatch.End();

            base.Draw(gameTime);
        }

        void MouseState()
        {
            
        }


    }
}