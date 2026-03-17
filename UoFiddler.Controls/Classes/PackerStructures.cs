using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace UoFiddler.Controls.Classes
{
    public class PackedFrameEntry
    {
        [JsonPropertyName("direction")] public int Direction { get; set; }
        [JsonPropertyName("index")] public int Index { get; set; }
        [JsonPropertyName("frame")] public Rect Frame { get; set; }
        [JsonPropertyName("center")] public PointStruct Center { get; set; }
    }

    public class PackedMeta
    {
        [JsonPropertyName("image")] public string Image { get; set; }
        [JsonPropertyName("size")] public SizeStruct Size { get; set; }
        [JsonPropertyName("format")] public string Format { get; set; }
    }

    public class PackedOutput
    {
        [JsonPropertyName("meta")] public PackedMeta Meta { get; set; }
        [JsonPropertyName("frames")] public List<PackedFrameEntry> Frames { get; set; }
    }

    public class PointStruct
    {
        [JsonPropertyName("x")] public int X { get; set; }
        [JsonPropertyName("y")] public int Y { get; set; }
    }

    public class Rect
    {
        [JsonPropertyName("x")] public int X { get; set; }
        [JsonPropertyName("y")] public int Y { get; set; }
        [JsonPropertyName("w")] public int W { get; set; }
        [JsonPropertyName("h")] public int H { get; set; }
    }

    public class SizeStruct
    {
        [JsonPropertyName("w")] public int W { get; set; }
        [JsonPropertyName("h")] public int H { get; set; }
    }
}
