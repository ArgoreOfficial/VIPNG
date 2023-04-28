using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using VIPNG.Physics.Constraints;

namespace VIPNG.Physics
{
    public class Bone
    {
        // physics

        Bone _parentBone;

        Vector2 _rootPosition;
        Vector2 _rootOffset;
        Vector2 _tipPosition;
        Vector2 _tipPreviousPosition;
        Vector2 _tipAcceleration;

        float _rootAngle = 0f;

        List<Constraint> _constraints = new List<Constraint>();

        float _damping;
        float _baseBoneLength; // used for texture scaling

        // graphics

        Texture2D _texture;
        Vector2 _textureOrigin;
        float _textureAngle = 0f;
        

        public Vector2 RootPosition { get => _rootPosition; }
        public Vector2 TipPosition { get => _tipPosition; }
        public Vector2 RealTipPosition { get => _tipPosition + _rootPosition; }
        public float Angle { get => _tipPosition.Angle(); }
        public float Length { get => _tipPosition.Length(); }

        public float RootAngle { get => _rootAngle; }

        public Bone(Vector2 rootPosition, Vector2 rootOffset, float angle, float length, float stiffness, float angularStiffness, float damping)
        {
            _rootPosition = rootPosition;
            
            _damping = damping;
            _baseBoneLength = length;

            _tipPosition = new Vector2(
                    MathF.Cos(angle) * length,
                    MathF.Sin(angle) * length
                );

            _tipPreviousPosition = _tipPosition;

            _constraints.Add(new RotationConstraint(angle, angularStiffness));
            _constraints.Add(new LengthConstraint(length, stiffness));
        }

        /// <summary>
        /// Create Bone from BoneData
        /// </summary>
        /// <param name="boneData"></param>
        public Bone(BoneData boneData) : this(
            boneData.Position, 
            boneData.Offset, 
            boneData.TargetAngle, 
            boneData.TargetLength, 
            boneData.Stiffness, 
            boneData.AngularStiffness, 
            boneData.Damping) 
        {
            
        }

        public void SetParent(Bone bone)
        {
            _parentBone = bone;
            
            SetPosition(_parentBone.RealTipPosition);
            SetAngle(_parentBone.Angle);

            Vector2 targetAnglePos = new Vector2(
                MathF.Cos(Angle + _rootAngle),
                MathF.Sin(Angle + _rootAngle));

            float rotate = Vector2.Normalize(_tipPosition).AngleTo(targetAnglePos);
            _tipPosition = _tipPosition.Rotate(-rotate);
            _tipPreviousPosition = _tipPosition;
        }

        public BoneData ToBoneData()
        {
            BoneData boneData = new BoneData(_rootPosition, _rootOffset, 0, 0, 0, 0, _damping);
            return boneData;
        }

        public void LoadTexture(Texture2D texture, Vector2 origin, float angle = 0f)
        {
            _texture = texture;
            _textureOrigin = origin;
            _textureAngle = angle;
        }

        public Bone GetParent()
        {
            return _parentBone;
        }


        #region UPDATE

        public void Update(GameTime gameTime, int physicsIterations)
        {
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            //float deltaTime = (1f / 60f) / physicsIterations;
            if (_parentBone != null) SetAngle(_parentBone.Angle);
            if (_parentBone != null) SetPosition(_parentBone.RealTipPosition);
            
            for (int i = 0; i < physicsIterations; i++)
            {
                for (int c = 0; c < _constraints.Count; c++)
                {
                    _tipPosition = _constraints[c].GetUpdatedTipPosition(this, deltaTime, physicsIterations);
                }

                TickPosition(deltaTime, physicsIterations);
            }
        }


        void TickPosition(float deltaTime, int physicsIterations)
        {
            Vector2 positionCopy = _tipPosition;
            _tipPosition += (_tipPosition - _tipPreviousPosition) + (deltaTime * deltaTime) * _tipAcceleration / 1;
            _tipPreviousPosition = positionCopy;

            // damping
            _tipAcceleration = (_tipPreviousPosition - _tipPosition) * (_damping / 1);
        }

        #endregion

        #region SET

        public void SetAcceleration(Vector2 acceleration)
        {
            _tipAcceleration = acceleration;
        }

        public void SetAcceleration(Vector2 position, float strength)
        {
            _tipAcceleration = Vector2.Normalize(position - _rootPosition - _tipPosition) * strength;
        }


        public void SetPosition(Vector2 position)
        {
            Vector2 relative = _rootPosition - position;
            _tipPosition += relative;
            _tipPreviousPosition += relative;

            _rootPosition = position;
            
        }

        public void SetAngle(float angle)
        {
            _rootAngle = angle;
        }

        /*
        public void LookAt(Vector2 position)
        {
            Vector2 relative = position - _rootPosition;
            _targetAngle = relative.Angle() - _rootAngle;
        }
        */

        #endregion

        #region DRAWING

        public void Draw(SpriteBatch spriteBatch)
        {
            if (_texture == null) return;

            float angle = _tipPosition.Angle();
            spriteBatch.Draw(
                _texture,
                new Rectangle(
                    (int)_rootPosition.X,
                    (int)_rootPosition.Y,
                    (int)(_texture.Width * (Length / _baseBoneLength)),
                    _texture.Height),
                new Rectangle(
                    0,
                    0,
                    _texture.Width,
                    _texture.Height),
                Color.White,
                angle + _textureAngle,
                _textureOrigin,
                SpriteEffects.None, 0f);
        }

        public void DrawWire(SpriteBatch spriteBatch, Color rootColor, Color tipColor, Color lineColor)
        {
            spriteBatch.DrawLine(_rootPosition, _rootPosition + _tipPosition, lineColor);
            
            spriteBatch.DrawCircle(_rootPosition, 5, 16, rootColor);
            spriteBatch.DrawCircle(_rootPosition + _tipPosition, 2, 16, tipColor, 2);
        }

        #endregion
    }
}
