/***************************************************************************
 *
 * "THE BEER-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer in return.
 *
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using UoFiddler.Plugin.Enhanced.EcUltima.Uop;

namespace UoFiddler.Plugin.Enhanced.Classes
{
    /// <summary>
    /// Saves changed UOP packages the same way in every editor: confirmation, warning for signed (version 5) files,
    /// "save as" into another folder, backup of the old file.
    /// </summary>
    public static class EcPackageSaver
    {
        /// <param name="saveAs">True: ask for a folder and write the files there (the originals stay unchanged).</param>
        /// <returns>True when something was saved.</returns>
        public static bool Save(IWin32Window owner, IEnumerable<EcUopPackage> packages, bool saveAs)
        {
            List<EcUopPackage> dirty = packages.Where(p => p != null && p.IsDirty).Distinct().ToList();
            if (dirty.Count == 0)
            {
                MessageBox.Show(owner, "Nothing to save.", "Enhanced Client", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return false;
            }

            string names = string.Join(", ", dirty.Select(p => Path.GetFileName(p.FilePath)));
            string folder = null;
            if (saveAs)
            {
                using (var dialog = new FolderBrowserDialog { Description = $"Folder for {names}" })
                {
                    if (dialog.ShowDialog(owner) != DialogResult.OK)
                    {
                        return false;
                    }
                    folder = dialog.SelectedPath;
                }
            }
            else if (!EcDialogs.Confirm(owner, $"Save {names}?\r\nThe old files are kept as .bak next to them.\r\n\r\nThe Enhanced Client must be closed."))
            {
                return false;
            }

            foreach (EcUopPackage package in dirty)
            {
                if (!EcDialogs.ConfirmSignedSave(owner, package))
                {
                    return false;
                }
            }

            Cursor old = Cursor.Current;
            Cursor.Current = Cursors.WaitCursor;
            try
            {
                foreach (EcUopPackage package in dirty)
                {
                    package.Save(folder == null ? package.FilePath : Path.Combine(folder, Path.GetFileName(package.FilePath)));
                }
                return true;
            }
            catch (Exception ex)
            {
                EcDialogs.ShowError(owner, "Saving failed (is the file opened by the client?).", ex);
                return false;
            }
            finally
            {
                Cursor.Current = old;
            }
        }
    }
}
