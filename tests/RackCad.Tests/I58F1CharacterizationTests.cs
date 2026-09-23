using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text.Json;
using System.Security.Cryptography;
using System.Text;
using RackCad.Application.Persistence;
using RackCad.Application.Systems.Shared;
using RackCad.Domain.Systems.Dynamic;
using RackCad.Domain.Systems.PushBack;
using RackCad.Domain.Systems.Cantilever;
using RackCad.Domain.RackFrames;
using Xunit;
using Xunit.Abstractions;
namespace RackCad.Tests;

public sealed class I58F1CharacterizationTests
{
    private readonly ITestOutputHelper output;
    public I58F1CharacterizationTests(ITestOutputHelper output) => this.output=output;
    public static IEnumerable<object[]> Matrix() => I58F1Matrix.Build().Select(c=>new object[]{c});
    [Theory]
    [MemberData(nameof(Matrix))]
    public void Frozen_matrix_against_current_port(I58F1Case c)
    {
        if(c.Expected!=RackAuthoredComparisonOutcome.Unreadable)
            foreach(var s in c.Input.Siblings) Assert.NotNull(I58F1Fixtures.Store.Deserialize(s.RawDesign));
        I58F1Oracles.AssertBaseline(c);
        output.WriteLine(c.Expected==RackAuthoredComparisonOutcome.Unreadable
            ? "VACUOUS BASELINE PASS: no reader/schema/unknown accreditation"
            : "BASELINE ONLY; future "+c.Expected+" requires RED overlay");
    }
    [Fact]
    public void Matrix_manifest_has_unique_identities_and_all_rows()
    {
        var cases=I58F1Matrix.Build();
        Assert.Equal(cases.Count,cases.Select(c=>c.Id).Distinct().Count());
        var rows=cases.SelectMany(c=>c.Rows).Append("CT58-22").Distinct().ToArray();
        foreach(int n in Enumerable.Range(1,32)) Assert.Contains($"CT58-{n:00}",rows);
        foreach(string k in new[]{"P","C","H"})foreach(int n in Enumerable.Range(1,8)) Assert.Contains($"MM-{k}{n:00}",rows);
        foreach(string n in new[]{"01","02","03","04","05","06","07","08","09a","09b","09c","09d","09e","10"})Assert.Contains("MM-D"+n,rows);
        var manifest=cases.Select(c=>new {c.Id,c.Kind,c.Rows,Expected=c.Expected.ToString(),c.Stimulus,
            Test="I58F1CharacterizationTests.Frozen_matrix_against_current_port",F1Status=c.Expected==RackAuthoredComparisonOutcome.Unreadable?"VACUOUS BASELINE PASS":"BASELINE GREEN / FUTURE RED",FixtureSha256=Hash(JsonSerializer.Serialize(c.Input))}).ToArray();
        Directory.CreateDirectory(Path.Combine(Path.GetTempPath(),"I58-F1-results"));
        File.WriteAllText(Path.Combine(Path.GetTempPath(),"I58-F1-results","matrix.json"),JsonSerializer.Serialize(manifest,new JsonSerializerOptions{WriteIndented=true}));
    }
    private static string Hash(string text)=>Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(text))).ToLowerInvariant();
}
internal static partial class I58F1Oracles
{
    public static void AssertBaseline(I58F1Case c)
    {
        switch(c.Kind) {
            case "dynamic": Baseline(RackAuthoredComparatorPorts.Dynamic<I58F1Input,DynamicRackDesign>().Compare(c.Input));break;
            case "pushback": Baseline(RackAuthoredComparatorPorts.PushBack<I58F1Input,PushBackDesign>().Compare(c.Input));break;
            case "cantilever": Baseline(RackAuthoredComparatorPorts.Cantilever<I58F1Input,CantileverLineDesign>().Compare(c.Input));break;
            case "cabecera": Baseline(RackAuthoredComparatorPorts.Cabecera<I58F1Input,RackFrameConfiguration>().Compare(c.Input));break;
            default:throw new ArgumentException(c.Kind);
        }
    }
    private static void Baseline<T>(RackAuthoredComparisonResult<T> r) {Assert.Equal(RackAuthoredComparisonOutcome.Unreadable,r.Outcome);Assert.Null(r.Authored);}
}
