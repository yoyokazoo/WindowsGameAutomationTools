using System.Collections.Generic;
using System.Drawing;

namespace WindowsGameAutomationTools.ImageDetection
{
    // TODO: Add summary comments
    public class ImageMatchMultiOffsetMultiColorPositions
    {
        // Offset to the top left of the image to check
        public List<Point> OffsetPoints { get; set; }

        // Offset from the top left of the image to the colors to check
        public List<Point> ColorPoints { get; set; }

        // The list of color lists to match against
        public List<List<Color>> ColorsToCheckAgainst { get; set; }

        public List<List<Point>> PointsToCheck { get; set; }

        public int Threshold { get; set; }

        public ImageMatchMultiOffsetMultiColorPositions(
            List<Point> offsetPoints,
            List<Point> colorPoints,
            List<List<Color>> colorsToCheckAgainst,
            int threshold = ColorComparison.DEFAULT_THRESHOLD)
        {
            OffsetPoints = offsetPoints;
            ColorPoints = colorPoints;
            ColorsToCheckAgainst = colorsToCheckAgainst;
            Threshold = threshold;

            PointsToCheck = new List<List<Point>>(OffsetPoints.Count);
            for (int offsetPointsIndex = 0; offsetPointsIndex < OffsetPoints.Count; offsetPointsIndex++)
            {
                List<Point> combinedColorPoints = new List<Point>(ColorPoints.Count);
                for (int colorPointsIndex = 0; colorPointsIndex < ColorPoints.Count; colorPointsIndex++)
                {
                    combinedColorPoints.Add(
                        new Point(
                            OffsetPoints[offsetPointsIndex].X + ColorPoints[colorPointsIndex].X,
                            OffsetPoints[offsetPointsIndex].Y + ColorPoints[colorPointsIndex].Y)
                        );
                }
                PointsToCheck.Add(combinedColorPoints);
            }
        }

        public int GetMatchingIndexFromBitmap(Bitmap sourceBitmap, int colorPointIndex)
        {
            for (int colorListIndex = 0; colorListIndex < ColorsToCheckAgainst.Count; colorListIndex++)
            {
                bool foundMismatch = false;
                for (int colorIndex = 0; colorIndex < ColorsToCheckAgainst[colorListIndex].Count; colorIndex++)
                {
                    //Color listColor = ColorsToCheckAgainst[colorListIndex][colorIndex];
                    //Color bitmapColor = sourceBitmap.GetPixel(PointsToCheck[colorPointIndex][colorIndex].X, PointsToCheck[colorPointIndex][colorIndex].Y);
                    if (!ColorComparison.ColorsAlmostMatch(ColorsToCheckAgainst[colorListIndex][colorIndex], sourceBitmap.GetPixel(PointsToCheck[colorPointIndex][colorIndex].X, PointsToCheck[colorPointIndex][colorIndex].Y), threshold: Threshold))
                    {
                        foundMismatch = true;
                    }
                }

                if (!foundMismatch)
                {
                    return colorListIndex;
                }
            }

            return -1;
        }
    }
}
