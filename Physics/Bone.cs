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

        float _rootAngle = 0f;
        float _tipAngle = 0f;

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
        public Vector2 Center { get => (_rootPosition + _rootPosition + _tipPosition) / 2; }
        public float Angle { get => _tipPosition.Angle(); }
        public float Length { get => _tipPosition.Length(); }
        public float RootAngle { get => _rootAngle; }
        public float TipAngle { get => _tipAngle; }

        public Bone(Vector2 rootPosition, float angle, float length, float damping)
        {
            _rootPosition = rootPosition;
            
            _damping = damping;
            _baseBoneLength = length;

            _tipPosition = new Vector2(
                    MathF.Cos(angle) * length,
                    MathF.Sin(angle) * length
                );

            _constraints.Add(new AngleConstraint(angle, 0.1f));
            _constraints.Add(new LengthConstraint(length, 0.9f));
        }

        public void SetConstraints(float length, float responseLength, float stiffness, float angle, float responseAngle, float angularStiffness)
        {
            _baseBoneLength = length;
            _constraints[0] = new AngleConstraint(angle, responseAngle, angularStiffness);
            _constraints[1] = new LengthConstraint(length, responseLength, stiffness);
        }

        /// <summary>
        /// Create Bone from BoneData
        /// </summary>
        /// <param name="boneData"></param>
        public Bone(BoneData boneData) : this(
            boneData.Position, 
            boneData.TargetAngle, 
            boneData.TargetLength, 
            boneData.Damping) 
        {
            
        }

        public void SetParent(Bone bone)
        {
            _parentBone = bone;
            if (bone == null) return;

            _rootPosition = _parentBone.RealTipPosition;
            _rootAngle = _parentBone.Angle;

            Vector2 targetAnglePos = new Vector2(
                MathF.Cos(Angle + _rootAngle),
                MathF.Sin(Angle + _rootAngle));

            float rotate = Vector2.Normalize(_tipPosition).AngleTo(targetAnglePos);
            _tipPosition = _tipPosition.Rotate(-rotate);
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

        public void Update(GameTime gameTime, bool isRigid, float responseAmount)
        {
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            
            if (_parentBone != null)
            {
                _rootAngle = _parentBone.Angle + _parentBone.TipAngle;
            }

            if (_parentBone != null)
            {
                Vector2 relative = _parentBone.RealTipPosition - _rootPosition;
                _tipPosition -= relative;
                _rootPosition += relative;
            }
            
            for (int c = 0; c < _constraints.Count; c++)
            {
                _tipPosition = _constraints[c].GetUpdatedTipPosition(this, isRigid);
                _constraints[c].InterpolateKeyframeTarget(MathHelper.Clamp(responseAmount, -1, 1));
            }   
        }

        #endregion

        #region SET

        public void SetRootPosition(Vector2 position)
        {
            MoveRoot(position - _rootPosition); 
        }

        public void MoveRoot(Vector2 direction)
        {
            if (_parentBone != null) _parentBone.MoveTip(direction);
            else _rootPosition += direction;

            //MoveTip(-direction);
        }

        public void SetTipPosition(Vector2 position)
        {
            MoveTip(_rootPosition - position);
        }

        public void MoveTip(Vector2 direction, bool lengthOnly = false, bool angleOnly = false)
        {
            // move tip position
            _tipPosition += direction;

            if(!angleOnly)
            {
                // get old length 
                LengthConstraint length = (LengthConstraint)_constraints.First(c => c is LengthConstraint);

                // set length
                ((LengthConstraint)_constraints[_constraints.IndexOf(length)]).SetNewLength(_tipPosition.Length());
                _baseBoneLength = length.TargetLength;
            }
            if(!lengthOnly)
            {
                // set new bone angle
                float newAngle = _tipPosition.Angle() - RootAngle; // new angle of bone

                AngleConstraint angle = (AngleConstraint)_constraints.First(c => c is AngleConstraint);
            
                float oldAngle = angle.TargetAngle;
            
                // set the new angle
                ((AngleConstraint)_constraints[_constraints.IndexOf(angle)]).SetNewAngle(newAngle);

                _tipAngle += oldAngle - newAngle;
            }
        }

        #endregion

        #region DRAWING

        public void Draw(SpriteBatch spriteBatch, float alpha)
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
                Color.White * alpha,
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
