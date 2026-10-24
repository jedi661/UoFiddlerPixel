// /***************************************************************************
//  *
//  * $Author: Nikodemus
//  *
//  * "THE BEER-WARE LICENSE"
//  * As long as you retain this notice you can do whatever you want with
//  * this stuff. If we meet some day, and you think this stuff is worth it,
//  * you can buy me a beer in return.
//  *
//  ***************************************************************************/

using System;
using System.IO;
using Elevation;
using Terrain;
using UoFiddler.Plugin.ConverterMultiTextPlugin.MapMakerKit;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.Class
{
    /// <summary>
    /// Creates Altitude.bmp from Terrain.bmp (every tile gets the default altitude of its terrain).
    /// Fixed: null reference after a failed load, missing UnlockBits, int overflow of the progress value
    /// on big maps, unknown terrain groups and the ignored bitmap stride.
    /// </summary>
    internal static class CreateElevationBitmapHelper
    {
        public static void MakeAltitudeImage(string projectPath, string terrainFile, string altitudeFile, ClsElevationTable iAltitude, ClsTerrainTable iTerrain, IProgress<int> Progress, IProgress<string> Logger)
        {
            try
            {
                AltitudePalette altitudes = new AltitudePalette(iAltitude);
                TerrainPalette terrains = new TerrainPalette(iTerrain, altitudes);
                string terrainPath = Path.Combine(projectPath, terrainFile);
                string altitudePath = Path.Combine(projectPath, altitudeFile);

                Logger?.Report("Load Terrain Image Map.");
                Progress?.Report(10);
                MapDocument doc = MapDocument.Load(terrainPath, null, terrains, altitudes, message => Logger?.Report(message));

                Logger?.Report("Creating Altitude Image Map.");
                Progress?.Report(50);
                HeightTools.ResetFromTerrain(doc, terrains, altitudes, null);

                Logger?.Report("Saving Altitude Image Map: " + altitudePath);
                Progress?.Report(80);
                doc.Save(null, altitudePath, terrains, altitudes);
                Progress?.Report(100);
                Logger?.Report("Done.");
            }
            catch (Exception ex)
            {
                Logger?.Report("Error while creating the altitude image: " + ex.Message);
            }
        }
    }
}
