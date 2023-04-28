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
        float _length = 16;
        float _targetLength = 16;
        float _stiffness;

        public LengthConstraint(float targetLength, float stiffness)
        {
            _targetLength = targetLength;
            _length = _targetLength;
            _stiffness = stiffness;
        }

        public override Vector2 GetUpdatedTipPosition(Bone bone, float deltaTime, int physicsIterations)
        {
            _length = bone.TipPosition.Length();

            Vector2 move = Vector2.Normalize(bone.TipPosition) * (_targetLength - _length) * (_stiffness / physicsIterations) * deltaTime;
            return bone.TipPosition + move;
        }
    }
}
