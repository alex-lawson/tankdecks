using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace TankDecks
{
    public class TickEffects
    {

        public TickEffects()
        {

        }
        public void testtick(Projectile pro)
        {
            Console.WriteLine("TICK");
        }

        public void slow(Projectile pro)
        {
            pro.physobj.vel = Vector2.Multiply(pro.physobj.vel, 0.97f);
        }

        public void speedup(Projectile pro)
        {
            pro.physobj.vel = Vector2.Multiply(pro.physobj.vel, 1.04f);
        }

        public void seek(Projectile pro)
        {
            if (pro.tarmob == null)
            {
                float neardist = 99999;
                foreach (Enemy enemy in GameLogic.enemies)
                {
                    float thisdist = Vector2.Subtract(pro.physobj.pos, enemy.physobj.pos).Length();
                    if ( thisdist < neardist)
                    {
                        neardist = thisdist;
                        pro.tarmob = enemy;
                    }
                }
            }
            Vector2 seekvec = Vector2.Multiply(Vector2.Normalize(Vector2.Subtract(pro.tarmob.physobj.pos, pro.physobj.pos)), 1.5f);
            pro.physobj.vel = Vector2.Add(pro.physobj.vel, seekvec);
            pro.angle = Physics.VectorToAngle(pro.physobj.vel);
            pro.physobj.chokespeed(10, 20);
        }

        public void spin(Projectile pro)
        {
            pro.angle += 0.1f;
        }

        public void spinfast(Projectile pro)
        {
            pro.angle += 0.3f;
        }

        public void whirl(Projectile pro)
        {
            float angle = (pro.duration / 3.0f) % Physics.twopi;
            pro.physobj.vel = Vector2.Add(pro.physobj.vel, Physics.AngleToVector(angle));
        }
    }
}
