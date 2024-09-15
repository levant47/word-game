public class LetterTable
{
    public int Width;
    public int Height;

    private List<List<char>> _cells;

    public static LetterTable Parse(string source)
    {
        var rows = source.Split(Environment.NewLine);
        return new()
        {
            _cells = rows.Select(line => line.ToList()).ToList(),
            Width = rows[0].Length,
            Height = rows.Length,
        };
    }

    public static LetterTable FromMatrix(List<List<char>> matrix) => new()
    {
        Width = matrix.Max(row => row.Count),
        Height = matrix.Count,
        _cells = matrix,
    };

    public char Get(Position position) => position.Y < _cells.Count && position.X < _cells[position.Y].Count ? _cells[position.Y][position.X] : ' ';
}
