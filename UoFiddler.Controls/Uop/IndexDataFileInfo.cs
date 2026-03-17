using System;
using System.Collections.Generic;

namespace UoFiddler.Controls.Uop
{
    public class IndexDataFileInfo
    {
        public UopFileReader File { get; private set; }
        public UopDataHeader UopHeader { get; private set; }
        public int FrameCount { get; private set; }
        
        // Autoriser explicitement null pour détecter et nettoyer les entrées null
        private readonly Dictionary<int, byte[]?> _modifiedDataByDirection = new Dictionary<int, byte[]?>();

        public IndexDataFileInfo(UopFileReader file, UopDataHeader uopHeader)
        {
            File = file;
            UopHeader = uopHeader;
            FrameCount = 0;
        }

        // Définit (ou supprime si data == null) les données modifiées pour une direction
        public void SetModifiedData(int direction, byte[]? data)
        {
            if (direction < 0 || direction >= 5) return;

            if (data == null)
            {
                if (_modifiedDataByDirection.Remove(direction))
                {
                    System.Diagnostics.Debug.WriteLine($"🗑️ SetModifiedData: Removed modified entry for direction={direction} (null provided)");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"⚠️ SetModifiedData: Ignored null for direction={direction} (no existing entry)");
                }
                return;
            }

            _modifiedDataByDirection[direction] = data;
            System.Diagnostics.Debug.WriteLine($"✅ SetModifiedData: direction={direction}, data.Length={data.Length}");
        }

        // Récupère les données pour une direction spécifique, robuste face aux valeurs null
        public byte[]? GetData(int direction)
        {
            System.Diagnostics.Debug.WriteLine($"🔍 GetData called: direction={direction}, HasModified={_modifiedDataByDirection.ContainsKey(direction)}, DictCount={_modifiedDataByDirection.Count}");

            if (_modifiedDataByDirection.TryGetValue(direction, out byte[]? modifiedData))
            {
                if (modifiedData == null)
                {
                    // entrée invalide : nettoyer pour éviter l'exception la prochaine fois
                    _modifiedDataByDirection.Remove(direction);
                    System.Diagnostics.Debug.WriteLine($"⚠️ GetData: direction={direction} - modified entry present but NULL, removed and falling back to original data");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"✅ GetData: direction={direction} - RETURNING MODIFIED DATA (Length={modifiedData.Length})");
                    return modifiedData;
                }
            }

            System.Diagnostics.Debug.WriteLine($"❌ GetData: direction={direction} - RETURNING ORIGINAL DATA");

            if (File == null)
            {
                System.Diagnostics.Debug.WriteLine($"⚠️ GetData: File is null for direction={direction}");
                return null;
            }

            if (!File.IsLoaded)
            {
                System.Diagnostics.Debug.WriteLine($"⚠️ GetData: File not loaded for direction={direction}");
                return null;
            }

            if (UopHeader.DecompressedSize == 0)
            {
                System.Diagnostics.Debug.WriteLine($"⚠️ GetData: UopHeader.DecompressedSize == 0 for direction={direction}");
                return null;
            }

            try
            {
                var original = File.ReadData(UopHeader);
                if (original == null)
                {
                    System.Diagnostics.Debug.WriteLine($"⚠️ GetData: File.ReadData returned null for direction={direction}");
                }
                return original;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ GetData: Exception reading original data for direction={direction}: {ex.Message}");
                return null;
            }
        }

        // Compatibilité : sans paramètre = direction 0
        public byte[]? GetData()
        {
            return GetData(0);
        }

        public void SetFrameCount(int frameCount)
        {
            FrameCount = frameCount;
        }
    }
}
