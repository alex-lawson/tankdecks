using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace TankDecks
{
    public class HitEffects
    {
        public HitEffects()
        {
            
        }

        public void testhit(Projectile pro, Mob mob = null)
        {
            Console.WriteLine("BOOM!");
        }

        public void fizzle(Projectile pro, Mob mob = null)
        {
            pro.duration = -1;
        }

        public void bounce(Projectile pro, Mob mob = null)
        {
            pro.physobj.pos = Vector2.Subtract(pro.physobj.pos, pro.physobj.vel);
            if (mob == null)
            {
                pro.physobj.vel = Vector2.Negate(pro.physobj.vel);
            }
            else if (mob.physobj.shape == CollideMode.circle)
            {
                pro.physobj.vel = Vector2.Reflect(pro.physobj.vel, Vector2.Normalize(Vector2.Subtract(pro.physobj.pos, mob.physobj.pos)));
            }
            else if (mob.physobj.shape == CollideMode.box)
            {
                float relx = Math.Abs(pro.physobj.pos.X - mob.physobj.pos.X);
                float rely = Math.Abs(pro.physobj.pos.Y - mob.physobj.pos.Y);
                if (relx > rely)
                {
                    //Console.WriteLine("x > y");
                    pro.physobj.vel = Vector2.Reflect(pro.physobj.vel, new Vector2(1, 0));
                }
                else
                {
                    //Console.WriteLine("y > x");
                    pro.physobj.vel = Vector2.Reflect(pro.physobj.vel, new Vector2(0, 1));
                }
            }
            else pro.physobj.vel = Vector2.Negate(pro.physobj.vel);

            pro.physobj.move();
            pro.angle = Physics.VectorToAngle(pro.physobj.vel);
        }

        public void burst(Projectile pro, Mob mob = null)
        {
            for (float i = 0; i < Physics.twopi; i += 0.3f)
            {
                Projectile newpro = new Projectile(GameLogic.blankattack, new PhysicsObject(pro.physobj.pos), new Vector2());
                newpro.physobj.vel = Vector2.Multiply(Physics.AngleToVector(i), 10);
                newpro.angle = i;
                newpro.drawshot = 5;
                newpro.duration = 10;
                GameLogic.particles.Add(newpro);
            }
        }
    }
}
