using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace TankDecks
{
    public static class Drawing
    {
        public static float scale = 4;
    }

    public class DrawingObject
    {
        public Texture2D tex;
        public Rectangle rect;
        public Vector2 cent;
        public Color col;

        public DrawingObject(Texture2D t)
        {
            col = Color.White;
            tex = t;
            rect = t.Bounds;
            cent = new Vector2(t.Bounds.Width / 2, t.Bounds.Height / 2);
        }

        public void draw(Vector2 loc, float angle)
        {
            GameLogic.spriteBatch.Draw(tex, loc, null, Color.White, angle, cent, Drawing.scale, SpriteEffects.None, 0);
        }
    }
}
