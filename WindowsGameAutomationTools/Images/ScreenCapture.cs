using System.Collections.Generic;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using static WindowsGameAutomationTools.Files.WindowsFilenameSorting;

// Update .nuspec file with new version
// From command line: nuget pack -p Configuration="Release"
// From package manager console: dotnet nuget push C:\Users\peter\Documents\WindowsGameAutomationTools\WindowsGameAutomationTools\WindowsGameAutomationTools.1.0.1.nupkg --source https://api.nuget.org/v3/index.json --api-key

// Started from http://www.developerfusion.com/code/4630/capture-a-screen-shot/ with heavy modifications done
namespace WindowsGameAutomationTools.Images
{
    // TODO: Split the regions into different classes?
    // Top level functionality goes somewhere, helpers exist separately, etc.
    public static class ScreenCapture
    {
        #region Constants

        private const string DEFAULT_TEST_SCREENSHOT_FOLDER = "TestScreenshots";

        #endregion

        #region DLL Imports

        // Helper class containing Gdi32 API functions
        private class Gdi32
        {

            public const int SRCCOPY = 0x00CC0020; // BitBlt dwRop parameter
            [DllImport("gdi32.dll")]
            public static extern bool BitBlt(IntPtr hObject, int nXDest, int nYDest,
                int nWidth, int nHeight, IntPtr hObjectSource,
                int nXSrc, int nYSrc, int dwRop);
            [DllImport("gdi32.dll")]
            public static extern IntPtr CreateCompatibleBitmap(IntPtr hDC, int nWidth,
                int nHeight);
            [DllImport("gdi32.dll")]
            public static extern IntPtr CreateCompatibleDC(IntPtr hDC);
            [DllImport("gdi32.dll")]
            public static extern bool DeleteDC(IntPtr hDC);
            [DllImport("gdi32.dll")]
            public static extern bool DeleteObject(IntPtr hObject);
            [DllImport("gdi32.dll")]
            public static extern IntPtr SelectObject(IntPtr hDC, IntPtr hObject);
        }

        // Helper class containing User32 API functions
        private class User32
        {
            // SetWindowPosition Constants https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-setwindowpos
            public const int SWP_SHOWWINDOW = 0x0040;

            [StructLayout(LayoutKind.Sequential)]
            public struct Rect
            {
                public int left;
                public int top;
                public int right;
                public int bottom;
            }
            [DllImport("user32.dll")]
            public static extern IntPtr GetDesktopWindow();
            [DllImport("user32.dll")]
            public static extern IntPtr GetWindowDC(IntPtr hWnd);
            [DllImport("user32.dll")]
            public static extern IntPtr ReleaseDC(IntPtr hWnd, IntPtr hDC);
            [DllImport("user32.dll")]
            public static extern IntPtr GetWindowRect(IntPtr hWnd, ref Rect rect);
            [DllImport("user32.dll")]
            public static extern int SetForegroundWindow(IntPtr hWnd);
            [DllImport("user32.dll")]
            public static extern bool SetWindowPos(IntPtr hWnd, int hWndInsertAfter, int left, int top, int width, int height, int wFlags);

        }

        #endregion

        #region Window Handles and Processes By Name

        public static void ListAllRunningProcesses()
        {
            Process[] processes = Process.GetProcesses();
            List<string> processStrings = new List<string>();

            foreach (Process p in processes)
            {
                processStrings.Add($"Process: {p.ProcessName}\t{p.MainWindowTitle}\t{p.Id}");
            }

            processStrings.Sort();
            processStrings.ForEach(s => Console.WriteLine(s));
        }

        public static IntPtr GetWindowHandleByName(string processName)
        {
            Process p = Process.GetProcessesByName(processName).FirstOrDefault();
            return p?.MainWindowHandle ?? IntPtr.Zero;
        }

        public static void SaveTestScreenshotOfAllProcesses()
        {
            SaveTestScreenshotsOfAllProcessesWithName(null, allProcesses: true, checkValidity: true);
        }

        public static void SaveTestScreenshotsOfAllProcessesWithName(string processName, bool allProcesses = false, bool checkValidity = false)
        {
            if (string.IsNullOrEmpty(processName) && !allProcesses)
            {
                Console.WriteLine("Process Name is null or empty!  Skipping SaveTestScreenshotsOfAllProcessesWithName");
                return;
            }

            if (GetWindowHandleByName(processName) == IntPtr.Zero && !allProcesses)
            {
                Console.WriteLine($"Unable to find any processes for {processName}!");
                return;
            }

            // find window handles
            Process[] processes;
            string folderName = "AllProcesses";

            if (allProcesses)
            {
                processes = Process.GetProcesses();
            }
            else
            {
                processes = Process.GetProcessesByName(processName);
                folderName = $"TestScreenshotsOf{processName}";
            }

            int processNum = 0;
            CreateEmptyFolder(folderName);

            foreach (Process p in processes)
            {
                Bitmap processScreenshot = CaptureBitmapFromWindowHandle(p.MainWindowHandle);
                if (checkValidity && !BitmapIsValid(processScreenshot))
                {
                    Console.WriteLine($"Bitmap invalid for process {p.ProcessName} ({processNum}), skipping save");
                    continue;
                }
                SaveBitmapToFile(processScreenshot, $"{folderName}{Path.DirectorySeparatorChar}Test_{p.ProcessName}_{processNum}.bmp");
                processNum++;
            }
        }

        public static void SaveTestScreenshotOfDesktopAfterFocusing(string processName)
        {
            IntPtr processPtr = GetWindowHandleByName(processName);

            SetForegroundWindow(processPtr);

            Thread.Sleep(3000);

            SaveTestDesktopScreenshot($"TestScreenshotOfDesktop{processName}.bmp");
        }

        public static void PrintProcessProperties(string processName)
        {
            IntPtr processPtr = GetWindowHandleByName(processName);

            Rectangle processWindowRect = GetWindowRectangleFromHandle(processPtr);
            Console.WriteLine($"Process window rect for {processName} is: {processWindowRect} ({processWindowRect.Location}, {processWindowRect.Size})");

            Bitmap rectangleBitmap = CaptureBitmapFromDesktopAndRectangle(processWindowRect);
            SaveBitmapToFile(rectangleBitmap, "Rectangle.bmp");
        }

        #endregion

        #region Window Handles and Processes By Num

        public static void SaveTestScreenshotsOfProcessNum(int processNum)
        {
            string folderName = "ProcessByNumber";
            CreateEmptyFolder(folderName);
            Process process = Process.GetProcessById(processNum);
            Bitmap processScreenshot = CaptureBitmapFromWindowHandle(process.MainWindowHandle);
            SaveBitmapToFile(processScreenshot, $"{folderName}{Path.DirectorySeparatorChar}Test_{process.ProcessName}_{processNum}.bmp");
        }

        public static void SaveTestScreenshotOfDesktopAfterFocusing(int processNum)
        {
            Process process = Process.GetProcessById(processNum);
            IntPtr processPtr = process.MainWindowHandle;

            SetForegroundWindow(processPtr);

            Thread.Sleep(3000);

            SaveTestDesktopScreenshot($"TestScreenshotOfDesktopByNum{processNum}.bmp");
        }

        public static void PrintProcessProperties(int processNum)
        {
            Process process = Process.GetProcessById(processNum);
            IntPtr processPtr = process.MainWindowHandle;

            Rectangle processWindowRect = GetWindowRectangleFromHandle(processPtr);
            Console.WriteLine($"Process window rect for process num {processNum} is: {processWindowRect} ({processWindowRect.Location}, {processWindowRect.Size})");

            Bitmap rectangleBitmap = CaptureBitmapFromDesktopAndRectangle(processWindowRect);
            SaveBitmapToFile(rectangleBitmap, "Rectangle.bmp");
        }

        #endregion

        #region File and Directory Management

        // Creates a folder if it doesn't exist, and blows away existing content if it does
        public static string CreateEmptyFolder(string folderName)
        {
            string newFolderPath = CreateFolder(folderName);
            BlowAwayFolderContents(newFolderPath);

            return newFolderPath;
        }

        // Creates a folder if it doesn't exist, returns path
        public static string CreateFolder(string folderName)
        {
            string currentDirectory = Directory.GetCurrentDirectory();
            string newDirectoryPath = Path.Combine(currentDirectory, folderName);

            Directory.CreateDirectory(newDirectoryPath);

            return newDirectoryPath;
        }

        public static void BlowAwayFolderContents(string folderPath)
        {
            var files = Directory.EnumerateFiles(folderPath);

            foreach (var file in files)
            {
                File.Delete(file);
            }
        }

        public static void SaveBitmapToFile(Bitmap bmp, string fileName)
        {
            bmp.Save(fileName, ImageFormat.Bmp);
        }

        #endregion

        #region Screen Capturing

        // Always deal in Bitmaps, if possible, so we can check their pixel values.
        // Methods that save bitmaps should be prefixed with SaveTest, to indicate they shouldn't be used in live applications

        public static void SaveTestDesktopScreenshot(string fileName)
        {
            string folderPath = CreateFolder(DEFAULT_TEST_SCREENSHOT_FOLDER);
            string imagePath = Path.Combine(folderPath, fileName);

            Bitmap bmp = CaptureBitmapFromDesktop();
            bmp.Save(imagePath, ImageFormat.Bmp);
            bmp.Dispose();
        }

        private static Bitmap CaptureBitmapFromDesktop()
        {
            IntPtr handle = User32.GetDesktopWindow();

            return CaptureBitmapFromWindowHandle(handle);
        }

        public static Bitmap CaptureBitmapFromDesktopAndRectangle(Rectangle rect)
        {
            IntPtr handle = User32.GetDesktopWindow();

            return CaptureBitmapFromWindowHandleAndRectangle(handle, rect);
        }

        private static Bitmap CaptureBitmapFromWindowHandle(IntPtr handle)
        {
            User32.Rect windowRect = new User32.Rect();
            User32.GetWindowRect(handle, ref windowRect);
            int width = windowRect.right - windowRect.left;
            int height = windowRect.bottom - windowRect.top;

            Rectangle rect = new Rectangle(0, 0, width, height);

            return CaptureBitmapFromWindowHandleAndRectangle(handle, rect);
        }

        private static Bitmap CaptureBitmapFromWindowHandleAndRectangle(IntPtr handle, Rectangle rect)
        {
            IntPtr hdcSrc = User32.GetWindowDC(handle);
            IntPtr hdcDest = Gdi32.CreateCompatibleDC(hdcSrc);

            IntPtr hBitmap = Gdi32.CreateCompatibleBitmap(hdcSrc, rect.Width, rect.Height);
            IntPtr hOld = Gdi32.SelectObject(hdcDest, hBitmap);
            Gdi32.BitBlt(hdcDest, 0, 0, rect.Width, rect.Height, hdcSrc, rect.Left, rect.Top, Gdi32.SRCCOPY);

            Gdi32.SelectObject(hdcDest, hOld);

            Gdi32.DeleteDC(hdcDest);
            User32.ReleaseDC(handle, hdcSrc);

            Bitmap bmp = Bitmap.FromHbitmap(hBitmap);
            Gdi32.DeleteObject(hBitmap);

            return bmp;
        }

        private static Image CaptureScreenshotOfRectangle(Rectangle rect)
        {
            IntPtr handle = User32.GetDesktopWindow();

            IntPtr hdcSrc = User32.GetWindowDC(handle);
            IntPtr hdcDest = Gdi32.CreateCompatibleDC(hdcSrc);
            // create a bitmap we can copy it to,
            // using GetDeviceCaps to get the width/height
            IntPtr hBitmap = Gdi32.CreateCompatibleBitmap(hdcSrc, rect.Width, rect.Height);
            // select the bitmap object
            IntPtr hOld = Gdi32.SelectObject(hdcDest, hBitmap);
            // bitblt over
            Gdi32.BitBlt(hdcDest, 0, 0, rect.Width, rect.Height, hdcSrc, rect.Left, rect.Top, Gdi32.SRCCOPY);
            // restore selection
            Gdi32.SelectObject(hdcDest, hOld);
            // clean up 
            Gdi32.DeleteDC(hdcDest);
            User32.ReleaseDC(handle, hdcSrc);
            // get a .NET image object for it
            Image img = Image.FromHbitmap(hBitmap);
            // free up the Bitmap object
            Gdi32.DeleteObject(hBitmap);
            return img;
        }

        public static void CapturePeriodicScreenshotsOfProcessName(string processName)
        {
            IntPtr processPtr = GetWindowHandleByName(processName);
            Rectangle processWindowRect = GetWindowRectangleFromHandle(processPtr);

            string folderPath = CreateEmptyFolder($"Periodic {processName}");

            int screenshotNum = 0;
            while (true)
            {
                string ssName = $"Periodic_{processName}_{screenshotNum}.bmp";
                string filePath = Path.Combine(folderPath, ssName);
                Bitmap rectangleBitmap = CaptureBitmapFromDesktopAndRectangle(processWindowRect);
                SaveBitmapToFile(rectangleBitmap, filePath);
                rectangleBitmap.Dispose();
                Thread.Sleep(500);
                screenshotNum++;
            }
        }

        public static void CapturePeriodicScreenshotsOfDesktop(int msBetweenScreenshots = 2000)
        {
            string folderPath = ScreenCapture.CreateEmptyFolder($"Periodic Desktop Screenshots");

            int screenshotNum = 0;
            while (true)
            {
                string ssName = $"Periodic_Desktop_{screenshotNum}.bmp";
                string filePath = Path.Combine(folderPath, ssName);
                SaveTestDesktopScreenshot(filePath);
                Thread.Sleep(msBetweenScreenshots);
                screenshotNum++;
            }
        }

        // Typically not a good idea if the images are large or we have a lot of them
        public static List<Bitmap> GetBitmapsFromInputFolderPath(string inputFolderPath)
        {
            string[] sourceFilePaths = Directory.GetFiles(inputFolderPath);
            Array.Sort(sourceFilePaths, new NaturalStringComparer());
            return GetBitmapsFromFilePaths(sourceFilePaths);
        }

        public static List<Bitmap> GetBitmapsFromFilePaths(string[] sourceFilePaths)
        {
            List<Bitmap> bitmaps = new List<Bitmap>();

            foreach (var inputFilePath in sourceFilePaths)
            {
                Bitmap sourceBitmap = new Bitmap(inputFilePath);
                bitmaps.Add(sourceBitmap);
            }

            return bitmaps;
        }

        #endregion

        #region Window Manipulation

        public static void SetForegroundWindow(IntPtr handle)
        {
            User32.SetForegroundWindow(handle);
        }

        public static Rectangle GetWindowRectangleFromHandle(IntPtr handle)
        {
            User32.Rect windowRect = new User32.Rect();
            User32.GetWindowRect(handle, ref windowRect);
            return new Rectangle(windowRect.left, windowRect.top, windowRect.right - windowRect.left, windowRect.bottom - windowRect.top);
        }

        public static void MoveAndResizeWindow(IntPtr handle, Rectangle rectangle)
        {
            // Can be used to get existing bounds
            // Control form = Control.FromHandle(handle);
            Thread.Sleep(3000);
            bool success = User32.SetWindowPos(handle, 0, rectangle.Left, rectangle.Top, rectangle.Width, rectangle.Height, 0/*User32.SWP_SHOWWINDOW*/);
            Console.WriteLine($"Set Window Pos: success = {success}");

        }

        #endregion

        #region Validation

        // This method is slow! Only use in non-live testing
        // Checks that the Bitmap is not null,
        // is larger than 1x1, and that it isn't all the same color
        private static bool BitmapIsValid(Bitmap bmp)
        {
            if (bmp == null)
            {
                return false;
            }

            if (bmp.Width <= 1 && bmp.Height <= 1)
            {
                return false;
            }

            bool foundColorMismatch = false;

            Color firstColor = bmp.GetPixel(0, 0);
            for (int x = 0; x < bmp.Width && !foundColorMismatch; x++)
            {
                for (int y = 0; y < bmp.Height && !foundColorMismatch; y++)
                {
                    if (bmp.GetPixel(x, y) != firstColor)
                    {
                        foundColorMismatch = true;
                    }
                }
            }

            if (!foundColorMismatch)
            {
                return false;
            }

            return true;
        }

        #endregion

        #region File/Folder Manipulation

        public static string SelectFolder(FolderBrowserDialog browserDialog, TextBox pathTextBox)
        {
            string currentDirectory = Directory.GetCurrentDirectory();
            browserDialog.SelectedPath = currentDirectory;

            DialogResult result = browserDialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                pathTextBox.Text = browserDialog.SelectedPath;
            }

            return browserDialog.SelectedPath;
        }

        public static string SelectBmpFile(OpenFileDialog fileDialog, TextBox bmpTextBox)
        {
            string currentDirectory = Directory.GetCurrentDirectory();
            fileDialog.InitialDirectory = currentDirectory;
            fileDialog.Filter = "BMP files (*.bmp)|*.bmp";

            DialogResult result = fileDialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                bmpTextBox.Text = fileDialog.FileName;
            }

            return fileDialog.FileName;
        }

        #endregion
    }
}