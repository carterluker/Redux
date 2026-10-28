using System.Text.Json.Serialization;
using API.Interfaces;

namespace API.Problems.NPComplete.NPC_AKARI.Verifiers;

class AkariVerifier : IVerifier<AKARI> {
    public const string CertificateGrammar = "a set of 0-indexed (row,column) bulb positions on white cells, e.g. {(r1,c1),(r2,c2)}";
    // Solves AKARI's default instance.
    public const string CertificateExample = "{(0,1),(1,0),(2,2)}";

    // --- Fields ---
    public string verifierName { get; } = "Akari Solution Verifier";
    public string verifierDefinition { get; } = "TODO";
    public string source { get; } = "TODO";
    public string sourceFile { get; } = SourceFile.Path();
    public string sourceLink { get; } = "TODO";
    public string[] contributors { get; } = { "Danny Fawson" };
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? complexity { get; set; } = null;
    private string _certificate = "";

    public string certificate {
        get {
            return _certificate;
        }
    }

    // --- Methods Including Constructors ---
    public AkariVerifier() {

    }

    public bool verify(AKARI problem, string certificate) {
        // On a malformed certificate, throw `CertificateParseException`
        // (see Interfaces/ParseExceptions.cs) rather than silently
        // returning false. The controller layer catches it and returns
        // HTTP 400 with `problem.certificateFormat` quoted back to the
        // caller.
        //
        // Return false only when the certificate parses successfully but
        // fails the verification predicate; return true when it satisfies it.

        // TODO: implement Akari Solution Verifier for AKARI
        // An unfinished verifier must fail loudly, not accept every certificate.
        throw new NotImplementedException("TODO: implement Akari Solution Verifier for AKARI");
    }
}
