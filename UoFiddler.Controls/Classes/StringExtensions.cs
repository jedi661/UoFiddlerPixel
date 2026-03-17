using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace UoFiddler.Controls.Classes
{
    public static class StringExtensions
    {
        public static string Capitalize(this string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return value;
            }

            var array = value.ToCharArray();
            array[0] = char.ToUpper(array[0]);

            return new string(array);
        }

        public static bool IsGraphic(this string value)
        {
            return int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out _);
        }

        public static string RemoveSpecialCharacters(this string value)
        {
            var stringBuilder = new StringBuilder();

            foreach (var character in value.Where(character =>
                (character >= '0' && character <= '9')
                ||
                (character >= 'A' && character <= 'Z')
                ||
                (character >= 'a' && character <= 'z')
                || character == '.'
                || character == '_'))
            {
                stringBuilder.Append(character);
            }

            return stringBuilder.ToString();
        }

        public static string ToFormattedClassName(this string value)
        {
            return value.Replace(" ", "").Capitalize();
        }

        public static IEnumerable<string> GetImportedClassNames(this string value)
        {
            return value.Split('\n').Where(line => line.StartsWith("import")).Select(line => line.Split(' ')[1]);
        }
    }
}