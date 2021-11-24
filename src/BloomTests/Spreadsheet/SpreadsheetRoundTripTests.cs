using Bloom.Book;
using Bloom.Spreadsheet;
using Moq;
using NUnit.Framework;
using OfficeOpenXml;
using SIL.IO;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace BloomTests.Spreadsheet
{
	/// <summary>
	/// This class tests roundtripping a book with formatted text to and from a spreadsheet.
	/// Note that formatting is now round-tripped whether or not retainMarkup is true,
	/// though the representation in the spreadsheet is different.
	/// The tests here save it both ways and expect the same results in both cases.
	/// </summary>
	public class SpreadsheetRoundtripTests
	{
		static SpreadsheetRoundtripTests()
		{
			// The package requires us to do this as a way of acknowledging that we
			// accept the terms of the NonCommercial license.
			ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
		}

		private const string roundtripTestBook = @"
<!DOCTYPE html>

<html>
<head>
</head>

<body data-l1=""es"" data-l2="""" data-l3="""">
	<div id=""bloomDataDiv"">
		<div data-book=""bookTitle"" lang=""en"" id=""idShouldGetKept"">
			<p><em>Pineapple</em></p>

            <p>Farm</p>

		</div>
        <div data-book=""topic"" lang=""en"">
            Health
		</div>
		<div data-book=""coverImage"" lang=""*"" src=""cover.png"" alt=""This picture, placeHolder.png, is missing or was loading too slowly."">
			cover.png
		</div>
		<div data-book=""licenseImage"" lang= ""*"" >
			license.png
		</div>
		<div data-book=""outside-back-cover-branding-bottom-html"" lang=""*""><img class=""branding"" src=""BloomWithTaglineAgainstLight.svg"" alt="""" data-copyright="""" data-creator="""" data-license=""""></img></div>
	</div>
    <div class=""bloom-page numberedPage customPage bloom-combinedPage A5Portrait side-right bloom-monolingual"" data-page="""" id=""dc90dbe0-7584-4d9f-bc06-0e0326060054"" data-pagelineage=""adcd48df-e9ab-4a07-afd4-6a24d0398382"" data-page-number=""1"" lang="""">
        <div class=""pageLabel"" data-i18n=""TemplateBooks.PageLabel.Basic Text &amp; Picture"" lang=""en"">
            Basic Text &amp; Picture
        </div>

        <div class=""pageDescription"" lang=""en""></div>

        <div class=""split-pane-component marginBox"" style="""">
            <div class=""split-pane horizontal-percent"" style=""min-height: 42px;"">
                <div class=""split-pane-component position-top"">
                    <div class=""split-pane-component-inner"" min-width=""60px 150px 250px"" min-height=""60px 150px 250px"">
                        <div class=""bloom-translationGroup bloom-trailingElement"" data-default-languages=""auto"">
                            <div class=""bloom-editable normal-style bloom-content1 bloom-visibility-code-on"" id=""simpleFormattingTest"" style=""min-height: 24px;"" tabindex=""0"" spellcheck=""true"" role=""textbox"" aria-label=""false"" data-languagetipcontent=""español"" lang=""es"" contenteditable=""true"">
                                <p>Once upon a time there was a very <strong>bold</strong> dog. This dog was <em>itchy</em>. It went <u>under</u> a tree. The tree had 10<sup>4</sup> leaves.</p>
                            </div>

                            <div class=""bloom-editable normal-style"" style="""" lang=""z"" contenteditable=""true"">
                                <p></p>
                            </div>

                            <div class=""bloom-editable normal-style bloom-contentNational1"" style="""" tabindex=""0"" spellcheck=""true"" role=""textbox"" aria-label=""false"" data-languagetipcontent=""English"" lang=""en"" contenteditable=""true"">
                                <p></p>
                            </div>
                        </div>
                    </div>
                </div>

                <div class=""split-pane-divider horizontal-divider""></div>

                <div class=""split-pane-component position-bottom"">
                    <div class=""split-pane-component-inner"" min-width=""60px 150px 250px"" min-height=""60px 150px 250px"">
                        <div class=""bloom-translationGroup bloom-trailingElement"">
                            <div class=""bloom-editable normal-style bloom-content1 bloom-visibility-code-on"" id=""nestedFormattingTest"" style=""min-height: 24px;"" tabindex=""0"" spellcheck=""true"" role=""textbox"" aria-label=""false"" data-languagetipcontent=""español"" lang=""es"" contenteditable=""true"">
                                <p><strong>One day the dog went for a walk. She had a very pleasant walk. After her <u>walk,</u> <em><u>she</u> decided to take a nap.</em></strong></p>

                                <p><strong><em>The next day,</em> the dog decided to go for a swim. She swam in a lake.</strong></p>
                            </div>

                            <div class=""bloom-editable normal-style bloom-contentNational1"" style="""" tabindex=""0"" spellcheck=""true"" role=""textbox"" aria-label=""false"" data-languagetipcontent=""English"" lang=""en"" contenteditable=""true"">
                                <p></p>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>

    <div class=""bloom-page numberedPage customPage bloom-combinedPage A5Portrait side-left bloom-monolingual"" data-page="""" id=""703ed5fc-ef1e-4699-b151-a6a46c1059ef"" data-pagelineage=""adcd48df-e9ab-4a07-afd4-6a24d0398382"" data-page-number=""2"" lang="""">
        <div class=""pageLabel"" data-i18n=""TemplateBooks.PageLabel.Basic Text &amp; Picture"" lang=""en"">
            Basic Text &amp; Picture
        </div>

        <div class=""pageDescription"" lang=""en""></div>

        <div class=""split-pane-component marginBox"" style="""">
            <div class=""split-pane horizontal-percent"" style=""min-height: 42px;"">
                <div class=""split-pane-component position-top"">
                    <div class=""split-pane-component-inner"" min-width=""60px 150px 250px"" min-height=""60px 150px 250px"">
                        <div class=""bloom-imageContainer bloom-leadingElement"" title=""Name: brain1.jpg Size: 68.62 kb Dots: 1100 x 880 For the current paper size: • The image container is 406 x 335 dots. • For print publications, you want between 300-600 DPI (Dots Per Inch). ⚠ This image would print at 260 DPI. • An image with 1269 x 1047 dots would fill this container at 300 DPI.""><img src=""brain1.jpg"" alt="""" data-copyright="""" data-creator="""" data-license=""""></img></div>
                    </div>
                </div>

                <div class=""split-pane-divider horizontal-divider""></div>

                <div class=""split-pane-component position-bottom"">
                    <div class=""split-pane-component-inner"" min-width=""60px 150px 250px"" min-height=""60px 150px 250px"">
                        <div class=""bloom-translationGroup bloom-trailingElement"" data-default-languages=""auto"">
                            <div class=""bloom-editable normal-style bloom-content1 bloom-visibility-code-on"" id=""whitespaceTest"" style=""min-height: 24px;"" tabindex=""0"" spellcheck=""true"" role=""textbox"" aria-label=""false"" data-languagetipcontent=""español"" lang=""es"" contenteditable=""true"">
                                <p></p>

                                <p>An empty paragraph comes before this one.</p>

                                <p></p>

                                <p></p>

                                <p>This sentence follows two empty paragraphs. It will be followed by a new line.<span class=""bloom-linebreak""></span>﻿This sentence is trailed by an empty paragraph.</p>

                                <p></p>
                            </div>

                            <div class=""bloom-editable normal-style"" style="""" lang=""z"" contenteditable=""true"">
                                <p></p>
                            </div>

                            <div class=""bloom-editable normal-style bloom-contentNational1"" style="""" tabindex=""0"" spellcheck=""true"" role=""textbox"" aria-label=""false"" data-languagetipcontent=""English"" lang=""en"" contenteditable=""true"">
                                <p></p>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>
</body>
</html>
";

		private HtmlDom _roundtrippedDom_retainMarkup;
		private HtmlDom _roundtrippedDom_noRetainMarkup;
		private HtmlDom _roundtrippedDom;

		[OneTimeSetUp]
		public void OneTimeSetUp()
		{
			_roundtrippedDom_noRetainMarkup =  SetupWithParams(new SpreadsheetExportParams());

			var retainMarkupParamsObj = new SpreadsheetExportParams();
			retainMarkupParamsObj.RetainMarkup = true;
			_roundtrippedDom_retainMarkup = SetupWithParams(retainMarkupParamsObj);
		}

		private HtmlDom SetupWithParams(SpreadsheetExportParams parameters)
		{
			var origDom = new HtmlDom(roundtripTestBook, true);
			var roundtrippedDom = new HtmlDom(roundtripTestBook, true); //Will get imported into
			AssertThatXmlIn.Dom(origDom.RawDom).HasSpecifiedNumberOfMatchesForXpath("//div[@id='simpleFormattingTest']", 1);
			AssertThatXmlIn.Dom(origDom.RawDom).HasSpecifiedNumberOfMatchesForXpath("//div[@id='nestedFormattingTest']", 1);
			var mockLangDisplayNameResolver = new Mock<ILanguageDisplayNameResolver>();
			mockLangDisplayNameResolver.Setup(x => x.GetLanguageDisplayName("en")).Returns("English");
			var exporter = new SpreadsheetExporter(mockLangDisplayNameResolver.Object);
			exporter.Params = parameters;
			var sheetFromExport = exporter.Export(origDom, "fakeImagesFolderpath");
			using (var tempFile = TempFile.WithExtension("xslx"))
			{
				sheetFromExport.WriteToFile(tempFile.Path);
				var sheet = InternalSpreadsheet.ReadFromFile(tempFile.Path);
				var importer = new SpreadsheetImporter(roundtrippedDom, sheet);
				importer.Import();
			}
			return roundtrippedDom;
		}

		[OneTimeTearDown]
		public void OneTimeTearDown()
		{

		}

		void SetupFor(string source)
		{
			if (source.Equals("retainMarkup"))
			{
				_roundtrippedDom = _roundtrippedDom_retainMarkup;
			}
			else if (source.Equals("noRetainMarkup"))
			{
				_roundtrippedDom = _roundtrippedDom_noRetainMarkup;
			}
			else
			{
				throw new System.ArgumentException();
			}
		}

		[TestCase("retainMarkup")]
		[TestCase("noRetainMarkup")]
		public void RoundtripSimpleFormatting(string source)
		{
			SetupFor(source);
			var nodeList = _roundtrippedDom.SafeSelectNodes("//div[@id='simpleFormattingTest']");
			Assert.That(nodeList.Count, Is.EqualTo(1));
			var node = nodeList[0];
			RemoveTopLevelWhitespace(node);
			Assert.That(node.InnerText, Is.EqualTo("Once upon a time there was a very bold dog. This dog was itchy. It went under a tree. The tree had 104 leaves."));

			Assert.That(FormatNodeContainsText("//div[@id='simpleFormattingTest']//strong", "bold"));
			Assert.That(FormatNodeContainsText("//div[@id='simpleFormattingTest']//em", "itchy"));
			Assert.That(FormatNodeContainsText("//div[@id='simpleFormattingTest']//u", "under"));
			Assert.That(FormatNodeContainsText("//div[@id='simpleFormattingTest']//sup", "4"));

			Assert.That(FormatNodeContainsText("//div[@id='simpleFormattingTest']//strong", "dog"), Is.False);
			Assert.That(FormatNodeContainsText("//div[@id='simpleFormattingTest']//em", "dog"), Is.False);
			Assert.That(FormatNodeContainsText("//div[@id='simpleFormattingTest']//u", "dog"), Is.False);
			Assert.That(FormatNodeContainsText("//div[@id='simpleFormattingTest']//sup", "dog"), Is.False);
		}

		[TestCase("retainMarkup")]
		[TestCase("noRetainMarkup")]
		public void RoundtripNestedFormatting(string source)
		{
			SetupFor(source);
			var nodeList = _roundtrippedDom.SafeSelectNodes("//div[@id='nestedFormattingTest']");
			Assert.That(nodeList.Count, Is.EqualTo(1));
			var node = nodeList[0];
			RemoveTopLevelWhitespace(node);
			Assert.That(node.InnerText, Is.EqualTo("One day the dog went for a walk. She had a very pleasant walk. After her walk, she decided to take a nap.The next day, the dog decided to go for a swim. She swam in a lake."));
			Assert.That(HasTextWithFormatting("//div[@id='nestedFormattingTest']//", "One day the dog went for a walk. She had a very pleasant walk. After her ",
				bold: true, italic: false, underlined: false, superscript: false));
			Assert.That(HasTextWithFormatting("//div[@id='nestedFormattingTest']//", "walk,",
				bold: true, italic: false, underlined: true, superscript: false));
			Assert.That(HasTextWithFormatting("//div[@id='nestedFormattingTest']//", "she",
				bold: true, italic: true, underlined: true, superscript: false));
			Assert.That(HasTextWithFormatting("//div[@id='nestedFormattingTest']//", " decided to take a nap.",
				bold: true, italic: true, underlined: false, superscript: false));
			Assert.That(HasTextWithFormatting("//div[@id='nestedFormattingTest']//", "The next day,",
				bold: true, italic: true, underlined: false, superscript: false));
			Assert.That(HasTextWithFormatting("//div[@id='nestedFormattingTest']//", " the dog decided to go for a swim. She swam in a lake.",
				bold: true, italic: false, underlined: false, superscript: false));
		}

		//Remove the whitespace between <p> tags that was originally there just for readability
		private void RemoveTopLevelWhitespace(XmlNode node)
		{
			foreach (XmlNode childNode in node.ChildNodes.Cast<XmlNode>().ToArray())
			{
				if (childNode.Name.Equals("#whitespace"))
				{
					node.RemoveChild(childNode);
				}
			}
		}

		[TestCase("noRetainMarkup")]
		[TestCase("retainMarkup")]
		public void WhitespaceTestKeepLineBreak(string source)
		{
			SetupFor(source);
			var nodeList = _roundtrippedDom.SafeSelectNodes("//div[@id='whitespaceTest']");
			Assert.That(nodeList.Count, Is.EqualTo(1));
			var node = nodeList[0];
			RemoveTopLevelWhitespace(node);
			var children = node.ChildNodes;
			Assert.That(children.Count, Is.EqualTo(6));
			foreach (XmlNode child in children)
			{
				Assert.That(child.Name, Is.EqualTo("p"));
			}

			Assert.That(children[0].InnerText, Is.EqualTo(""));
			Assert.That(children[1].InnerText, Is.EqualTo("An empty paragraph comes before this one."));
			Assert.That(children[2].InnerText, Is.EqualTo(""));
			Assert.That(children[3].InnerText, Is.EqualTo(""));
			Assert.That(children[4].InnerXml, Is.EqualTo("This sentence follows two empty paragraphs. It will be followed by a new line.<span class=\"bloom-linebreak\"></span>\xfeffThis sentence is trailed by an empty paragraph."));
			Assert.That(children[5].InnerText, Is.EqualTo(""));
		}

		[TestCase("noRetainMarkup")]
		[TestCase("retainMarkup")]
		public void DatDivUnchanged(string source)
		{
			SetupFor(source);
			Assert.That(HasTextWithFormatting("//div[@id='bloomDataDiv']/div[@data-book='bookTitle' and @lang='en' and @id='idShouldGetKept']//",
				"Pineapple", bold: false, italic: true, underlined: false, superscript: false));
			Assert.That(HasTextWithFormatting("//div[@id='bloomDataDiv']/div[@data-book='bookTitle' and @lang='en' and @id='idShouldGetKept']//",
				"Farm", bold: false, italic: false, underlined: false, superscript: false));
		}

		[TestCase("noRetainMarkup")]
		[TestCase("retainMarkup")]
		public void DatDivImagesUnchanged(string source)
		{
			SetupFor(source);
			Assert.That(FormatNodeContainsText("//div[@id='bloomDataDiv']/div[@data-book='coverImage' and @src='cover.png']", "cover.png"));
			Assert.That(FormatNodeContainsText("//div[@id='bloomDataDiv']/div[@data-book='licenseImage' and not(@src)]", "license.png"));
		}

		private bool HasTextWithFormatting(string baseXPath, string text, bool bold, bool italic, bool underlined, bool superscript)
		{
			return
				   FormatNodeContainsText(baseXPath + "strong", text) == bold
				&& FormatNodeContainsText(baseXPath + "em", text) == italic
				&& FormatNodeContainsText(baseXPath + "u", text) == underlined
				&& FormatNodeContainsText(baseXPath + "sup", text) == superscript;
		}

		private bool FormatNodeContainsText(string xPath, string text)
		{
			IEnumerable<XmlNode> nodeList = _roundtrippedDom.SafeSelectNodes(xPath).Cast<XmlNode>();
			return Enumerable.Any(nodeList, x =>x.InnerText.Contains(text));
		}

		[TestCase("", "")]
		[TestCase("Nothing to escape here.\r\n\t Here neither.", "Nothing to escape here.\r\n\t Here neither.")]
		[TestCase("foo_x000D_\nbar", "foo\r\nbar")]
		[TestCase("foo_x000D_bar\r\n_x000D_\r\nbaz_x000D_\n\n", "foo\rbar\r\n\r\r\nbaz\r\n\n")]
		[TestCase("foo_x000D_bar_x005F_baz_x000D_", "foo\rbar_baz\r")]
		public void UndoExcelEscapedChars(string input, string expected)
		{
			Assert.That(SpreadsheetIO.ReplaceExcelEscapedChars(input), Is.EqualTo(expected));

		}
	}
}
