using Microsoft.Xna.Framework;
using NAudio.SoundFont;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VIPNG.Physics.Constraints
{
    public class AngleConstraint : Constraint
    {
        float _restingAngle;
        float _targetAngle;
        float _keyframeTargetAngle;
        float _stiffness;

        public float Stiffness { get => _stiffness; }
        public float TargetAngle { get => _targetAngle; }
        public float KeyframeTargetAngle { get => _keyframeTargetAngle; }
        public float RestingAngle { get => _restingAngle; set => _restingAngle = value; }

        public AngleConstraint(float targetAngle, float keyframeTargetAngle, float stiffness)
        {
            _restingAngle = targetAngle;
            _targetAngle = targetAngle;
            _keyframeTargetAngle = keyframeTargetAngle;
            if (targetAngle != keyframeTargetAngle) _responsive = true;

            _stiffness = stiffness;
        }

        public AngleConstraint(float targetAngle, float stiffness) : this(targetAngle, targetAngle, stiffness) { }

        public void SetNewAngle(float angle)
        {
            _restingAngle = angle;
            _targetAngle = angle;
            
            // set response length
            if (!_responsive)
            {
                _keyframeTargetAngle = angle;
            }
        }

        public override Vector2 GetUpdatedTipPosition(Bone bone, bool isRigid)
        {
            Vector2 targetAnglePos = new Vector2(
                MathF.Cos(_targetAngle + bone.RootAngle),
                MathF.Sin(_targetAngle + bone.RootAngle));

            float rotate = Vector2.Normalize(bone.TipPosition).AngleTo(targetAnglePos) * (isRigid ? 1 : _stiffness);
            return bone.TipPosition.Rotate(-rotate);
        }

        public override void InterpolateKeyframeTarget(float amount)
        {
            if (!_responsive) return;

            _targetAngle = MathHelper.Lerp(_restingAngle, _keyframeTargetAngle, amount);
        }
    }
}
