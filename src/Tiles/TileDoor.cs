using System;
using Microsoft.Xna.Framework;

namespace TearsInRain.Tiles { 
    public class TileDoor : TileBase {
        public bool Locked;
        public bool IsOpen;

        public TileDoor(bool locked, bool open) : base (Color.Orange, Color.Transparent, 260) {
            Glyph = 260;
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
            IsBlockingLOS = true;
            IsBlockingMove = true;

            if (GameLoop.GameTime > 100)
                GameLoop.SoundLibrary["door_close"].Play();
        }

        public void Open() {
            IsOpen = true;
            IsBlockingLOS = false;
            IsBlockingMove = false;

            if (GameLoop.GameTime > 100)
                GameLoop.SoundLibrary["door_open"].Play();
        }


        public void UpdateGlyph() {
            if (IsOpen) {
                Glyph = 261;
            } else {
                Glyph = 260;
            }
        }



        public void Lock() {
            Locked = true;
        }

        public void Unlock() {
            Locked = false;
        }
    }
}