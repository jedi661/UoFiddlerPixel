/***************************************************************************
 *
 * "THE BEER-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer in return.
 *
 ***************************************************************************/

using System;

namespace UoFiddler.Plugin.ClientCheck.Classes
{
    public enum CheckSeverity
    {
        Error,
        Warning,
        Info
    }

    [Flags]
    public enum CheckCategory
    {
        None = 0,
        ArtTileData = 1,
        LandTiles = 2,
        Equipment = 4,
        Bodies = 8,
        RadarCol = 16,
        Multis = 32,
        Statics = 64,
        Cliloc = 128,
        All = ArtTileData | LandTiles | Equipment | Bodies | RadarCol | Multis | Statics | Cliloc
    }

    public enum PreviewKind
    {
        None,
        Item,
        Land,
        Gump,
        Animation
    }

    public enum FixKind
    {
        None,
        RadarItem,
        RadarLand,
        MobTypes
    }

    /// <summary>One finding of the check. The texts are built from <see cref="Key"/> + <see cref="Args"/> in the current language.</summary>
    public sealed class CheckIssue
    {
        public CheckSeverity Severity { get; set; }
        public CheckCategory Category { get; set; }

        /// <summary>main id (item, land, body, multi …) for sorting and "copy id"</summary>
        public int Id { get; set; }

        /// <summary>how the id is shown ("0x1F03", "Body 400" …)</summary>
        public string IdText { get; set; }

        public string Key { get; set; }
        public object[] Args { get; set; } = Array.Empty<object>();

        public PreviewKind Preview { get; set; }
        public int PreviewId { get; set; }

        public FixKind Fix { get; set; }

        /// <summary>data for the fix (e.g. the mobtypes line)</summary>
        public string FixData { get; set; }

        public bool Fixed { get; set; }

        public string Message => Loc.F(Key, Args);

        public string Hint => Loc.F(Key + ".hint", Args);

        public string SeverityText => Loc.T("sev." + Severity);

        public string CategoryText => Loc.T("cat." + Category);
    }
}
