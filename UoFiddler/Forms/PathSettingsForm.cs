/***************************************************************************
 *
 * $Author: Turley
 * 
 * "THE BEER-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with 
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer in return.
 *
 ***************************************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
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

        private void ReloadPath(object sender, EventArgs e)
        {
            Files.ReLoadDirectory();
            Files.LoadMulPath();
            MapHelper.CheckForNewMapSize();
            pgPaths.SelectedObject = new DictionaryPropertyGridAdapter(Files.MulPath);
            pgPaths.Refresh();
            tsTbRootPath.Text = Files.RootDir;
        }

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

        private void newDirAndMulToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog dialog = new FolderBrowserDialog())
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    string directoryPath = dialog.SelectedPath;
                    string[] files = Directory.GetFiles(directoryPath, "*.mul"); // Filter für .mul Dateien

                    foreach (string filePath in files)
                    {
                        string key = Path.GetFileNameWithoutExtension(filePath); // Verwenden Sie den Dateinamen ohne Erweiterung als Schlüssel
                        Files.MulPath.Add(key, filePath);
                    }

                    pgPaths.SelectedObject = new DictionaryPropertyGridAdapter(Files.MulPath);
                    pgPaths.Refresh();
                }
            }
        }

        private void loadSingleMulFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.Filter = "mul files (*.mul)|*.mul"; // Filter für .mul Dateien
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    string filePath = dialog.FileName;
                    string key = Path.GetFileNameWithoutExtension(filePath); // Verwenden Sie den Dateinamen ohne Erweiterung als Schlüssel
                    Files.MulPath.Add(key, filePath);

                    pgPaths.SelectedObject = new DictionaryPropertyGridAdapter(Files.MulPath);
                    pgPaths.Refresh();
                }
            }
        }

        private void DeleteLineToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (pgPaths.SelectedGridItem != null)
            {
                string key = pgPaths.SelectedGridItem.Label; // Der Schlüssel ist der Label des ausgewählten GridItems
                if (Files.MulPath.ContainsKey(key))
                {
                    Files.MulPath.Remove(key);
                    pgPaths.SelectedObject = new DictionaryPropertyGridAdapter(Files.MulPath);
                    pgPaths.Refresh();
                }
            }
        }
    }

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
}