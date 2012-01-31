using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace SunshineSlashers
{
    enum Guns{ Melee = 0, Deagle = 1, AK47 = 2, Shotty = 3, Panzerschreck = 4};
    public class Being
    {
        #region Declarations
        public Int32 Health, CurrentHealth;
        public FirstPersonCamera camera;
        public int CurrentGun;
        public ArrayList GunList;
        public int TimeOfLastShot = 0;
        public bool Reloading = false;
        public int TimeSinceBeginReloading = 0;
        public int StarScore;
        //AnimatedModel model;
        Model model;
        Demo Game;
        BoundingSphere Head;
        Vector3 HeadPos;
        const float HeadScale = 10.0f;
        BoundingBox Torso;
        //AI stuff, only used for computer folks
        public bool IsHuman = false;
        public int timeToChangeDirection;
        public int TimeOfLastDeath = 0;
        public bool Dead = false;
        #endregion
        public Being(Demo Game)
        {
            this.Game = Game;
            GunList = new ArrayList();
            GunList.Add(new Gun(7, 28, (int)Guns.Deagle, Game.Deagle));
            GunList.Add(new Gun(30, 90, (int)Guns.AK47, Game.AK47));
            GunList.Add(new Gun(8, 24, (int)Guns.Shotty, Game.Shotty));
            CurrentGun = (int)Guns.Deagle;
            camera = new FirstPersonCamera(Game);
            Health = CurrentHealth = 100;
            timeToChangeDirection = Environment.TickCount;
            StarScore = 0;
            TimeOfLastDeath = Environment.TickCount;
            model = Game.Leet;
        }
        public void Draw(GameTime gameTime)
        {
            Matrix world; Vector3 pos;
            #region Draw bounding stuffs (if in debug mode)
#if DEBUG
            world = Matrix.CreateScale(HeadScale, HeadScale, HeadScale) *
                Matrix.CreateTranslation(HeadPos);
            Game.DrawModel(Game.UnitSphere, world);

            Vector3 newPos = camera.Position;
            newPos.Y -= 50.0f;
            world = Matrix.CreateScale(25, 50, 25) *
                Matrix.CreateTranslation(newPos);
            Game.DrawModel(Game.UnitBox, world);
#endif
            #endregion
            #region Draw weapon
            pos = camera.Position;
            pos -= camera.ZAxis * 35;
            pos += camera.XAxis * 15;
            pos -= camera.YAxis * 17;
            world =
                Matrix.CreateRotationX((float)Math.PI * camera.PitchDegrees / 180.0f) * 
                Matrix.CreateRotationY((float)Math.PI * camera.HeadingDegrees / 180.0f) *
                Matrix.CreateTranslation(pos);
            Game.DrawModel(GetCurrentGun().GunModel, world);
            #endregion
            #region Draw player
            world = Matrix.CreateRotationY((float)Math.PI * camera.HeadingDegrees / 180.0f) *
                        Matrix.CreateTranslation(camera.Position);
            if (!IsHuman)
                Game.DrawModel(model, world);
            #endregion
        }
        public Gun GetCurrentGun()
        {
            foreach (Gun gun in GunList)
                if (gun.GunCode == CurrentGun)
                    return gun;
            return null;
        }
        public String GetGunString()
        {
            switch (CurrentGun)
            {
                case (int)Guns.Melee:
                    {
                        return "Melee";
                    }
                case (int)Guns.Deagle:
                    {
                        return "Deagle";
                    }
                case (int)Guns.AK47:
                    {
                        return "AK47";
                    }
                case (int)Guns.Shotty:
                    {
                        return "Shotty";
                    }
                case (int)Guns.Panzerschreck:
                    {
                        return "Panzerschreck";
                    }
                default:
                    return "Melee";
            }
        }
        public float? Intersect(Ray Shot)
        {
            if (Shot.Intersects(Head).HasValue)
            {
                Game.gotHeadshot = true;
                return Shot.Intersects(Head).Value;
            }
            if (Shot.Intersects(Torso).HasValue)
            {
                return Shot.Intersects(Torso).Value;
            }
            return null;
        }

        public float? RemoteIntersect(Ray Shot)
        {
            if (Shot.Intersects(Head).HasValue)
            {
                Game.RemotePlayerGotAHeadShot = true;
                return Shot.Intersects(Head).Value;
            }
            if (Shot.Intersects(Torso).HasValue)
            {
                return Shot.Intersects(Torso).Value;
            }
            return null;
        }

        public bool SwitchWeapon(int WeaponIdentifier)
        {
            WeaponIdentifier = WeaponIdentifier == 5 ? 0 : WeaponIdentifier;
            WeaponIdentifier = WeaponIdentifier == -1 ? 4 : WeaponIdentifier;
            
            foreach (Gun gun in GunList)
                if (gun.GunCode == WeaponIdentifier)
                {
                    CurrentGun = WeaponIdentifier;
                    Reloading = false;
                    try
                    {
                        Game.cockGun.Play();
                    }
                    catch (Exception e)
                    {
                        e.ToString();
                    }
                    return true;
                }
            return false;
        }

        public void UpdateOnlyBounds(GameTime gameTime)
        {
            HeadPos = camera.Position;
            HeadPos.Y += 7.0f;
            Head = new BoundingSphere(camera.Position, HeadScale);
            Vector3 BoxMin = camera.Position;
            Vector3 BoxMax = camera.Position;
            BoxMin.X -= 20.0f;
            BoxMin.Y -= 50.0f;
            BoxMin.Z -= 20.0f;
            BoxMax.X += 20.0f;
            BoxMax.Y += 20.0f;
            BoxMax.Z += 20.0f;
            Torso = new BoundingBox(BoxMin, BoxMax);
        }

        public void Update(GameTime gameTime)
        {
            #region Death/rotating and such
            if (CurrentHealth < 1 && !Dead)
            {
                Dead = true;
                TimeOfLastDeath = Environment.TickCount;

                Game.starspawn(new Vector3(camera.Position.X, 85, camera.Position.Z));
                
                
                Game.ammospawn(new Vector3(camera.Position.X + 10, 14, camera.Position.Z + 10));
                Game.PlayDeathSound();
            }
            if (Dead)
            {
                int TimeSinceLastDeath = Environment.TickCount - TimeOfLastDeath;
                if (TimeSinceLastDeath < 5000)
                {
                    camera.CurrentVelocity = Vector3.Zero;
                    Vector3 temp = camera.Position;
                    temp.Y-=3.0f;
                    camera.Position = temp;
                }
                else if (TimeSinceLastDeath > 10000 && TimeSinceLastDeath < 15000)
                {
                    Dead = false;
                    Vector3 temp = camera.Position;
                    temp.Y = 110.0f;
                    camera.Position = temp;
                    CurrentHealth = 100;
                    camera.Position = new Vector3(Game.Rand.Next() % 8192 - 8192 / 2, 110.0f, Game.Rand.Next() % 8192 - 8192 / 2);
                    camera.Rotate(-camera.HeadingDegrees, -camera.PitchDegrees);
                }
            }
            #endregion
            #region Bounds Updates
            HeadPos = camera.Position;
            HeadPos.Y += 7.0f;
            Head = new BoundingSphere(camera.Position, HeadScale);
            Vector3 BoxMin = camera.Position;
            Vector3 BoxMax = camera.Position;
            BoxMin.X -= 20.0f;
            BoxMin.Y -= 50.0f;
            BoxMin.Z -= 20.0f;
            BoxMax.X += 20.0f;
            BoxMax.Y += 20.0f;
            BoxMax.Z += 20.0f;
            Torso = new BoundingBox(BoxMin, BoxMax);
            #endregion
            #region Collision/Animations
            if (Game.IsCollision(new Ray(camera.Position, camera.ViewDirection)))
            {
                camera.CurrentVelocity = Vector3.Zero;
                //model.SetAnimation("Idle");
            }
            /*
            if (camera.CurrentVelocity.Length() > 2.3f && model.CurrentAnimation != "Run")
                model.SetAnimation("Run");
            else if (camera.CurrentVelocity.Length() < 2.3f && model.CurrentAnimation != "Idle")
                model.SetAnimation("Idle");
            model.Update(gameTime);
             */
            #endregion
            #region Reloading
            if (Reloading)
            {
                switch (CurrentGun)
                {
                    case (int)Guns.Deagle:
                        {
                            if (Environment.TickCount - TimeSinceBeginReloading > 1000)
                            {
                                foreach(Gun gun in GunList)
                                    if(gun.GunCode == CurrentGun)
                                    {
                                        int RoundsToPutInDeagle = 7 - gun.RoundsInClip;
                                        if (RoundsToPutInDeagle > gun.TotalRounds)
                                        {
                                            gun.RoundsInClip += gun.TotalRounds;
                                            gun.TotalRounds = 0;
                                        }
                                        else
                                        {
                                            gun.RoundsInClip = 7;
                                            gun.TotalRounds -= RoundsToPutInDeagle;
                                        }

                                    }
                                Reloading = false;
                            }
                            break;
                        }
                    case (int)Guns.AK47:
                        {
                            if (Environment.TickCount - TimeSinceBeginReloading > 2000)
                            {
                                foreach (Gun gun in GunList)
                                    if (gun.GunCode == CurrentGun)
                                    {
                                        int RoundsToPutInAK = 30 - gun.RoundsInClip;
                                        if (RoundsToPutInAK > gun.TotalRounds)
                                        {
                                            gun.RoundsInClip += gun.TotalRounds;
                                            gun.TotalRounds = 0;
                                        }
                                        else
                                        {
                                            gun.RoundsInClip = 30;
                                            gun.TotalRounds -= RoundsToPutInAK;
                                        }

                                    }
                                Reloading = false;
                            }
                            break;
                        }
                    case (int)Guns.Shotty:
                        {
                            if (Environment.TickCount - TimeSinceBeginReloading > 3000)
                            {
                                foreach (Gun gun in GunList)
                                    if (gun.GunCode == CurrentGun)
                                    {
                                        int RoundsToPutInShotty = 8 - gun.RoundsInClip;
                                        if (RoundsToPutInShotty > gun.TotalRounds)
                                        {
                                            gun.RoundsInClip += gun.TotalRounds;
                                            gun.TotalRounds = 0;
                                        }
                                        else
                                        {
                                            gun.RoundsInClip = 8;
                                            gun.TotalRounds -= RoundsToPutInShotty;
                                        }

                                    }
                                Reloading = false;
                            }
                            break;
                        }
                }

            }
            #endregion
        }
    }
}
