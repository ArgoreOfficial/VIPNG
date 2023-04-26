using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VIPNG.Physics
{
    [Serializable]
    public struct BoneData
    {
        public Vector2 Position;
        public Vector2 Offset;
        public float TargetAngle;
        public float TargetLength;

        public float Stiffness;
        public float AngularStiffness;
        public float Damping;

        public int ParentID;

        public BoneData(Vector2 position, Vector2 offset, float targetAngle, float targetLength, float stiffness, float angularStiffness, float damping)
        {
            Position = position;
            Offset = offset;
            TargetAngle = targetAngle;
            TargetLength = targetLength;
            Stiffness = stiffness;
            AngularStiffness = angularStiffness;
            Damping = damping;
            ParentID = -1;
        }
    }
}
