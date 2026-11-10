using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json.Serialization;
using API.Interfaces;

namespace API.Problems.NPComplete.NPC_AKARI.Verifiers;

class AkariVerifier : IVerifier<AKARI> {
    public const string CertificateGrammar = "{{(x,y)} | x,y are integer coordinates of lights}";
    public const string CertificateExample = "{(0,1),(2,4),(3,1)}";

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

        // TODO: implement Akari Verifier for AKARI
        int[][] problemGrid = (int[][])problem.grid.Clone();
        try {
            var lights = ParseCertificate(certificate);
        } catch {
            return false;
        }

        foreach(var light in lights) {
            if (!CheckLight(grid, light.x, light.y)) {return false;}
        }

        for(int y = 0; y < grid.Length; y++) {
            for(int x = 0; x < grid[0].Length; x++) {
                if (grid[y][x] >= 0) {
                    if (!CheckHint(grid, x, y)) {return false;}
                }
            }
        }

        return true;
    }

    private static List<(int x, int y)> ParseCertificate(int[][] grid, string certificate) {
        if (string.IsNullOrWhiteSpace(certificate)) {
               throw new CertificateParseException(problem, certificate, "certificate is empty");
           }
        
        // The certificate is scanned as written (whitespace skipped between tokens) so that
        // reported positions match what the caller sees, including multi-line instances.
        int pos = 0;
        int x;
        int y;

        void SkipWhitespace() {
            while (pos < certificate.Length && char.IsWhiteSpace(certificate[pos])) { pos++; }
        }

        void Expect(char c) {
            SkipWhitespace();
            if (pos >= certificate.Length || certificate[pos] != c) {
                string found = pos >= certificate.Length ? "end of certificate" : $"'{certificate[pos]}'";
                throw new CertificateParseException(problem, certificate, $"expected '{c}' but found {found}");
            }
            pos++;
        }

        bool TryConsume(char c) {
            SkipWhitespace();
            if (pos < certificate.Length && certificate[pos] == c) { pos++; return true; }
            return false;
        }

        int GetNumber() {
            var numberBuilder = new StringBuilder();
            SkipWhitespace();
            while(Char.IsDigit(certificate, pos))
            {
                numberBuilder.Append(certificate[pos]);
                pos++;
            }
            string numberString = numberBuilder.ToString();
            if (numberString.Length == 0) {throw new CertificateParseException(problem, certificate, "No number provided");}
            return Int32.Parse(numberString);
        }

        var lights = new List<(int x, int y)>();
        Expect('{');
        while (true) {
            Expect('(');
            x = GetNumber();
            Expect(',');
            y = GetNumber();
            Expect(')');
            lights.Add((x,y));
            if(TryConsume(',')) { continue; }
            break;
        }
        Expect('}');
        SkipWhitespace();
        if (pos != certificate.Length) {
            throw new CertificateParseException(problem, certificate, $"unexpected trailing characters");
        }
        return lights;
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

    private static bool CheckHint(int[][] grid, int x, int y) {
        int value = grid[y][x];
        int count = 0;

        if (x > 0) {count += grid[y][x - 1] == LIGHT ? 1 : 0;}
        if (x < grid[0].Length - 1) {count += grid[y][x + 1] == LIGHT ? 1 : 0;}
        if (y > 0) {count += grid[y - 1][x] == LIGHT ? 1 : 0;}
        if (y < grid.Length - 1) {count += grid[y + 1][x] == LIGHT ? 1 : 0;}

        return count == value;
    }
}
