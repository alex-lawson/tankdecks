using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TankDecks
{
    public static class AIModes
    {
        public static Random random = new Random();

        public static class Stop
        {
            public static void ontick(Mob mob)
            {

            }

            public static void onplan(Mob mob)
            {

            }

            public static void oncollide(Mob mob)
            {

            }
        }

        public static class Bounce
        {
            public static void ontick(Mob mob)
            {

            }

            public static void onplan(DogBrain brain)
            {
                brain.plantime = random.Next(50, 300);
                int dir = random.Next(0, 4);
                switch (dir)
                {
                    case 0:
                        brain.mob.physobj.vel = new Vector2(0, 8);
                        break;
                    case 1:
                        brain.mob.physobj.vel = new Vector2(0, -8);
                        break;
                    case 2:
                        brain.mob.physobj.vel = new Vector2(8, 0);
                        break;
                    case 3:
                        brain.mob.physobj.vel = new Vector2(-8, 0);
                        break;
                }
            }

            public static void oncollide(Mob mob)
            {
                mob.physobj.vel = Vector2.Negate(mob.physobj.vel);
            }
        }
    }
}
