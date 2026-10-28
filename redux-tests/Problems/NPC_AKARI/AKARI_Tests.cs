using Xunit;
using API.Interfaces;
using API.Problems.NPComplete.NPC_AKARI;

namespace redux_tests;
#pragma warning disable CS1591

public class AKARI_Tests {

    [Fact]
    public void AKARI_DefaultInstance_Builds() {
        AKARI problem = new AKARI();
        Assert.Equal(problem.defaultInstance, problem.instance);
    }

    [Fact]
    public void AKARI_DefaultInstance_HoldsExpectedValues() {
        // [[.,.,1],[.,#,.],[.,.,.]]
        AKARI problem = new AKARI();
        Assert.Equal(3, problem.rows);
        Assert.Equal(3, problem.cols);
        Assert.Equal(new[] { AKARI.White, AKARI.White, 1 }, problem.grid[0]);
        Assert.Equal(new[] { AKARI.White, AKARI.Wall, AKARI.White }, problem.grid[1]);
        Assert.Equal(new[] { AKARI.White, AKARI.White, AKARI.White }, problem.grid[2]);
    }

    [Fact]
    public void AKARI_Constructor_IgnoresWhitespace() {
        AKARI problem = new AKARI("[ [ ., 0 ],\n  [ #, 4 ] ]");
        Assert.Equal(new[] { AKARI.White, 0 }, problem.grid[0]);
        Assert.Equal(new[] { AKARI.Wall, 4 }, problem.grid[1]);
    }

    [Fact]
    public void AKARI_Constructor_ParsesSingleCell() {
        AKARI problem = new AKARI("[[.]]");
        Assert.Equal(1, problem.rows);
        Assert.Equal(1, problem.cols);
        Assert.False(problem.IsWall(0, 0));
    }

    [Theory]
    [InlineData("")]
    [InlineData("{{{")]
    [InlineData("[[]]")]
    [InlineData("[.,.]")]
    [InlineData("[[.,.],[.]]")]       // not rectangular
    [InlineData("[[.,5],[.,.]]")]     // numbers only go up to 4
    [InlineData("[[.,x],[.,.]]")]     // unknown cell
    [InlineData("[[.,.],,[.,.]]")]    // bad row separator
    [InlineData("[[.,.],[[.,.]]")]    // stray bracket
    public void AKARI_Constructor_ThrowsOnMalformedInstance(string instance) {
        Assert.Throws<ProblemParseException>(() => new AKARI(instance));
    }
}
