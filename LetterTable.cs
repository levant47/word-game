public class LetterTable
{
    public int Width;
    public int Height;

    private List<char> _cells;

    public static LetterTable Parse(string source)
    {
        var rows = source.Split(Environment.NewLine);
        return new()
        {
            _cells = rows.SelectMany(c => c).ToList(),
            Width = rows[0].Length,
            Height = rows.Length,
        };
    }

    public char Get(Position position) => _cells[position.Y * Width + position.X];
}
