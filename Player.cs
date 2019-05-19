using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RogueSharp;

public class Player {
    public int X { get; set; }
    public int Y { get; set; }
    public float Scale { get; set; }
    public Texture2D Sprite { get; set; }


    public Player (int inX, int inY, float inScale, Texture2D inTex) {
        X = inX;
        Y = inY;
        Scale = inScale;
        Sprite = inTex;
    }

    public void Draw(SpriteBatch spriteBatch) {
        float multiplier = Scale * Sprite.Width;
        spriteBatch.Draw(Sprite, new Vector2(X * multiplier, Y * multiplier), null, null, null, 0.0f, new Vector2(Scale, Scale), Color.White, SpriteEffects.None, 0f);
        System.Console.WriteLine("X: " + X * multiplier + ", Y: " + Y * multiplier);
    }


    public void basicMove(int xChange, int yChange, IMap map) {
        if (map.GetCell(X + xChange, Y + yChange).IsWalkable) {
            X += xChange;
            Y += yChange;
        }
    }
}