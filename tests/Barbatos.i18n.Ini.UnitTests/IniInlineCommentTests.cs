// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Pham The Hung and Barbatos.i18n Contributors.
// All Rights Reserved.

using System.Globalization;
using AwesomeAssertions;
using Barbatos.i18n;
using Barbatos.i18n.Ini;

namespace Barbatos.i18n.Ini.UnitTests;

/// <summary>
/// An inline comment has to be preceded by whitespace. Truncating at any '#' or ';' silently destroyed
/// legitimate translation text.
/// </summary>
public class IniInlineCommentTests
{
    private static LocalizationSet Parse(string contents)
    {
        LocalizationBuilder builder = new();
        builder.FromIniString("t", new CultureInfo("en-US"), contents);

        return builder.Build().GetLocalizationSet(new CultureInfo("en-US"), "t")!;
    }

    [Fact]
    public void Parse_KeepsAValueThatStartsWithHash()
    {
        LocalizationSet set = Parse("PrimaryColor=#FF0000");

        _ = set["PrimaryColor"].Should().Be("#FF0000");
    }

    [Fact]
    public void Parse_KeepsASemicolonThatIsNotPrecededByWhitespace()
    {
        LocalizationSet set = Parse("Copyright=(c) 2026 Barbatos; All rights reserved");

        _ = set["Copyright"].Should().Be("(c) 2026 Barbatos; All rights reserved");
    }

    [Fact]
    public void Parse_KeepsAHashThatIsNotPrecededByWhitespace()
    {
        LocalizationSet set = Parse("Reference=Item#42 is ready\nEscaped=It&#39;s ready");

        _ = set["Reference"].Should().Be("Item#42 is ready");
        _ = set["Escaped"].Should().Be("It&#39;s ready");
    }

    [Fact]
    public void Parse_TreatsAWhitespacePrecededHashAsAComment_SoSuchTextMustBeQuoted()
    {
        // Standard INI cannot distinguish this from a real inline comment; quoting is the documented escape.
        LocalizationSet set = Parse("Bare=Use #tags to organise\nQuoted=\"Use #tags to organise\"");

        _ = set["Bare"].Should().Be("Use");
        _ = set["Quoted"].Should().Be("Use #tags to organise");
    }

    [Fact]
    public void Parse_StillStripsAnInlineCommentPrecededByWhitespace()
    {
        LocalizationSet set = Parse("Greeting=Hello ; translator note\nFarewell=Bye # another note");

        _ = set["Greeting"].Should().Be("Hello");
        _ = set["Farewell"].Should().Be("Bye");
    }

    [Fact]
    public void Parse_StillTreatsAWholeLineCommentAsAComment()
    {
        LocalizationSet set = Parse("; a comment\n# another comment\nGreeting=Hello");

        _ = set["Greeting"].Should().Be("Hello");
    }

    [Fact]
    public void Parse_StillHonoursQuotedValues()
    {
        LocalizationSet set = Parse("Greeting=\"Hello ; not a comment\"");

        _ = set["Greeting"].Should().Be("Hello ; not a comment");
    }
}
