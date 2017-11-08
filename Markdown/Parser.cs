using System.Collections.Generic;
using System.Text;

namespace Markdown
{
	enum BlockInformation
	{
		BlockWithSingleUnderline,
		BlockWithDoubleUnderline
	}

	struct Segment
	{
		public int BeginIndex;
		public int EndIndex;

		public Segment(int begin, int end)
		{
			BeginIndex = begin;
			EndIndex = end;
		}
	}

	class Parser
	{
		private readonly List<char> specialSymbols;
		private readonly Dictionary<BlockInformation, int> currentOpenTags;
		private readonly Dictionary<BlockInformation, List<Segment>> segmentOfTags;

		public Parser()
		{
			currentOpenTags = new Dictionary<BlockInformation, int>
			{
				{BlockInformation.BlockWithSingleUnderline, -1},
				{BlockInformation.BlockWithDoubleUnderline, -1}
			};


			segmentOfTags = new Dictionary<BlockInformation, List<Segment>>
			{
				{BlockInformation.BlockWithSingleUnderline, new List<Segment>()},
				{BlockInformation.BlockWithDoubleUnderline, new List<Segment>()}
			};

			specialSymbols = new List<char>
			{
				'_', '#'
			};
		}

		public bool IsShielded(string line, int pos)
		{
			if (pos == 0)
				return false;
			if (!specialSymbols.Contains(line[pos]))
				return false;
			return line[pos] == '\\';
		}

		public bool IsCharAtPosition(string line, int pos, char c)
		{
			if (pos >= line.Length)
				return false;
			if (pos < 0)
				return false;
			return line[pos] == c;
		}

		public bool IsStartSingleUnderlineBlock(string line, int pos)
		{
			return line[pos] == '_' &&
			       !IsShielded(line, pos) &&
				   !IsCharAtPosition(line, pos + 1, ' ') && 
				   !IsCharAtPosition(line, pos + 1, '_') &&
				   !IsCharAtPosition(line, pos - 1, '_');
		}

		public bool IsEndSingleUnderline(string line, int pos)
		{
			return line[pos] == '_' &&
			       !IsShielded(line, pos) &&
			       !IsCharAtPosition(line, pos - 1, ' ') &&
			       !IsCharAtPosition(line, pos - 1, '_') &&
			       !IsCharAtPosition(line, pos + 1, '_');
		}

		public string Parse(string line)
		{
			var parseLine = new StringBuilder();
			for (var i = 0; i < line.Length; i++)
			{
				if (IsEndSingleUnderline(line, i) &&
				    currentOpenTags[BlockInformation.BlockWithSingleUnderline] != -1)
				{
					var ind = currentOpenTags[BlockInformation.BlockWithSingleUnderline];
					currentOpenTags[BlockInformation.BlockWithSingleUnderline] = -1;
					segmentOfTags[BlockInformation.BlockWithSingleUnderline].Add(new Segment(ind, i));
				}
				else
				{
					if (IsStartSingleUnderlineBlock(line, i) &&
					    currentOpenTags[BlockInformation.BlockWithSingleUnderline] == -1)
					{
						currentOpenTags[BlockInformation.BlockWithSingleUnderline] = i;
					}
				}
			}

			var index = 0;
			
			for (var i = 0; i < line.Length; i++)
			{
				if (index < segmentOfTags[BlockInformation.BlockWithSingleUnderline].Count)
				{
					if (segmentOfTags[BlockInformation.BlockWithSingleUnderline][index].
						BeginIndex == i)
					{
						parseLine.Append("<em>");
						continue;
					}
					if (segmentOfTags[BlockInformation.BlockWithSingleUnderline][index].
						    EndIndex == i)
					{
						parseLine.Append("<//em>");
						index++;
						continue;
					}
				}
				parseLine.Append(line[i]);
			}
			return parseLine.ToString();
		}
	}
}
