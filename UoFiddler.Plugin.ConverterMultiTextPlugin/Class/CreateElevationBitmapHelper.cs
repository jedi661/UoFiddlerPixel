// /***************************************************************************
//  *
//  * $Author: Turley
//  * 
//  * "THE BEER-WARE LICENSE"
//  * As long as you retain this notice you can do whatever you want with 
//  * this stuff. If we meet some day, and you think this stuff is worth it,
//  * you can buy me a beer in return.
//  *
//  ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Elevation;
using Terrain;
using Logger;
using Microsoft.VisualBasic.CompilerServices;



namespace UoFiddler.Plugin.ConverterMultiTextPlugin.Class
{
    internal class CreateElevationBitmapHelper
    {
        public static void MakeAltitudeImage(string projectPath, string terrainFile, string altitudeFile, ClsElevationTable iAltitude, ClsTerrainTable iTerrain, IProgress<int> Progress, IProgress<string> Logger)
        {
            Bitmap bitmap = null;
            Bitmap bitmap1 = null;
            try
            {
                Logger.Report("Load Terrain Image Map.");
                bitmap1 = new Bitmap(string.Format("{0}\\{1}", projectPath, terrainFile));
                bitmap = new Bitmap(bitmap1.Width, bitmap1.Height, PixelFormat.Format8bppIndexed);
            }
            catch (Exception exception1)
            {
                ProjectData.SetProjectError(exception1);
                Exception exception = exception1;
                Logger.Report("Error in loading image maps.\r\n");
                Logger.Report(exception.Message);
                ProjectData.ClearProjectError();
            }
            bitmap.Palette = iAltitude.GetAltPalette();
            int width = bitmap1.Width;
            Rectangle rectangle = new Rectangle(0, 0, width, bitmap1.Height);
            BitmapData bitmapDatum = bitmap1.LockBits(rectangle, ImageLockMode.ReadWrite, PixelFormat.Format8bppIndexed);
            IntPtr scan0 = bitmapDatum.Scan0;
            int num = checked(bitmapDatum.Width * bitmapDatum.Height);
            byte[] numArray = new byte[checked(checked(num - 1) + 1)];
            Marshal.Copy(scan0, numArray, 0, num);
            BitmapData bitmapDatum1 = bitmap.LockBits(rectangle, ImageLockMode.ReadWrite, PixelFormat.Format8bppIndexed);
            IntPtr intPtr = bitmapDatum1.Scan0;
            int width1 = checked(bitmapDatum1.Width * bitmapDatum1.Height);
            byte[] numArray1 = new byte[checked(checked(width1 - 1) + 1)];
            Marshal.Copy(intPtr, numArray1, 0, width1);
            Logger.Report("Creating Altitude Image Map.");
            int num1 = checked(num - 1);
            for (int i = 0; i <= num1; i++)
            {
                if ((i % 1000) == 0) Progress.Report(i * 100 / width1);
                byte altID = iTerrain.TerrianGroup(numArray[i]).AltID;
                numArray1[i] = altID;
            }
            Marshal.Copy(numArray1, 0, intPtr, width1);
            bitmap.UnlockBits(bitmapDatum1);
            try
            {
                string str = string.Format("{0}\\{1}", projectPath, altitudeFile);
                Logger.Report("Saving Altitude Image Map.\r\n");
                Logger.Report(str);
                bitmap.Save(str, ImageFormat.Bmp);
            }
            catch (Exception exception3)
            {
                ProjectData.SetProjectError(exception3);
                Exception exception2 = exception3;
                Logger.Report("Error in saving image.\r\n");
                Logger.Report(exception2.Message);
                ProjectData.ClearProjectError();
            }
            bitmap.Dispose();
            bitmap1.Dispose();
            Logger.Report("Done.");
        }
    }
}
