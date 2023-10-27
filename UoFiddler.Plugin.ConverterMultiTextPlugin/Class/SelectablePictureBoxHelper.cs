using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.Class
{
    internal static class SelectablePictureBoxHelper
    {
        public static void UndoDrawing(SelectablePictureBox[] pictureBoxes)
        {
            foreach (var pictureBox in pictureBoxes)
            {
                pictureBox.UndoDrawing();
            }
        }
    }
}
