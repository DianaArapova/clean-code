﻿using NUnit.Framework;
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
		[TestCase("_о_", "<em>о</em>")]
		[TestCase("_о_asfew_s_", "<em>о</em>asfew<em>s</em>")]
		[TestCase("_о_asfew_shj_hgvhk__", "<em>о</em>asfew<em>shj</em>hgvhk__")]
		[TestCase("_о_asfew_shj_hgvhk_ _", "<em>о</em>asfew<em>shj</em>hgvhk_ _")]

		[TestCase("_a __a__ a_", "<em>a __a__ a</em>")]//Внутри одинарного двойное не работает
		[TestCase("__a _a_ a__", "<strong>a <em>a</em> a</strong>")]//Внутри двойного одинарное работает
		[TestCase("_a __a__ __a__ a_", "<em>a __a__ __a__ a</em>")]
		[TestCase("_a __a__ a", "_a <strong>a</strong> a")]
		[TestCase("_a __a__ __a__ a", "_a <strong>a</strong> <strong>a</strong> a")]
		public void TestRenderToNtml_WithSigleUndergraund(string input, string output)
		{
			var md = new Md();
			md.RenderToHtml(input).Should().Be(output);
		}

		[TestCase("__", "__")]
		[TestCase("asd", "asd")]
		[TestCase("a a", "a a")]
		[TestCase("__о__", "<strong>о</strong>")]
		[TestCase("___о___", "<strong><em>о</em></strong>")]
		[TestCase("___a_", "__<em>a</em>")]
		[TestCase("__ a_", "__ a_")]
		[TestCase("__A_A_A__", "<strong>A<em>A</em>A</strong>")]
		[TestCase("_A__A__A_", "<em>A__A__A</em>")]
		[TestCase("__A___a_", "<strong>A</strong><em>a</em>")]
		[TestCase("_A___a__", "<strong>A</strong><em>a</em>")]
		[TestCase("_ab_ _a", "<em>ab</em> _a")]
		[TestCase(@"_hello\_world_", "<em>hello_world</em>")]
		[TestCase("\\_\\_", "__")]
		[TestCase("_\\_\\__", "<em>__</em>")]
		[TestCase("__непарные _символы", "__непарные _символы")]
		public void TestRenderToNtml_WithDoubleUndergraund(string input, string output)
		{
			var md = new Md();
			md.RenderToHtml(input).Should().Be(output);
		}
	}
}