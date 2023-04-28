using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VIPNG.Physics.Constraints
{
    public class RotationConstraint : Constraint
    {
        float _targetAngle;
        float _stiffness;

        public RotationConstraint(float targetAngle, float stiffness)
        {
            _targetAngle = targetAngle;
            _stiffness = stiffness;
        }

        public override Vector2 GetUpdatedTipPosition(Bone bone, float deltaTime, int physicsIterations)
        {
            Vector2 targetAnglePos = new Vector2(
                MathF.Cos(_targetAngle + bone.RootAngle),
                MathF.Sin(_targetAngle + bone.RootAngle));

            float rotate = Vector2.Normalize(bone.TipPosition).AngleTo(targetAnglePos) * (_stiffness / physicsIterations) * deltaTime;
            return bone.TipPosition.Rotate(-rotate);
        }
    }
}
