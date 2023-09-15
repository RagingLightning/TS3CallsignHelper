using NuGet.Frameworks;
using TS3CallsignHelper.Game.Extensions;

namespace TS3CallsignHelper.Tests.Game.Extensions;
public class ParserExtensionsTests {

  [Test]
  public void Find_FirstOccurrence_WithoutExclude() {
    string[] haystack = "RUNWAY 27R AT C TAXI VIA A B C".Split(' ');
    var index = haystack.GetIndex("RUNWAY");

    Assert.That(index, Is.EqualTo(1));
  }

  [Test]
  public void Find_FirstOccurrence_WithExcludeAfter() {
    string[] haystack = "RUNWAY 27R AT C TAXI VIA A B C".Split(' ');
    var index = haystack.GetIndex("RUNWAY", "TAXI VIA");

    Assert.That(index, Is.EqualTo(1));
  }

  [Test]
  public void Find_FirstOccurrence_WithExcludeBefore() {
    string[] haystack = "HOLD SHORT OF RUNWAY 27R".Split(' ');
    var index = haystack.GetIndex("RUNWAY", "HOLD SHORT OF");

    Assert.That(index, Is.Null);
  }

  [Test]
  public void Find_FirstOccurrence_WithWildcard() {
    string[] haystack = "WIND IS 230 AT 25 RUNWAY 22 AT G CFT".Split(' ');
    var index = haystack.GetIndex("RUNWAY ? AT");

    Assert.That(index, Is.EqualTo(8));
  }

  [Test]
  public void Find_FirstOccurrence_WithExcludeWildcard() {
    string[] haystack = "WIND IS 230 AT 25 RUNWAY 22 AT G CFT".Split(' ');
    var index = haystack.GetIndex("AT", "WIND IS ?");

    Assert.That(index, Is.EqualTo(8));
  }

  [Test]
  public void Find_Later() {
    string[] haystack = "HOLD SHORT OF RUNWAY 27R".Split(' ');
    var index = haystack.GetIndex("RUNWAY");

    Assert.That(index, Is.EqualTo(4));
  }

  [Test]
  public void Find_FirstOccurrence_WithSecondOccurrence() {
    string[] haystack = "RUNWAY 27R CLEARED TO LAND HOLD SHORT OF RUNWAY 22 FOR CROSSING TRAFFIC".Split(' ');
    var index = haystack.GetIndex("RUNWAY");

    Assert.That(index, Is.EqualTo(1));
  }
}
