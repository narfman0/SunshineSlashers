using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace SunshineSlashers
{
    public class GameObject
    {
        public Model model;
        Demo Game;
        public Vector3 Pos;
        public float Scale = 1.0f;
        BoundingBox Boundary;
        public float Rotation = 0.0f;

        public GameObject(Demo Game, Model model1)
        {
            this.Game = Game;
            Pos = new Vector3(0, 0, 0);
            model = model1;
        }
        public GameObject(Demo Game, Model model1, Vector3 StartPos, float scale)
            :this(Game, model1)
        {
            Pos = StartPos; Scale = scale;
            Boundary = Game.GetModelBB(model1);
            Boundary.Max *= scale;
            Boundary.Max += Pos;
            Boundary.Min *= scale;
            Boundary.Min += Pos;
            Rotation = 0.0f;
        }
        public void Draw(GameTime gameTime)
        {
            Matrix world;
            #region Draw bounding stuffs (if in debug mode)
#if DEBUG
            Vector3 tempPos = new Vector3(Pos.X, Pos.Y, Pos.Z);
            if (model == Game.HouseModel)
                tempPos.Z -= (Boundary.Max.Z - Boundary.Min.Z) / 2.0f;
            if (model == Game.Crate1Model || model == Game.Crate2Model || model == Game.Crate3Model || model == Game.Crate4Model || model == Game.Crate5Model)
                tempPos.Y -= (Boundary.Max.Y - Boundary.Min.Y) / 2.0f;
            world = Matrix.CreateScale(Boundary.Max.X - Boundary.Min.X, Boundary.Max.Y - Boundary.Min.Y, Boundary.Max.Z - Boundary.Min.Z) *
                Matrix.CreateTranslation(tempPos);
            Game.DrawModel(Game.UnitBox, world);
#endif
            #endregion
            #region Draw Object
            world = Matrix.CreateScale(Scale) * Matrix.CreateRotationY(Rotation);
            world *= Matrix.CreateTranslation(Pos);
            Game.DrawModel(model, world);
            #endregion
        }
        public float IsCollision(Ray ray)
        {
            if (Boundary.Intersects(ray).HasValue)
                return Boundary.Intersects(ray).Value;
            return 0;
        }
    }
}
