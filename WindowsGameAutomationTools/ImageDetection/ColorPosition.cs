using System.Drawing;

namespace WindowsGameAutomationTools.ImageDetection
{
    public class ColorPosition
    {
        public int X;
        public int Y;
        public Color Color { get; set; }

        public ColorPosition(int x, int y, Color color)
        {
            X = x;
            Y = y;
            Color = color;
        }
    }
}
