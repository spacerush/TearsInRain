using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using TearsInRain.Serializers;
using TearsInRain.src.Interfaces;

namespace TearsInRain.Tiles {

    [JsonConverter(typeof(TileJsonConverter))]
    public class TileDoor : TileBase, ILockable {
        public bool IsLocked { get; set; }
        public bool IsOpen { get; set; }

        public TileDoor(bool locked, bool open) : base (Color.Orange, Color.Transparent, 260) {
            Glyph = 260;
            Name = "door";
            Background = new Color(71, 36, 0);

            IsLocked = locked;
            IsOpen = open;

            if (!IsLocked && IsOpen)
                Open(false);
            else if (IsLocked || !IsOpen)
                Close(false);
        }

        public void Close(bool sound = true) {
            IsOpen = false;
            IsBlockingLOS = true;
            IsBlockingMove = true;

            if (sound)
                GameLoop.SoundLibrary["door_close"].Play();
        }

        public void Open(bool sound = true) {
            IsOpen = true;
            IsBlockingLOS = false;
            IsBlockingMove = false;

            if (sound)
                GameLoop.SoundLibrary["door_open"].Play();
        }


        public void UpdateGlyph() {
            if (IsOpen) {
                Glyph = 261;
            } else {
                Glyph = 260;
            }
        }



        public void ToggleLock(bool manual = false, bool Locked = true) {
            if (!manual) {
                if (IsLocked) {
                    IsLocked = false;
                } else {
                    IsLocked = true;
                }
            } else {
                IsLocked = Locked;
            }
        }
    }
}