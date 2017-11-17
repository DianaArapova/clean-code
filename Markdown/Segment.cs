namespace Markdown
{
	public struct Segment
	{
		public int BeginIndex;
		public int EndIndex;

		public Segment(int begin, int end)
		{
			BeginIndex = begin;
			EndIndex = end;
		}
	}
}
