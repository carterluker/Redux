using System.Text.Json.Serialization;
using API.Interfaces;

namespace API.Problems.NPComplete.NPC_AKARI.Verifiers;

class AkariVerifier : IVerifier<AKARI> {

    // --- Fields ---
    public string verifierName { get; } = "Akari (Light-Up) Puzzle Solution Verifier";
    public string verifierDefinition { get; } = "This is a polynomial time verifier for the Akari (Light-Up) Puzzle.";
    public string source { get; } = " ";
    public string sourceLink { get; } = "TODO";
    public string sourceFile { get; } = SourceFile.Path();
    public string[] contributors { get; } = { "Danny Fawson", "Carter Luker" };
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? complexity { get; set; } = null;
    private string _certificate = "";

    public const int LIT_CELL = -4;
    public const int LIGHT = -3;
    public const int EMPTY_CELL = -2;
    public const int WALL = -1;

    public string certificate {
        get {
            return _certificate;
        }
    }

    // --- Methods Including Constructors ---
    public AkariVerifier() {

    }

    public bool verify(AKARI problem, string certificate) {
        public const string CertificateGrammar = "{(K1, K2...Kn) | K1-Kn are equal-size char lists ('_' is an open space, '#' is a wall, '*' is a bulb, '0'-'4' is a hint)}";
        public const string CertificateExample = "((_,*,_,_,_),(_,3,*,2,*),(_,*,_,_,_),(*,_,_,1,_),(1,_,_,*,_))";


        // TODO: implement Akari Verifier for AKARI
        int[][] problemGrid = (int[][])problem.grid.Clone();
        try {
            var objects = ParseCertificate(certificate);
        } catch {
            return false;
        }

        foreach(var light in objects.lights) {
            if (!CheckLight(grid, light.x, light.y)) {return false;}
        }

        foreach(var hint in objects.hints) {
            if (!CheckHint(grid, hint.x, hint.y, hint.value)) {return false;}
        }

        return true;
    }

    private static (List<(int x, int y)> lights, List<(int x, int y, int value)> hints) ParseCertificate(int[][] grid, string certificate) {
        if (string.IsNullOrWhiteSpace(certificate)) {
               throw new CertificateParseException(problem, certificate, "certificate is empty");
           }
        
        // The certificate is scanned as written (whitespace skipped between tokens) so that
        // reported positions match what the caller sees, including multi-line instances.
        int pos = 0;
        int x = 0;
        int y = 0;

        void SkipWhitespace() {
            while (pos < certificate.Length && char.IsWhiteSpace(certificate[pos])) { pos++; }
        }

        // 1-based line and column of the current position in the original certificate.
        string Location() {
            int line = 1;
            int lineStart = 0;
            for (int i = 0; i < pos && i < certificate.Length; i++) {
                if (certificate[i] == '\n') { line++; lineStart = i + 1; }
            }
            return $"line {line}, column {pos - lineStart + 1}";
        }

        void Expect(char c) {
            SkipWhitespace();
            if (pos >= certificate.Length || certificate[pos] != c) {
                string found = pos >= certificate.Length ? "end of certificate" : $"'{certificate[pos]}'";
                throw new CertificateParseException(problem, certificate, $"expected '{c}' but found {found} at {Location()}");
            }
            pos++;
        }

        bool TryConsume(char c) {
            SkipWhitespace();
            if (pos < certificate.Length && certificate[pos] == c) { pos++; return true; }
            return false;
        }

        var lights = new List<(int x, int y)>();
        var hints = new List<(int x, int y, int value)>();
        Expect('(');
        while (true) {
            x = 0;
            Expect('(');
            while (true) {
                SkipWhitespace();
                if (pos >= certificate.Length) {
                    throw new CertificateParseException(problem, certificate, $"unexpected end of certificate inside row {y + 1} at {Location()}");
                }
                char c = certificate[pos];
                int cell = c switch {
                    '*' => EMPTY_CELL,
                    '_' => EMPTY_CELL,
                    '#' => WALL,
                    >= '0' and <= '4' => c - '0',
                    _ => throw new CertificateParseException(problem, certificate, $"invalid cell character '{c}' at row {y + 1}, cell {x + 1} ({Location()}); allowed: _, *, #, 0-4")
                };
                if (cell != grid[y][x]) {
                    throw new CertificateParseException(problem, certificate, $"problem-certificate mismatch at row {y + 1}, cell {x + 1} ({Location()})");
                }
                if (c == '*') {lights.Add((x, y));}
                if (cell >= 0 && cell <= 4) {hints.add((x, y, cell));}
                pos++;
                x++;
                if (TryConsume(',')) { continue; }
                break;
            }
            Expect(')');
            if ((x + 1) != grid[0].Length) {
                throw new CertificateParseException(problem, input, $"mismatch: certificate has {x + 1} cells in row {y + 1}, solution has {grid[0].Length}");
            }
            y++;
            if (TryConsume(',')) { continue; }
            break;
        }
        if ((y + 1) != grid.Length) {
                throw new CertificateParseException(problem, input, $"mismatch: certificate has {y + 1} rows, solution has {grid.Length}");
        }
        Expect(')');
        SkipWhitespace();
        if (pos != certificate.Length) {
            throw new CertificateParseException(problem, certificate, $"unexpected trailing characters at {Location()}");
        }
        return (lights, hints);
    }

    private static bool CheckLight(int[][] grid, int x, int y) {
        if (grid[y][x] != EMPTY_CELL) {return false;}

        grid[y][x] = LIGHT;

        TraceLightPath(grid, x, y, 1, 0);
        TraceLightPath(grid, x, y, -1, 0);
        TraceLightPath(grid, x, y, 0, 1);
        TraceLightPath(grid, x, y, 0, -1);

        return true;
    }

    private static void TraceLightPath(int[][] grid, int x, int y, int deltaX, int deltaY) {
        x += deltaX;
        y += deltaY;

        while (x >= 0 && x < grid[0].Length && y >= 0 && y < grid.Length) {
            if (grid[y][x] != EMPTY_CELL && grid[y][x] != LIT_CELL) {return;}

            grid[y][x] = LIT_CELL;

            x += deltaX;
            y += deltaY;
        }
    }

    private static bool CheckHint(int[][] grid, int x, int y, int value) {
        int count = 0;

        if (x > 0) {count += grid[y][x - 1] == LIGHT ? 1 : 0;}
        if (x < grid[0].Length - 1) {count += grid[y][x + 1] == LIGHT ? 1 : 0;}
        if (y > 0) {count += grid[y - 1][x] == LIGHT ? 1 : 0;}
        if (y < grid.Length - 1) {count += grid[y + 1][x] == LIGHT ? 1 : 0;}

        return count == value;
    }
}
