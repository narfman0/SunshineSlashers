using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using XNAnimation;
using XNAnimation.Controllers;
using XNAnimation.Effects;

namespace SunshineSlashers
{
    class AnimatedModel
    {
        const float SCALE = 5.0f;
        public string CurrentAnimation;
        private Matrix[] finalBoneTransform;
        private SkinnedModel skinnedModel;
        private AnimationController animationController;
        private bool enableNormalTexture = false;
        private Demo Game1;
        public AnimatedModel(Demo Game)
        {
            Game1 = Game;
            LoadContent();
        }
        public void Draw(GameTime gameTime, FirstPersonCamera camera)
        {
            foreach (ModelMesh modelMesh in skinnedModel.Model.Meshes)
            {
                foreach (ModelMeshPart meshPart in modelMesh.MeshParts)
                {
                    XNAnimation.Effects.SkinnedModelBasicEffect basicEffect = (XNAnimation.Effects.SkinnedModelBasicEffect)meshPart.Effect;
                    Vector3 RenderPos = camera.Position;
                    RenderPos.Y -= 110.0f;
                    basicEffect.World = Matrix.CreateScale(SCALE, SCALE, SCALE) *
                        Matrix.CreateRotationY((float)Math.PI + (float)Math.PI*camera.HeadingDegrees/180.0f) *
                        Matrix.CreateTranslation(RenderPos);
                    basicEffect.Bones = animationController.SkinnedBoneTransforms;
                    basicEffect.View = Game1.Player.camera.ViewMatrix;
                    basicEffect.Projection = Game1.Player.camera.ProjectionMatrix;

                    // OPTIONAL - Configure material
                    basicEffect.Material.DiffuseColor = new Vector3(0.8f);
                    basicEffect.Material.SpecularColor = new Vector3(0.3f);
                    basicEffect.Material.SpecularPower = 8;
                    basicEffect.NormalMapEnabled = enableNormalTexture;
                    basicEffect.SpecularMapEnabled = false;

                    // OPTIONAL - Configure lights
                    basicEffect.AmbientLightColor = new Vector3(0.1f);
                    basicEffect.LightEnabled = true;
                    basicEffect.EnabledLights = XNAnimation.Effects.EnabledLights.One;
                    basicEffect.PointLights[0].Color = Vector3.One;
                    basicEffect.PointLights[0].Position = new Vector3(100, 100, 100);
                }
                modelMesh.Draw();
            }
        }
        private void LoadContent()
        {
            // Load the skinned model
            skinnedModel = Game1.Content.Load<SkinnedModel>(@"Models\PlayerMarine");
            finalBoneTransform = new Matrix[skinnedModel.SkeletonBones.Count];

            // Create an animation controller and start a clip
            animationController = new AnimationController(skinnedModel.SkeletonBones);
            SetAnimation("Idle");
        }
        public void SetAnimation(string ToAnimation)
        {
            animationController.StartClip(skinnedModel.AnimationClips[ToAnimation]);
            CurrentAnimation = ToAnimation;
        }
        public void Update(GameTime gameTime)
        {
            animationController.Update(gameTime.ElapsedGameTime, Matrix.Identity);
        }
    }
}
