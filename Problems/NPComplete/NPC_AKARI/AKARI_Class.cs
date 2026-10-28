using API.Interfaces;
using API.DummyClasses;
using API.Problems.NPComplete.NPC_AKARI.Solvers;
using API.Problems.NPComplete.NPC_AKARI.Verifiers;

namespace API.Problems.NPComplete.NPC_AKARI;

class AKARI : IProblem<AkariSolver, AkariVerifier, DummyVisualization> {

    // --- Fields ---
    public string problemName { get; } = "Akari";
    public string problemLink { get; } = "https://en.wikipedia.org/wiki/Light_Up_(puzzle)";
    public string formalDefinition { get; } =
        "AKARI = {G | G is an m×n grid whose cells are white or black, some black cells carry a number 0…4, " +
        "and there exists a set L of white cells (light bulbs) such that: " +
        "(1) every white cell is in L or shares a row or column with a bulb in L with no black cell between them; " +
        "(2) no two bulbs in L share a row or column with no black cell between them; " +
        "(3) every numbered black cell has exactly that many bulbs in L orthogonally adjacent to it }";
    public string problemDefinition { get; } = "Akari (also called Light Up) is a grid puzzle of white cells and black walls, some of which are numbered. The goal is to place light bulbs on white cells so that every white cell is lit, no bulb shines on another bulb, and each numbered wall touches exactly that many bulbs. A bulb lights its own cell and every cell in its row and column up to the nearest wall.";
    public string inputDescription { get; } = "A rectangular grid of white cells, unnumbered black walls, and black walls numbered 0-4";
    public string outputDescription { get; } = "A set of (row,column) bulb positions that lights the whole grid under the Akari rules, or {} if no placement exists";
    public string source { get; } = "McPhail, Brandon. 2005. \"Light Up is NP-Complete.\"";
    public string sourceFile { get; } = SourceFile.Path();
    public string sourceLink { get; } = "https://en.wikipedia.org/wiki/Light_Up_(puzzle)";
    public const string InstanceGrammar = "a 2-dimensional nested list: [ [row 1], [row 2], ... ], each row a comma-separated list of cells, where '.' is a white cell, '#' is an unnumbered black wall, and 0-4 is a black wall with that number; every row must have the same length";
    // A small 3x3 test instance. Solved by bulbs at (0,1), (1,0), (2,2).
    private static readonly string _defaultInstance = "[[.,.,1],[.,#,.],[.,.,.]]";
    public string defaultInstance { get; } = _defaultInstance;
    public string instanceFormat { get; } = $"Format: {InstanceGrammar} Example: {_defaultInstance}";
    public string certificateFormat { get; } =
        $"Format: {AkariVerifier.CertificateGrammar} Example: {AkariVerifier.CertificateExample}";
    public string instance { get; set; } = string.Empty;
    public string wikiName { get; } = "";
    public AkariSolver defaultSolver { get; } = new AkariSolver();
    public AkariVerifier defaultVerifier { get; } = new AkariVerifier();
    public DummyVisualization defaultVisualization { get; } = new DummyVisualization();
    public string[] contributors { get; } = { "Danny Fawson" };
    // Declared, not derived. Deciding whether an Akari (Light Up) puzzle has a solution
    // is NP-complete (McPhail, 2005).
    public ComplexityClass complexityClass { get; } = ComplexityClass.NPComplete;
    public ProblemType problemType { get; } = ProblemType.GamesAndPuzzles;

    // Cell values in grid: a white cell, an unnumbered wall, or a numbered wall 0-4.
    public const int White = -1;
    public const int Wall = -2;
    public const int MaxNumber = 4;

    // --- Properties ---
    public int[][] grid { get; }
    public int rows => grid.Length;
    public int cols => grid.Length == 0 ? 0 : grid[0].Length;

    // --- Methods and Constructors ---
    public AKARI() : this(_defaultInstance) {

    }

    public AKARI(string stringInstance) {
        instance = stringInstance;
        grid = ParseGrid(stringInstance ?? string.Empty);
    }

    // True for a black cell, numbered or not.
    public bool IsWall(int row, int col) => grid[row][col] != White;

    // Reads "[[.,.,1],[.,#,.]]" into a grid of White, Wall, or 0-4. Whitespace is ignored.
    private int[][] ParseGrid(string input) {
        string text = string.Concat(input.Where(c => !char.IsWhiteSpace(c)));

        if (text.Length < 4 || !text.StartsWith("[[") || !text.EndsWith("]]"))
            throw new ProblemParseException(problemName, input, "Akari grid must be a nested list starting with '[[' and ending with ']]'.");

        // Strip the outer "[[" and "]]", leaving rows joined by "],[".
        string[] rowTexts = text[2..^2].Split("],[");
        var parsed = new int[rowTexts.Length][];

        for (int r = 0; r < rowTexts.Length; r++) {
            if (rowTexts[r].Contains('[') || rowTexts[r].Contains(']'))
                throw new ProblemParseException(problemName, input, $"Row {r + 1} has a misplaced bracket; rows must be separated by '],['.");

            string[] cells = rowTexts[r].Split(',');
            if (r > 0 && cells.Length != parsed[0].Length)
                throw new ProblemParseException(problemName, input, $"Row {r + 1} has {cells.Length} cells but row 1 has {parsed[0].Length}; the grid must be rectangular.");

            parsed[r] = new int[cells.Length];
            for (int c = 0; c < cells.Length; c++)
                parsed[r][c] = ParseCell(cells[c], r, c, input);
        }

        return parsed;
    }

    private int ParseCell(string cell, int row, int col, string input) {
        if (cell == ".")
            return White;
        if (cell == "#")
            return Wall;
        if (cell.Length == 1 && cell[0] >= '0' && cell[0] <= '0' + MaxNumber)
            return cell[0] - '0';
        throw new ProblemParseException(problemName, input, $"Row {row + 1}, column {col + 1}: '{cell}' is not a cell; use '.' (white), '#' (wall), or 0-{MaxNumber} (numbered wall).");
    }
}
