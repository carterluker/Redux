using API.Interfaces;

namespace API.Problems.NPComplete.NPC_AKARI.Solvers;

class AkariBacktrackSolver : ISolver<AKARI> {

    // --- Fields ---
    public string solverName { get; } = "Akari Backtracking Solver";
    public string solverDefinition { get; } = "TODO";
    public string source { get; } = "TODO";
    public string sourceFile { get; } = SourceFile.Path();
    public string[] contributors { get; } = { "Carter Luker" };
    // Declared, not derived — see API.Interfaces.SolverType and
    // API.Interfaces.SolverComplexityBucket. Leave these commented out (the
    // interface defaults to Unclassified) until you've actually read the
    // algorithm below and are confident which values fit — don't guess from
    // the solver's name. Getting this wrong is a public correctness claim
    // the API will serve to callers.
    // public SolverType solverType {get;} = SolverType.TODO;
    // public SolverComplexityBucket complexityBucket {get;} = SolverComplexityBucket.TODO;
    // Only set this to a confidently-known Big-O string, never a guess, e.g. "O(n log n)".
    // public string complexity {get;} = "TODO";
    public bool timerHasExpired { get; set; }

    // --- Methods Including Constructors ---
    public AkariBacktrackSolver() { }

    public string solve(AKARI problem) {
        // To get parsed problem instance, use the problem.grid property.

        // Problem property copies
        int[][] grid = problem.grid;
        const int EMPTY_CELL = AKARI.EMPTY_CELL;
        const int WALL = AKARI.WALL;

        // Set of grid coordinates for each grid element type.
        HashSet<(int, int)> emptyCells = new(); 
        HashSet<(int, int)> unhintedWalls = new();
        HashSet<(int, int)> hintedWalls = new();

        // Build grid element sets
        for (int i = 0; i < grid.GetLength(0); i++) {
            for (int j = 0; j < grid.GetLength(1); j++) {
                switch (grid[i][j]) {
                    case EMPTY_CELL:
                        emptyCells.Add((i, j));
                        break;
                    case WALL:
                        unhintedWalls.Add((i, j));
                        break;
                    case 0 or 1 or 2 or 3 or 4:
                        hintedWalls.Add((i, j));
                        break;
                }
            }
        }

        

        // make sure to check timerHasExpired occasionally, and return if it has
        if (timerHasExpired)
            return "timeout";

        return "{}";
    }
}
