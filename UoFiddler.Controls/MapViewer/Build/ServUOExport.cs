/***************************************************************************
 *
 * "THE BEER-WINE-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer and Wine in return.
 *
 ***************************************************************************/

using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace UoFiddler.Controls.MapViewer.Build
{
    /// <summary>
    /// Scripts for ServUO / RunUO: addon class (same layout as the Multi Script window, with hues)
    /// and decoration files (Data/Decoration/*.cfg, absolute map coordinates).
    /// </summary>
    public static class ServUOExport
    {
        /// <summary>Makes a valid C# class name</summary>
        public static string ClassName(string name)
        {
            var sb = new StringBuilder();
            bool upper = true;
            foreach (char c in name ?? string.Empty)
            {
                if (char.IsLetterOrDigit(c))
                {
                    sb.Append(upper ? char.ToUpperInvariant(c) : c);
                    upper = false;
                }
                else
                {
                    upper = true;
                }
            }

            string result = sb.ToString();
            if (result.Length == 0 || char.IsDigit(result[0]))
            {
                result = "Building" + result;
            }

            return result + "Addon";
        }

        /// <summary>
        /// BaseAddon + BaseAddonDeed. Only visible parts (flag != 0) are used.
        /// </summary>
        public static string AddonScript(MultiTemplate t, string className)
        {
            className = string.IsNullOrWhiteSpace(className) ? ClassName(t.Name) : className;
            string deed = className.EndsWith("Addon") ? className.Substring(0, className.Length - 5) + "Deed" : className + "Deed";
            List<MultiComponent> parts = t.Components.Where(c => c.IsVisible).ToList();

            var sb = new StringBuilder();
            sb.AppendLine("// Created with UoFiddler (Region Viewer / Build)");
            sb.AppendLine($"// {t.Name}: {parts.Count} parts, {t.Width} x {t.Height}, z {t.MinZ}..{t.MaxZ}");
            sb.AppendLine("using System;");
            sb.AppendLine("using Server;");
            sb.AppendLine("using Server.Items;");
            sb.AppendLine();
            sb.AppendLine("namespace Server.Items");
            sb.AppendLine("{");
            sb.AppendLine($"    public class {className} : BaseAddon");
            sb.AppendLine("    {");
            sb.AppendLine("        // item id, x, y, z, hue");
            sb.AppendLine("        private static readonly int[,] m_Components = new int[,]");
            sb.AppendLine("        {");
            foreach (MultiComponent c in parts)
            {
                sb.AppendLine(string.Format(CultureInfo.InvariantCulture, "            {{ 0x{0:X4}, {1}, {2}, {3}, {4} }},", c.ItemId, c.X, c.Y, c.Z, c.Hue));
            }

            sb.AppendLine("        };");
            sb.AppendLine();
            sb.AppendLine($"        public override BaseAddonDeed Deed {{ get {{ return new {deed}(); }} }}");
            sb.AppendLine();
            sb.AppendLine("        [Constructable]");
            sb.AppendLine($"        public {className}()");
            sb.AppendLine("        {");
            sb.AppendLine("            for (int i = 0; i < m_Components.GetLength(0); i++)");
            sb.AppendLine("            {");
            sb.AppendLine("                AddonComponent component = new AddonComponent(m_Components[i, 0]);");
            sb.AppendLine("                if (m_Components[i, 4] != 0)");
            sb.AppendLine("                    component.Hue = m_Components[i, 4];");
            sb.AppendLine("                AddComponent(component, m_Components[i, 1], m_Components[i, 2], m_Components[i, 3]);");
            sb.AppendLine("            }");
            sb.AppendLine("        }");
            sb.AppendLine();
            sb.AppendLine($"        public {className}(Serial serial) : base(serial) {{ }}");
            sb.AppendLine();
            sb.AppendLine("        public override void Serialize(GenericWriter writer)");
            sb.AppendLine("        {");
            sb.AppendLine("            base.Serialize(writer);");
            sb.AppendLine("            writer.Write(0); // version");
            sb.AppendLine("        }");
            sb.AppendLine();
            sb.AppendLine("        public override void Deserialize(GenericReader reader)");
            sb.AppendLine("        {");
            sb.AppendLine("            base.Deserialize(reader);");
            sb.AppendLine("            int version = reader.ReadInt();");
            sb.AppendLine("        }");
            sb.AppendLine("    }");
            sb.AppendLine();
            sb.AppendLine($"    public class {deed} : BaseAddonDeed");
            sb.AppendLine("    {");
            sb.AppendLine($"        public override BaseAddon Addon {{ get {{ return new {className}(); }} }}");
            sb.AppendLine();
            sb.AppendLine("        [Constructable]");
            sb.AppendLine($"        public {deed}()");
            sb.AppendLine("        {");
            sb.AppendLine($"            Name = \"{(t.Name ?? className).Replace("\"", "'")}\";");
            sb.AppendLine("        }");
            sb.AppendLine();
            sb.AppendLine($"        public {deed}(Serial serial) : base(serial) {{ }}");
            sb.AppendLine();
            sb.AppendLine("        public override void Serialize(GenericWriter writer)");
            sb.AppendLine("        {");
            sb.AppendLine("            base.Serialize(writer);");
            sb.AppendLine("            writer.Write(0); // version");
            sb.AppendLine("        }");
            sb.AppendLine();
            sb.AppendLine("        public override void Deserialize(GenericReader reader)");
            sb.AppendLine("        {");
            sb.AppendLine("            base.Deserialize(reader);");
            sb.AppendLine("            int version = reader.ReadInt();");
            sb.AppendLine("        }");
            sb.AppendLine("    }");
            sb.AppendLine("}");
            return sb.ToString();
        }

        /// <summary>
        /// Decoration file for ServUO/RunUO ([Decorate): "Static 0xID (Hue=0x..)" followed by the map positions.
        /// </summary>
        public static string DecorationCfg(IEnumerable<MapStatic> statics, string comment)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"# {comment}");
            sb.AppendLine("# Created with UoFiddler (Region Viewer / Build)");
            sb.AppendLine();

            foreach (IGrouping<(ushort Id, short Hue), MapStatic> group in statics.GroupBy(s => (s.Id, s.Hue)).OrderBy(g => g.Key.Id).ThenBy(g => g.Key.Hue))
            {
                sb.AppendLine(group.Key.Hue != 0
                    ? string.Format(CultureInfo.InvariantCulture, "Static 0x{0:X4} (Hue=0x{1:X})", group.Key.Id, (ushort)group.Key.Hue)
                    : string.Format(CultureInfo.InvariantCulture, "Static 0x{0:X4}", group.Key.Id));
                foreach (MapStatic s in group)
                {
                    sb.AppendLine(string.Format(CultureInfo.InvariantCulture, "{0} {1} {2}", s.X, s.Y, s.Z));
                }

                sb.AppendLine();
            }

            return sb.ToString();
        }

        /// <summary>
        /// Text for the "Multi Script" window (one line per part: "0xID X Y Z Hue")
        /// </summary>
        public static string MultiScriptText(MultiTemplate t)
        {
            return string.Join("\r\n", t.Components.Where(c => c.IsVisible)
                .Select(c => string.Format(CultureInfo.InvariantCulture, "0x{0:X4} {1} {2} {3} {4}", c.ItemId, c.X, c.Y, c.Z, c.Hue)));
        }
    }
}
