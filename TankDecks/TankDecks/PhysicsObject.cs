using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace TankDecks
{
    public static class Physics
    {
        public static float twopi = 2*(float)Math.PI; 

        public static Vector2 AngleToVector(float angle)
        {
            return new Vector2((float)Math.Sin(angle), -(float)Math.Cos(angle));
        }


        public static float VectorToAngle(Vector2 vector)
        {
            float retang = (float)Math.Atan2(vector.X, -vector.Y);
            //if (retang < 0)
            //{
            //    retang += 2*(float)Math.PI;
            //    //retang = retang % (2 * (float)Math.PI);
            //}
            return retang;
        }

        public static float AngleDiff(float ang1, float ang2)
        {
            if (ang1 < 0) ang1 += twopi;
            if (ang2 < 0) ang2 += twopi;
            float diff = ang1 - ang2;
            if (diff > Math.PI) diff -= twopi;
            else if (diff < -Math.PI) diff += twopi;
            return diff;
        }
    }

    public enum CollideMode { box = 0, circle }

    public class PhysicsObject
    {
        public Vector2 pos;
        public Vector2 vel;
        public float size;
        public float mass;
        public float angle;
        public float friction;
        public CollideMode shape;

        public PhysicsObject(Vector2 p)
        {
            pos = p;
            vel = new Vector2(0, 0);
            mass = 10.0f;
            friction = 0.95f;
            angle = 0;
            size = 0;
        }

        public Point ppos()
        {
            return new Point((int)pos.X, (int)pos.Y);
        }

        public virtual void move()
        {
            pos = Vector2.Add(pos, vel);
            if (vel.Length() > 0) angle = (float)Math.Atan2(vel.X, -vel.Y);
        }

        public virtual void stop()
        {
            pos = Vector2.Subtract(pos, vel);
            vel = new Vector2(0, 0);
        }

        public bool collide(PhysicsObject obj)
        {
            switch (shape)
            {
                case CollideMode.box: //box
                    int boxsize = (int)(size + obj.size);
                    Rectangle cbox = new Rectangle((int)pos.X - boxsize, (int)pos.Y - boxsize, 2 * boxsize, 2 * boxsize);
                    if (cbox.Contains((int)obj.pos.X, (int)obj.pos.Y)) return true;
                    else return false;
                case CollideMode.circle: //circle
                    float radius = size + obj.size;
                    if (Vector2.Subtract(pos, obj.pos).Length() < radius) return true;
                    else return false;
                break;
            }
            return false;
        }

        public void impel(Vector2 force)
        {
            Vector2 adjforce = Vector2.Divide(force, mass);
            vel = Vector2.Add(vel, adjforce);
        }

        public void dofrict()
        {
            vel = Vector2.Multiply(vel, friction);
            if (vel.Length() < 0.1) vel = new Vector2(0, 0);
        }

        public void chokespeed(float minspeed, float maxspeed)
        {
            if (vel.Length() > maxspeed)
            {
                vel.Normalize();
                vel = Vector2.Multiply(vel, maxspeed);
            }
            else if (vel.Length() < minspeed)
            {
                vel.Normalize();
                vel = Vector2.Multiply(vel, minspeed);
            }
        }
    }

    public class TankBase : PhysicsObject
    {
        public float turnspeed;
        public float speed;
        public float maxspeed;
        public float accel;

        public TankBase(Vector2 p)
            : base (p)
        {
            turnspeed = 0.05f;
            speed = 0;
            maxspeed = 10;
            accel = 0.15f;
        }

        public void moveto(Vector2 target)
        {
            //build some vectors
            Vector2 dirvec = Vector2.Normalize(Vector2.Subtract(target, pos));
            Vector2 curvec = Physics.AngleToVector(angle);

            float dot = Vector2.Dot(dirvec, curvec);
            float tarang = Physics.VectorToAngle(dirvec);

            //drive in reverse if it's closer to aligning
            if (dot < 0)
            {
                tarang += (float)Math.PI;
                speed -= accel;
            }
            else speed += accel;

            //align toward target
            float adiff = Physics.AngleDiff(tarang, angle);
            if (Math.Abs(adiff) < turnspeed) angle = tarang;
            else if (adiff < 0) angle -= turnspeed;
            else if (adiff > 0) angle += turnspeed;
        }

        public override void move()
        {
            if (speed > maxspeed) speed = maxspeed;
            else if (speed < -maxspeed) speed = -maxspeed;
            pos = Vector2.Add(pos, Vector2.Multiply(Physics.AngleToVector(angle), speed));
            speed *= friction;
            if (Math.Abs(speed) < 0.1) speed = 0;
        }

        public override void stop()
        {
            pos = Vector2.Subtract(pos, Vector2.Multiply(Physics.AngleToVector(angle), speed));
            speed = 0;
        }
    }
}
