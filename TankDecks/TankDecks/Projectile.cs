using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TankDecks
{
    public class Projectile
    {
        public PhysicsObject physobj;
        public int drawshot;
        public float angle;
        public Vector2 tarpos;
        public Mob tarmob = null;
        public float duration;
        public Attack source;

        public Projectile(Attack src, PhysicsObject pho, Vector2 tar = new Vector2())
        {
            source = src;
            physobj = pho;
            tarpos = tar;

        }

        public void tick()
        {
            duration--;
            if (duration <= 0)
            {
                source.onexpire(this);
            }
            else
            {
                source.ontick(this);
                physobj.move();
            }
        }
    }

    public class Attack
    {
        public int drawshot;
        public float duration;
        public float speed, maxspeed;
        public double inaccuracy;
        public float cooldown, curcooldown;
        public int repeat, currepeat;
        public float damage;
        public int pierce;
        public bool hostile;
        public int projcount;
        public float spread;
        ZigguratGaussianSampler random;

        public delegate void HitEventHandler(Projectile pro, Mob mob = null);
        public delegate void MoveEventHandler(Projectile pro);
        public event HitEventHandler handleexpire, handlecollide;
        public event MoveEventHandler handletick;

        public Attack()
        {
            speed = 10;
            cooldown = 20;
            currepeat = 0;
            curcooldown = 0;
            drawshot = 0;
            duration = 500;
            inaccuracy = 0;
            spread = 0;
            projcount = 1;
            random = new ZigguratGaussianSampler();
            handleexpire += delegate(Projectile pro, Mob mob) { return; };
            handlecollide += delegate(Projectile pro, Mob mob) { return; };
            handletick += delegate(Projectile pro) { return; };
        }

        public void onexpire(Projectile pro)
        {
            handleexpire(pro, null);
        }

        public void oncollide(Projectile pro, Mob mob)
        {
            handlecollide(pro, mob);
        }

        public void ontick(Projectile pro)
        {
            handletick(pro);
        }

        public void tick()
        {
            if (curcooldown > 0)
            {
                curcooldown--;
            }
        }

        public void fire(List<Projectile> shots, Vector2 origin, Vector2 target)
        {
            if (curcooldown <= 0)
            {
                for (int i = 0; i < projcount; i++)
                {
                    Projectile pro = new Projectile(this, new PhysicsObject(origin), target);
                    Vector2 tvel;
                    if (projcount > 1 && spread > 0)
                    {
                        //give spreads the velocity for the appropriate angle
                        float thisang = Physics.VectorToAngle(Vector2.Subtract(target, origin)) + (i + 0.5f) * (spread / projcount) - (spread / 2);
                        tvel = Physics.AngleToVector(thisang);
                    }
                    else
                    {
                        //standard trajectory
                        tvel = Vector2.Normalize(Vector2.Subtract(target, origin));
                    }
                    if (inaccuracy != 0)
                    {
                        //add some inaccuracy
                        double thisdeviation = random.NextSample(0, inaccuracy);
                        tvel = Vector2.Transform(tvel, Matrix.CreateRotationZ((float)thisdeviation));
                    }
                    pro.physobj.vel = Vector2.Multiply(tvel, speed);
                    pro.physobj.pos = Vector2.Subtract(pro.physobj.pos, pro.physobj.vel);
                    pro.drawshot = drawshot;
                    pro.duration = duration;
                    pro.angle = Physics.VectorToAngle(pro.physobj.vel);
                    shots.Add(pro);
                }
                curcooldown = cooldown;
            }
        }
    }
}
