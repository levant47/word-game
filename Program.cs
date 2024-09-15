using Raylib_cs;
using static MyMath;

var SHOW_FPS = true;
var SOURCE = """
HTLHHUHNKSEKEINES
EDDWINIEESTNWKBWR
UKLWOHRWSOHRUDMSH
WDESSTALLDNHIEMDD
ICFDKGKBTSIDDSHSA
DSBSNFAHCSEADIPCB
TPFERDWKBKUHIBRNS
IEMHNMWDMSOHRABLN
SCHAFEIEMDATEETOL
AIEGBHDSISBWKUWMR
OWMIWERSEDBWDESNN
TNBAUERNTOEICRDIK
SECSBWINIEEDSISEL
RSEGEIZWREBWINIWA
OBWIWDMSENMSOHRHI
TMSOICMDATMSATECP
KMDADSBSWEKBTLDSL
AENEIERNIWDMSRIKA
RLTTMSOHRICMDDOEW
TISRMDATMILCHSKHL
""";
var TARGET_WORDS_SOURCE = """
BAUER
BAEUERIN
WEIDE
TRAKTOR
SCHEUNE
FELD
STALL
HEU
STROH
SCHWEIN
KUH
SCHAF
PFERD
ZIEGE
HUHN
ENTE
MILCH
EIER
""";
var targetWords = TARGET_WORDS_SOURCE.Split(Environment.NewLine);
var table = LetterTable.Parse(SOURCE);
var solutions = Solver.Solve(targetWords, table);

Raylib.SetConfigFlags(ConfigFlags.ResizableWindow);
Raylib.InitWindow(0, 0, "Word Game");
var currentMonitor = Raylib.GetCurrentMonitor();
var (monitorWidth, monitorHeight) = (Raylib.GetMonitorWidth(currentMonitor), Raylib.GetMonitorHeight(currentMonitor));
var (initialWindowWidth, initialWindowHeight) = (monitorWidth / 2, monitorHeight / 2);
Raylib.SetWindowSize(initialWindowWidth, initialWindowHeight);
Raylib.SetWindowPosition((monitorWidth - initialWindowWidth) / 2, (monitorHeight - initialWindowHeight) / 2); // center the window
Raylib.SetTargetFPS(60);

var fpsCounter = new FpsCounter();
while (!Raylib.WindowShouldClose())
{
    var frameTime = Raylib.GetFrameTime();
    fpsCounter.AddFrameTime(frameTime);

    var (windowWidth, windowHeight) = (Raylib.GetScreenWidth(), Raylib.GetScreenHeight());

    Raylib.BeginDrawing();
    Raylib.ClearBackground(Color.White);

    var cellSize = (int)(Math.Min(windowWidth, windowHeight) * 0.8 / Math.Max(table.Width, table.Height));
    var cellPadding = (int)(cellSize * 0.1);
    var letterFontSize = cellSize - cellPadding * 2;
    var tableX0 = 20;
    var tableY0 = 20;

    // draw the table
    for (var column = 0; column <= table.Width; column++)
    {
        Raylib.DrawLine(tableX0 + cellSize * column, tableY0, tableX0 + cellSize * column, tableY0 + cellSize * table.Height, Color.Black);
    }
    for (var row = 0; row <= table.Height; row++)
    {
        Raylib.DrawLine(tableX0, tableY0 + row * cellSize, tableX0 + table.Width * cellSize, tableY0 + row * cellSize, Color.Black);
    }

    // draw the letters
    for (var y = 0; y < table.Height; y++)
    {
        for (var x = 0; x < table.Width; x++)
        {
            var letter = table.Get(new(x, y)).ToString();
            var letterWidth = Raylib.MeasureText(letter, letterFontSize);
            var cellX = tableX0 + cellSize * x + (cellSize - letterWidth) / 2;
            var cellY = tableY0 + cellSize * y + (cellSize - letterFontSize) / 2;
            Raylib.DrawText(letter, cellX, cellY, letterFontSize, Color.Black);
        }
    }

    // draw word list
    int? selectedWordIndex = null;
    var wordListX0 = tableX0 + table.Width * cellSize + windowWidth / 25;
    var wordListY0 = tableY0;
    var wordFontSize = letterFontSize;
    for (var i = 0; i < targetWords.Length; i++)
    {
        var text = $"{i + 1}. {targetWords[i]}";
        var textX0 = wordListX0;
        var textY0 = wordListY0 + i * (wordFontSize + 5);
        Raylib.DrawText(text, textX0, textY0, wordFontSize, Color.Black);
        var textWidth = Raylib.MeasureText(text, wordFontSize);
        var mouse = Raylib.GetMousePosition();
        if (IsBetween(textX0, textX0 + textWidth, mouse.X) && IsBetween(textY0, textY0 + wordFontSize, mouse.Y))
        {
            Raylib.DrawRectangleLines(textX0, textY0, textWidth, wordFontSize, Color.Red);
            selectedWordIndex = i;
        }
    }

    // draw the solutions
    {
        var highlightColor = new Color(255, 255, 0, 127);
        var selectedColor = new Color(255, 0, 0, 127);
        foreach (var solution in solutions)
        {
            var wordIndex = Array.IndexOf(targetWords, solution.Word);
            var wordIndexString = (wordIndex + 1).ToString();

            var startCellX = tableX0 + solution.Start.X * cellSize;
            var startCellY = tableY0 + solution.Start.Y * cellSize;
            var startX = startCellX + cellSize / 2;
            var startY = startCellY + cellSize / 2;
            var endX = tableX0 + solution.End.X * cellSize + cellSize / 2 + Math.Sign(solution.End.X - solution.Start.X) * cellSize / 4;;
            var endY = tableY0 + solution.End.Y * cellSize + cellSize / 2 + Math.Sign(solution.End.Y - solution.Start.Y) * cellSize / 4;;
            Raylib.DrawCircle(startX, startY, cellSize / 2, selectedWordIndex == wordIndex ? selectedColor : highlightColor);
            Raylib.DrawLineEx(new(startX, startY), new(endX, endY), cellSize / 3, selectedWordIndex == wordIndex ? selectedColor : highlightColor);

            // draw word index
            var labelSize = cellSize / 3;
            var measuredTextSize = Raylib.MeasureTextEx(Raylib.GetFontDefault(), wordIndexString, labelSize, 0);
            var textWidth = (int)Math.Round(measuredTextSize.X);
            var textHeight = (int)Math.Round(measuredTextSize.Y - labelSize * 0.3);
            Raylib.DrawRectangle(startCellX, startCellY + 1, Math.Max(labelSize, textWidth + 2), labelSize, Color.Red);
            Raylib.DrawText(wordIndexString, startCellX + (labelSize - textWidth) / 2, startCellY + (labelSize - textHeight) / 2, labelSize, Color.White);
        }
    }

    var fps = fpsCounter.CalculateFps();
    if (SHOW_FPS && fps != 0) { Raylib.DrawText($"FPS: {fps:F2}", 0, 0, 20, Color.Red); }

    Raylib.EndDrawing();
}
Raylib.CloseWindow();
