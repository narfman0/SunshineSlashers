using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SunshineSlashers
{
    class Particle
    {
        public Vector3 pos;
        public Vector3 vel;
        public Texture2D tex;
        public int timeToDie;
        public Particle(Vector3 Position, Vector3 Velocity, Texture2D Texture)
        {
            pos = Position; vel = Velocity; tex = Texture; 
            timeToDie = Environment.TickCount+500;
        }
    }
    class ParticleList
    {
        Random rand;
        public ArrayList list;
        public void AddParticle(Vector3 Position, Vector3 Velocity, Texture2D Texture)
        {
            list.Add(new Particle(Position, Velocity, Texture));
        }
        public void AddParticleBatch(int NumParticles, Vector3 Position, Vector3 Velocity, Texture2D Texture)
        {
            const float RandMod = 473741824.0f;
            Vector3 vel;
            for (int i = 0; i < NumParticles; ++i)
            {
                vel = Velocity;
                vel += new Vector3((float)Math.Pow(-1, rand.Next()%2) * rand.Next() / RandMod,
                    (float)Math.Pow(-1, rand.Next() % 2) * rand.Next() / RandMod,
                    (float)Math.Pow(-1, rand.Next() % 2) * rand.Next() / RandMod);
                AddParticle(Position, vel, Texture);
            }
        }
        public ParticleList()
        {
            rand = new Random();
            list = new ArrayList();
        }
        public void Draw(GameTime gameTime, Demo game)
        {
            foreach (Particle particle in list)
            {
                game.DrawModel(game.UnitSphere, Matrix.CreateTranslation(particle.pos));
            }
        }
        public void Update(GameTime gameTime)
        {
            ArrayList ToKill = new ArrayList();
            foreach (Particle particle in list)
            {
                particle.pos += particle.vel;
                particle.vel.Y -= 0.477f;
                if (particle.timeToDie < Environment.TickCount)
                    ToKill.Add(particle);
            }
            foreach (Particle particle in ToKill)
                list.Remove(particle);
        }
    }
}
