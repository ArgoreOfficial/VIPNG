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

        MouseState _mouseState;
        MouseState _previousMouseState;
        Vector2 _mousePanStart;
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
                if (device.FriendlyName.Contains("Mikrofon"))
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
        }

        protected override void Update(GameTime gameTime)
        {

            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            
            MouseState();

            float micVolume = 0;
            if (_mic != null) micVolume = _mic.AudioMeterInformation.MasterPeakValue * 100;
            
            //_model.ReactRoot(MathF.Sin(((float)gameTime.TotalGameTime.TotalSeconds * 15) + 1) / 2 * micVolume * 40);

            //_model.ReactRoot(micVolume * -100);
            
            // select bone
            if(GetLeftDown())
            {
                _model.Select(_mouseState.Position.ToVector2());
            }

            // add bone
            if (GetRightDown())
            {
                _model.AddBoneToSelected(_mouseState.Position.ToVector2());
            }

            // add bone
            if(_mouseState.MiddleButton == ButtonState.Pressed)
            {
                Vector2 relative = _mouseState.Position.ToVector2() - _previousMouseState.Position.ToVector2();
                _model.MoveRoot(relative);
            }


            if (Keyboard.GetState().IsKeyUp(Keys.Space))  _model.Update(gameTime);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            _spriteBatch.Begin();

            
            if(Keyboard.GetState().IsKeyDown(Keys.LeftShift)) _model.Draw(_spriteBatch);
            else _model.DrawWire(_spriteBatch);

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
        bool GetMiddleDown()
        {
            return _mouseState.MiddleButton == ButtonState.Pressed && _previousMouseState.MiddleButton != ButtonState.Pressed;
        }
    }
}