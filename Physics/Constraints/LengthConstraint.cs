using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VIPNG.Physics.Constraints
{
    public class LengthConstraint : Constraint
    {
        float _baseTargetLength = 16;
        float _length = 16;
        float _targetLength = 16;
        float _keyframeTargetLength = 16;
        float _stiffness;

        public float Stiffness { get => _stiffness; }
        public float TargetLength { get => _targetLength; set => _targetLength = value; }
        public float KeyframeTargetLength { get => _keyframeTargetLength; }
        public float RestingTargetLength { get => _baseTargetLength; set => _baseTargetLength = value; }

        public LengthConstraint(float targetLength, float keyframeTargetLength, float stiffness)
        {
            _baseTargetLength = targetLength;
            _targetLength = targetLength;
            if (targetLength != keyframeTargetLength) _responsive = true;
            _keyframeTargetLength = keyframeTargetLength;

            _length = targetLength;
            _stiffness = stiffness;
        }
        public LengthConstraint(float targetLength, float stiffness) : this(targetLength, targetLength, stiffness) { }

        public void SetNewLength(float length)
        {
            _baseTargetLength = length;
            _targetLength = length;
            _length = length;

            // set response length
            if(!_responsive)
            {
                _keyframeTargetLength = length;
            }
        }

        public override Vector2 GetUpdatedTipPosition(Bone bone, bool isRigid)
        {
            _length = bone.TipPosition.Length();

            Vector2 move = Vector2.Normalize(bone.TipPosition) * (_targetLength - _length);
            Vector2 newPos = Vector2.Lerp(bone.TipPosition, bone.TipPosition + move, (isRigid ? 1 : _stiffness));
            return newPos;
        }

        public override void InterpolateKeyframeTarget(float amount)
        {
            if (!_responsive) return;

            _targetLength = MathHelper.Lerp(_baseTargetLength, _keyframeTargetLength, amount);
        }
    }
}
