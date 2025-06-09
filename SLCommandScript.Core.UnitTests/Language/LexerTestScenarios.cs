using NUnit.Framework;
using SLCommandScript.Core.Language;
using System;

namespace SLCommandScript.Core.UnitTests.Language;

[TestFixture]
public partial class LexerTests
{
    private static readonly object[][] _testsData = [
        [string.Empty, new[] { "TestEmpty" }, PlayerPermissions.KickingAndShortTermBanning, Array.Empty<Core.Language.Token>(), 0],

    [BlankLine, new[] { "TestBlank" }, PlayerPermissions.KickingAndShortTermBanning, Array.Empty<Core.Language.Token>(), 1],

        [@"

", new[] { "" }, PlayerPermissions.KickingAndShortTermBanning, Array.Empty<Core.Language.Token>(), 2],

        [@"
    cassie why am I here #What is the point of life?
    bc 5 I have no idea!
", new[] { "TestBasicCommands" }, PlayerPermissions.KickingAndShortTermBanning, new Core.Language.Token[] { new(TokenType.Text, "cassie"),
            new(TokenType.Text, "why"), new(TokenType.Text, "am"), new(TokenType.Text, "I"), new(TokenType.Text, "here"),
            new(TokenType.Text, "bc"), new(TokenType.Number, "5", 5), new(TokenType.Text, "I"), new(TokenType.Text, "have"),
            new(TokenType.Text, "no"), new(TokenType.Text, "idea!") }, 3],

        [@"
    512 00035 0003% 324% 7%74 78ad78 23%%
", new[] { "TestNumbers" }, PlayerPermissions.KickingAndShortTermBanning, new Core.Language.Token[] { new(TokenType.Number, "512", 512),
            new(TokenType.Number, "00035", 35), new(TokenType.Percentage, "0003%", 3), new(TokenType.Percentage, "324%", 324), new(TokenType.Text, "7%74"),
            new(TokenType.Text, "78ad78"), new(TokenType.Text, "23%%") }, 2],

        [@"
    bc 10 This is a very \
    long one boiiii \\ \  
", new[] { "TestLineBreak" }, PlayerPermissions.KickingAndShortTermBanning, new Core.Language.Token[] { new(TokenType.Text, "bc"), new(TokenType.Number, "10", 10),
            new(TokenType.Text, "This"), new(TokenType.Text, "is"), new(TokenType.Text, "a"), new(TokenType.Text, "very"),
            new(TokenType.Text, "long"), new(TokenType.Text, "one"), new(TokenType.Text, "boiiii"), new(TokenType.Text, "\\") }, 3],

        ["\\\r\\", new[] { "TestLineBreakText" }, PlayerPermissions.KickingAndShortTermBanning, Array.Empty<Core.Language.Token>(), 1],

        ["#\nhello", new[] { "TestLineOnCommentStart" }, PlayerPermissions.KickingAndShortTermBanning, new Core.Language.Token[] {
            new(TokenType.Text, "hello") }, 2],

        [@"
    bc 10 Long comment #I am a storm \
    that is approaching \
    Provoking black clouds...
", new[] { "TestLineBreakComment" }, PlayerPermissions.KickingAndShortTermBanning, new Core.Language.Token[] { new(TokenType.Text, "bc"),
            new(TokenType.Number, "10", 10), new(TokenType.Text, "Long"), new(TokenType.Text, "comment") }, 4],

        [@"
    [ print If true elSe [
    loop foReAch human ]
    ]
    [#print dELayBy 5 ]#
", new[] { "TestDirectiveAndKeywords" }, PlayerPermissions.KickingAndShortTermBanning, new Core.Language.Token[] { new(TokenType.LeftSquare, "["),
            new(TokenType.Text, "print"), new(TokenType.If, "If"), new(TokenType.Text, "true"), new(TokenType.Else, "elSe"),
            new(TokenType.LeftSquare, "["), new(TokenType.Text, "loop"), new(TokenType.Foreach, "foReAch"), new(TokenType.Text, "human"),
            new(TokenType.RightSquare, "]"), new(TokenType.RightSquare, "]"), new(TokenType.LeftSquare, "["), new(TokenType.Text, "#print"),
            new(TokenType.DelayBy, "dELayBy"), new(TokenType.Number, "5", 5), new(TokenType.RightSquare, "]"), new(TokenType.Text, "#") }, 5],

        [@"
    print ski$()bidi bop$(name) $(no#?)yes $(what?)
    prin$(t [hello the]re $(general) 23$(light)sabers
", new[] { "TestVariables" }, PlayerPermissions.KickingAndShortTermBanning, new Core.Language.Token[] { new(TokenType.Text, "print"),
            new(TokenType.Text, "ski$()bidi"), new(TokenType.Variable, "bop$(name)"), new(TokenType.Variable, "$(no#?)yes"),
            new(TokenType.Variable, "$(what?)"), new(TokenType.Text, "prin$(t"), new(TokenType.LeftSquare, "["), new(TokenType.Text, "hello"),
            new(TokenType.Text, "the"), new(TokenType.RightSquare, "]"), new(TokenType.Text, "re"), new(TokenType.Variable, "$(general)"),
            new(TokenType.Variable, "23$(light)sabers") }, 3],

        [@"
    cassie why am I here # This is a comment \
    #? Console
    print I have no idea #? Console $(0)
    #?RemoteAdmin Hello \
    #? !wowlo!
", new[] { "TestScopeGuards" }, PlayerPermissions.KickingAndShortTermBanning, new Core.Language.Token[] { new(TokenType.Text, "cassie"),
            new(TokenType.Text, "why"), new(TokenType.Text, "am"), new(TokenType.Text, "I"), new(TokenType.Text, "here"),
            new(TokenType.Text, "print"), new(TokenType.Text, "I"), new(TokenType.Text, "have"), new(TokenType.Text, "no"),
            new(TokenType.Text, "idea"), new(TokenType.ScopeGuard, "#?"), new(TokenType.Text, "Console"), new(TokenType.Text, "$(0)"),
            new(TokenType.ScopeGuard, "#?"), new(TokenType.Text, "RemoteAdmin"), new(TokenType.Text, "Hello"),
            new(TokenType.Text, "#?"), new(TokenType.Text, "!wowlo!") }, 6],

        [@"
    \cassie why am I here \if# This is a comment \
    #? Console
    print \I \have no id\ea \23 \[ #! bruh
    this sh\#ould \#not appea\#r #? test?
", new[] { "TestQuotation" }, PlayerPermissions.KickingAndShortTermBanning, new Core.Language.Token[] { new(TokenType.Text, "cassie"),
            new(TokenType.Text, "why"), new(TokenType.Text, "am"), new(TokenType.Text, "I"), new(TokenType.Text, "here"), new(TokenType.Text, "if#"),
            new(TokenType.Text, "This"), new(TokenType.Text, "is"), new(TokenType.Text, "a"), new(TokenType.Text, "comment"),
            new(TokenType.ScopeGuard, "#?"), new(TokenType.Text, "Console"),
            new(TokenType.Text, "print"), new(TokenType.Text, "I"), new(TokenType.Text, "have"), new(TokenType.Text, "no"),
            new(TokenType.Text, "id\\ea"), new(TokenType.Number, "23", 23), new(TokenType.Text, "["), new(TokenType.ScopeGuard, "#?"), new(TokenType.Text, "test?") }, 5],

        [@"
    cassie why am I here # This is a comment \
    #! ServerConsoleCommands
    print \I have no idea #! \ServerC?..6onsoleCommands
    print \This \should not appear
    #! Noclip! ?Announcer \
    #!ServerConsoleCommands
    print This should not appear #? Console
    more invisible text
    #! Noclip
    print Hello there #!Noclip Announcer
    print Class d has micro p p #!
    print 1 ... #! \
    !92424..awwghow*(
", new[] { "TestPermissionGuards" }, PlayerPermissions.Noclip | PlayerPermissions.Announcer, new Core.Language.Token[] { new(TokenType.Text, "cassie"),
            new(TokenType.Text, "why"), new(TokenType.Text, "am"), new(TokenType.Text, "I"), new(TokenType.Text, "here"),
            new(TokenType.Text, "print"), new(TokenType.Text, "I"), new(TokenType.Text, "have"), new(TokenType.Text, "no"),
            new(TokenType.Text, "idea"), new(TokenType.ScopeGuard, "#?"), new(TokenType.Text, "Console"), new(TokenType.Text, "print"),
            new(TokenType.Text, "Hello"), new(TokenType.Text, "there"), new(TokenType.Text, "print"), new(TokenType.Text, "Class"),
            new(TokenType.Text, "d"), new(TokenType.Text, "has"), new(TokenType.Text, "micro"), new(TokenType.Text, "p"), new(TokenType.Text, "p"),
            new(TokenType.Text, "print"), new(TokenType.Number, "1", 1), new(TokenType.Text, "...") }, 14],

        [@"
    cassie why am I here # This is a comment \
    #$ 2137
    print \I have no idea #$ 5
    print \This \should not appear
    #$ 2\
    
    print This should not appear #? Console
    more invisible text
    #$ 0 \

    print Hello there #$0
    print Class d has micro p p #$
    print 1 ... #$ \
    19
", new[] { "TestArgumentsGuards" }, PlayerPermissions.Noclip, new Core.Language.Token[] { new(TokenType.Text, "cassie"),
            new(TokenType.Text, "why"), new(TokenType.Text, "am"), new(TokenType.Text, "I"), new(TokenType.Text, "here"),
            new(TokenType.Text, "print"), new(TokenType.Text, "I"), new(TokenType.Text, "have"), new(TokenType.Text, "no"),
            new(TokenType.Text, "idea"), new(TokenType.ScopeGuard, "#?"), new(TokenType.Text, "Console"), new(TokenType.Text, "print"),
            new(TokenType.Text, "Hello"), new(TokenType.Text, "there"), new(TokenType.Text, "print"), new(TokenType.Text, "Class"),
            new(TokenType.Text, "d"), new(TokenType.Text, "has"), new(TokenType.Text, "micro"), new(TokenType.Text, "p"), new(TokenType.Text, "p"),
            new(TokenType.Text, "print"), new(TokenType.Number, "1", 1), new(TokenType.Text, "...") }, 15],

        ["$(2) $(00001) $(0)\n$(3) $(4) $(5) #Hello $(3)", new[] { "TestSimpleArgs", "happenned ?", "#What", BlankLine, "number 1 5%",
            string.Empty }, PlayerPermissions.Noclip, new Core.Language.Token[] { new(TokenType.Text, "#What"), new(TokenType.Text, "happenned"),
            new(TokenType.Text, "?"), new(TokenType.Text, "TestSimpleArgs"), new(TokenType.Text, "number"), new(TokenType.Number, "1", 1),
            new(TokenType.Percentage, "5%", 5) }, 2],

        ["$(15)", new[] { "TestBigNumArgs", null, null, null, null, null, null, null, null, null, null, null, null, null, null,
            " Hello $(test) " }, PlayerPermissions.Noclip, new Core.Language.Token[] { new(TokenType.Text, "Hello"), new(TokenType.Variable, "$(test)") }, 1],

        ["$(1)", new[] { "TestInnerArgs", " Example\t$(1) \ninjection" }, PlayerPermissions.Noclip, new Core.Language.Token[] {
            new(TokenType.Text, "Example"), new(TokenType.Variable, "$(1)"), new(TokenType.Text, "injection") }, 1],

        [@"
    $(1)    $(2)
    postfix$(0001)   postfix$(002)
    $(1)prefix  $(2)prefix
    su$(1)fix  su$(00002)fix
", new[] { "TestNoTokensInjection", null, BlankLine }, PlayerPermissions.Noclip, new Core.Language.Token[] { new(TokenType.Text, "postfix"),
            new(TokenType.Text, "postfix"), new(TokenType.Text, "prefix"),  new(TokenType.Text, "prefix"), new(TokenType.Text, "sufix"),
            new(TokenType.Text, "su"), new(TokenType.Text, "fix") }, 5],

        [@"
    $(1)    $(2) $(3)  $(04)
    postfix$(0001)   postfix$(002) postfix$(3) postfix$(4)
    $(1)prefix  $(2)prefix $(3)prefix $(4)prefix
    su$(1)fix  su$(00002)fix su$(3)fix su$(4)fix
", new[] { "TestOneTokenInjection", "1", " 1", "1 ", " 1 " }, PlayerPermissions.Noclip, new Core.Language.Token[] { new(TokenType.Number, "1", 1),
            new(TokenType.Number, "1", 1), new(TokenType.Number, "1", 1), new(TokenType.Number, "1", 1), new(TokenType.Text, "postfix1"),
            new(TokenType.Text, "postfix"), new(TokenType.Number, "1", 1), new(TokenType.Text, "postfix1"), new(TokenType.Text, "postfix"),
            new(TokenType.Number, "1", 1), new(TokenType.Text, "1prefix"),  new(TokenType.Text, "1prefix"), new(TokenType.Number, "1", 1),
            new(TokenType.Text, "prefix"), new(TokenType.Number, "1", 1), new(TokenType.Text, "prefix"), new(TokenType.Text, "su1fix"),
            new(TokenType.Text, "su"), new(TokenType.Text, "1fix"), new(TokenType.Text, "su1"), new(TokenType.Text, "fix"),
            new(TokenType.Text, "su"), new(TokenType.Number, "1", 1), new(TokenType.Text, "fix") }, 5],

        [@"
    $(1)    $(2) $(3)  $(04)
    postfix$(0001)   postfix$(002) postfix$(3) postfix$(4)
    $(1)prefix  $(2)prefix $(3)prefix $(4)prefix
    su$(1)fix  su$(00002)fix su$(3)fix su$(4)fix
", new[] { "TestTwoTokensInjection", "1 2", " 1 2", "1 2 ", " 1 2 " }, PlayerPermissions.Noclip, new Core.Language.Token[] { new(TokenType.Number, "1", 1),
            new(TokenType.Number, "2", 2), new(TokenType.Number, "1", 1), new(TokenType.Number, "2", 2), new(TokenType.Number, "1", 1), new(TokenType.Number, "2", 2),
            new(TokenType.Number, "1", 1), new(TokenType.Number, "2", 2), new(TokenType.Text, "postfix1"), new(TokenType.Number, "2", 2),
            new(TokenType.Text, "postfix"), new(TokenType.Number, "1", 1), new(TokenType.Number, "2", 2), new(TokenType.Text, "postfix1"),
            new(TokenType.Number, "2", 2), new(TokenType.Text, "postfix"), new(TokenType.Number, "1", 1), new(TokenType.Number, "2", 2),
            new(TokenType.Number, "1", 1), new(TokenType.Text, "2prefix"), new(TokenType.Number, "1", 1), new(TokenType.Text, "2prefix"),
            new(TokenType.Number, "1", 1), new(TokenType.Number, "2", 2), new(TokenType.Text, "prefix"), new(TokenType.Number, "1", 1),
            new(TokenType.Number, "2", 2), new(TokenType.Text, "prefix"), new(TokenType.Text, "su1"), new(TokenType.Text, "2fix"),
            new(TokenType.Text, "su"), new(TokenType.Number, "1", 1), new(TokenType.Text, "2fix"), new(TokenType.Text, "su1"),
            new(TokenType.Number, "2", 2), new(TokenType.Text, "fix"), new(TokenType.Text, "su"), new(TokenType.Number, "1", 1),
            new(TokenType.Number, "2", 2), new(TokenType.Text, "fix") }, 5],

        [@"
    $(1)    $(2) $(3)  $(04)
    postfix$(0001)   postfix$(002) postfix$(3) postfix$(4)
    $(1)prefix  $(2)prefix $(3)prefix $(4)prefix
    su$(1)fix  su$(00002)fix su$(3)fix su$(4)fix
", new[] { "TestMultiTokensInjection", "1 2 (\n) 3", " 1 2 (\n) 3", "1 2 (\n) 3 ", " 1 2 (\n) 3 " }, PlayerPermissions.Noclip, new Core.Language.Token[] {
            new(TokenType.Number, "1", 1), new(TokenType.Number, "2", 2), new(TokenType.Text, "("), new(TokenType.Text, ")"), new(TokenType.Number, "3", 3),
            new(TokenType.Number, "1", 1), new(TokenType.Number, "2", 2), new(TokenType.Text, "("), new(TokenType.Text, ")"), new(TokenType.Number, "3", 3),
            new(TokenType.Number, "1", 1), new(TokenType.Number, "2", 2), new(TokenType.Text, "("), new(TokenType.Text, ")"), new(TokenType.Number, "3", 3),
            new(TokenType.Number, "1", 1), new(TokenType.Number, "2", 2), new(TokenType.Text, "("), new(TokenType.Text, ")"), new(TokenType.Number, "3", 3),
            new(TokenType.Text, "postfix1"), new(TokenType.Number, "2", 2), new(TokenType.Text, "("), new(TokenType.Text, ")"),
            new(TokenType.Number, "3", 3), new(TokenType.Text, "postfix"), new(TokenType.Number, "1", 1), new(TokenType.Number, "2", 2),
            new(TokenType.Text, "("), new(TokenType.Text, ")"), new(TokenType.Number, "3", 3), new(TokenType.Text, "postfix1"),
            new(TokenType.Number, "2", 2), new(TokenType.Text, "("), new(TokenType.Text, ")"), new(TokenType.Number, "3", 3),
            new(TokenType.Text, "postfix"), new(TokenType.Number, "1", 1), new(TokenType.Number, "2", 2), new(TokenType.Text, "("),
            new(TokenType.Text, ")"), new(TokenType.Number, "3", 3), new(TokenType.Number, "1", 1), new(TokenType.Number, "2", 2), new(TokenType.Text, "("),
            new(TokenType.Text, ")"), new(TokenType.Text, "3prefix"), new(TokenType.Number, "1", 1), new(TokenType.Number, "2", 2),
            new(TokenType.Text, "("), new(TokenType.Text, ")"), new(TokenType.Text, "3prefix"), new(TokenType.Number, "1", 1),
            new(TokenType.Number, "2", 2), new(TokenType.Text, "("), new(TokenType.Text, ")"), new(TokenType.Number, "3", 3),
            new(TokenType.Text, "prefix"), new(TokenType.Number, "1", 1), new(TokenType.Number, "2", 2), new(TokenType.Text, "("),
            new(TokenType.Text, ")"), new(TokenType.Number, "3", 3), new(TokenType.Text, "prefix"), new(TokenType.Text, "su1"),
            new(TokenType.Number, "2", 2), new(TokenType.Text, "("), new(TokenType.Text, ")"), new(TokenType.Text, "3fix"),
            new(TokenType.Text, "su"), new(TokenType.Number, "1", 1), new(TokenType.Number, "2", 2), new(TokenType.Text, "("), new(TokenType.Text, ")"),
            new(TokenType.Text, "3fix"), new(TokenType.Text, "su1"), new(TokenType.Number, "2", 2), new(TokenType.Text, "("),
            new(TokenType.Text, ")"), new(TokenType.Number, "3", 3), new(TokenType.Text, "fix"), new(TokenType.Text, "su"), new(TokenType.Number, "1", 1),
            new(TokenType.Number, "2", 2), new(TokenType.Text, "("), new(TokenType.Text, ")"), new(TokenType.Number, "3", 3), new(TokenType.Text, "fix")
        }, 5],

        [@"
    12$(1)3 75$(1) $(1)34
    12$(1)3% 2$(1)% 75$(1) $(1)34%
    12$(2)3 75$(2) $(2)34
    12$(2)3% 2$(2)% 75$(2) $(2)34%
", new[] { "TestNoTokensNumberInjection", null, BlankLine }, PlayerPermissions.Noclip, new Core.Language.Token[] { new(TokenType.Number, "123", 123),
            new(TokenType.Number, "75", 75), new(TokenType.Number, "34", 34),  new(TokenType.Percentage, "123%", 123), new(TokenType.Percentage, "2%", 2),
            new(TokenType.Number, "75", 75), new(TokenType.Percentage, "34%", 34),
            new(TokenType.Number, "12", 12), new(TokenType.Number, "3", 3), new(TokenType.Number, "75", 75), new(TokenType.Number, "34", 34),
            new(TokenType.Number, "12", 12), new(TokenType.Percentage, "3%", 3), new(TokenType.Number, "2", 2), new(TokenType.Text, "%"),
            new(TokenType.Number, "75", 75), new(TokenType.Percentage, "34%", 34)}, 5],

         [@"
    $(1)34 $(1)34% $(1)% $(2)%
", new[] { "TestTextInjectionBeforeNumber", "hello", "2" }, PlayerPermissions.Noclip, new Core.Language.Token[] { new(TokenType.Text, "hello34"),
            new(TokenType.Text, "hello34%"), new(TokenType.Text, "hello%"),  new(TokenType.Percentage, "2%", 2) }, 2],

        [@"
    12$(1)3 75$(1) $(1)34 $(1)
", new[] { "TestDirectiveInjection", "[", }, PlayerPermissions.Noclip, new Core.Language.Token[] { new(TokenType.Number, "12", 12), new(TokenType.LeftSquare, "["),
            new(TokenType.Number, "3", 3), new(TokenType.Number, "75", 75), new(TokenType.LeftSquare, "["), new(TokenType.LeftSquare, "["),
            new(TokenType.Number, "34", 34), new(TokenType.LeftSquare, "[") }, 3],

        [@"
    12$(1)3 12$(2)3 12$(3)3 12$(4)3 12$(5)3 12$(6)3 12$(7)3 12$(8)3
    12$(1)3% 12$(2)3% 12$(3)3% 12$(4)3% 12$(5)3% 12$(6)3% 12$(7)3% 12$(8)3%
    2$(1)% 2$(2)% 2$(3)% 2$(4)% 2$(5)% 2$(6)% 2$(7)% 2$(8)%
    75$(1) 75$(2) 75$(3) 75$(4) 75$(5) 75$(6) 75$(7) 75$(8)
    $(1)34 $(2)34 $(3)34 $(4)34 $(5)34 $(6)34 $(7)34 $(8)34
    $(1)34% $(2)34% $(3)34% $(4)34% $(5)34% $(6)34% $(7)34% $(8)34%
", new[] { "TestOneTokenNumberInjection", "1", " 1", "1 ", " 1 ", "1%", " 1%", "1% ", " 1% " }, PlayerPermissions.Noclip, new Core.Language.Token[] {
            new(TokenType.Number, "1213", 1213), new(TokenType.Number, "12", 12), new(TokenType.Number, "13", 13), new(TokenType.Number, "121", 121),
            new(TokenType.Number, "3", 3), new(TokenType.Number, "12", 12), new(TokenType.Number, "1", 1), new(TokenType.Number, "3", 3),
            new(TokenType.Text, "121%3"), new(TokenType.Number, "12", 12), new(TokenType.Text, "1%3"), new(TokenType.Percentage, "121%", 121),
            new(TokenType.Number, "3", 3), new(TokenType.Number, "12", 12), new(TokenType.Percentage, "1%", 1), new(TokenType.Number, "3", 3),
            new(TokenType.Percentage, "1213%", 1213), new(TokenType.Number, "12", 12), new(TokenType.Percentage, "13%", 13), new(TokenType.Number, "121", 121),
            new(TokenType.Percentage, "3%", 3), new(TokenType.Number, "12", 12), new(TokenType.Number, "1", 1), new(TokenType.Percentage, "3%", 3),
            new(TokenType.Text, "121%3%"), new(TokenType.Number, "12", 12), new(TokenType.Text, "1%3%"), new(TokenType.Percentage, "121%", 121),
            new(TokenType.Percentage, "3%", 3), new(TokenType.Number, "12", 12), new(TokenType.Percentage, "1%", 1), new(TokenType.Percentage, "3%", 3),
            new(TokenType.Percentage, "21%", 21), new(TokenType.Number, "2", 2), new(TokenType.Percentage, "1%", 1), new(TokenType.Number, "21", 21),
            new(TokenType.Text, "%"), new(TokenType.Number, "2", 2), new(TokenType.Number, "1", 1), new(TokenType.Text, "%"),
            new(TokenType.Text, "21%%"), new(TokenType.Number, "2", 2), new(TokenType.Text, "1%%"), new(TokenType.Percentage, "21%", 21),
            new(TokenType.Text, "%"), new(TokenType.Number, "2", 2), new(TokenType.Percentage, "1%", 1), new(TokenType.Text, "%"),
            new(TokenType.Number, "751", 751), new(TokenType.Number, "75", 75), new(TokenType.Number, "1", 1), new(TokenType.Number, "751", 751),
            new(TokenType.Number, "75", 75), new(TokenType.Number, "1", 1),
            new(TokenType.Percentage, "751%", 751), new(TokenType.Number, "75", 75), new(TokenType.Percentage, "1%", 1), new(TokenType.Percentage, "751%", 751),
            new(TokenType.Number, "75", 75), new(TokenType.Percentage, "1%", 1),
            new(TokenType.Number, "134", 134), new(TokenType.Number, "134", 134), new(TokenType.Number, "1", 1),
            new(TokenType.Number, "34", 34), new(TokenType.Number, "1", 1), new(TokenType.Number, "34", 34),
            new(TokenType.Text, "1%34"), new(TokenType.Text, "1%34"), new(TokenType.Percentage, "1%", 1),
            new(TokenType.Number, "34", 34), new(TokenType.Percentage, "1%", 1), new(TokenType.Number, "34", 34),
            new(TokenType.Percentage, "134%", 134), new(TokenType.Percentage, "134%", 134), new(TokenType.Number, "1", 1),
            new(TokenType.Percentage, "34%", 34), new(TokenType.Number, "1", 1), new(TokenType.Percentage, "34%", 34),
            new(TokenType.Text, "1%34%"), new(TokenType.Text, "1%34%"), new(TokenType.Percentage, "1%", 1),
            new(TokenType.Percentage, "34%", 34), new(TokenType.Percentage, "1%", 1), new(TokenType.Percentage, "34%", 34) }, 7],

        [@"
    12$(1)3 12$(2)3 12$(3)3 12$(4)3 12$(5)3 12$(6)3 12$(7)3 12$(8)3
    12$(1)3% 12$(2)3% 12$(3)3% 12$(4)3% 12$(5)3% 12$(6)3% 12$(7)3% 12$(8)3%
    2$(1)% 2$(2)% 2$(3)% 2$(4)% 2$(5)% 2$(6)% 2$(7)% 2$(8)%
    75$(1) 75$(2) 75$(3) 75$(4) 75$(5) 75$(6) 75$(7) 75$(8)
    $(1)34 $(2)34 $(3)34 $(4)34 $(5)34 $(6)34 $(7)34 $(8)34
    $(1)34% $(2)34% $(3)34% $(4)34% $(5)34% $(6)34% $(7)34% $(8)34%
", new[] { "TestTwoTokensNumberInjection", "1 2", " 1 2", "1 2 ", " 1 2 ", "1% 2%", " 1% 2%", "1% 2% ", " 1% 2% " }, PlayerPermissions.Noclip, new Core.Language.Token[] {
            new(TokenType.Number, "121", 121), new(TokenType.Number, "23", 23), new(TokenType.Number, "12", 12), new(TokenType.Number, "1", 1),
            new(TokenType.Number, "23", 23), new(TokenType.Number, "121", 121), new(TokenType.Number, "2", 2), new(TokenType.Number, "3", 3),
            new(TokenType.Number, "12", 12), new(TokenType.Number, "1", 1), new(TokenType.Number, "2", 2), new(TokenType.Number, "3", 3),
            new(TokenType.Percentage, "121%", 121), new(TokenType.Text, "2%3"), new(TokenType.Number, "12", 12), new(TokenType.Percentage, "1%", 1),
            new(TokenType.Text, "2%3"), new(TokenType.Percentage, "121%", 121), new(TokenType.Percentage, "2%", 2), new(TokenType.Number, "3", 3),
            new(TokenType.Number, "12", 12), new(TokenType.Percentage, "1%", 1), new(TokenType.Percentage, "2%", 2), new(TokenType.Number, "3", 3),
            new(TokenType.Number, "121", 121), new(TokenType.Percentage, "23%", 23), new(TokenType.Number, "12", 12), new(TokenType.Number, "1", 1),
            new(TokenType.Percentage, "23%", 23), new(TokenType.Number, "121", 121), new(TokenType.Number, "2", 2), new(TokenType.Percentage, "3%", 3),
            new(TokenType.Number, "12", 12), new(TokenType.Number, "1", 1), new(TokenType.Number, "2", 2), new(TokenType.Percentage, "3%", 3),
            new(TokenType.Percentage, "121%", 121), new(TokenType.Text, "2%3%"), new(TokenType.Number, "12", 12), new(TokenType.Percentage, "1%", 1),
            new(TokenType.Text, "2%3%"), new(TokenType.Percentage, "121%", 121), new(TokenType.Percentage, "2%", 2), new(TokenType.Percentage, "3%", 3),
            new(TokenType.Number, "12", 12), new(TokenType.Percentage, "1%", 1), new(TokenType.Percentage, "2%", 2), new(TokenType.Percentage, "3%", 3),
            new(TokenType.Number, "21", 21), new(TokenType.Percentage, "2%", 2), new(TokenType.Number, "2", 2), new(TokenType.Number, "1", 1),
            new(TokenType.Percentage, "2%", 2), new(TokenType.Number, "21", 21), new(TokenType.Number, "2", 2), new(TokenType.Text, "%"), new(TokenType.Number, "2", 2),
            new(TokenType.Number, "1", 1), new(TokenType.Number, "2", 2), new(TokenType.Text, "%"),
            new(TokenType.Percentage, "21%", 21), new(TokenType.Text, "2%%"), new(TokenType.Number, "2", 2), new(TokenType.Percentage, "1%", 1),
            new(TokenType.Text, "2%%"), new(TokenType.Percentage, "21%", 21), new(TokenType.Percentage, "2%", 2), new(TokenType.Text, "%"),
            new(TokenType.Number, "2", 2), new(TokenType.Percentage, "1%", 1), new(TokenType.Percentage, "2%", 2), new(TokenType.Text, "%"),
            new(TokenType.Number, "751", 751), new(TokenType.Number, "2", 2), new(TokenType.Number, "75", 75), new(TokenType.Number, "1", 1),
            new(TokenType.Number, "2", 2), new(TokenType.Number, "751", 751), new(TokenType.Number, "2", 2), new(TokenType.Number, "75", 75),
            new(TokenType.Number, "1", 1), new(TokenType.Number, "2", 2), new(TokenType.Percentage, "751%", 751), new(TokenType.Percentage, "2%", 2),
            new(TokenType.Number, "75", 75), new(TokenType.Percentage, "1%", 1),
            new(TokenType.Percentage, "2%", 2), new(TokenType.Percentage, "751%", 751), new(TokenType.Percentage, "2%", 2), new(TokenType.Number, "75", 75),
            new(TokenType.Percentage, "1%", 1), new(TokenType.Percentage, "2%", 2),
            new(TokenType.Number, "1", 1), new(TokenType.Number, "234", 234), new(TokenType.Number, "1", 1), new(TokenType.Number, "234", 234),
            new(TokenType.Number, "1", 1), new(TokenType.Number, "2", 2), new(TokenType.Number, "34", 34), new(TokenType.Number, "1", 1), new(TokenType.Number, "2", 2),
            new(TokenType.Number, "34", 34), new(TokenType.Percentage, "1%", 1), new(TokenType.Text, "2%34"), new(TokenType.Percentage, "1%", 1),
            new(TokenType.Text, "2%34"), new(TokenType.Percentage, "1%", 1), new(TokenType.Percentage, "2%", 2), new(TokenType.Number, "34", 34),
            new(TokenType.Percentage, "1%", 1), new(TokenType.Percentage, "2%", 2), new(TokenType.Number, "34", 34),
            new(TokenType.Number, "1", 1), new(TokenType.Percentage, "234%", 234), new(TokenType.Number, "1", 1), new(TokenType.Percentage, "234%", 234),
            new(TokenType.Number, "1", 1), new(TokenType.Number, "2", 2), new(TokenType.Percentage, "34%", 34), new(TokenType.Number, "1", 1),
            new(TokenType.Number, "2", 2), new(TokenType.Percentage, "34%", 34),
            new(TokenType.Percentage, "1%", 1), new(TokenType.Text, "2%34%"), new(TokenType.Percentage, "1%", 1),  new(TokenType.Text, "2%34%"),
            new(TokenType.Percentage, "1%", 1), new(TokenType.Percentage, "2%", 2), new(TokenType.Percentage, "34%", 34), new(TokenType.Percentage, "1%", 1),
            new(TokenType.Percentage, "2%", 2), new(TokenType.Percentage, "34%", 34) }, 7],

        [@"
    12$(1)3 12$(2)3 12$(3)3 12$(4)3 12$(5)3 12$(6)3 12$(7)3 12$(8)3
    12$(1)3% 12$(2)3% 12$(3)3% 12$(4)3% 12$(5)3% 12$(6)3% 12$(7)3% 12$(8)3%
    2$(1)% 2$(2)% 2$(3)% 2$(4)% 2$(5)% 2$(6)% 2$(7)% 2$(8)%
    75$(1) 75$(2) 75$(3) 75$(4) 75$(5) 75$(6) 75$(7) 75$(8)
    $(1)34 $(2)34 $(3)34 $(4)34 $(5)34 $(6)34 $(7)34 $(8)34
    $(1)34% $(2)34% $(3)34% $(4)34% $(5)34% $(6)34% $(7)34% $(8)34%
", new[] { "TestMultiTokensNumberInjection", "1 X 2", " 1 X 2", "1 X 2 ", " 1 X 2 ", "1% X 2%", " 1% X 2%", "1% X 2% ", " 1% X 2% " }, PlayerPermissions.Noclip,
        new Core.Language.Token[] {
            new(TokenType.Number, "121", 121), new(TokenType.Text, "X"), new(TokenType.Number, "23", 23), new(TokenType.Number, "12", 12),
            new(TokenType.Number, "1", 1), new(TokenType.Text, "X"), new(TokenType.Number, "23", 23), new(TokenType.Number, "121", 121), new(TokenType.Text, "X"),
            new(TokenType.Number, "2", 2), new(TokenType.Number, "3", 3), new(TokenType.Number, "12", 12), new(TokenType.Number, "1", 1), new(TokenType.Text, "X"),
            new(TokenType.Number, "2", 2), new(TokenType.Number, "3", 3),
            new(TokenType.Percentage, "121%", 121), new(TokenType.Text, "X"), new(TokenType.Text, "2%3"), new(TokenType.Number, "12", 12),
            new(TokenType.Percentage, "1%", 1), new(TokenType.Text, "X"), new(TokenType.Text, "2%3"), new(TokenType.Percentage, "121%", 121), new(TokenType.Text, "X"),
            new(TokenType.Percentage, "2%", 2), new(TokenType.Number, "3", 3), new(TokenType.Number, "12", 12), new(TokenType.Percentage, "1%", 1),
            new(TokenType.Text, "X"), new(TokenType.Percentage, "2%", 2), new(TokenType.Number, "3", 3),
            new(TokenType.Number, "121", 121), new(TokenType.Text, "X"), new(TokenType.Percentage, "23%", 23), new(TokenType.Number, "12", 12),
            new(TokenType.Number, "1", 1), new(TokenType.Text, "X"), new(TokenType.Percentage, "23%", 23), new(TokenType.Number, "121", 121), new(TokenType.Text, "X"),
            new(TokenType.Number, "2", 2), new(TokenType.Percentage, "3%", 3), new(TokenType.Number, "12", 12), new(TokenType.Number, "1", 1), new(TokenType.Text, "X"),
            new(TokenType.Number, "2", 2), new(TokenType.Percentage, "3%", 3), new(TokenType.Percentage, "121%", 121), new(TokenType.Text, "X"),
            new(TokenType.Text, "2%3%"), new(TokenType.Number, "12", 12), new(TokenType.Percentage, "1%", 1), new(TokenType.Text, "X"), new(TokenType.Text, "2%3%"),
            new(TokenType.Percentage, "121%", 121), new(TokenType.Text, "X"), new(TokenType.Percentage, "2%", 2), new(TokenType.Percentage, "3%", 3),
            new(TokenType.Number, "12", 12), new(TokenType.Percentage, "1%", 1), new(TokenType.Text, "X"), new(TokenType.Percentage, "2%", 2),
            new(TokenType.Percentage, "3%", 3),
            new(TokenType.Number, "21", 21), new(TokenType.Text, "X"), new(TokenType.Percentage, "2%", 2), new(TokenType.Number, "2", 2), new(TokenType.Number, "1", 1),
            new(TokenType.Text, "X"), new(TokenType.Percentage, "2%", 2), new(TokenType.Number, "21", 21), new(TokenType.Text, "X"), new(TokenType.Number, "2", 2),
            new(TokenType.Text, "%"), new(TokenType.Number, "2", 2), new(TokenType.Number, "1", 1), new(TokenType.Text, "X"), new(TokenType.Number, "2", 2),
            new(TokenType.Text, "%"), new(TokenType.Percentage, "21%", 21), new(TokenType.Text, "X"), new(TokenType.Text, "2%%"), new(TokenType.Number, "2", 2),
            new(TokenType.Percentage, "1%", 1), new(TokenType.Text, "X"), new(TokenType.Text, "2%%"), new(TokenType.Percentage, "21%", 21), new(TokenType.Text, "X"),
            new(TokenType.Percentage, "2%", 2), new(TokenType.Text, "%"), new(TokenType.Number, "2", 2), new(TokenType.Percentage, "1%", 1), new(TokenType.Text, "X"),
            new(TokenType.Percentage, "2%", 2), new(TokenType.Text, "%"),
            new(TokenType.Number, "751", 751), new(TokenType.Text, "X"), new(TokenType.Number, "2", 2), new(TokenType.Number, "75", 75), new(TokenType.Number, "1", 1),
            new(TokenType.Text, "X"), new(TokenType.Number, "2", 2), new(TokenType.Number, "751", 751), new(TokenType.Text, "X"), new(TokenType.Number, "2", 2),
            new(TokenType.Number, "75", 75), new(TokenType.Number, "1", 1), new(TokenType.Text, "X"), new(TokenType.Number, "2", 2),
            new(TokenType.Percentage, "751%", 751), new(TokenType.Text, "X"), new(TokenType.Percentage, "2%", 2), new(TokenType.Number, "75", 75),
            new(TokenType.Percentage, "1%", 1), new(TokenType.Text, "X"), new(TokenType.Percentage, "2%", 2), new(TokenType.Percentage, "751%", 751),
            new(TokenType.Text, "X"), new(TokenType.Percentage, "2%", 2), new(TokenType.Number, "75", 75), new(TokenType.Percentage, "1%", 1), new(TokenType.Text, "X"),
            new(TokenType.Percentage, "2%", 2),
            new(TokenType.Number, "1", 1), new(TokenType.Text, "X"), new(TokenType.Number, "234", 234), new(TokenType.Number, "1", 1), new(TokenType.Text, "X"),
            new(TokenType.Number, "234", 234), new(TokenType.Number, "1", 1), new(TokenType.Text, "X"), new(TokenType.Number, "2", 2), new(TokenType.Number, "34", 34),
            new(TokenType.Number, "1", 1), new(TokenType.Text, "X"), new(TokenType.Number, "2", 2),  new(TokenType.Number, "34", 34), new(TokenType.Percentage, "1%", 1),
            new(TokenType.Text, "X"), new(TokenType.Text, "2%34"), new(TokenType.Percentage, "1%", 1), new(TokenType.Text, "X"), new(TokenType.Text, "2%34"),
            new(TokenType.Percentage, "1%", 1), new(TokenType.Text, "X"), new(TokenType.Percentage, "2%", 2), new(TokenType.Number, "34", 34),
            new(TokenType.Percentage, "1%", 1), new(TokenType.Text, "X"), new(TokenType.Percentage, "2%", 2), new(TokenType.Number, "34", 34),
            new(TokenType.Number, "1", 1), new(TokenType.Text, "X"), new(TokenType.Percentage, "234%", 234), new(TokenType.Number, "1", 1), new(TokenType.Text, "X"),
            new(TokenType.Percentage, "234%", 234), new(TokenType.Number, "1", 1), new(TokenType.Text, "X"), new(TokenType.Number, "2", 2),
            new(TokenType.Percentage, "34%", 34), new(TokenType.Number, "1", 1), new(TokenType.Text, "X"), new(TokenType.Number, "2", 2),
            new(TokenType.Percentage, "34%", 34), new(TokenType.Percentage, "1%", 1), new(TokenType.Text, "X"), new(TokenType.Text, "2%34%"),
            new(TokenType.Percentage, "1%", 1), new(TokenType.Text, "X"), new(TokenType.Text, "2%34%"), new(TokenType.Percentage, "1%", 1), new(TokenType.Text, "X"),
            new(TokenType.Percentage, "2%", 2), new(TokenType.Percentage, "34%", 34), new(TokenType.Percentage, "1%", 1), new(TokenType.Text, "X"),
            new(TokenType.Percentage, "2%", 2), new(TokenType.Percentage, "34%", 34) }, 7]
    ];
}
