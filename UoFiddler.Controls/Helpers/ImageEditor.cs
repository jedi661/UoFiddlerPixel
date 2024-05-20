// /***************************************************************************
//  *
//  * $Author: Nikodemus
//  * 
//  * 
//  * \"THE BEER-WINE-WARE LICENSE\"
//  * As long as you retain this notice you can do whatever you want with 
//  * this stuff. If we meet some day, and you think this stuff is worth it,
//  * you can buy me a beer and Wine in return.
//  *
//  ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UoFiddler.Controls.Helpers
{
    internal class ImageEditor
    {
        private Stack<Bitmap> undoStack = new Stack<Bitmap>();
        private Stack<Bitmap> redoStack = new Stack<Bitmap>();

        public Bitmap CurrentImage { get; set; }

        public void ApplyChange(Bitmap newImage)
        {
            // Save the current image state for undo
            undoStack.Push(new Bitmap(CurrentImage));

            // Apply the change
            CurrentImage = newImage;

            // Clear the redo stack whenever a new change is applied
            redoStack.Clear();
        }

        public void Undo()
        {
            if (undoStack.Count > 0)
            {
                // Save the current image state for redo
                redoStack.Push(new Bitmap(CurrentImage));

                // Revert to the previous image state
                CurrentImage = undoStack.Pop();
            }
        }

        public void Redo()
        {
            if (redoStack.Count > 0)
            {
                // Save the current image state for undo
                undoStack.Push(new Bitmap(CurrentImage));

                // Revert to the next image state
                CurrentImage = redoStack.Pop();
            }
        }
    }
}
