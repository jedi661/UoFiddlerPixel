/***************************************************************************
 *
 * $Author: Turley
 * Advanced Nikodemus
 * 
 * "THE BEER-WINE-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with 
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer and Wine in return.
 *
 ***************************************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Ultima;
using Ultima.Helpers;
using UoFiddler.Controls.Classes;

namespace UoFiddler.Forms
{
    public partial class PathSettingsForm : Form
    {
        public PathSettingsForm()
        {
            InitializeComponent();
            Icon = Options.GetFiddlerIcon();

            pgPaths.SelectedObject = new DictionaryPropertyGridAdapter(Files.MulPath);
            tsTbRootPath.Text = Files.RootDir;
        }

        #region [ ReloadPath ]
        private void ReloadPath(object sender, EventArgs e)
        {
            Files.ReLoadDirectory();
            Files.LoadMulPath();
            MapHelper.CheckForNewMapSize();
            pgPaths.SelectedObject = new DictionaryPropertyGridAdapter(Files.MulPath);
            pgPaths.Refresh();
            tsTbRootPath.Text = Files.RootDir;
        }
        #endregion

        #region [ OnClickManual ]
        private void OnClickManual(object sender, EventArgs e)
        {
            using (FolderBrowserDialog dialog = new FolderBrowserDialog())
            {
                dialog.Description = "Select directory containing the client files";
                dialog.ShowNewFolderButton = false;
                if (dialog.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                Files.SetMulPath(dialog.SelectedPath);
                pgPaths.SelectedObject = new DictionaryPropertyGridAdapter(Files.MulPath);
                pgPaths.Update();
                tsTbRootPath.Text = Files.RootDir;
                MapHelper.CheckForNewMapSize();
            }
        }
        #endregion

        #region [ OnKeyDownDir ]
        private void OnKeyDownDir(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter)
            {
                return;
            }

            Files.SetMulPath(tsTbRootPath.Text);
            pgPaths.SelectedObject = new DictionaryPropertyGridAdapter(Files.MulPath);
            pgPaths.Refresh();
            tsTbRootPath.Text = Files.RootDir;
            MapHelper.CheckForNewMapSize();
        }
        #endregion

        #region [ newDirAndMulToolStripMenuItem ]
        private void newDirAndMulToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog dialog = new FolderBrowserDialog())
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    string directoryPath = dialog.SelectedPath;
                    string[] mulFiles = Directory.GetFiles(directoryPath, "*.mul"); // Filter for .mul files
                    string[] uopFiles = Directory.GetFiles(directoryPath, "*.uop"); // Filter for .uop files
                    string[] files = mulFiles.Concat(uopFiles).ToArray(); // Combine .mul and .uop files

                    foreach (string filePath in files)
                    {
                        string key = Path.GetFileNameWithoutExtension(filePath); // Use the file name without an extension as the key
                        Files.MulPath.Add(key, filePath);
                    }

                    pgPaths.SelectedObject = new DictionaryPropertyGridAdapter(Files.MulPath);
                    pgPaths.Refresh();
                }
            }
        }
        #endregion

        #region [ loadSingleMulFileToolStripMenuItem ]
        private void loadSingleMulFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.Filter = "mul files (*.mul)|*.mul|uop files (*.uop)|*.uop"; // Filter for .mul and .uop files
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    string filePath = dialog.FileName;
                    string key = Path.GetFileName(filePath); // Use the full file name as the key

                    if (Files.MulPath.ContainsKey(key))
                    {
                        Files.MulPath[key] = filePath; // Update the path if the key already exists
                    }
                    else
                    {
                        Files.MulPath.Add(key, filePath); // Add the key if it doesn't already exist
                    }

                    pgPaths.SelectedObject = new DictionaryPropertyGridAdapter(Files.MulPath);
                    pgPaths.Refresh();
                }
            }
        }

        #endregion

        #region [ DeleteLineToolStripMenuItem ]
        private void DeleteLineToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (pgPaths.SelectedGridItem != null)
            {
                string key = pgPaths.SelectedGridItem.Label; // The key is the label of the selected GridItem
                if (Files.MulPath.ContainsKey(key))
                {
                    Files.MulPath.Remove(key);
                    pgPaths.SelectedObject = new DictionaryPropertyGridAdapter(Files.MulPath);
                    pgPaths.Refresh();
                }
            }
        }
        #endregion

        #region [ tsBtnBackup ]
        private void tsBtnBackup_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(tsTbRootPath.Text) || !Directory.Exists(tsTbRootPath.Text))
            {
                MessageBox.Show("Please specify a valid source directory in tsTbRootPath.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            using (FolderBrowserDialog dialog = new FolderBrowserDialog())
            {
                dialog.Description = "Select the destination directory for the backup";
                dialog.ShowNewFolderButton = true;

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    string sourceDir = tsTbRootPath.Text;
                    string destDir = dialog.SelectedPath;

                    DialogResult result = MessageBox.Show("This function will copy all files and folders from the source directory to the destination directory. Do you want to proceed?", "Confirm Backup", MessageBoxButtons.OKCancel, MessageBoxIcon.Information);

                    if (result == DialogResult.OK)
                    {
                        try
                        {
                            CopyDirectory(sourceDir, destDir);
                            MessageBox.Show("Backup completed successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"An error occurred during the backup: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
        }
        #endregion

        #region [ CopyDirectory ]
        private void CopyDirectory(string sourceDir, string destDir)
        {
            DirectoryInfo dir = new DirectoryInfo(sourceDir);
            DirectoryInfo[] dirs = dir.GetDirectories();

            if (!Directory.Exists(destDir))
            {
                Directory.CreateDirectory(destDir);
            }

            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                string tempPath = Path.Combine(destDir, file.Name);
                file.CopyTo(tempPath, false);
            }

            foreach (DirectoryInfo subdir in dirs)
            {
                string tempPath = Path.Combine(destDir, subdir.Name);
                CopyDirectory(subdir.FullName, tempPath);
            }
        }
        #endregion
    }


    #region DictionaryPropertyGridAdapter
    internal class DictionaryPropertyGridAdapter : ICustomTypeDescriptor
    {
        private readonly IDictionary _dictionary;

        public DictionaryPropertyGridAdapter(IDictionary d)
        {
            _dictionary = d;
        }

        public string GetComponentName()
        {
            return TypeDescriptor.GetComponentName(this, true);
        }

        public EventDescriptor GetDefaultEvent()
        {
            return TypeDescriptor.GetDefaultEvent(this, true);
        }

        public string GetClassName()
        {
            return TypeDescriptor.GetClassName(this, true);
        }

        public EventDescriptorCollection GetEvents(Attribute[] attributes)
        {
            return TypeDescriptor.GetEvents(this, attributes, true);
        }

        EventDescriptorCollection ICustomTypeDescriptor.GetEvents()
        {
            return TypeDescriptor.GetEvents(this, true);
        }

        public TypeConverter GetConverter()
        {
            return TypeDescriptor.GetConverter(this, true);
        }

        public object GetPropertyOwner(PropertyDescriptor pd)
        {
            return _dictionary;
        }

        public AttributeCollection GetAttributes()
        {
            return TypeDescriptor.GetAttributes(this, true);
        }

        public object GetEditor(Type editorBaseType)
        {
            return TypeDescriptor.GetEditor(this, editorBaseType, true);
        }

        public PropertyDescriptor GetDefaultProperty()
        {
            return null;
        }

        PropertyDescriptorCollection
            ICustomTypeDescriptor.GetProperties()
        {
            return ((ICustomTypeDescriptor)this).GetProperties(Array.Empty<Attribute>());
        }

        public PropertyDescriptorCollection GetProperties(Attribute[] attributes)
        {
            List<PropertyDescriptor> properties = new List<PropertyDescriptor>();
            foreach (DictionaryEntry e in _dictionary)
            {
                properties.Add(new DictionaryPropertyDescriptor(_dictionary, e.Key));
            }

            PropertyDescriptor[] props = properties.ToArray();

            return new PropertyDescriptorCollection(props);
        }
    }
    #endregion

    #region DictionaryPropertyDescriptor
    internal class DictionaryPropertyDescriptor : PropertyDescriptor
    {
        private readonly IDictionary _dictionary;
        private readonly object _key;

        internal DictionaryPropertyDescriptor(IDictionary d, object key)
            : base(key.ToString(), null)
        {
            _dictionary = d;
            _key = key;
        }

        public override Type PropertyType => typeof(string);

        public override void SetValue(object component, object value)
        {
            _dictionary[_key] = value;
        }

        public override object GetValue(object component)
        {
            return _dictionary[_key];
        }

        public override bool IsReadOnly => false;

        public override Type ComponentType => null;

        public override bool CanResetValue(object component)
        {
            return false;
        }

        public override void ResetValue(object component)
        {
        }

        public override bool ShouldSerializeValue(object component)
        {
            return false;
        }
    }
    #endregion
}