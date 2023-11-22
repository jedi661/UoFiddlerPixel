using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Drawing.Printing;
using System.Diagnostics;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.Forms
{
    public partial class CreateDataIndexForm : Form
    {
        public CreateDataIndexForm()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // Here you call the method that contains the code from your core class
            string output = Core.Run();

            // Display the output in the TextBox
            this.textBox1.Text = output;

            // Update the label after all files are generated
            labelFileCount.Text = $"Completed, Process generated {Core.fileCount} Files this run";
        }

        internal class Core
        {
            public static int fileCount = 0; // This variable counts the files for this run

            public Core() { }


            public static string Run()
            {

                StringBuilder output = new StringBuilder();

                fileCount = 0;  // Reset fileCount every time the Run method is called

                string dataDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");
                string xmlFilePath = Path.Combine(dataDirectory, "Data.xml");

                string loggerDirectory = Path.Combine(dataDirectory, "Logger");

                if (!Directory.Exists(loggerDirectory))
                {
                    Directory.CreateDirectory(loggerDirectory);
                    output.AppendLine("Logger directory created.");
                }

                try
                {
                    if (!Directory.Exists(loggerDirectory))
                    {
                        Directory.CreateDirectory(loggerDirectory);
                        output.AppendLine("Logger directory created.");
                    }


                    if (!File.Exists(xmlFilePath))
                    {
                        output.AppendLine("Writing Data into a Data.xml file");
                        XmlTextWriter xmlTextWriter = new XmlTextWriter(xmlFilePath, Encoding.ASCII)
                        {
                            Formatting = Formatting.Indented,
                            IndentChar = '\t',
                            Indentation = 2
                        };
                        xmlTextWriter.WriteStartElement("Config");
                        xmlTextWriter.WriteComment("Configuration File for DataList");
                        xmlTextWriter.WriteStartElement("DataName");
                        xmlTextWriter.WriteAttributeString("Name", "UOL");
                        xmlTextWriter.WriteEndElement();
                        xmlTextWriter.WriteStartElement("Directories");

                        string[] directories = new string[]
                        {
                "Data",
                "Data\\System",
                "Data\\Photoshop",
                "Data\\Import Files",
                "Data\\Statics",
                "Data\\Logger",
                "Data\\Transitions",
                "Data\\Transitions\\Citified Terrains",
                "Data\\Transitions\\Citified Terrains\\3way",
                "Data\\Transitions\\Natural Terrains",
                "Data\\Transitions\\Natural Terrains\\3way"
                        };

                        foreach (string dir in directories)
                        {
                            xmlTextWriter.WriteStartElement("Directory");
                            xmlTextWriter.WriteAttributeString("Name", dir);
                            xmlTextWriter.WriteEndElement();
                        }

                        xmlTextWriter.WriteEndElement();
                        xmlTextWriter.WriteEndElement();
                        xmlTextWriter.Close();
                    }
                }
                catch
                {
                    output.AppendLine("Error writing into Data.xml");
                    return output.ToString();
                }

                string value = null;
                ArrayList arrayLists = new ArrayList();

                try
                {
                    XmlDocument xmlDocument = new XmlDocument();
                    xmlDocument.Load(xmlFilePath);
                    XmlNode itemOf = xmlDocument.GetElementsByTagName("Config")[0];
                    value = itemOf["DataName"].Attributes["Name"].Value;

                    foreach (XmlNode elementsByTagName in xmlDocument.GetElementsByTagName("Directory"))
                    {
                        XmlAttribute xmlAttribute = elementsByTagName.Attributes["Name"];
                        arrayLists.Add(xmlAttribute.Value);
                    }
                }
                catch
                {
                    output.AppendLine("Error reading Data.cfg");
                }

                output.AppendFormat("Searching {0} Data...", value);

                FileStream fileStream = null;
                StreamWriter streamWriter = null;

                try
                {
                    if (!Directory.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, arrayLists[0].ToString())))
                    {
                        output.AppendLine("Failed... Present Directory not found...");
                    }
                    else
                    {
                        fileStream = new FileStream(Path.Combine(loggerDirectory, $"{value}_Data.log"), FileMode.Create);
                        streamWriter = new StreamWriter(fileStream);
                        output.AppendLine("Found...");
                        streamWriter.WriteLine("***FullName***");
                        output.AppendLine("***FullName***");

                        int num = 1;

                        foreach (string arrayList in arrayLists)
                        {
                            string[] directories = Directory.GetDirectories(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, arrayList));

                            for (int i = 0; i < directories.Length; i++)
                            {
                                string[] files = Directory.GetFiles(directories[i]);

                                for (int j = 0; j < files.Length; j++)
                                {
                                    string str = files[j];
                                    streamWriter.WriteLine(str);
                                    output.AppendFormat("Writing to File: {0}", str);
                                    num++;
                                    fileCount++;
                                }
                            }
                        }

                        output.AppendLine();
                        streamWriter.WriteLine();
                        output.AppendLine();
                        streamWriter.WriteLine();
                        output.AppendLine();
                        streamWriter.WriteLine();
                        output.AppendLine("***Name of Files without the path of \"Data\"***");
                        streamWriter.WriteLine("***Name of Files without the path of \"Data\"***");
                        output.AppendLine();
                        streamWriter.WriteLine();

                        foreach (string arrayList1 in arrayLists)
                        {
                            string[] strArrays = Directory.GetFiles(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, arrayList1));

                            for (int k = 0; k < strArrays.Length; k++)
                            {
                                string str1 = strArrays[k];
                                int length = AppDomain.CurrentDomain.BaseDirectory.Length + 5;
                                streamWriter.WriteLine(Path.GetFullPath(str1).Substring(length));
                                output.AppendFormat("Writing to File: {0}", Path.GetFullPath(str1).Substring(length));
                            }
                        }

                        output.AppendLine();
                        streamWriter.WriteLine();
                        output.AppendLine();
                        streamWriter.WriteLine();
                        output.AppendLine();
                        streamWriter.WriteLine();
                        output.AppendLine("***Name of Files***");
                        streamWriter.WriteLine("***Name of Files***");
                        output.AppendLine();
                        streamWriter.WriteLine();

                        foreach (string arrayList2 in arrayLists)
                        {
                            string[] directories1 = Directory.GetDirectories(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, arrayList2));

                            for (int l = 0; l < directories1.Length; l++)
                            {
                                FileInfo[] fileInfoArray = (new DirectoryInfo(directories1[l])).GetFiles();

                                for (int m = 0; m < fileInfoArray.Length; m++)
                                {
                                    FileInfo fileInfo = fileInfoArray[m];
                                    streamWriter.WriteLine(fileInfo.Name);
                                    output.AppendFormat("Writing to File: {0}", fileInfo.Name);
                                }
                            }
                        }

                        output.AppendLine();
                        output.AppendFormat("Completed, Process generated {0} Files", num);
                    }
                }
                catch (Exception exception1)
                {
                    Exception exception = exception1;
                    output.AppendLine("Failed...");
                    output.AppendLine();
                    output.AppendLine(exception.ToString());
                }
                finally
                {
                    if (streamWriter != null && fileStream != null)
                    {
                        streamWriter.Close();
                        fileStream.Close();
                    }
                }

                return output.ToString();
            }
        }

        #region Button copy Clipbord
        private void button2_Click(object sender, EventArgs e)
        {
            // Make sure your TextBox is named textBox1
            Clipboard.SetText(textBox1.Text);
        }
        #endregion


        #region Button Print
        private void button3_Click(object sender, EventArgs e)
        {
            PrintDialog printDialog = new PrintDialog();
            PrintDocument printDocument = new PrintDocument();

            printDocument.PrintPage += (s, ev) =>
            {
                ev.Graphics.DrawString(textBox1.Text, new Font("Arial", 12), Brushes.Black, new PointF(10, 10));
            };

            printDialog.Document = printDocument;

            if (printDialog.ShowDialog() == DialogResult.OK)
            {
                printDialog.Document.Print();
            }
        }
        #endregion

        #region Button Logger
        private void buttonOpenLogger_Click(object sender, EventArgs e)
        {
            string loggerDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "Logger");
            if (Directory.Exists(loggerDirectory))
            {
                Process.Start("explorer.exe", loggerDirectory);
            }
            else
            {
                MessageBox.Show("Logger directory does not exist.");
            }
        }
        #endregion
    }
}
