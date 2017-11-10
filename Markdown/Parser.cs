using System;
using System.Collections.Generic;
using System.Text;

namespace Markdown
{
	class Parser
	{
		private readonly List<char> specialSymbols;
		private readonly Dictionary<TypeOfTag, int> currentOpenTags;
		private readonly Dictionary<TypeOfTag, List<Segment>> segmentOfTags;

		public Parser()
		{
			currentOpenTags = new Dictionary<TypeOfTag, int>
			{
				{TypeOfTag.Em, -1},
				{TypeOfTag.Strong, -1}
			};


			segmentOfTags = new Dictionary<TypeOfTag, List<Segment>>
			{
				{TypeOfTag.Em, new List<Segment>()},
				{TypeOfTag.Strong, new List<Segment>()},
				{TypeOfTag.ShieldedBlock, new List<Segment>()}
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
			return line[pos - 1] == '\\';
		}

		public bool IsCharAtPosition(string line, int pos, char c)
		{
			if (pos >= line.Length)
				return false;
			if (pos < 0)
				return false;
			return line[pos] == c;
		}

		public bool IsSingleUnderline(string line, int pos)
		{
			return line[pos] == '_' &&
			       !IsShielded(line, pos);
		}

		public bool IsDoubleUnderline(string line, int pos)
		{
			return line[pos] == '_' &&
			       !IsShielded(line, pos) &&
			       IsCharAtPosition(line, pos + 1, '_');
		}

		public bool IsStartSingleUnderlineBlock(string line, int pos)
		{
			return IsSingleUnderline(line, pos) && 
				   !IsCharAtPosition(line, pos + 1, ' ');
		}

		public bool IsEndSingleUnderline(string line, int pos)
		{
			return IsSingleUnderline(line, pos) && 
			       !IsCharAtPosition(line, pos - 1, ' ');
		}

		public bool IsBeginDoubleUnderline(string line, int pos)
		{
			return IsDoubleUnderline(line, pos) &&
				!IsCharAtPosition(line, pos + 2, ' ');
		}

		public bool IsEndDoubleUnderline(string line, int pos)
		{
			return IsDoubleUnderline(line, pos) &&
			       !IsCharAtPosition(line, pos - 1, ' ');
		}

		public bool IsOnlyDoubleUnderline(string line, int pos)
		{
			return IsDoubleUnderline(line, pos) &&
			       !IsCharAtPosition(line, pos + 2, '_') &&
			       !IsCharAtPosition(line, pos - 1, '_');
		}

		public string Parse(string line)
		{
			var parseLine = new StringBuilder();
			for (var i = 0; i < line.Length; i++)
			{
				if (line[i] == '_' && IsShielded(line, i))
				{
					segmentOfTags[TypeOfTag.ShieldedBlock].Add(new Segment(i - 1, i - 1));
					continue;
				}

				
				if (IsEndDoubleUnderline(line, i) &&
				    currentOpenTags[TypeOfTag.Strong] != -1)
				{
					var ind = currentOpenTags[TypeOfTag.Strong];
					currentOpenTags[TypeOfTag.Strong] = -1;
					segmentOfTags[TypeOfTag.Strong].Add(new Segment(ind, i));
					i++;
					continue;
				}

				if (IsBeginDoubleUnderline(line, i) &&
				    currentOpenTags[TypeOfTag.Strong] == -1)
				{
					currentOpenTags[TypeOfTag.Strong] = i;
					i++;
					continue;
				}

				if (IsOnlyDoubleUnderline(line, i))
					continue;

				if (IsEndSingleUnderline(line, i) &&
				    currentOpenTags[TypeOfTag.Em] != -1)
				{
					var ind = currentOpenTags[TypeOfTag.Em];
					currentOpenTags[TypeOfTag.Em] = -1;
					segmentOfTags[TypeOfTag.Em].Add(new Segment(ind, i));
					continue;
				}

				

				if (IsStartSingleUnderlineBlock(line, i) &&
					currentOpenTags[TypeOfTag.Em] == -1)
				{
					currentOpenTags[TypeOfTag.Em] = i;
				}

				
			}


			var index = 0;
			var index1 = 0;
			var index2 = 0;
			var isStartSingleTags = false;
			for (var i = 0; i < line.Length; i++)
			{
				if (index2 < segmentOfTags[TypeOfTag.ShieldedBlock].Count)
				{
					if (segmentOfTags[TypeOfTag.ShieldedBlock][index2].BeginIndex == i)
					{
						index2++;
						continue;
					}
				}
				if (index < segmentOfTags[TypeOfTag.Em].Count)
				{
					if (segmentOfTags[TypeOfTag.Em][index].
						    BeginIndex == i)
					{
						isStartSingleTags = true;
						parseLine.Append("<em>");
						continue;
					}
					if (segmentOfTags[TypeOfTag.Em][index].
						    EndIndex == i)
					{
						isStartSingleTags = false;
						parseLine.Append("</em>");
						index++;
						continue;
					}
				}
				if (index1 < segmentOfTags[TypeOfTag.Strong].Count)
				{
					if (segmentOfTags[TypeOfTag.Strong][index1].BeginIndex == i)
					{
						if (isStartSingleTags)
						{
							index1++;
							parseLine.Append(line[i]);
							continue;
						}
						parseLine.Append("<strong>");
						i++;
						continue;
					}
					if (segmentOfTags[TypeOfTag.Strong][index1].EndIndex == i)
					{
						parseLine.Append("</strong>");
						index1++;
						i++;
						continue;
					}
				}
				parseLine.Append(line[i]);
			}
			return parseLine.ToString();
		}
	}
}
