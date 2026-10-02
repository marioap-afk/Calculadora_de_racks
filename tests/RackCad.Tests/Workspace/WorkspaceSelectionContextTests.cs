#nullable enable annotations
using System.Collections.Generic;
using RackCad.Application.Workspace;
using Xunit;

namespace RackCad.Tests;

// I-64 F1-T1-MODEL: D-05, D-06 / INV-16, INV-17.
public class WorkspaceSelectionContextTests
{
    private static DefinitionFact Ok(string id) => new(true, id);

    private static SelectionContext Classify(int others, params DefinitionFact[] facts) =>
        SelectionContext.Classify(new List<DefinitionFact>(facts), others);

    [Fact]
    public void NothingSelectedIsNone()
    {
        var c = Classify(0);
        Assert.Equal(SelectionContextKind.None, c.Kind);
        Assert.Null(c.RackId);
    }

    [Fact]
    public void OneInterpretableReferenceWithAnIdIsOneWithThatId()
    {
        var c = Classify(0, Ok("R1"));
        Assert.Equal(SelectionContextKind.One, c.Kind);
        Assert.Equal("R1", c.RackId);
    }

    [Fact]
    public void SeveralReferencesOfTheSameRackAreStillOne()
    {
        var c = Classify(0, Ok("R1"), Ok("R1"));
        Assert.Equal(SelectionContext.One("R1"), c);
    }

    [Fact]
    public void SeveralRacksAreASelectionWithTheirCount()
    {
        var c = Classify(0, Ok("R1"), Ok("R2"), Ok("R3"));
        Assert.Equal(SelectionContextKind.Selection, c.Kind);
        Assert.Equal(3, c.RackCount);
        Assert.Equal(0, c.OtherCount);
        Assert.Null(c.RackId);
    }

    [Fact]
    public void RacksPlusNonRackEntitiesCountBothSeparately()
    {
        var c = Classify(4, Ok("R1"));
        Assert.Equal(SelectionContextKind.Selection, c.Kind);
        Assert.Equal(1, c.RackCount);
        Assert.Equal(4, c.OtherCount);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\t\r\n")]
    public void ABlankOrMissingIdIsNoIdentityAndNeverInventsARackId(string? id)
    {
        var c = Classify(0, new DefinitionFact(true, id));
        Assert.Equal(SelectionContextKind.NoIdentity, c.Kind);
        Assert.Null(c.RackId);
    }

    [Fact]
    public void AnUnreadableDefinitionIsADiagnosticEvenIfProbingAttributesAnId()
    {
        var c = Classify(0, new DefinitionFact(false, null, "R1"));
        Assert.Equal(SelectionContextKind.Diagnostic, c.Kind);
        Assert.Null(c.RackId);

        var withEnvelopeId = Classify(0, new DefinitionFact(false, "R1", "R1"));
        Assert.Equal(SelectionContextKind.Diagnostic, withEnvelopeId.Kind);
    }

    [Fact]
    public void OneIsNeverBuiltFromABlankId()
    {
        Assert.Throws<System.ArgumentException>(() => SelectionContext.One(" "));
    }

    [Fact]
    public void EqualContextsAreEqualSoTheSameSelectionDoesNotNavigate()
    {
        Assert.Equal(Classify(0, Ok("R1")), Classify(0, Ok("R1")));
        Assert.NotEqual(Classify(0, Ok("R1")), Classify(0, Ok("R2")));
    }

    [Fact]
    public void ApplyingAnEqualContextIsNotAChange()
    {
        var s = new WorkspaceSessionRegistry().GetOrCreate(1);
        Assert.True(s.ApplyContext(SelectionContext.One("R1")));
        Assert.False(s.ApplyContext(SelectionContext.One("R1")));
        Assert.Equal(SelectionContext.One("R1"), s.Selection);
        Assert.True(s.ApplyContext(SelectionContext.None));
        Assert.Equal(SelectionContext.None, s.Selection);
    }
}
