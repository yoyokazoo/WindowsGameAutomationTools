using System.Drawing;
using System.Linq;
using Tesseract;

namespace WindowsGameAutomationTools.ImageDetection
{
    public class ImageMatchTextArea
    {
        public int X;
        public int Y;
        public int Width;
        public int Height;

        public readonly Rect TesseractRect;

        public ImageMatchTextArea(int x, int y, int width, int height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;

            TesseractRect = new Rect(x, y, width, height);
        }

        public string GetText(TesseractEngine engine, Bitmap sourceBitmap)
        {
            using (Page page = engine.Process(sourceBitmap, TesseractRect, PageSegMode.SingleBlock))
            {
                return page.GetText();
            }
        }

        // Expects a string like "$XXX" or "$XXX.YY", and trims everything outside that.
        public bool GetCurrencyTextAsDouble(TesseractEngine engine, Bitmap sourceBitmap, out double result)
        {
            result = 0;
            string text = GetText(engine, sourceBitmap);

            int dollarSignIndex = text.IndexOf('$');
            int decimalPointIndex = text.IndexOf('.');

            if (dollarSignIndex == -1)
            {
                return false;
            }

            string numberOnlyString;
            if (decimalPointIndex == -1)
            {
                // if we don't have a decimal point, we only have dollars, so strip every non-number and parse that
                numberOnlyString = new string(text.Where(c => char.IsDigit(c)).ToArray());
            }
            else
            {
                // if we have a decimal point, grab between the $ and the cents
                int startIndex = dollarSignIndex + 1;
                int numberSubstringLength = (decimalPointIndex - dollarSignIndex) + 2;
                if (startIndex + numberSubstringLength >= text.Length)
                {
                    return false;
                }
                numberOnlyString = text.Substring(startIndex, numberSubstringLength);
            }

            return double.TryParse(numberOnlyString, out result);
        }
    }
}
