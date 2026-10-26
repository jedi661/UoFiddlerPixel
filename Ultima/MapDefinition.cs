using System;

namespace Ultima
{
    /// <summary>
    /// Describes one map that can be shown in UoFiddler (built-in or added by the user).
    /// </summary>
    public sealed class MapDefinition
    {
        private Map _map;
        private int _width;
        private int _height;

        /// <summary>
        /// Map id (as used by the client/server and for markers). Unique inside <see cref="MapRegistry"/>.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Display name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Index of the files: map{FileIndex}.mul / staidx{FileIndex}.mul / statics{FileIndex}.mul
        /// </summary>
        public int FileIndex { get; set; }

        /// <summary>
        /// Directory with the map files. Null or empty = client directory (<see cref="Files"/>).
        /// </summary>
        public string CustomPath { get; set; }

        /// <summary>
        /// Built-in maps (Felucca ... Intermediate World) can only be renamed or hidden.
        /// </summary>
        public bool IsBuiltIn { get; internal set; }

        /// <summary>
        /// Hidden maps are not shown in the map list of the map control.
        /// </summary>
        public bool Visible { get; set; } = true;

        public int Width
        {
            get => IsBuiltIn ? (Map.GetBuiltIn(Id)?.Width ?? _width) : _width;
            set => _width = value;
        }

        public int Height
        {
            get => IsBuiltIn ? (Map.GetBuiltIn(Id)?.Height ?? _height) : _height;
            set => _height = value;
        }

        public bool UsesCustomPath => !string.IsNullOrWhiteSpace(CustomPath);

        /// <summary>
        /// Returns the map instance. Built-in maps return the static instances of <see cref="Map"/>.
        /// </summary>
        public Map GetMap()
        {
            if (IsBuiltIn)
            {
                return Map.GetBuiltIn(Id);
            }

            if (_map == null || _map.Width != _width || _map.Height != _height)
            {
                _map?.ResetAll();
                _map = new Map(UsesCustomPath ? CustomPath : null, FileIndex, FileIndex, _width, _height);
            }

            return _map;
        }

        /// <summary>
        /// Drops the map instance (files are opened again on next access).
        /// </summary>
        public void Reset()
        {
            if (IsBuiltIn)
            {
                return;
            }

            _map?.ResetAll();
            _map = null;
        }

        /// <summary>
        /// Closes the files and drops the caches but keeps the map instance (used by <see cref="Map.Reload"/>).
        /// </summary>
        public void ResetFiles()
        {
            _map?.ResetAll();
        }

        /// <summary>
        /// True if the map file exists (mul or uop).
        /// </summary>
        public bool FilesExist()
        {
            try
            {
                return GetMap()?.Tiles.MapPath != null;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public MapDefinition Clone()
        {
            return new MapDefinition
            {
                Id = Id,
                Name = Name,
                FileIndex = FileIndex,
                CustomPath = CustomPath,
                IsBuiltIn = IsBuiltIn,
                Visible = Visible,
                _width = _width,
                _height = _height
            };
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
