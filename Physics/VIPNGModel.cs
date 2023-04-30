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

        public List<Bone> Bones { get => _bones; }
        public Bone SelectedBone { get => _selected < 0 ? null : _bones[_selected]; }

        public VIPNGModel()
        {
            
        }

        public void Load(Texture2D section, Texture2D end)
        {
            _section = section;
            _end = end;
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

        public void MoveSelectedRoot(Vector2 direction)
        {
            if (_selected < 0) return;

            _bones[_selected].MoveRoot(direction);
        }

        public void MoveSelectedTip(Vector2 direction, bool lengthOnly = false, bool angleOnly = false)
        {
            if (_selected < 0) return;

            _bones[_selected].MoveTip(direction, lengthOnly, angleOnly);
        }


        public void AddBone(Vector2 rootPosition, Vector2 tipPosition)
        {
            Vector2 relative = tipPosition - rootPosition;

            Bone newBone = new Bone(
                    rootPosition, //position
                    relative.Angle(), // angle 
                    relative.Length(), // length 
                    100 // damping
                    );
            
            newBone.LoadTexture(_section, new Vector2(50, 30));

            _bones.Add(newBone);
        }

        public void AddBoneToSelected(Vector2 tipPosition)
        {
            if (_selected < 0) return;

            Vector2 relative = tipPosition - _bones[_selected].RealTipPosition;

            Bone newBone = new Bone(
                    _bones[_selected].RootPosition, //position
                    relative.Angle() - _bones[_selected].Angle - _bones[_selected].TipAngle, // angle
                    relative.Length(), // length
                    100 // damping
                    );

            newBone.SetConstraints(
                relative.Length(),
                relative.Length(),
                1f,
                relative.Angle() - _bones[_selected].Angle - _bones[_selected].TipAngle,
                relative.Angle() - _bones[_selected].Angle - _bones[_selected].TipAngle,
                0.1f);
            newBone.SetParent(_bones[_selected]);

            newBone.LoadTexture(_end, new Vector2(23, 70));

            _bones.Add(newBone);

            _bones[_selected].LoadTexture(_section, new Vector2(50, 30));

            _selected = _bones.Count - 1;
        }

        public void RemoveSelected()
        {
            if (_selected < 0) return;

            for (int i = 0; i < _bones.Count; i++)
            {
                if (_bones[i].GetParent() == _bones[_selected])
                {
                    _bones[i].SetParent(null);
                }
            }

            _bones.RemoveAt(_selected);
            _selected = -1;
        }


        public void TrySelect(Vector2 mousePosition)
        {
            float closestDistance = 50f;
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

        // basic loops

        public void Update(GameTime gt, bool isRigid, float responseAmount)
        {
            for (int i = 0; i < _bones.Count; i++)
            {
                _bones[i].Update(gt, isRigid, responseAmount);
            }
        }

        public void Draw(SpriteBatch sb, float alpha)
        {
            for (int i = 0; i < _bones.Count; i++)
            {
                _bones[i].Draw(sb, alpha);
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
