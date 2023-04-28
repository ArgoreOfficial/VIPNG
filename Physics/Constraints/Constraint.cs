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
        protected bool _responsive = false;
        public abstract Vector2 GetUpdatedTipPosition(Bone bone, bool isRigid);
        public abstract void InterpolateKeyframeTarget(float amount);
    }
}
