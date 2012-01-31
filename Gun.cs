using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;

namespace SunshineSlashers
{
    public class Gun
    {
        public int RoundsInClip;
        public int TotalRounds;
        public int GunCode;
        public Model GunModel;
        public Gun(int RoundsInClip, int TotalRounds, int GunCode, Model GunModel)
        {
            this.RoundsInClip = RoundsInClip;
            this.TotalRounds = TotalRounds;
            this.GunCode = GunCode;
            this.GunModel = GunModel;
        }
    }
}
