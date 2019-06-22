namespace TearsInRain.src.Interfaces {
    public interface ILockable {
        bool IsLocked { get; set; }
        bool IsOpen { get; set; }

        void ToggleLock(bool manual, bool Locked);
    }
}
