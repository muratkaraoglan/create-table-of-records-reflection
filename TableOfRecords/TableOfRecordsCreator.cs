namespace TableOfRecords;

/// <summary>
/// Presents method that write in table form to the text stream a set of elements of type T.
/// </summary>
public static class TableOfRecordsCreator
{
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
            throw new ArgumentException("Collection must not be empty", nameof(collection));
        }

        var properties = typeof(T).GetProperties();

        var columnWidths = properties.ToDictionary(
            property => property.Name,
            property =>
            {
                var maxValueLength = collection
                    .Select(item =>
                        property.GetValue(item)?.ToString() ?? string.Empty)
                    .Max(value => value.Length);

                return Math.Max(
                    property.Name.Length,
                    maxValueLength);
            });

        string CreateSeparator()
        {
            return "+" +
                   string.Join(
                       "+",
                       properties.Select(property =>
                           new string(
                               '-',
                               columnWidths[property.Name] + 2))) +
                   "+" +
                   Environment.NewLine;
        }

        string CreateHeader()
        {
            var cells = properties
                .Select(property =>
                    $" {property.Name.PadRight(columnWidths[property.Name])} ");

            return "|" +
                   string.Join("|", cells) +
                   "|" +
                   Environment.NewLine;
        }

        string CreateRow(T item)
        {
            var cells = properties.Select(property =>
            {
                var value =
                    property.GetValue(item)?.ToString()
                    ?? string.Empty;

                if (property.PropertyType == typeof(string) || property.PropertyType == typeof(char))
                {
                    return $" {value.PadRight(columnWidths[property.Name])} ";
                }

                return $" {value.PadLeft(columnWidths[property.Name])} ";
            });

            return "|" +
                   string.Join("|", cells) +
                   "|" +
                   Environment.NewLine;
        }

        writer.Write(CreateSeparator());
        writer.Write(CreateHeader());
        writer.Write(CreateSeparator());

        foreach (var item in collection)
        {
            writer.Write(CreateRow(item));
            writer.Write(CreateSeparator());
        }

        writer.Flush();
    }
}
