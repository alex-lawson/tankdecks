using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TankDecks
{
    public class Mob
    {
        public PhysicsObject physobj;
        public DrawingObject drawobj;
        

        public Mob(PhysicsObject pho, DrawingObject dro)
        {
            physobj = pho;
            drawobj = dro;
            physobj.size = 40;
        }

        public virtual void tick()
        {
            
            physobj.move();
        }

        public virtual void draw()
        {
            drawobj.draw(physobj.pos, physobj.angle);
        }
    }

    public class Collidable : Mob
    {
        public Collidable(PhysicsObject pho, DrawingObject dro, CollideMode shape)
            : base(pho, dro)
        {
            physobj.shape = shape;
            physobj.size = Drawing.scale * (int)drawobj.cent.X;
        }
    }

    public class Enemy : Mob
    {
        public DogBrain brain;
        public int colchecks = 0;

        public Enemy(PhysicsObject pho, DrawingObject dro)
            : base(pho, dro)
        {
            brain = new DogBrain(this);
            brain.curplan = AIMode.bounce;
        }

        public override void tick()
        {
            colchecks = 0;
            brain.tick();
            physobj.move();
        }

        public void oncollide(PhysicsObject tar)
        {
            brain.oncollide(tar);
        }
    }

    public class Player : Mob
    {
        public new TankBase physobj;
        public DrawingObject turdraw;
        public float turangle;

        public Player(TankBase pho, DrawingObject dro, DrawingObject tur)
            : base(pho, dro)
        {
            physobj = pho;
            turdraw = tur;
            turdraw.cent = new Vector2(10, 13);
            turangle = 0;
        }

        public override void draw()
        {
            drawobj.draw(physobj.pos, physobj.angle);
            turdraw.draw(physobj.pos, turangle);
        }

        public Vector2 barrel()
        {
            return Vector2.Multiply(Physics.AngleToVector(turangle),15*Drawing.scale);
        }
    }
}
