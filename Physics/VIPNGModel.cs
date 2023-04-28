using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VIPNG.Physics
{
    public class VIPNGModel
    {
        List<Bone> _bones = new List<Bone>();

        Texture2D _section;
        Texture2D _end;

        int _selected = -1;

        public VIPNGModel()
        {
            AddBone(new Vector2(400, 400), new Vector2(400, 230));
        }

        public void MoveRoot(Vector2 direction)
        {
            _bones[0].SetPosition(_bones[0].RootPosition + direction);
        }

        public void Load(Texture2D section, Texture2D end)
        {
            _section = section;
            _end = end;
        }

        public void AddBone(Vector2 rootPosition, Vector2 tipPosition)
        {
            Vector2 relative = tipPosition - rootPosition;

            Bone newBone = new Bone(
                    rootPosition, //position
                    Vector2.Zero, // offset
                    relative.Angle(), relative.Length(), // angle and length 
                    1f, 1f, 100 // stiffness and damping
                    );

            _bones.Add( newBone );
        }

        public void AddBoneToSelected(Vector2 tipPosition)
        {
            if (_selected < 0) return;

            Vector2 relative = tipPosition - _bones[_selected].RealTipPosition;

            Bone newBone = new Bone(
                    _bones[_selected].RootPosition, //position
                    Vector2.Zero, // offset
                    relative.Angle() - _bones[_selected].Angle,
                    MathF.Min(relative.Length(), 170), // angle and length 
                    1f, 1f, 100 // stiffness and damping
                    );

            newBone.SetParent(_bones[_selected]);

            newBone.LoadTexture(_end, new Vector2(23, 70));

            _bones.Add( newBone );

            _bones[_selected].LoadTexture(_section, new Vector2(50, 30));

            _selected = _bones.Count - 1;
        }

        public BoneData BoneToBoneData(Bone bone)
        {
            BoneData data = bone.ToBoneData();
            Bone parent = bone.GetParent();
            if (parent != null)
            {
                int parentID = _bones.IndexOf(parent);
                data.ParentID = parentID;
            }

            return data;
        }

        public void Select(Vector2 mousePosition)
        {
            float closestDistance = 30f;
            Deselect();

            for (int i = 0; i < _bones.Count; i++)
            {
                float distance = (_bones[i].RealTipPosition - mousePosition).Length();
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    _selected = i;
                }
            }

        }

        public void Deselect()
        {
            _selected = -1;
        }

        public void Update(GameTime gt)
        {
            for (int i = 0; i < _bones.Count; i++)
            {
                _bones[i].Update(gt, 200);
            }
        }

        public void Draw(SpriteBatch sb)
        {
            for (int i = 0; i < _bones.Count; i++)
            {
                _bones[i].Draw(sb);
            }
        }

        public void DrawWire(SpriteBatch sb)
        {
            for (int i = 0; i < _bones.Count; i++)
            {
                _bones[i].DrawWire(
                    sb, 
                    (i == _selected) ? Color.DarkRed : Color.White,
                    (i == _selected) ? Color.Red : Color.White,
                    (i == _selected) ? Color.Red : Color.White);

            }
        }
    }
}
