using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework.Constraints;

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

	class ParserForUnderline
	{
		private readonly List<char> specialSymbols;
		private readonly Dictionary<TypeOfTag, int> currentOpenTags;
		private readonly Dictionary<TypeOfTag, List<Segment>> segmentOfTags;
		private readonly Dictionary<TypeOfTag, Tags> tags;
		private readonly bool[] usedChar;

		public ParserForUnderline()
		{
			usedChar = new bool[10000];
			tags = new Dictionary<TypeOfTag, Tags>
			{
				{TypeOfTag.Em, new Tags("<em>", "</em>", 0, true)},
				{TypeOfTag.Strong, new Tags("<strong>", "</strong>", 1, false)},
				{TypeOfTag.ShieldedBlock, new Tags("", "", 0, false)}
			};

			currentOpenTags = new Dictionary<TypeOfTag, int>();
			foreach (var tag in Enum.GetNames(typeof(TypeOfTag)))
			{
				if (Enum.TryParse(tag, out TypeOfTag tagEnum))
					currentOpenTags.Add(tagEnum, -1);
			}

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

		public bool IsNotUsedCharAtPosition(string line, int pos, char c)
		{
			if (pos >= line.Length || pos < 0)
				return false;
			if (usedChar[pos])
				return false;
			return line[pos] == c;
		}

		public bool IsStartSingleUnderlineBlock(string line, int pos)
		{
			return IsOnlySingleUnderline(line, pos) && 
				   !IsNotUsedCharAtPosition(line, pos + 1, ' ');
		}

		public bool IsBeginDoubleUnderline(string line, int pos)
		{
			return IsOnlyDoubleUnderline(line, pos) &&
				!IsNotUsedCharAtPosition(line, pos + 2, ' ');
		}

		public bool IsOnlyDoubleUnderline(string line, int pos)
		{
			return line[pos] == '_' &&
			       !IsShielded(line, pos) &&
			       IsNotUsedCharAtPosition(line, pos + 1, '_') &&
			       !IsNotUsedCharAtPosition(line, pos + 2, '_') &&
			       !IsNotUsedCharAtPosition(line, pos - 1, '_');
		}

		public bool IsOnlySingleUnderline(string line, int pos)
		{
			return line[pos] == '_' &&
			       !IsShielded(line, pos) &&
			       !IsNotUsedCharAtPosition(line, pos + 1, '_') &&
			       !IsNotUsedCharAtPosition(line, pos - 1, '_');
		}

		public bool EndOfDoubleUnderline(string line, int i)
		{
			if (!IsOnlyDoubleUnderline(line, i) ||
				IsNotUsedCharAtPosition(line, i - 1, ' ') ||
			    currentOpenTags[TypeOfTag.Strong] == -1)
				return false;
			var ind = currentOpenTags[TypeOfTag.Strong];
			usedChar[i] = true;
			usedChar[i + 1] = true;
			usedChar[ind] = true;
			usedChar[ind + 1] = true;
			currentOpenTags[TypeOfTag.Strong] = -1;
			segmentOfTags[TypeOfTag.Strong].Add(new Segment(ind, i));
			return true;
		}

		public bool EndSingleUnderline(string line, int i)
		{
			if (!IsOnlySingleUnderline(line, i) ||
				IsNotUsedCharAtPosition(line, i - 1, ' ') ||
			    currentOpenTags[TypeOfTag.Em] == -1)
				return false;
			var ind = currentOpenTags[TypeOfTag.Em];
			currentOpenTags[TypeOfTag.Em] = -1;
			usedChar[i] = true;
			usedChar[ind] = true;
			segmentOfTags[TypeOfTag.Em].Add(new Segment(ind, i));
			return true;
		}

		public void UpdateStringByDoubleUnderline(string line)
		{
			for (var i = 0; i < line.Length; i++)
			{
				if (EndOfDoubleUnderline(line, i))
				{
					i++;
					continue;
				}

				if (IsBeginDoubleUnderline(line, i) &&
				    currentOpenTags[TypeOfTag.Strong] == -1)
				{
					currentOpenTags[TypeOfTag.Strong] = i;
					i++;
				}
			}	
		}

		public void UpdateStringBySingleUnderline(string line)
		{
			for (var i = 0; i < line.Length; i++)
			{
				if (IsOnlyDoubleUnderline(line, i))
					continue;

				if (!usedChar[i] && EndSingleUnderline(line, i))
					usedChar[i] = true;

				if (!usedChar[i] &&
					IsStartSingleUnderlineBlock(line, i) &&
				    currentOpenTags[TypeOfTag.Em] == -1)
				{
					usedChar[i] = true;
					currentOpenTags[TypeOfTag.Em] = i;
				}
			}
		}

		public void UpdateStringByShieldedUnderline(string line)
		{
			for (var i = 0; i < line.Length; i++)
			{
				if (line[i] != '_' || !IsShielded(line, i))
					continue;
				segmentOfTags[TypeOfTag.ShieldedBlock].
					Add(new Segment(i - 1, i - 1));
				usedChar[i] = true;
			}
		}

		public bool IsGoodTag(Dictionary<TypeOfTag, int> index, TypeOfTag tagEnum, 
			bool isStartSingleTags)
		{
			return index[tagEnum] >= segmentOfTags[tagEnum].Count ||
			        tagEnum == TypeOfTag.Strong && isStartSingleTags;
		}

		public bool IsContainDigit(string line, Segment segment)
		{
			for (var i = segment.BeginIndex; i <= segment.EndIndex; i++)
				if (line[i] >= '0' && line[i] <= '9')
					return true;
			return false;
		}

		public bool IsTagOpenForCreateString(Dictionary<TypeOfTag, int> index, TypeOfTag tagEnum,
			ref bool isStartSingleTags, ref int i, string line, StringBuilder parseLine)
		{
			if (IsContainDigit(line, segmentOfTags[tagEnum][index[tagEnum]]))
			{
				index[tagEnum]++;
				return false;
			}
			if (tags[tagEnum].IsSingle)
				isStartSingleTags = true;
			if (tagEnum == TypeOfTag.ShieldedBlock)
				index[tagEnum]++;
			i += tags[tagEnum].LenMd;
			parseLine.Append(tags[tagEnum].OpenTag);
			return true;
		}

		public bool IsTagCloseForCreateString(Dictionary<TypeOfTag, int> index, TypeOfTag tagEnum,
			ref bool isStartSingleTags, ref int i, string line, StringBuilder parseLine)
		{
			if (tags[tagEnum].IsSingle)
				isStartSingleTags = false;
			parseLine.Append(tags[tagEnum].CloseTag);
			i += tags[tagEnum].LenMd;
			index[tagEnum]++;
			return true;
		}

		public string GetString(string line)
		{
			var parseLine = new StringBuilder();
			var index = new Dictionary<TypeOfTag, int>
			{
				{TypeOfTag.Em, 0},
				{TypeOfTag.ShieldedBlock, 0},
				{TypeOfTag.Strong, 0}
			};

			var isStartSingleTags = false;
			var isTagOpenOrClose = false;
			for (var i = 0; i < line.Length; i++)
			{
				foreach (var tag in Enum.GetNames(typeof(TypeOfTag)))
				{
					isTagOpenOrClose = false;
					if (Enum.TryParse(tag, out TypeOfTag tagEnum))
					{
						if (IsGoodTag(index, tagEnum, isStartSingleTags)) 
							continue;

						if (segmentOfTags[tagEnum][index[tagEnum]].BeginIndex == i)
						{
							isTagOpenOrClose = IsTagOpenForCreateString(index, tagEnum, ref isStartSingleTags,
								ref i, line, parseLine);
							break;
						}
						if (segmentOfTags[tagEnum][index[tagEnum]].EndIndex == i )
						{
							isTagOpenOrClose = IsTagCloseForCreateString(index, tagEnum, ref isStartSingleTags,
								ref i, line, parseLine);
							break;
						}
					}
				}
				if (!isTagOpenOrClose)
					parseLine.Append(line[i]);
			}
			return parseLine.ToString();
		}
	}
}
