// /***************************************************************************
//  *
//  * $Author: Prapilk
//  * Built-in : Nikodeus
//  * 
//  * \"THE BEER-WINE-WARE LICENSE\"
//  * As long as you retain this notice you can do whatever you want with 
//  * this stuff. If we meet some day, and you think this stuff is worth it,
//  * you can buy me a beer and Wine in return.
//  *
//  ***************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using System.Threading.Tasks;
using System.Text;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.Class
{
    internal class XMLgenerator
    {
        public static string? InitialLandTypeId { get; set; }
        private static readonly List<string> AllowedAlphaTypes = new List<string> { "A_DR", "A_DL", "A_UU", "A_LL", "A_UR", "A_UL", "B_UL", "B_UR", "B_DR", "B_DL", "B_UU", "B_LL" };

        public void GenerateXML(List<string> texture1FilePaths, List<string> texture2FilePaths, List<string> alphaImageFileNames, string outputPath, string nameTextureA, string brushIdA, string nameTextureB, string brushIdB)
        {
            XDocument xmlDocument = new XDocument(new XElement("transition"));

            string currentID = InitialLandTypeId; // Declared outside of the call to GenerateXML

            GenerateBrush(xmlDocument, nameTextureA, nameTextureB, ref currentID, texture1FilePaths, alphaImageFileNames.Where(x => x.Contains("A_")).ToList(), brushIdB, nameTextureB, brushIdA);
            GenerateBrush(xmlDocument, brushIdA, brushIdB, ref currentID, texture2FilePaths, alphaImageFileNames.Where(x => x.Contains("B_")).ToList(), nameTextureB, nameTextureA, nameTextureA);



            string xmlFilePath = Path.Combine(outputPath, "transition.xml");
            xmlDocument.Save(xmlFilePath);

            Console.WriteLine($"XML file was generated successfully : {xmlFilePath}");
        }

        private static void GenerateBrush(XDocument xmlDocument, string brushName, string brushId, ref string currentID, List<string> textureFilePaths, List<string> alphaImageFileNames, string oppositeBrushId, string nameTextureB, string nameTextureA)
        {
            if (xmlDocument.Root != null)
            {
                XElement brushElement = new XElement("Brush",
                    new XAttribute("Id", brushId),
                    new XAttribute("Name", brushName));

                foreach (string filePath in textureFilePaths)
                {
                    string textureName = Path.GetFileNameWithoutExtension(filePath);
                    string hexValue = ExtractHexNumber(textureName);

                    XElement landElement = new XElement("Land",
                        new XAttribute("ID", $"{hexValue}"));
                    brushElement.Add(landElement);
                }

                string nextID = IncrementHexID(currentID);
                bool isFirstTextureA = true; // Initialize isFirstTextureA to true or false depending on your logic

                XElement edgeElement = new XElement("Edge");
                edgeElement.SetAttributeValue("To", oppositeBrushId);
                edgeElement.Add(new XComment($"{(isFirstTextureA ? nameTextureA : nameTextureB)}"));

                foreach (string alphaFileName in alphaImageFileNames)
                {
                    string alphaName = Path.GetFileNameWithoutExtension(alphaFileName);
                    string alphaType = ExtractAlphaType(alphaName);
                    string hexValue = ExtractHexNumber(alphaName);

                    // Remove "A_" and "B_" prefixes from alphaType value
                    if (alphaType.StartsWith("A_") || alphaType.StartsWith("B_"))
                    {
                        alphaType = alphaType.Substring(2); // Remove the first two characters (the prefix)
                    }

                    // Create the <Land> element by specifying the "Type" attribute with the modified alphaType value
                    XElement landElement = new XElement("Land",
                        new XAttribute("Type", alphaType),
                        new XAttribute("ID", $"0x{currentID}"));

                    edgeElement.Add(landElement);

                    currentID = IncrementHexID(currentID);
                }

                brushElement.Add(edgeElement);
                xmlDocument.Root.Add(brushElement);
            }
            else
            {
                Console.WriteLine("Warning: xmlDocument.Root is null. Check the correct loading of the XML document.");
            }
        }

        private static string ExtractHexNumber(string fileName)
        {
            return Regex.Match(fileName, @"\b0x[0-9a-fA-F]+\b").Value;
        }

        private static string ExtractAlphaType(string fileName)
        {
            string alphaType = fileName.Substring(0, 4);

            return AllowedAlphaTypes.Contains(alphaType) ? alphaType : "Unknown";

        }

        private static string IncrementHexID(string currentID)
        {
            // Convert current ID to integer
            int intValue = Convert.ToInt32(currentID, 16);

            // Increment integer by 1
            intValue++;

            // Convert this integer to hexadecimal representation with a specific 4-digit format
            string nextHexID = intValue.ToString("X4");

            return nextHexID;
        }
    }
}
