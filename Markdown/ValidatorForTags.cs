using System.Collections.Generic;

namespace Markdown
{
	public enum TypeOfTag
	{
		Em,
		Strong,
		ShieldedBlock
	}

	internal class ValidatorForTags
	{

		public static bool IsEscaped(string line, int pos)
		{
			if (pos == 0)
				return false;
			if (line[pos] != '_')
				return false;
			return line[pos - 1] == '\\';
		}

		public static bool IsNotUsedCharAtPosition(string line, int pos, char c, bool[] usedChar)
		{
			if (pos >= line.Length || pos < 0)
				return false;
			if (usedChar[pos])
				return false;
			return line[pos] == c;
		}

		public static bool IsGoodTag(Dictionary<TypeOfTag, int> index, TypeOfTag tagEnum,
			bool isStartSingleTags, Dictionary<TypeOfTag, List<Segment>> segmentOfTags)
		{
			return index[tagEnum] >= segmentOfTags[tagEnum].Count ||
			       tagEnum == TypeOfTag.Strong && isStartSingleTags;
		}

		public static bool IsTagContainDigit(string line, Segment segment)
		{
			for (var i = segment.BeginIndex; i <= segment.EndIndex; i++)
				if (line[i] >= '0' && line[i] <= '9')
					return true;
			return false;
		}

		public static bool IsStartSingleUnderlineBlock(string line, int pos, bool[] usedChar)
		{
			return IsOnlySingleUnderline(line, pos, usedChar) &&
			       !IsNotUsedCharAtPosition(line, pos + 1, ' ', usedChar);
		}

		public static bool IsBeginDoubleUnderline(string line, int pos, bool[] usedChar)
		{
			return IsOnlyDoubleUnderline(line, pos, usedChar) &&
			       !IsNotUsedCharAtPosition(line, pos + 2, ' ', usedChar);
		}

		public static bool IsOnlyDoubleUnderline(string line, int pos, bool[] usedChar)
		{
			return line[pos] == '_' &&
			       !IsEscaped(line, pos) &&
			       IsNotUsedCharAtPosition(line, pos + 1, '_', usedChar) &&
			       !IsNotUsedCharAtPosition(line, pos + 2, '_', usedChar) &&
			       !IsNotUsedCharAtPosition(line, pos - 1, '_', usedChar);
		}

		public static bool IsOnlySingleUnderline(string line, int pos, bool[] usedChar)
		{
			return line[pos] == '_' &&
			       !IsEscaped(line, pos) &&
			       !IsNotUsedCharAtPosition(line, pos + 1, '_', usedChar) &&
			       !IsNotUsedCharAtPosition(line, pos - 1, '_', usedChar);
		}

		public static bool EndOfDoubleUnderline(string line, int i, bool[] usedChar, 
			Dictionary<TypeOfTag, int> currentOpenTags, 
			Dictionary<TypeOfTag, List<Segment>> segmentOfTags)
		{
			if (!IsOnlyDoubleUnderline(line, i, usedChar) ||
			    IsNotUsedCharAtPosition(line, i - 1, ' ', usedChar) ||
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

		public static bool EndSingleUnderline(string line, int i, bool[] usedChar,
			Dictionary<TypeOfTag, int> currentOpenTags,
			Dictionary<TypeOfTag, List<Segment>> segmentOfTags)
		{
			if (!IsOnlySingleUnderline(line, i, usedChar) ||
			    IsNotUsedCharAtPosition(line, i - 1, ' ', usedChar) ||
			    currentOpenTags[TypeOfTag.Em] == -1)
				return false;
			var ind = currentOpenTags[TypeOfTag.Em];
			currentOpenTags[TypeOfTag.Em] = -1;
			usedChar[i] = true;
			usedChar[ind] = true;
			segmentOfTags[TypeOfTag.Em].Add(new Segment(ind, i));
			return true;
		}
	}
}
