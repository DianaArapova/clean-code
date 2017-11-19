using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using FluentAssertions;

namespace Markdown
{
	public class Md
	{
		private IEnumerable<string> GetLinesFromFile(string fileWithMarkdown)
		{
			using (var fs = new StreamReader(fileWithMarkdown))
			{
				while (true)
				{
					var temp = fs.ReadLine();
					if (temp == null) 
						yield break;
					yield return temp + '\n';
				}
			}
		}

		public string RenderToHtmlFromFile(string fileWithMarkdown)
		{
			return string.
				Join(String.Empty, GetLinesFromFile(fileWithMarkdown).
				Select(partOfMarkdown => new ParserForUnderline(partOfMarkdown).
				GetHtmlTextFromMdText())).
				Replace("\n", "<br>");
		}
		public string RenderToHtml(string markdown)
		{
			var stringSeparators = new []{"\n"};
			var paragraphsOfMarkdown = markdown.Split(stringSeparators, 
				StringSplitOptions.None);

			var arrayOfParagraphs = new StringBuilder();
			
			
			for (var i = 0; i < paragraphsOfMarkdown.Length; i++)
			{
				var partOfMarkdown = paragraphsOfMarkdown[i];

				var parser = new ParserForUnderline(partOfMarkdown);
				arrayOfParagraphs.Append(parser.GetHtmlTextFromMdText());

				if (i < paragraphsOfMarkdown.Length - 1)
					arrayOfParagraphs.Append("<br>");
			}
			
			return arrayOfParagraphs.ToString(); 
		}
	}

	[TestFixture]
	public class Md_ShouldRender
	{
		[TestCase("_о_", "<em>о</em>")]
		[TestCase("_о_asfew_s_", "<em>о</em>asfew<em>s</em>")]
		[TestCase("_о_asfew_shj_hgvhk__", "<em>о</em>asfew<em>shj</em>hgvhk__")]
		[TestCase("_о_asfew_shj_hgvhk_ _", "<em>о</em>asfew<em>shj</em>hgvhk_ _")]
		[TestCase("_a __a__ a_", "<em>a __a__ a</em>")]
		[TestCase("__a _a_ a__", "<strong>a <em>a</em> a</strong>")]
		[TestCase("_a __a__ __a__ a_", "<em>a __a__ __a__ a</em>")]
		[TestCase("_a __a__ a", "_a <strong>a</strong> a")]
		[TestCase("_a __a__ __a__ a", "_a <strong>a</strong> <strong>a</strong> a")]
		[TestCase("_9dw_a__a__", "_9dw_a<strong>a</strong>")]
		[TestCase("__", "__")]
		[TestCase("asd", "asd")]
		[TestCase("a a", "a a")]
		[TestCase("__о__", "<strong>о</strong>")]
		[TestCase("___о___", "___о___")]
		[TestCase("___a_", "___a_")]
		[TestCase("__ a_", "__ a_")]
		[TestCase("__A_A_A__", "<strong>A<em>A</em>A</strong>")]
		[TestCase("_A__A__A_", "<em>A__A__A</em>")]
		[TestCase("_A___a__", "_A___a__")]
		[TestCase("_A___a_", "<em>A___a</em>")]
		[TestCase("_ab_ _a", "<em>ab</em> _a")]
		[TestCase(@"_hello\_world_", "<em>hello_world</em>")]
		[TestCase("\\_\\_", "__")]
		[TestCase("_\\_\\__", "<em>__</em>")]
		[TestCase("__непарные _символы", "__непарные _символы")]
		[TestCase("__непарные _символы\n_a", "__непарные _символы<br>_a")]
		[TestCase("_a", "_a")]
		public void TestRenderToHtml_CorrectHtmfFromUndeline(string input, string output)
		{
			var md = new Md();
			md.RenderToHtml(input).Should().Be(output);
		}

		[Test]
		public void TestRenderToHtmlFromFile_CorrectHtmfFromUndeline()
		{
			var md = new Md();
			var nameOfFile = Directory.GetCurrentDirectory() + "1.txt";
			var markdown = "__a__\n_a__\n__a_a_a__\n";
			File.Create(Directory.GetCurrentDirectory() + "\\1.txt");
			using (var fs = new StreamWriter(nameOfFile))
			{
				fs.Write(markdown);
			}
			md.RenderToHtmlFromFile(nameOfFile).Should().
				Be(md.RenderToHtml(markdown));
		}
	}
}