using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using System;
using System.Diagnostics;
using System.Reflection;
using VIPNG.Physics;
using Myra;
using Myra.Graphics2D.UI;
using System.Collections.Generic;

namespace VIPNG
{
    public class Game1 : Game
    {
        private Desktop _mainUI;
        private Desktop _boneUI;

        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        // mic
        MMDevice _mic;
        List<string> _micNames = new List<string>();
        float _micVolume = 10;

        // model
        VIPNGModel _model = new VIPNGModel();
        bool _isInEditMode = false;
        bool _isPaused = false;
        /*
                texture
                length
                angle
                volume
                sensitivity
                reactive length
                reactive angle
            */
        // ui
        HorizontalSlider _boneVolume;

        SpinButton _boneLength;
        SpinButton _boneAngle;
        SpinButton _targetBoneLength;
        SpinButton _targetBoneAngle;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            SetAudioDevice("asdadsas");

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

            LoadMyra();
        }

        #region MYRA_UI

        void LoadMyra()
        {
            MyraEnvironment.Game = this;

            _mainUI = new Desktop();
            _boneUI = new Desktop();
            
            _mainUI.Root = LoadMyraNormal();
            _boneUI.Root = LoadMyraBone();

        }

        Grid LoadMyraNormal()
        {
            var grid = new Grid
            {
                RowSpacing = 8,
                ColumnSpacing = 8,
                Padding = new Myra.Graphics2D.Thickness(12)
            };

            grid.ColumnsProportions.Add(new Proportion(ProportionType.Auto));
            grid.ColumnsProportions.Add(new Proportion(ProportionType.Auto));
            grid.RowsProportions.Add(new Proportion(ProportionType.Auto));
            grid.RowsProportions.Add(new Proportion(ProportionType.Auto));

            // mic device
            var device = new ComboBox
            {
                GridColumn = 0,
                GridRow = 0
            };

            for (int i = 0; i < _micNames.Count; i++)
            {
                device.Items.Add(new ListItem(_micNames[i]));
            }

            device.SelectedIndexChanged += (s, a) =>
            {
                SetAudioDevice(device.SelectedItem.Text);
            };

            device.SelectedIndex = 0;
            grid.Widgets.Add(device);



            // mic volume grid
            Grid volumeGrid = LabeledGrid("Volume", 1, 0);
            // mic volume slier
            var volumeSlider = new HorizontalSlider
            {
                GridColumn = 1,
                Width = 200,
                Value = 10f,
                Minimum = 0,
                Maximum = 30
            };
            volumeSlider.ValueChanged += (s, a) =>
            {
                _micVolume = volumeSlider.Value;
            };

            _micVolume = volumeSlider.Value;

            volumeGrid.Widgets.Add(volumeSlider);
            grid.Widgets.Add(volumeGrid);


            // paused checkbox
            CheckBox paused = new CheckBox
            {
                GridRow = 2,
                Text = "Pause Reaction",
                IsChecked = false
            };

            paused.Click += (s, a) =>
            {
                _isPaused = paused.IsChecked;
            };

            grid.Widgets.Add(paused);

            return grid;
        }

        Grid LoadMyraBone()
        {

            Grid boneGrid = new Grid
            {
                RowSpacing = 4,
                ColumnSpacing = 4,
                HorizontalAlignment = HorizontalAlignment.Right,
                Padding = new Myra.Graphics2D.Thickness(12)
            };
            boneGrid.ColumnsProportions.Add(new Proportion(ProportionType.Auto));
            boneGrid.ColumnsProportions.Add(new Proportion(ProportionType.Auto));
            boneGrid.RowsProportions.Add(new Proportion(ProportionType.Auto));
            boneGrid.RowsProportions.Add(new Proportion(ProportionType.Auto));
            boneGrid.RowsProportions.Add(new Proportion(ProportionType.Auto));
            boneGrid.RowsProportions.Add(new Proportion(ProportionType.Auto));



            // resting values

            Grid lengthGrid = LabeledGrid("Length", 1, 0, HorizontalAlignment.Right);
            _boneLength = new SpinButton
            {
                Width = 80,
                GridRow = 0,
                GridColumn = 1
            };
            _boneLength.ValueChanged += (s, e) =>
            {
                if (_boneLength.Value != null && _boneLength.Value != 0) _model.SelectedBone.SetLength((float)_boneLength.Value);
            };
            lengthGrid.Widgets.Add(_boneLength);


            Grid angleGrid = LabeledGrid("Angle", 2, 0, HorizontalAlignment.Right);
            _boneAngle = new SpinButton
            {
                Width = 80,
                GridColumn = 1,
                Increment = 1f
            };
            _boneAngle.ValueChanged += (s, e) =>
            {
                if(_boneAngle.Value != null) _model.SelectedBone.SetAngle(MathHelper.ToRadians((float)_boneAngle.Value));
            };
            angleGrid.Widgets.Add(_boneAngle);


            // target values


            Grid targetLengthGrid = LabeledGrid("Target Length", 3, 0, HorizontalAlignment.Right);
            _targetBoneLength = new SpinButton
            {
                Width = 80,
                GridRow = 0,
                GridColumn = 1,
            };
            _targetBoneLength.ValueChanged += (s, e) =>
            {
                if (_targetBoneLength.Value != null && _targetBoneLength.Value != 0)
                {
                    _model.SelectedBone.UpdateConstraints(
                        (float)_targetBoneLength.Value,
                        MathHelper.ToRadians((float)_targetBoneAngle.Value));
                }
            };
            targetLengthGrid.Widgets.Add(_targetBoneLength);


            Grid targetAngleGrid = LabeledGrid("Target Angle", 4, 0, HorizontalAlignment.Right);
            _targetBoneAngle = new SpinButton
            {
                Width = 80,
                GridColumn = 1,
                Increment = 1f
            };
            _targetBoneAngle.ValueChanged += (s, e) =>
            {
                if (_targetBoneAngle.Value != null)
                {
                    _model.SelectedBone.UpdateConstraints(
                        (float)_targetBoneLength.Value,
                        MathHelper.ToRadians((float)_targetBoneAngle.Value));
                }
            };
            targetAngleGrid.Widgets.Add(_targetBoneAngle);

            // add stuff
            boneGrid.Widgets.Add(lengthGrid);
            boneGrid.Widgets.Add(angleGrid);
            boneGrid.Widgets.Add(targetLengthGrid);
            boneGrid.Widgets.Add(targetAngleGrid);


            return boneGrid;
        }

        Grid LabeledGrid(string text, int row, int column, HorizontalAlignment alignment = HorizontalAlignment.Left)
        {
            Grid grid = new Grid
            {
                RowSpacing = 4,
                ColumnSpacing = 4,
                GridColumn = column,
                GridRow = row,
                HorizontalAlignment = alignment
            };
            grid.ColumnsProportions.Add(new Proportion(ProportionType.Auto));
            grid.RowsProportions.Add(new Proportion(ProportionType.Auto));

            var label = new Label
            {
                Text = text
            };
            grid.Widgets.Add(label);

            return grid;
        }

        #endregion

        #region NAUDIO

        public void SetAudioDevice(string deviceName)
        {
            MMDeviceEnumerator enm = new MMDeviceEnumerator();
            var devices = enm.EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active);

            foreach (var device in devices)
            {
                if(!_micNames.Contains(device.FriendlyName)) _micNames.Add(device.FriendlyName);

                if (device.FriendlyName == deviceName || device.FriendlyName.Contains(deviceName))
                {
                    _mic = device;
                    break;
                }
            }

        }
        float MicResponse(GameTime gameTime)
        {
            float micVolume = 0;
            /*if (_mic != null) micVolume = 
                    MathF.Sin((float)gameTime.TotalGameTime.TotalSeconds * 20) 
                    * (_mic.AudioMeterInformation.MasterPeakValue * _micVolume);
            */
            if (_mic != null) micVolume = _mic.AudioMeterInformation.MasterPeakValue * _micVolume;


            //Debug.WriteLine(micVolume);

            return micVolume;
        }


        #endregion

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

            _model.Update(gameTime, _isPaused, _isPaused ? 0 : MicResponse(gameTime));


            if(_model.SelectedBone != null)
            {
                // angle
                if(!_boneAngle.IsKeyboardFocused && _boneAngle.Value != MathHelper.ToDegrees(_model.SelectedBone.RestingAngle)) 
                    _boneAngle.Value = MathHelper.ToDegrees(_model.SelectedBone.RestingAngle);
                // length
                if(!_boneLength.IsKeyboardFocused && _boneLength.Value != _model.SelectedBone.RestingLength)
                    _boneLength.Value = _model.SelectedBone.RestingLength;
                // target angle
                if (!_targetBoneAngle.IsKeyboardFocused && _targetBoneAngle.Value != MathHelper.ToDegrees(_model.SelectedBone.TargetAngle))
                    _targetBoneAngle.Value = MathHelper.ToDegrees(_model.SelectedBone.TargetAngle);
                // target length
                if (!_targetBoneLength.IsKeyboardFocused && _targetBoneLength.Value != _model.SelectedBone.TargetLength)
                    _targetBoneLength.Value = _model.SelectedBone.TargetLength;
            }


            base.Update(gameTime);
        }

        void EditMode()
        {
            if (_boneUI.IsMouseOverGUI) return;
            if (_mainUI.IsMouseOverGUI) return;

            // select bone
            if (PiInput.LeftJustPressed())
            {
                _model.TrySelect(PiInput.CurrentMouseState.Position.ToVector2());
            }

            // add bone
            if (PiInput.RightJustPressed())
            {
                if (_model.SelectedBone != null) _model.AddBoneToSelected(PiInput.CurrentMouseState.Position.ToVector2());
                else _model.AddBone(
                    PiInput.CurrentMouseState.Position.ToVector2(),
                    PiInput.CurrentMouseState.Position.ToVector2() + new Vector2(262, 0));
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
                _mainUI.Render();
                if (_model.SelectedBone != null) _boneUI.Render();
            }
            else
            {
                //_model.Draw(_spriteBatch, 1f);
                _model.DrawWire(_spriteBatch);
            }

            _spriteBatch.End();


            base.Draw(gameTime);
        }

    }
}