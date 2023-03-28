using System;
using System.Drawing;

namespace WindowsGameAutomationTools.ImageDetection
{
    public static class ColorComparison
    {
        public static bool ColorsExactlyMatch(Color firstColor, Color secondColor)
        {
            if (firstColor.R == secondColor.R &&
                firstColor.G == secondColor.G &&
                firstColor.B == secondColor.B)
            {
                return true;
            }

            return false;
        }

        public static bool ColorsAlmostMatch(Color firstColor, Color secondColor, int threshold = 20)
        {
            int r = Math.Abs(firstColor.R - secondColor.R);
            int g = Math.Abs(firstColor.G - secondColor.G);
            int b = Math.Abs(firstColor.B - secondColor.B);

            if (r > threshold || g > threshold || b > threshold)
            {
                return false;
            }

            return true;
        }
    }
}
