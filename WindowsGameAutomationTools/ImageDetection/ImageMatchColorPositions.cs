using System.Collections.Generic;
using System.Drawing;

namespace WindowsGameAutomationTools.ImageDetection
{
    public class ImageMatchColorPositions
    {
        public int X;
        public int Y;
        public List<ColorPosition> ColorPositions;

        public ImageMatchColorPositions(int x, int y, List<ColorPosition> colorPositionList)
        {
            X = x;
            Y = y;
            ColorPositions = colorPositionList;
        }

        public bool MatchesSourceImage(Bitmap sourceImage)
        {
            foreach (ColorPosition colorPosition in ColorPositions)
            {
                if (!ColorComparison.ColorsAlmostMatch(sourceImage.GetPixel(X + colorPosition.X, Y + colorPosition.Y), colorPosition.Color))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
