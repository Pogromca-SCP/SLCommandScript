using CommandSystem;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SLCommandScript.Core.Language;
using SLCommandScript.Core.Permissions;
using System;
using System.Collections.Generic;

namespace SLCommandScript.Core.UnitTests.Language;

[TestFixture]
public class LexerTests
{
    private const string BlankLine = "      ";

    private static readonly int[] _argSizes = [2, 3, 4];

    private static readonly char[] _testCharacters = [' ', '\t', '4', '\0', 'x', 'A', '#', '[', '?'];

    #region Gold Flow Test Case Sources
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
    #endregion

    private static ArraySegment<string?> EmptyArgs => new([], 0, 0);

    #region IsWhitespace Tests
    [TestCaseSource(nameof(_testCharacters))]
    public void IsWhiteSpace_ShouldProperlyDetectWhiteSpace(char ch)
    {
        // Act
        var result = Lexer.IsWhiteSpace(ch);

        // Assert
        result.Should().Be(char.IsWhiteSpace(ch) || ch == '\0');
    }
    #endregion

    #region IsDigit Tests
    [TestCaseSource(nameof(_testCharacters))]
    public void IsDigit_ShouldProperlyDetectDigit(char ch)
    {
        // Act
        var result = Lexer.IsDigit(ch);

        // Assert
        result.Should().Be(ch >= '0' && ch <= '9');
    }
    #endregion

    #region IsSpecialCharacter Tests
    [TestCaseSource(nameof(_testCharacters))]
    public void IsSpecialCharacter_ShouldProperlyDetectSpecialCharacter(char ch)
    {
        // Act
        var result = Lexer.IsSpecialCharacter(ch);

        // Assert
        result.Should().Be(ch == '[' || ch == ']');
    }
    #endregion

    #region IsKeyword Tests
    [TestCase("", false)]
    [TestCase(null, false)]
    [TestCase("xd", false)]
    [TestCase("    ", false)]
    [TestCase("if", true)]
    [TestCase("FOreach", true)]
    [TestCase("|", true)]
    public void IsKeyword_ShouldProperlyDetectKeyword(string str, bool expectedResult)
    {
        // Act
        var result = Lexer.IsKeyword(str);

        // Assert
        result.Should().Be(expectedResult);
    }
    #endregion

    #region Constructor Tests
    [Test]
    public void Lexer_ShouldProperlyInitialize_WhenSourceIsNull()
    {
        // Act
        var lexer = new Lexer(null, EmptyArgs, null);

        // Assert
        lexer.Source.Should().Be(string.Empty);
        lexer.Arguments.Should().BeEmpty();
        lexer.Sender.Should().BeNull();
        lexer.PermissionsResolver.Should().NotBeNull();
        lexer.Line.Should().Be(0);
        lexer.ErrorMessage.Should().BeNull();
        lexer.IsAtEnd.Should().BeTrue();
    }

    [Test]
    public void Lexer_ShouldProperlyInitialize_WhenSourceIsNotNull()
    {
        // Act
        var lexer = new Lexer(BlankLine, EmptyArgs, null);

        // Assert
        lexer.Source.Should().Be(BlankLine);
        lexer.Arguments.Should().BeEmpty();
        lexer.Sender.Should().BeNull();
        lexer.PermissionsResolver.Should().NotBeNull();
        lexer.Line.Should().Be(0);
        lexer.ErrorMessage.Should().BeNull();
        lexer.IsAtEnd.Should().BeFalse();
    }

    [TestCaseSource(nameof(_argSizes))]
    public void Lexer_ShouldProperlyInitialize_WhenArgumentsAreProvided(int size)
    {
        // Act
        var lexer = new Lexer(BlankLine, new(new string[size], 0, size), null);

        // Assert
        lexer.Source.Should().Be(BlankLine);
        lexer.Arguments.Should().HaveCount(size);
        lexer.Sender.Should().BeNull();
        lexer.PermissionsResolver.Should().NotBeNull();
        lexer.Line.Should().Be(0);
        lexer.ErrorMessage.Should().BeNull();
        lexer.IsAtEnd.Should().BeFalse();
    }

    [Test]
    public void Lexer_ShouldProperlyInitialize_WhenSenderIsProvided()
    {
        // Arrange
        var senderMock = new Mock<CommandSender>(MockBehavior.Strict);

        // Act
        var lexer = new Lexer(BlankLine, EmptyArgs, senderMock.Object);

        // Assert
        lexer.Source.Should().Be(BlankLine);
        lexer.Arguments.Should().BeEmpty();
        lexer.Sender.Should().Be(senderMock.Object);
        lexer.PermissionsResolver.Should().NotBeNull();
        lexer.Line.Should().Be(0);
        lexer.ErrorMessage.Should().BeNull();
        lexer.IsAtEnd.Should().BeFalse();
    }

    [Test]
    public void Lexer_ShouldProperlyInitialize_WhenResolverIsProvided()
    {
        // Arrange
        var resolverMock = new Mock<IPermissionsResolver>(MockBehavior.Strict);

        // Act
        var lexer = new Lexer(BlankLine, EmptyArgs, null, resolverMock.Object);

        // Assert
        lexer.Source.Should().Be(BlankLine);
        lexer.Arguments.Should().BeEmpty();
        lexer.Sender.Should().BeNull();
        lexer.PermissionsResolver.Should().Be(resolverMock.Object);
        lexer.Line.Should().Be(0);
        lexer.ErrorMessage.Should().BeNull();
        lexer.IsAtEnd.Should().BeFalse();
    }
    #endregion

    #region Reset Tests
    [Test]
    public void Reset_ShouldProperlyResetLexer()
    {
        // Arrange
        var lexer = new Lexer(BlankLine, EmptyArgs, null);

        // Act
        var result = lexer.ScanNextLine();
        lexer.Reset();

        // Assert
        result.Should().BeEmpty();
        lexer.Source.Should().Be(BlankLine);
        lexer.Arguments.Should().BeEmpty();
        lexer.Sender.Should().BeNull();
        lexer.PermissionsResolver.Should().NotBeNull();
        lexer.Line.Should().Be(0);
        lexer.ErrorMessage.Should().BeNull();
        lexer.IsAtEnd.Should().BeFalse();
    }

    [Test]
    public void Reset_ShouldProperlyResetLexer_WithNewSource()
    {
        // Arrange
        var lexer = new Lexer(BlankLine, EmptyArgs, null);
        const string newSrc = "test";

        // Act
        var result = lexer.ScanNextLine();
        lexer.Reset(newSrc);

        // Assert
        result.Should().BeEmpty();
        lexer.Source.Should().Be(newSrc);
        lexer.Arguments.Should().BeEmpty();
        lexer.Sender.Should().BeNull();
        lexer.PermissionsResolver.Should().NotBeNull();
        lexer.Line.Should().Be(0);
        lexer.ErrorMessage.Should().BeNull();
        lexer.IsAtEnd.Should().BeFalse();
    }

    [TestCaseSource(nameof(_argSizes))]
    public void Reset_ShouldProperlyResetLexer_WithNewArguments(int size)
    {
        // Arrange
        var lexer = new Lexer(BlankLine, EmptyArgs, null);

        // Act
        var result = lexer.ScanNextLine();
        lexer.Reset(new ArraySegment<string?>(new string[size], 0, size));

        // Assert
        result.Should().BeEmpty();
        lexer.Source.Should().Be(BlankLine);
        lexer.Arguments.Should().HaveCount(size);
        lexer.Sender.Should().BeNull();
        lexer.PermissionsResolver.Should().NotBeNull();
        lexer.Line.Should().Be(0);
        lexer.ErrorMessage.Should().BeNull();
        lexer.IsAtEnd.Should().BeFalse();
    }

    [Test]
    public void Reset_ShouldProperlyResetLexer_WithNewSender()
    {
        // Arrange
        var lexer = new Lexer(BlankLine, EmptyArgs, null);
        var senderMock = new Mock<CommandSender>(MockBehavior.Strict);

        // Act
        var result = lexer.ScanNextLine();
        lexer.Reset(senderMock.Object);

        // Assert
        result.Should().BeEmpty();
        lexer.Source.Should().Be(BlankLine);
        lexer.Arguments.Should().BeEmpty();
        lexer.Sender.Should().Be(senderMock.Object);
        lexer.PermissionsResolver.Should().NotBeNull();
        lexer.Line.Should().Be(0);
        lexer.ErrorMessage.Should().BeNull();
        lexer.IsAtEnd.Should().BeFalse();
    }

    [Test]
    public void Reset_ShouldProperlyResetLexer_WithNewResolver()
    {
        // Arrange
        var lexer = new Lexer(BlankLine, EmptyArgs, null);
        var resolverMock = new Mock<IPermissionsResolver>(MockBehavior.Strict);

        // Act
        var result = lexer.ScanNextLine();
        lexer.Reset(resolverMock.Object);

        // Assert
        result.Should().BeEmpty();
        lexer.Source.Should().Be(BlankLine);
        lexer.Arguments.Should().BeEmpty();
        lexer.Sender.Should().BeNull();
        lexer.PermissionsResolver.Should().Be(resolverMock.Object);
        lexer.Line.Should().Be(0);
        lexer.ErrorMessage.Should().BeNull();
        lexer.IsAtEnd.Should().BeFalse();
    }

    [TestCaseSource(nameof(_argSizes))]
    public void Reset_ShouldProperlyResetLexer_WithNewSourceAndArguments(int size)
    {
        // Arrange
        var lexer = new Lexer(BlankLine, EmptyArgs, null);
        const string newSrc = "test";

        // Act
        var result = lexer.ScanNextLine();
        lexer.Reset(newSrc, new ArraySegment<string?>(new string[size], 0, size));

        // Assert
        result.Should().BeEmpty();
        lexer.Source.Should().Be(newSrc);
        lexer.Arguments.Should().HaveCount(size);
        lexer.Sender.Should().BeNull();
        lexer.PermissionsResolver.Should().NotBeNull();
        lexer.Line.Should().Be(0);
        lexer.ErrorMessage.Should().BeNull();
        lexer.IsAtEnd.Should().BeFalse();
    }

    [Test]
    public void Reset_ShouldProperlyResetLexer_WithNewSourceAndSender()
    {
        // Arrange
        var lexer = new Lexer(BlankLine, EmptyArgs, null);
        var senderMock = new Mock<CommandSender>(MockBehavior.Strict);
        const string newSrc = "test";

        // Act
        var result = lexer.ScanNextLine();
        lexer.Reset(newSrc, senderMock.Object);

        // Assert
        result.Should().BeEmpty();
        lexer.Source.Should().Be(newSrc);
        lexer.Arguments.Should().BeEmpty();
        lexer.Sender.Should().Be(senderMock.Object);
        lexer.PermissionsResolver.Should().NotBeNull();
        lexer.Line.Should().Be(0);
        lexer.ErrorMessage.Should().BeNull();
        lexer.IsAtEnd.Should().BeFalse();
    }

    [Test]
    public void Reset_ShouldProperlyResetLexer_WithNewSourceAndResolver()
    {
        // Arrange
        var lexer = new Lexer(BlankLine, EmptyArgs, null);
        var resolverMock = new Mock<IPermissionsResolver>(MockBehavior.Strict);
        const string newSrc = "test";

        // Act
        var result = lexer.ScanNextLine();
        lexer.Reset(newSrc, resolverMock.Object);

        // Assert
        result.Should().BeEmpty();
        lexer.Source.Should().Be(newSrc);
        lexer.Arguments.Should().BeEmpty();
        lexer.Sender.Should().BeNull();
        lexer.PermissionsResolver.Should().Be(resolverMock.Object);
        lexer.Line.Should().Be(0);
        lexer.ErrorMessage.Should().BeNull();
        lexer.IsAtEnd.Should().BeFalse();
    }

    [TestCaseSource(nameof(_argSizes))]
    public void Reset_ShouldProperlyResetLexer_WithNewArgumentsAndSender(int size)
    {
        // Arrange
        var lexer = new Lexer(BlankLine, EmptyArgs, null);
        var senderMock = new Mock<CommandSender>(MockBehavior.Strict);

        // Act
        var result = lexer.ScanNextLine();
        lexer.Reset(new ArraySegment<string?>(new string[size], 0, size), senderMock.Object);

        // Assert
        result.Should().BeEmpty();
        lexer.Source.Should().Be(BlankLine);
        lexer.Arguments.Should().HaveCount(size);
        lexer.Sender.Should().Be(senderMock.Object);
        lexer.PermissionsResolver.Should().NotBeNull();
        lexer.Line.Should().Be(0);
        lexer.ErrorMessage.Should().BeNull();
        lexer.IsAtEnd.Should().BeFalse();
    }

    [TestCaseSource(nameof(_argSizes))]
    public void Reset_ShouldProperlyResetLexer_WithNewArgumentsAndResolver(int size)
    {
        // Arrange
        var lexer = new Lexer(BlankLine, EmptyArgs, null);
        var resolverMock = new Mock<IPermissionsResolver>(MockBehavior.Strict);

        // Act
        var result = lexer.ScanNextLine();
        lexer.Reset(new ArraySegment<string?>(new string[size], 0, size), resolverMock.Object);

        // Assert
        result.Should().BeEmpty();
        lexer.Source.Should().Be(BlankLine);
        lexer.Arguments.Should().HaveCount(size);
        lexer.Sender.Should().BeNull();
        lexer.PermissionsResolver.Should().Be(resolverMock.Object);
        lexer.Line.Should().Be(0);
        lexer.ErrorMessage.Should().BeNull();
        lexer.IsAtEnd.Should().BeFalse();
    }

    [Test]
    public void Reset_ShouldProperlyResetLexer_WithNewSenderAndResolver()
    {
        // Arrange
        var lexer = new Lexer(BlankLine, EmptyArgs, null);
        var senderMock = new Mock<CommandSender>(MockBehavior.Strict);
        var resolverMock = new Mock<IPermissionsResolver>(MockBehavior.Strict);

        // Act
        var result = lexer.ScanNextLine();
        lexer.Reset(senderMock.Object, resolverMock.Object);

        // Assert
        result.Should().BeEmpty();
        lexer.Source.Should().Be(BlankLine);
        lexer.Arguments.Should().BeEmpty();
        lexer.Sender.Should().Be(senderMock.Object);
        lexer.PermissionsResolver.Should().Be(resolverMock.Object);
        lexer.Line.Should().Be(0);
        lexer.ErrorMessage.Should().BeNull();
        lexer.IsAtEnd.Should().BeFalse();
    }

    [TestCaseSource(nameof(_argSizes))]
    public void Reset_ShouldProperlyResetLexer_WithNewArgumentsAndSenderAndResolver(int size)
    {
        // Arrange
        var lexer = new Lexer(BlankLine, EmptyArgs, null);
        var senderMock = new Mock<CommandSender>(MockBehavior.Strict);
        var resolverMock = new Mock<IPermissionsResolver>(MockBehavior.Strict);

        // Act
        var result = lexer.ScanNextLine();
        lexer.Reset(new ArraySegment<string?>(new string[size], 0, size), senderMock.Object, resolverMock.Object);

        // Assert
        result.Should().BeEmpty();
        lexer.Source.Should().Be(BlankLine);
        lexer.Arguments.Should().HaveCount(size);
        lexer.Sender.Should().Be(senderMock.Object);
        lexer.PermissionsResolver.Should().Be(resolverMock.Object);
        lexer.Line.Should().Be(0);
        lexer.ErrorMessage.Should().BeNull();
        lexer.IsAtEnd.Should().BeFalse();
    }

    [Test]
    public void Reset_ShouldProperlyResetLexer_WithNewSourceAndSenderAndResolver()
    {
        // Arrange
        var lexer = new Lexer(BlankLine, EmptyArgs, null);
        var senderMock = new Mock<CommandSender>(MockBehavior.Strict);
        var resolverMock = new Mock<IPermissionsResolver>(MockBehavior.Strict);
        const string newSrc = "test";

        // Act
        var result = lexer.ScanNextLine();
        lexer.Reset(newSrc, senderMock.Object, resolverMock.Object);

        // Assert
        result.Should().BeEmpty();
        lexer.Source.Should().Be(newSrc);
        lexer.Arguments.Should().BeEmpty();
        lexer.Sender.Should().Be(senderMock.Object);
        lexer.PermissionsResolver.Should().Be(resolverMock.Object);
        lexer.Line.Should().Be(0);
        lexer.ErrorMessage.Should().BeNull();
        lexer.IsAtEnd.Should().BeFalse();
    }

    [TestCaseSource(nameof(_argSizes))]
    public void Reset_ShouldProperlyResetLexer_WithNewSourceAndArgumentsAndResolver(int size)
    {
        // Arrange
        var lexer = new Lexer(BlankLine, EmptyArgs, null);
        var resolverMock = new Mock<IPermissionsResolver>(MockBehavior.Strict);
        const string newSrc = "test";

        // Act
        var result = lexer.ScanNextLine();
        lexer.Reset(newSrc, new(new string[size], 0, size), null, resolverMock.Object);

        // Assert
        result.Should().BeEmpty();
        lexer.Source.Should().Be(newSrc);
        lexer.Arguments.Should().HaveCount(size);
        lexer.Sender.Should().BeNull();
        lexer.PermissionsResolver.Should().Be(resolverMock.Object);
        lexer.Line.Should().Be(0);
        lexer.ErrorMessage.Should().BeNull();
        lexer.IsAtEnd.Should().BeFalse();
    }

    [TestCaseSource(nameof(_argSizes))]
    public void Reset_ShouldProperlyResetLexer_WithNewSourceAndArgumentsAndSender(int size)
    {
        // Arrange
        var lexer = new Lexer(BlankLine, EmptyArgs, null);
        var senderMock = new Mock<CommandSender>(MockBehavior.Strict);
        const string newSrc = "test";

        // Act
        var result = lexer.ScanNextLine();
        lexer.Reset(newSrc, new(new string[size], 0, size), senderMock.Object);

        // Assert
        result.Should().BeEmpty();
        lexer.Source.Should().Be(newSrc);
        lexer.Arguments.Should().HaveCount(size);
        lexer.Sender.Should().Be(senderMock.Object);
        lexer.PermissionsResolver.Should().NotBeNull();
        lexer.Line.Should().Be(0);
        lexer.ErrorMessage.Should().BeNull();
        lexer.IsAtEnd.Should().BeFalse();
    }

    [TestCaseSource(nameof(_argSizes))]
    public void Reset_ShouldProperlyResetLexer_WithNewSourceAndArgumentsAndSenderAndResolver(int size)
    {
        // Arrange
        var lexer = new Lexer(BlankLine, EmptyArgs, null);
        var senderMock = new Mock<CommandSender>(MockBehavior.Strict);
        var resolverMock = new Mock<IPermissionsResolver>(MockBehavior.Strict);
        const string newSrc = "test";

        // Act
        var result = lexer.ScanNextLine();
        lexer.Reset(newSrc, new(new string[size], 0, size), senderMock.Object, resolverMock.Object);

        // Assert
        result.Should().BeEmpty();
        lexer.Source.Should().Be(newSrc);
        lexer.Arguments.Should().HaveCount(size);
        lexer.Sender.Should().Be(senderMock.Object);
        lexer.PermissionsResolver.Should().Be(resolverMock.Object);
        lexer.Line.Should().Be(0);
        lexer.ErrorMessage.Should().BeNull();
        lexer.IsAtEnd.Should().BeFalse();
    }
    #endregion

    #region ScanNextLine Tests
    [Test]
    public void ScanNextLine_ShouldFail_WhenLexerFailedBefore()
    {
        // Arrange
        const string src = "$(1)";
        var lexer = new Lexer(src, EmptyArgs, null);

        // Act
        lexer.ScanNextLine();
        var result = lexer.ScanNextLine();

        // Assert
        lexer.Source.Should().Be(src);
        lexer.Arguments.Should().BeEmpty();
        lexer.Sender.Should().BeNull();
        lexer.PermissionsResolver.Should().NotBeNull();
        lexer.Line.Should().Be(1);
        lexer.ErrorMessage.Should().Be("Invalid argument $(1), provided arguments array has incorrect offset (0)");
        lexer.IsAtEnd.Should().BeTrue();
        result.Should().BeEmpty();
    }

    [Test]
    public void ScanNextLine_ShouldFail_WhenPermissionsCheckEmitsAnError()
    {
        // Arrange
        const string src = "#!Test Perm Fail";
        var message = "Permission check failed for some reason";
        var resolverMock = new Mock<IPermissionsResolver>(MockBehavior.Strict);
        resolverMock.Setup(x => x.CheckPermission(null, "Test", out message)).Returns(false);
        var lexer = new Lexer(src, EmptyArgs, null, resolverMock.Object);

        // Act
        var result = lexer.ScanNextLine();

        // Assert
        lexer.Source.Should().Be(src);
        lexer.Arguments.Should().BeEmpty();
        lexer.Sender.Should().BeNull();
        lexer.PermissionsResolver.Should().Be(resolverMock.Object);
        lexer.Line.Should().Be(1);
        lexer.ErrorMessage.Should().Be(message);
        lexer.IsAtEnd.Should().BeFalse();
        result.Should().BeEmpty();
        resolverMock.VerifyAll();
    }

    [TestCaseSource(nameof(_argSizes))]
    public void ScanNextLine_ShouldFail_WhenArgumentsArrayIsNull(int argNum)
    {
        // Arrange
        var src = $"$({argNum}) $(var)";
        var lexer = new Lexer(src, new(), null);

        // Act
        var result = lexer.ScanNextLine();

        // Assert
        lexer.Source.Should().Be(src);
        lexer.Sender.Should().BeNull();
        lexer.PermissionsResolver.Should().NotBeNull();
        lexer.Line.Should().Be(1);
        lexer.ErrorMessage.Should().Be($"Invalid argument $({argNum}), provided arguments array is null");
        lexer.IsAtEnd.Should().BeFalse();
        result.Should().BeEmpty();
    }

    [TestCaseSource(nameof(_argSizes))]
    public void ScanNextLine_ShouldFail_WhenArgumentsArrayHasIncorrectOffset(int argNum)
    {
        // Arrange
        var src = $"$({argNum}) bc";
        var lexer = new Lexer(src, new(new string[argNum], 0, argNum), null);

        // Act
        var result = lexer.ScanNextLine();

        // Assert
        lexer.Source.Should().Be(src);
        lexer.Arguments.Should().HaveCount(argNum);
        lexer.Sender.Should().BeNull();
        lexer.PermissionsResolver.Should().NotBeNull();
        lexer.Line.Should().Be(1);
        lexer.ErrorMessage.Should().Be($"Invalid argument $({argNum}), provided arguments array has incorrect offset (0)");
        lexer.IsAtEnd.Should().BeFalse();
        result.Should().BeEmpty();
    }

    [TestCaseSource(nameof(_argSizes))]
    public void ScanNextLine_ShouldFail_WhenArgumentDoesNotExist(int argNum)
    {
        // Arrange
        var actualSize = argNum - 2;
        var src = $"$({argNum}) ";
        var lexer = new Lexer(src, new(new string[actualSize + 1], 1, actualSize), null);

        // Act
        var result = lexer.ScanNextLine();

        // Assert
        lexer.Source.Should().Be(src);
        lexer.Arguments.Should().HaveCount(actualSize);
        lexer.Sender.Should().BeNull();
        lexer.PermissionsResolver.Should().NotBeNull();
        lexer.Line.Should().Be(1);
        lexer.ErrorMessage.Should().Be($"Missing argument $({argNum}), sender provided only {actualSize} arguments");
        lexer.IsAtEnd.Should().BeFalse();
        result.Should().BeEmpty();
    }

    [Test]
    public void ScanNextLine_ShouldFail_WhenArgsGuardHasMoreContent()
    {
        // Arrange
        const string src = "#$ 3 hge1";
        var lexer = new Lexer(src, EmptyArgs, null);

        // Act
        var result = lexer.ScanNextLine();

        // Assert
        lexer.Source.Should().Be(src);
        lexer.Arguments.Should().BeEmpty();
        lexer.Sender.Should().BeNull();
        lexer.PermissionsResolver.Should().NotBeNull();
        lexer.Line.Should().Be(1);
        lexer.ErrorMessage.Should().Be("Encountered an unexpected additional value in arguments guard");
        lexer.IsAtEnd.Should().BeFalse();
        result.Should().BeEmpty();
    }

    [Test]
    public void ScanNextLine_ShouldFail_WhenArgsGuardIsNotANumber()
    {
        // Arrange
        const string src = "#$ test";
        var lexer = new Lexer(src, EmptyArgs, null);

        // Act
        var result = lexer.ScanNextLine();

        // Assert
        lexer.Source.Should().Be(src);
        lexer.Arguments.Should().BeEmpty();
        lexer.Sender.Should().BeNull();
        lexer.PermissionsResolver.Should().NotBeNull();
        lexer.Line.Should().Be(1);
        lexer.ErrorMessage.Should().Be("Arguments guard value is not a number");
        lexer.IsAtEnd.Should().BeFalse();
        result.Should().BeEmpty();
    }

    [Test]
    public void ScanNextLine_ShouldFail_WhenArgsGuardValueEndsWithText()
    {
        // Arrange
        const string src = "#$ 56b";
        var lexer = new Lexer(src, EmptyArgs, null);

        // Act
        var result = lexer.ScanNextLine();

        // Assert
        lexer.Source.Should().Be(src);
        lexer.Arguments.Should().BeEmpty();
        lexer.Sender.Should().BeNull();
        lexer.PermissionsResolver.Should().NotBeNull();
        lexer.Line.Should().Be(1);
        lexer.ErrorMessage.Should().Be("Arguments guard value has an unexpected suffix");
        lexer.IsAtEnd.Should().BeFalse();
        result.Should().BeEmpty();
    }

    [TestCaseSource(nameof(_testsData))]
    public void ScanNextLine_ShouldReturnProperTokens_WhenGoldFlow(string src, string[] args, PlayerPermissions perms, Core.Language.Token[] expectedTokens, int expectedLine)
    {
        // Arrange
        var resolver = new LexerTestResolver(perms);
        var lexer = new Lexer(src, new(args, 1, args.Length - 1), null, resolver);
        var result = new List<Core.Language.Token>();

        // Act
        while (!lexer.IsAtEnd)
        {
            result.AddRange(lexer.ScanNextLine());
        }

        // Assert
        lexer.Source.Should().Be(src);
        lexer.Arguments.Should().HaveCount(args.Length - 1);
        lexer.Sender.Should().BeNull();
        lexer.PermissionsResolver.Should().Be(resolver);
        lexer.Line.Should().Be(expectedLine);
        lexer.ErrorMessage.Should().BeNull();
        lexer.IsAtEnd.Should().BeTrue();
        result.Should().BeEquivalentTo(expectedTokens, options => options.ComparingByValue<Core.Language.Token>());
    }
    #endregion
}

public class LexerTestResolver(PlayerPermissions permissions) : IPermissionsResolver
{
    public PlayerPermissions Permissions { get; } = permissions;

    public bool CheckPermission(ICommandSender? sender, string? permission, out string? message)
    {
        Console.WriteLine($"Lexer test permission resolving: {permission}");
        var parsed = Enum.TryParse<PlayerPermissions>(permission, true, out var result);
        message = null;
        return parsed && (result & Permissions) != 0;
    }
}
