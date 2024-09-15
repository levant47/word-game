public record Config(string ChatGptToken);

public record Position(int X, int Y);

public record Solution(Position Start, Position End, string Word);
