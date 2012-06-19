using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TankDecks
{
    public enum AIMode { stop = 0, roam, seek, bounce }

    public class DogBrain
    {
        public Mob mob;
        public AIMode curplan;
        public float plantime;
        public static Random random;

        public DogBrain(Mob parent)
        {
            curplan = AIMode.stop;
            plantime = 0;
            mob = parent;
            random = new Random();
        }

        public void tick()
        {
            if (plantime > 0)
            {
                plantime--;
            }
            else if (plantime == 0)
            {
                plan();
            }
            else if (plantime < 0)
            {

            }

            act();
        }

        public void plan()
        {
            switch (curplan)
            {
                case AIMode.stop:
                    plantime = 500;
                    break;
                case AIMode.bounce:
                    plantime = random.Next(50,300);
                    int dir = random.Next(0, 4);
                    switch (dir)
                    {
                        case 0:
                            mob.physobj.vel = new Vector2(0, 8);
                            break;
                        case 1:
                            mob.physobj.vel = new Vector2(0, -8);
                            break;
                        case 2:
                            mob.physobj.vel = new Vector2(8, 0);
                            break;
                        case 3:
                            mob.physobj.vel = new Vector2(-8, 0);
                            break;
                    }
                    break;
            }
        }

        public void act()
        {
            switch (curplan)
            {
                case AIMode.bounce:
                    if (!GameLogic.boundbox.Contains((int)mob.physobj.pos.X, (int)mob.physobj.pos.Y))
                    {
                        mob.physobj.pos = Vector2.Clamp(mob.physobj.pos, new Vector2(GameLogic.boundbox.Left, GameLogic.boundbox.Top), new Vector2(GameLogic.boundbox.Right, GameLogic.boundbox.Bottom));
                        mob.physobj.vel = Vector2.Negate(mob.physobj.vel);
                    }
                break;
            }
        }

        public void oncollide(PhysicsObject tar)
        {
            switch (curplan)
            {
                case AIMode.bounce:
                    mob.physobj.pos = Vector2.Subtract(mob.physobj.pos, mob.physobj.vel);
                    float relx = Math.Abs(tar.pos.X - mob.physobj.pos.X);
                    float rely = Math.Abs(tar.pos.Y - mob.physobj.pos.Y);
                    if (relx > rely)
                    {
                        //Console.WriteLine("x > y");
                        mob.physobj.vel = Vector2.Reflect(mob.physobj.vel, new Vector2(1, 0));
                    }
                    else
                    {
                        //Console.WriteLine("y > x");
                        mob.physobj.vel = Vector2.Reflect(mob.physobj.vel, new Vector2(0, 1));
                    }
                    //mob.physobj.move();
                    break;
            }
        }
    }
}
