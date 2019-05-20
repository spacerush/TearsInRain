using Microsoft.Xna.Framework;
using SadConsole;
using SadConsole.Themes;

namespace TearsInRain.UI {
    class GlobalTheme : SadConsole.Themes.WindowTheme {
        Cell CustomPrintStyle;

        public override void RefreshTheme(Colors themeColors) {
            base.RefreshTheme(themeColors);
            CustomPrintStyle = new Cell(themeColors.Black, themeColors.White);
        }

        public override void Draw(ControlsConsole console, CellSurface hostSurface) {
            // Use the existing theme's drawing which clears the console with the FillStyle property
            base.Draw(console, hostSurface);

            //hostSurface.Fill(Color.Black, Color.Black, ' ');
        }
    }
}