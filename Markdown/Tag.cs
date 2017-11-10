namespace Markdown
{
	public enum TypeOfTag
	{
		Em,
		Strong,
		ShieldedBlock
	}

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

	public class Tag
	{
		public TypeOfTag TagType;
		public Segment TagSegment;


	}
}
