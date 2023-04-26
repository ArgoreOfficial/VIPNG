using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using NAudio.CoreAudioApi;
using System;
using System.Diagnostics;
using VIPNG.Physics;

namespace VIPNG
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        MMDevice _mic;

        VIPNGModel _model = new VIPNGModel();

        MouseState _mouseState;
        MouseState _previousMouseState;

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

            foreach (var device in devices)
            {

                if (device.FriendlyName.Contains("CABLE"))
                {
                    Debug.WriteLine("Found " + device.FriendlyName);

                    _mic = device;
                    break;
                }
                
            }


            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _model.Load(
                    Content.Load<Texture2D>("lower"),
                    Content.Load<Texture2D>("hand")
                );
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            
            MouseState();

            float micVolume = MathF.Min(_mic.AudioMeterInformation.MasterPeakValue * 5, 1f);

            _model.ReactRoot(MathF.Sin(((float)gameTime.TotalGameTime.TotalSeconds * 15) + 1) / 2 * micVolume * 40);

            if(GetLeftDown())
            {
                _model.Select(_mouseState.Position.ToVector2());
            }
            if (GetRightDown())
            {
                _model.AddBoneToSelected(_mouseState.Position.ToVector2());
            }

            //bone.SetAngle(-0.785398163f + MathF.Sin((float)gameTime.TotalGameTime.TotalSeconds * 4) * 0.5f);
            _model.Update(gameTime);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            _spriteBatch.Begin();

            _model.Draw(_spriteBatch);

            if(Keyboard.GetState().IsKeyDown(Keys.LeftShift)) _model.DrawWire(_spriteBatch);

            _spriteBatch.End();

            base.Draw(gameTime);
        }

        void MouseState()
        {
            _previousMouseState = _mouseState;
            _mouseState = Mouse.GetState();
        }

        bool GetLeftDown()
        {
            return _mouseState.LeftButton == ButtonState.Pressed && _previousMouseState.LeftButton != ButtonState.Pressed;
        }
        bool GetRightDown()
        {
            return _mouseState.RightButton == ButtonState.Pressed && _previousMouseState.RightButton != ButtonState.Pressed;
        }
    }
}