using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace SunshineSlashers
{
    class Level
    {
        public GameObject Car;
        public const int MAX_HOUSES = 10;
        public GameObject[] House;
        public const int MAX_WELLS = 5;
        public GameObject[] Well;
        public const int MAX_CRATES = 5;
        public GameObject[] Crate;
        public const int MAX_TREES = 10;
        public GameObject[] Tree;

        public const int MAX_MUSHROOMS = 10;
        public GameObject[] Mushroom;

        public Level(Demo Game)
        {
            Well = new GameObject[MAX_WELLS];
            House = new GameObject[MAX_HOUSES];
            Crate = new GameObject[MAX_CRATES];
            Tree = new GameObject[MAX_TREES];
            Mushroom = new GameObject[MAX_MUSHROOMS];
        }

        public void Draw(GameTime gameTime)
        {
            Car.Draw(gameTime);
            for (int i = 0; i < MAX_HOUSES; i++)
            {
                House[i].Draw(gameTime);
            }
            for (int i = 0; i < MAX_WELLS; i++)
            {
                Well[i].Draw(gameTime);
            }
            for (int i = 0; i < MAX_CRATES; i++)
            {
                Crate[i].Draw(gameTime);
            }
            for (int i = 0; i < MAX_TREES; i++)
            {
                Tree[i].Draw(gameTime);
            }
            for (int i = 0; i < MAX_MUSHROOMS; i++)
            {
                Mushroom[i].Draw(gameTime);
            }
        }
    }
}
