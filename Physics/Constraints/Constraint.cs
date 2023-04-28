using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VIPNG.Physics.Constraints
{
    public abstract class Constraint
    {
        public abstract Vector2 GetUpdatedTipPosition(Bone bone, float deltaTime, int physicsIterations);
    }
}
