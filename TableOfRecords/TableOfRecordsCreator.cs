using System.Globalization;
using System.Reflection;
using System.Text;

namespace TableOfRecords;

/// <summary>
/// Presents method that write in table form to the text stream a set of elements of type T.
/// </summary>
public static class TableOfRecordsCreator
{
    private const char CornerChar = '+';
    private const char HorizontalChar = '-';
    private const char VerticalChar = '|';
    private const int CellPadding = 2;

    /// <summary>
    /// Write in table form to the text stream a set of elements of type T (<see cref="ICollection{T}"/>),
    /// where the state of each object of type T is described by public properties that have only build-in
    /// type (int, char, string etc.)
    /// </summary>
    /// <typeparam name="T">Type selector.</typeparam>
    /// <param name="collection">Collection of elements of type T.</param>
    /// <param name="writer">Text stream.</param>
    /// <exception cref="ArgumentNullException">Throw if <paramref name="collection"/> is null.</exception>
    /// <exception cref="ArgumentNullException">Throw if <paramref name="writer"/> is null.</exception>
    /// <exception cref="ArgumentException">Throw if <paramref name="collection"/> is empty.</exception>
    public static void WriteTable<T>(ICollection<T>? collection, TextWriter? writer)
    {
        ArgumentNullException.ThrowIfNull(collection);
        ArgumentNullException.ThrowIfNull(writer);

        if (collection.Count == 0)
        {
            throw new ArgumentException(
                "Collection must not be empty",
                nameof(collection));
        }

        var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

        var columnWidths = properties.ToDictionary(
            property => property.Name,
            property =>
            {
                var maxValueLength = collection
                    .Select(item =>
                        Convert.ToString(property.GetValue(item), CultureInfo.InvariantCulture) ?? string.Empty)
                    .Max(value => value.Length);

                return Math.Max(
                    property.Name.Length,
                    maxValueLength);
            });

        string CreateSeparator()
        {
            return CornerChar +
                   string.Join(
                       CornerChar,
                       properties.Select(property =>
                           new string(
                               HorizontalChar,
                               columnWidths[property.Name] + CellPadding))) +
                   CornerChar +
                   Environment.NewLine;
        }

        string CreateHeader()
        {
            var cells = properties.Select(property =>
                $" {property.Name.PadRight(columnWidths[property.Name])} ");

            return VerticalChar +
                   string.Join(VerticalChar, cells) +
                   VerticalChar +
                   Environment.NewLine;
        }

        bool IsLeftAligned(Type type)
        {
            var underlyingType = Nullable.GetUnderlyingType(type) ?? type;
            return underlyingType == typeof(string) || underlyingType == typeof(char);
        }

        string CreateRow(T item)
        {
            var cells = properties.Select(property =>
            {
                var value =
                    Convert.ToString(property.GetValue(item), CultureInfo.InvariantCulture) ?? string.Empty;

                if (IsLeftAligned(property.PropertyType))
                {
                    return $" {value.PadRight(columnWidths[property.Name])} ";
                }

                return $" {value.PadLeft(columnWidths[property.Name])} ";
            });

            return VerticalChar +
                   string.Join(VerticalChar, cells) +
                   VerticalChar +
                   Environment.NewLine;
        }

        var builder = new StringBuilder();

        builder.Append(CreateSeparator());
        builder.Append(CreateHeader());
        builder.Append(CreateSeparator());

        foreach (var item in collection)
        {
            builder.Append(CreateRow(item));
            builder.Append(CreateSeparator());
        }

        writer.Write(builder.ToString());
        writer.Flush();
    }
}
