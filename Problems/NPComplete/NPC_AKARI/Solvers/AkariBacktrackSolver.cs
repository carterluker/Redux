using API.Interfaces;

namespace API.Problems.NPComplete.NPC_AKARI.Solvers;

class AkariBacktrackSolver : ISolver<AKARI> {

    // --- Fields ---
    public string solverName { get; } = "Akari Backtracking Solver";
    public string solverDefinition { get; } = "Places every bulb forced by numbered walls, then runs a depth-first include/exclude search over the remaining candidate cells, pruning branches where a numbered wall can no longer be satisfied.";
    public string source { get; } = "Pulles, B. 2021. Analysis of Akari. Bachelor's thesis, Radboud University. https://www.cs.ru.nl/bachelors-theses/2021/Bram_Pulles___1015194___Analysis_of_Akari.pdf (solver source: https://github.com/Borroot/akari)";
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

    // C# port of the backtracking ("track") solver by Bram Pulles:
    //   Paper: https://www.cs.ru.nl/bachelors-theses/2021/Bram_Pulles___1015194___Analysis_of_Akari.pdf
    //   Repo:  https://github.com/Borroot/akari (src/tracksolver.py)
    // Naming conventions mirror the original so the two can be compared.
    // Deviations from the original:
    //   1. In Backtrack, the bulb is added to the solution BEFORE PlaceBulb (as the Python trivial
    //      solver already does), so a just-saturated clue wall prunes its other neighbours immediately.
    //      This finds the same first solution while exploring fewer nodes.
    //   2. TrivialSolve iterates over a snapshot of the wall list instead of mutating it mid-iteration.
    //   3. Only the first solution is searched for; the solution count cap and difficulty stats are dropped.

    // Left, up, right, down (same order as DIRS in the Python constants.py).
    private static readonly (int dr, int dc)[] DIRS = { (0, -1), (-1, 0), (0, 1), (1, 0) };

    // Per-solve state, gets reset at the start of solve().
    private int[][] grid = Array.Empty<int[]>();
    private int rows, cols;
    private List<(int r, int c)> clueWalls = new();    // walls with clue 1-4, row-major
    private List<(int r, int c)> solution = new();
    private HashSet<(int r, int c)> solutionSet = new();
    private bool timedOut;

    // --- Methods Including Constructors ---
    public AkariBacktrackSolver() { }

    private bool InBounds(int r, int c) => r >= 0 && r < rows && c >= 0 && c < cols;

    private static bool IsClue(int cell) => cell >= 0 && cell <= 4;

    private IEnumerable<(int r, int c)> Neighbours((int r, int c) cell) {
        foreach (var (dr, dc) in DIRS) {
            int nr = cell.r + dr, nc = cell.c + dc;
            if (InBounds(nr, nc)) yield return (nr, nc);
        }
    }

    private int PlacedAround((int r, int c) wall) => Neighbours(wall).Count(solutionSet.Contains);

    // Python: _place_bulb. The caller must add `cell` to the solution before calling this.
    // Removes the cell and everything it lights from shadows/candidates, and removes the
    // neighbours of any adjacent clue wall that this bulb saturates from candidates.
    private void PlaceBulb((int r, int c) cell, HashSet<(int r, int c)> shadows, List<(int r, int c)> candidates) {
        shadows.Remove(cell);
        candidates.Remove(cell);

        foreach (var (dr, dc) in DIRS) {
            int r = cell.r + dr, c = cell.c + dc;

            if (InBounds(r, c) && IsClue(grid[r][c])) {
                if (PlacedAround((r, c)) == grid[r][c])
                    foreach (var n in Neighbours((r, c))) candidates.Remove(n);
            } else {
                while (InBounds(r, c) && grid[r][c] == AKARI.EMPTY_CELL) {
                    shadows.Remove((r, c));
                    candidates.Remove((r, c));
                    r += dr;
                    c += dc;
                }
            }
        }
    }

    // Python: _remove_zeros + _trivialsolve. Places every bulb forced by a clue wall whose
    // remaining need equals its open candidate neighbours, until nothing changes.
    // Returns false if some clue can never be satisfied.
    private bool TrivialSolve(HashSet<(int r, int c)> shadows, List<(int r, int c)> candidates) {
        for (int r = 0; r < rows; r++)
            for (int c = 0; c < cols; c++)
                if (grid[r][c] == 0)
                    foreach (var n in Neighbours((r, c))) candidates.Remove(n);

        var walls = new List<(int r, int c)>(clueWalls);
        bool done = false;

        while (!done) {
            done = true;

            foreach (var wall in walls.ToList()) {
                int number = grid[wall.r][wall.c] - PlacedAround(wall);
                var open = Neighbours(wall).Where(candidates.Contains).ToList();

                if (open.Count < number) return false;

                if (open.Count == number) {
                    done = false;
                    walls.Remove(wall);
                    foreach (var n in open) {
                        solution.Add(n);
                        solutionSet.Add(n);
                        PlaceBulb(n, shadows, candidates);
                    }
                }
            }
        }
        return true;
    }

    // Python: _wall_unsatisfiable. True if a clue wall needs more bulbs than it has open neighbours.
    private bool WallUnsatisfiable(List<(int r, int c)> candidates) =>
        clueWalls.Any(w => grid[w.r][w.c] - PlacedAround(w) > Neighbours(w).Count(candidates.Contains));

    // Python: _walls_satisfied. True if every clue wall has exactly its number of adjacent bulbs.
    private bool WallsSatisfied() =>
        clueWalls.All(w => PlacedAround(w) == grid[w.r][w.c]);

    // Python: _sort_candidates. Ordered so that popping from the end yields clue-adjacent cells
    // first, then lowest column, then lowest row (Python sorts (x, y) tuples, x = column).
    private List<(int r, int c)> SortCandidates(IEnumerable<(int r, int c)> candidates) =>
        candidates
            .OrderByDescending(cell => Neighbours(cell).Any(n => grid[n.r][n.c] >= 1 && grid[n.r][n.c] <= 4) ? 0 : 1)
            .ThenByDescending(cell => cell.c)
            .ThenByDescending(cell => cell.r)
            .ToList();

    // Python: _backtracksolve. Depth-first include/exclude search over the candidates, stopping at
    // the first solution. Branch A mutates the live collections; branch B resumes from copies.
    private bool Backtrack(HashSet<(int r, int c)> shadows, List<(int r, int c)> candidates) {
        if (timerHasExpired) {
            timedOut = true;
            return false;
        }

        if (shadows.Count == 0 && WallsSatisfied()) return true;
        if (candidates.Count == 0 || WallUnsatisfiable(candidates)) return false;

        var candidate = candidates[^1];
        candidates.RemoveAt(candidates.Count - 1);
        var shadowsCopy = new HashSet<(int r, int c)>(shadows);
        var candidatesCopy = new List<(int r, int c)>(candidates);

        // Try with a light bulb at this place.
        solution.Add(candidate);
        solutionSet.Add(candidate);
        PlaceBulb(candidate, shadows, candidates);
        if (Backtrack(shadows, candidates)) return true;
        solution.RemoveAt(solution.Count - 1);
        solutionSet.Remove(candidate);
        if (timedOut) return false;

        // Try with no light bulb at this place.
        return Backtrack(shadowsCopy, candidatesCopy);
    }

    // Python: _trackanalyse / tracksolve.
    public string solve(AKARI problem) {
        grid = problem.grid;
        rows = grid.Length;
        cols = rows > 0 ? grid[0].Length : 0;
        clueWalls = new();
        solution = new();
        solutionSet = new();
        timedOut = false;

        // Shadows: empty cells not yet lit. Candidates: empty cells that may still take a bulb.
        var shadows = new HashSet<(int r, int c)>();
        var candidates = new List<(int r, int c)>();
        for (int r = 0; r < rows; r++) {
            for (int c = 0; c < cols; c++) {
                if (grid[r][c] == AKARI.EMPTY_CELL) {
                    shadows.Add((r, c));
                    candidates.Add((r, c));
                } else if (grid[r][c] >= 1 && grid[r][c] <= 4) {
                    clueWalls.Add((r, c));
                }
            }
        }

        if (!TrivialSolve(shadows, candidates)) return "{}";

        bool solved = Backtrack(shadows, SortCandidates(candidates));
        if (timedOut) return "timeout";
        return solved ? FormatSolution() : "{}";
    }

    // The instance format with bulbs marked 'L', e.g. ((_,L,#),(L,2,_)).
    private string FormatSolution() {
        var rowStrings = grid.Select((row, r) => "(" + string.Join(",", row.Select((cell, c) =>
            solutionSet.Contains((r, c)) ? "L"
            : cell == AKARI.EMPTY_CELL ? "_"
            : cell == AKARI.WALL ? "#"
            : cell.ToString())) + ")");
        return "(" + string.Join(",", rowStrings) + ")";
    }
}
