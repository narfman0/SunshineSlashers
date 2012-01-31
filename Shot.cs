using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace SunshineSlashers
{
    class Shot
    {
        public float Caliber;
        public int TimeToDie;
        public Ray Tracer;
        public Being Origin;
        public Shot(Ray Trac, float Caliber, Being Origin)
        {
            Tracer = Trac;
            TimeToDie = Environment.TickCount+100;
            this.Caliber = Caliber;
            this.Origin = Origin;
        }
    }
    class ShotList
    {
        public ArrayList Shots;
        public ShotList()
        {
            Shots = new ArrayList();
        }
        public void AddShot(Ray Trac, float Caliber, Being Player)
        {
            Shots.Add(new Shot(Trac, Caliber, Player));
        }
        public void Update(GameTime gameTime)
        {
            ArrayList ToKill = new ArrayList();
            foreach (Shot shot in Shots)
            {
                if (Environment.TickCount > shot.TimeToDie)
                    ToKill.Add(shot);
            }
            foreach (Shot shot in ToKill)
            {
                Shots.Remove(shot);
            }
        }
    }
}
