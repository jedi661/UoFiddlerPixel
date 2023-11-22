using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;
using System;
using System.Collections;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Xml;
namespace Transition
{
	public class TransitionTable
	{
		private Hashtable i_Transitions;
		public Hashtable GetTransitionTable
		{
			get
			{
				return this.i_Transitions;
			}
		}
		public Transition Transition(int iKey)
		{
				return (Transition)this.i_Transitions[iKey];
		}

		public TransitionTable()
		{
			this.i_Transitions = new Hashtable();
		}
		public void Clear()
		{
			this.i_Transitions.Clear();
		}
		public void Add(Transition iValue)
		{
			try
			{
				this.i_Transitions.Add(iValue.HashKey, iValue);
			}
			catch (Exception expr_17)
			{
				ProjectData.SetProjectError(expr_17);
				Exception ex = expr_17;
				Interaction.MsgBox(ex.Message, MsgBoxStyle.OkOnly, null);
				ProjectData.ClearProjectError();
			}
		}
		public void Remove(Transition iValue)
		{
			this.i_Transitions.Remove(iValue.HashKey);
		}
		public void Display(ListBox iList)
		{
			iList.Items.Clear();

            IEnumerator enumerator = this.i_Transitions.Values.GetEnumerator();

			try
			{
				while (enumerator.MoveNext())
				{
					Transition item = (Transition)enumerator.Current;
					iList.Items.Add(item);
				}
			}
			finally
			{
				if (enumerator is IDisposable)
				{
					((IDisposable)enumerator).Dispose();
				}
			}
		}
		public void Load(string iFilename)
		{
			XmlDocument xmlDocument = new XmlDocument();
			try
			{
				xmlDocument.Load(iFilename);

                IEnumerator enumerator = xmlDocument.SelectNodes("//Trans/TransInfo").GetEnumerator();

				try
				{
					while (enumerator.MoveNext())
					{
						XmlElement xmlInfo = (XmlElement)enumerator.Current;
						Transition transition = new Transition(xmlInfo);
						this.i_Transitions.Add(transition.HashKey, transition);
					}
				}
				finally
				{
					if (enumerator is IDisposable)
					{
						((IDisposable)enumerator).Dispose();
					}
				}
			}
			catch (Exception expr_74)
			{
				ProjectData.SetProjectError(expr_74);
				Interaction.MsgBox(string.Format("XMLFile:{0}", iFilename), MsgBoxStyle.OkOnly, null);
				ProjectData.ClearProjectError();
			}
		}
		public void Save(string iFilename)
		{
			SaveFileDialog saveFileDialog = new SaveFileDialog();
			saveFileDialog.FileName = iFilename;
			saveFileDialog.Filter = "xml files (*.xml)|*.xml";
			if (saveFileDialog.ShowDialog() == DialogResult.OK)
			{
				XmlTextWriter xmlTextWriter = new XmlTextWriter(saveFileDialog.FileName, Encoding.UTF8);
				xmlTextWriter.Indentation = 2;
				xmlTextWriter.Formatting = Formatting.Indented;
				xmlTextWriter.WriteStartDocument();
				xmlTextWriter.WriteStartElement("Trans");

                IEnumerator enumerator = this.i_Transitions.Values.GetEnumerator();

				try
				{
					while (enumerator.MoveNext())
					{
						Transition transition = (Transition)enumerator.Current;
						transition.Save(xmlTextWriter);
					}
				}
				finally
				{
					if (enumerator is IDisposable)
					{
						((IDisposable)enumerator).Dispose();
					}
				}
				xmlTextWriter.WriteEndElement();
				xmlTextWriter.WriteEndDocument();
				xmlTextWriter.Close();
			}
		}
		public void Save(string iPath, string iFilename)
		{
			XmlTextWriter xmlTextWriter = new XmlTextWriter(string.Format("{0}/{1}.xml", iPath, iFilename), Encoding.UTF8);
			xmlTextWriter.Indentation = 2;
			xmlTextWriter.Formatting = Formatting.Indented;
			xmlTextWriter.WriteStartDocument();
			xmlTextWriter.WriteStartElement("Trans");

            IEnumerator enumerator = this.i_Transitions.Values.GetEnumerator();

			try
			{
				while (enumerator.MoveNext())
				{
					Transition transition = (Transition)enumerator.Current;
					transition.Save(xmlTextWriter);
				}
			}
			finally
			{
				if (enumerator is IDisposable)
				{
					((IDisposable)enumerator).Dispose();
				}
			}
			xmlTextWriter.WriteEndElement();
			xmlTextWriter.WriteEndDocument();
			xmlTextWriter.Close();
		}
		public void MassLoad(string iPath)
		{
			this.ProcessDirectory(iPath);
		}
		public void ProcessDirectory(string targetDirectory)
		{
			string[] files = Directory.GetFiles(targetDirectory, "*.xml");
			string[] array = files;
			checked
			{
				for (int i = 0; i < array.Length; i++)
				{
					string iFilename = array[i];
					this.Load(iFilename);
				}
				string[] directories = Directory.GetDirectories(targetDirectory);
				string[] array2 = directories;
				for (int j = 0; j < array2.Length; j++)
				{
					string targetDirectory2 = array2[j];
					this.ProcessDirectory(targetDirectory2);
				}
			}
		}
	}
}
