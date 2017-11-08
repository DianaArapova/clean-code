using NUnit.Framework;
using FluentAssertions;

namespace Markdown
{
	public class Md
	{
		public string RenderToHtml(string markdown)
		{
			var parser = new Parser();

			return parser.Parse(markdown); 
		}
	}

	[TestFixture]
	public class Md_ShouldRender
	{
		[TestCase("_о_", "<em>о<//em>")]
		[TestCase("_о_asfew_s_", "<em>о<//em>asfew<em>s<//em>")]
		[TestCase("_о_asfew_shj_hgvhk__", "<em>о<//em>asfew<em>shj<//em>hgvhk__")]
		[TestCase("_о_asfew_shj_hgvhk_ _", "<em>о<//em>asfew<em>shj<//em>hgvhk_ _")]
		public void TestRenderToNtml_WithSigleUndergraund(string input, string output)
		{
			var md = new Md();
			md.RenderToHtml(input).Should().Be(output);
		}
	}
}