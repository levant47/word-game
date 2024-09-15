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

var alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

var myBlack = new Color(34, 34, 34, 255);
var myWhite = new Color(238, 238, 238, 255);
var lightGreen = new Color(144, 238, 144, 255);

Raylib.SetConfigFlags(ConfigFlags.ResizableWindow);
Raylib.InitWindow(0, 0, "Word Game");
var currentMonitor = Raylib.GetCurrentMonitor();
var (monitorWidth, monitorHeight) = (Raylib.GetMonitorWidth(currentMonitor), Raylib.GetMonitorHeight(currentMonitor));
var (initialWindowWidth, initialWindowHeight) = (monitorWidth / 2, monitorHeight / 2);
Raylib.SetWindowSize(initialWindowWidth, initialWindowHeight);
Raylib.SetWindowPosition((monitorWidth - initialWindowWidth) / 2, (monitorHeight - initialWindowHeight) / 2); // center the window
Raylib.SetTargetFPS(60);

var config = JsonConvert.DeserializeObject<Config>(File.ReadAllText("config.json"));
ChatGpt.Token = config.ChatGptToken;

var fpsCounter = new FpsCounter();
var mode = Mode.ShowingSolution;
Position? selectedCell = null;
while (!Raylib.WindowShouldClose())
{
    var frameTime = Raylib.GetFrameTime();
    fpsCounter.AddFrameTime(frameTime);

    var (windowWidth, windowHeight) = (Raylib.GetScreenWidth(), Raylib.GetScreenHeight());
    var mouse = Raylib.GetMousePosition();
    var isClick = Raylib.IsMouseButtonPressed(MouseButton.Left);
    var pressedKey = Raylib.GetKeyPressed();

    Raylib.BeginDrawing();
    Raylib.ClearBackground(myWhite);

    var cellSize = (int)(Math.Min(windowWidth, windowHeight) * 0.8 / Math.Max(table.Width, table.Height));
    var cellPadding = (int)(cellSize * 0.1);
    var fontSize = cellSize - cellPadding * 2;

    switch (mode)
    {
        case Mode.Loading:
        {
            var text = "Loading...";
            var textWidth = Raylib.MeasureText(text, fontSize);
            Raylib.DrawText(text, (windowWidth - textWidth) / 2, (windowHeight - fontSize) / 2, fontSize, myBlack);
            break;
        }
        case Mode.ShowingSolution:
        {
            if (selectedCell != null && pressedKey != 0)
            {
                var keyName = Enum.GetName((KeyboardKey)pressedKey).ToUpper();
                if (keyName.Length == 1 && alphabet.Contains(keyName[0]))
                {
                    table.Set(selectedCell, keyName[0]);
                    solutions = Solver.Solve(targetWords, table);
                    selectedCell = null;
                }
            }

            // draw "New Puzzle" button
            var newPuzzleButtonX0 = 20;
            var newPuzzleButtonY0 = 20;
            var newPuzzleButtonPadding = 5;
            var newPuzzleButtonHeight = fontSize + 2 * newPuzzleButtonPadding;
            {
                var text = "New Puzzle";
                var textHeight = newPuzzleButtonHeight - newPuzzleButtonPadding * 2;
                var textWidth = Raylib.MeasureText(text, textHeight);
                var newPuzzleButtonWidth = textWidth + newPuzzleButtonPadding * 2;
                var isHovering = IsBetween(newPuzzleButtonX0, newPuzzleButtonX0 + newPuzzleButtonWidth, mouse.X) && IsBetween(newPuzzleButtonY0, newPuzzleButtonY0 + newPuzzleButtonHeight, mouse.Y);
                if (isHovering)
                {
                    Raylib.DrawRectangle(newPuzzleButtonX0, newPuzzleButtonY0, newPuzzleButtonWidth, newPuzzleButtonHeight, myBlack);
                    Raylib.DrawText(text, newPuzzleButtonX0 + newPuzzleButtonPadding, newPuzzleButtonY0 + newPuzzleButtonPadding, textHeight, myWhite);
                    if (isClick)
                    {
                        var path = FilePicker.Open(["*.png", "*.webp"]);
                        if (path != null)
                        {
                            mode = Mode.Loading;
                            new Thread(() =>
                            {
                                var result = ChatGpt.AnalyzeImage(path);
                                targetWords = result.Words.ToArray();
                                table = LetterTable.FromMatrix(result.Table);
                                solutions = Solver.Solve(targetWords, table);
                                mode = Mode.ShowingSolution;
                            }).Start();
                        }
                    }
                }
                else
                {
                    Raylib.DrawRectangleLines(newPuzzleButtonX0, newPuzzleButtonY0, newPuzzleButtonWidth, newPuzzleButtonHeight, myBlack);
                    Raylib.DrawText(text, newPuzzleButtonX0 + newPuzzleButtonPadding, newPuzzleButtonY0 + newPuzzleButtonPadding, textHeight, myBlack);
                }
            }

            var tableX0 = 20;
            var tableY0 = newPuzzleButtonY0 + newPuzzleButtonHeight + 20;
            var letterFontSize = fontSize;

            // draw the table
            for (var column = 0; column <= table.Width; column++)
            {
                Raylib.DrawLine(tableX0 + cellSize * column, tableY0, tableX0 + cellSize * column, tableY0 + cellSize * table.Height, myBlack);
            }
            for (var row = 0; row <= table.Height; row++)
            {
                Raylib.DrawLine(tableX0, tableY0 + row * cellSize, tableX0 + table.Width * cellSize, tableY0 + row * cellSize, myBlack);
            }

            // draw the letters
            var clickedAnyLetter = false;
            for (var y = 0; y < table.Height; y++)
            {
                for (var x = 0; x < table.Width; x++)
                {
                    var letter = table.Get(new(x, y)).ToString();
                    var letterWidth = Raylib.MeasureText(letter, letterFontSize);
                    var cellX = tableX0 + cellSize * x;
                    var cellY = tableY0 + cellSize * y;
                    var textX = cellX + (cellSize - letterWidth) / 2;
                    var textY = cellY + (cellSize - letterFontSize) / 2;

                    var isSelected = selectedCell == new Position(x, y);
                    var isHovering = IsBetween(cellX, cellX + cellSize, mouse.X) && IsBetween(cellY, cellY + cellSize, mouse.Y);
                    if (isSelected)
                    {
                        Raylib.DrawRectangle(cellX, cellY + 1, cellSize - 1, cellSize - 1, OscillateColors(Raylib.GetTime(), myWhite, lightGreen));
                    }
                    else if (isHovering)
                    {
                        Raylib.DrawRectangle(cellX, cellY + 1, cellSize - 1, cellSize - 1, Color.White);
                        if (isClick)
                        {
                            selectedCell = new(x, y);
                            clickedAnyLetter = true;
                        }
                    }
                    Raylib.DrawText(letter, textX, textY, letterFontSize, myBlack);
                }
            }
            if (isClick && !clickedAnyLetter)
            {
                selectedCell = null;
            }

            // draw word list
            int? selectedWordIndex = null;
            var wordListX0 = tableX0 + table.Width * cellSize + windowWidth / 25;
            var wordListY0 = tableY0;
            var wordFontSize = fontSize;
            for (var i = 0; i < targetWords.Length; i++)
            {
                var text = $"{i + 1}. {targetWords[i]}";
                var textX0 = wordListX0;
                var textY0 = wordListY0 + i * (wordFontSize + 5);
                Raylib.DrawText(text, textX0, textY0, wordFontSize, myBlack);
                var textWidth = Raylib.MeasureText(text, wordFontSize);
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
                    Raylib.DrawText(wordIndexString, startCellX + (labelSize - textWidth) / 2, startCellY + (labelSize - textHeight) / 2, labelSize, myWhite);
                }
            }
            break;
        }
    }

    var fps = fpsCounter.CalculateFps();
    if (SHOW_FPS && fps != 0) { Raylib.DrawText($"FPS: {fps:F2}", 0, 0, 20, Color.Red); }

    Raylib.EndDrawing();
}
Raylib.CloseWindow();

Color OscillateColors(double time, Color startColor, Color endColor, double speed = 1)
{
    var t = (Math.Sin(2 * Math.PI * speed * time) + 1) / 2;
    var r = startColor.R * (1 - t) + endColor.R * t;
    var g = startColor.G * (1 - t) + endColor.G * t;
    var b = startColor.B * (1 - t) + endColor.B * t;
    return new(Round(r), Round(g), Round(b), 255);

    int Round(double value) => (int)Math.Clamp(Math.Round(value), 0, 255);
}

enum Mode
{
    Loading,
    ShowingSolution,
}
