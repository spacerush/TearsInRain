using System;
using Microsoft.Xna.Framework;

namespace TearsInRain.Tiles { 
    public class TileDoor : TileBase {
        public bool Locked;
        public bool IsOpen;

        public TileDoor(bool locked, bool open) : base (Color.Orange, Color.Transparent, '+') {
            Glyph = '+';
            Name = "door";
            Background = new Color(71, 36, 0);

            Locked = locked;
            IsOpen = open;

            if (!Locked && IsOpen)
                Open();
            else if (Locked || !IsOpen)
                Close();
        }

        public void Close() {
            IsOpen = false;
            Glyph = '+';
            IsBlockingLOS = true;
            IsBlockingMove = true;
        }

        public void Open() {
            IsOpen = true;
            IsBlockingLOS = false;
            IsBlockingMove = false;
            Glyph = '`';
        }
    }
}