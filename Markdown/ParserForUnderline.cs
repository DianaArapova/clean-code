using System;
using System.Collections.Generic;
using System.Text;

namespace Markdown
{
	internal class ParserForUnderline
	{
		private readonly Dictionary<TypeOfTag, int> currentOpenTags;
		private readonly Dictionary<TypeOfTag, List<Segment>> segmentOfTags = 
			new Dictionary<TypeOfTag, List<Segment>>
		{
			{TypeOfTag.Em, new List<Segment>()},
			{TypeOfTag.Strong, new List<Segment>()},
			{TypeOfTag.ShieldedBlock, new List<Segment>()}
		};
		private readonly Dictionary<TypeOfTag, Tags> tags = new Dictionary<TypeOfTag, Tags>
		{
			{TypeOfTag.Em, new Tags("<em>", "</em>", 0, true)},
			{TypeOfTag.Strong, new Tags("<strong>", "</strong>", 1, false)},
			{TypeOfTag.ShieldedBlock, new Tags("", "", 0, false)}
		};

		private readonly bool[] usedChar = new bool[10000];

		public ParserForUnderline()
		{
			currentOpenTags = new Dictionary<TypeOfTag, int>();
			foreach (var tag in Enum.GetNames(typeof(TypeOfTag)))
			{
				if (Enum.TryParse(tag, out TypeOfTag tagEnum))
					currentOpenTags.Add(tagEnum, -1);
			}
		}		

		private void UpdateStringByDoubleUnderline(string line)
		{
			for (var i = 0; i < line.Length; i++)
			{
				if (ValidatorForTags.EndOfDoubleUnderline(line, i, usedChar, currentOpenTags, segmentOfTags))
				{
					i++;
					continue;
				}

				if (ValidatorForTags.IsBeginDoubleUnderline(line, i, usedChar) &&
				    currentOpenTags[TypeOfTag.Strong] == -1)
				{
					currentOpenTags[TypeOfTag.Strong] = i;
					i++;
				}
			}	
		}

		private void UpdateStringBySingleUnderline(string line)
		{
			for (var i = 0; i < line.Length; i++)
			{
				if (ValidatorForTags.IsOnlyDoubleUnderline(line, i, usedChar))
					continue;

				if (!usedChar[i] && ValidatorForTags.EndSingleUnderline(line, i, usedChar, currentOpenTags, segmentOfTags))
					usedChar[i] = true;

				if (!usedChar[i] &&
					ValidatorForTags.IsStartSingleUnderlineBlock(line, i, usedChar) &&
				    currentOpenTags[TypeOfTag.Em] == -1)
				{
					usedChar[i] = true;
					currentOpenTags[TypeOfTag.Em] = i;
				}
			}
		}

		private void UpdateStringByShieldedUnderline(string line)
		{
			for (var i = 0; i < line.Length; i++)
			{
				if (line[i] != '_' || !ValidatorForTags.IsShielded(line, i))
					continue;
				segmentOfTags[TypeOfTag.ShieldedBlock].
					Add(new Segment(i - 1, i - 1));
				usedChar[i] = true;
			}
		}

		private bool IsTagOpenForCreateString(Dictionary<TypeOfTag, int> index, TypeOfTag tagEnum,
			ref bool isStartSingleTags, ref int i, string line, StringBuilder parseLine)
		{
			if (ValidatorForTags.IsTagContainDigit(line, 
				segmentOfTags[tagEnum][index[tagEnum]]))
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

		private bool IsTagCloseForCreateString(Dictionary<TypeOfTag, int> index, TypeOfTag tagEnum,
			ref bool isStartSingleTags, ref int i, StringBuilder parseLine)
		{
			if (tags[tagEnum].IsSingle)
				isStartSingleTags = false;
			parseLine.Append(tags[tagEnum].CloseTag);
			i += tags[tagEnum].LenMd;
			index[tagEnum]++;
			return true;
		}

		public string GetHtmlTextFromMdText(string line)
		{
			UpdateStringByShieldedUnderline(line);
			UpdateStringByDoubleUnderline(line);
			UpdateStringBySingleUnderline(line);

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
						if (ValidatorForTags.IsGoodTag(index, tagEnum, 
							isStartSingleTags, segmentOfTags)) 
							continue;

						if (segmentOfTags[tagEnum][index[tagEnum]].BeginIndex == i)
						{
							isTagOpenOrClose = IsTagOpenForCreateString(index, tagEnum, 
								ref isStartSingleTags,
								ref i, line, parseLine);
							break;
						}
						if (segmentOfTags[tagEnum][index[tagEnum]].EndIndex == i )
						{
							isTagOpenOrClose = IsTagCloseForCreateString(index, tagEnum, 
								ref isStartSingleTags,
								ref i, parseLine);
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
