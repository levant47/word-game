public static class Solver
{
    private record WordCandidate(string Value, string Direction);

    private enum Direction
    {
        DirectHorizontal,
        ReverseHorizontal,
        DirectVertical,
        ReverseVertical,
        DiagonalNw,
        DiagonalNe,
        DiagonalSw,
        DiagonalSe,
    };

    public static List<Solution> Solve(string[] targetWords, LetterTable table)
    {
        var result = new List<Solution>();
        for (var y0 = 0; y0 < table.Height; y0++)
        {
            for (var x0 = 0; x0 < table.Width; x0++)
            {
                var directions = Enum.GetValues(typeof(Direction)).Cast<Direction>().ToList();
                foreach (var word in targetWords)
                {
                    foreach (var direction in directions)
                    {
                        var positions = GeneratePositionsForDirection(new(x0, y0), direction, table).Take(word.Length).ToList();
                        var candidate = string.Concat(positions.Select(table.Get));
                        if (candidate == word)
                        {
                            result.Add(new(positions.First(), positions.Last(), word));
                        }
                    }
                }
            }
        }
        return result;
    }

    private static IEnumerable<Position> GeneratePositionsForDirection(Position start, Direction direction, LetterTable table) => direction switch
    {
        Direction.DirectHorizontal => Enumerable.Range(start.X, table.Width - start.X).Select(x => new Position(x, start.Y)),
        Direction.ReverseHorizontal => Enumerable.Range(0, start.X + 1).Select(x => new Position(x, start.Y)).Reverse(),
        Direction.DirectVertical => Range(start.Y, table.Height).Select(y => new Position(start.X, y)),
        Direction.ReverseVertical => Range(start.Y, 0, -1).Select(y => new Position(start.X, y)),
        Direction.DiagonalNw => DoubleRange(start.X, -1, -1, start.Y, -1, -1).Select(pair => new Position(pair.x, pair.y)),
        Direction.DiagonalNe => DoubleRange(start.X, table.Width, 1, start.Y, -1, -1).Select(pair => new Position(pair.x, pair.y)),
        Direction.DiagonalSw => DoubleRange(start.X, -1, -1, start.Y, table.Height, 1).Select(pair => new Position(pair.x, pair.y)),
        Direction.DiagonalSe => DoubleRange(start.X, table.Width, 1, start.Y, table.Height, 1).Select(pair => new Position(pair.x, pair.y)),
        _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, null)
    };

    private static IEnumerable<int> Range(int start, int end, int step = 1)
    {
        for (var i = start; i != end; i += step)
        {
            yield return i;
        }
    }

    private static IEnumerable<(int x, int y)> DoubleRange(int start1, int end1, int step1, int start2, int end2, int step2)
    {
        var i = start1;
        var k = start2;
        while (i != end1 && k != end2)
        {
            yield return (i, k);
            i += step1;
            k += step2;
        }
    }
}
