namespace Markdown
{
	public struct Tags
	{
		public string OpenTag;
		public string CloseTag;
		public int LenMd;
		public bool IsSingle;

		public Tags(string openTag, string closeTag, int lenMd, bool isSingle)
		{
			OpenTag = openTag;
			CloseTag = closeTag;
			LenMd = lenMd;
			IsSingle = isSingle;
		}
	}
}
