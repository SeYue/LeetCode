// https://leetcode.cn/problems/longest-substring-without-repeating-characters/description/
public class 无重复字符的最长子串
{
	public 无重复字符的最长子串()
	{
		string str = "aab";
		int length = LengthOfLongestSubstring(str);
		Console.WriteLine($"字符串:{str},无重复字符的最长字串:{length}");

		str = "abcabcbb";
		length = LengthOfLongestSubstring(str);
		Console.WriteLine($"字符串:{str},无重复字符的最长字串:{length}");

		str = "bbbbb";
		length = LengthOfLongestSubstring(str);
		Console.WriteLine($"字符串:{str},无重复字符的最长字串:{length}");

		str = "pwwkew";
		length = LengthOfLongestSubstring(str);
		Console.WriteLine($"字符串:{str},无重复字符的最长字串:{length}");

		str = " ";
		length = LengthOfLongestSubstring(str);
		Console.WriteLine($"字符串:{str},无重复字符的最长字串:{length}");
	}

	// 暴力解法
	public int LengthOfLongestSubstring_1(string s)
	{
		int maxLength = 0;
		HashSet<char> chars = new HashSet<char>();
		for (int i = 0; i < s.Length; i++)
		{
			chars.Clear();
			for (int j = i; j < s.Length; j++)
			{
				char c = s[j];
				if (!chars.Contains(c))
				{
					chars.Add(c);
				}
				else
				{
					break;
				}
			}

			if (maxLength < chars.Count)
			{
				maxLength = chars.Count;
			}
		}
		return maxLength;
	}

	// 滑动窗口,时间复杂度最小
	public int LengthOfLongestSubstring(string s)
	{
		if (s.Length == 0 || s.Length == 1)
		{
			return s.Length;
		}
		int fastIndex = 0;
		int slowIndex = 0;
		int maxLength = 0;

		while (fastIndex + 1 < s.Length)
		{
			fastIndex++;
			for (int i = slowIndex; i < fastIndex; i++)
			{
				if (s[i] == s[fastIndex])
				{
					if (fastIndex - slowIndex > maxLength)
					{
						maxLength = fastIndex - slowIndex;
					}
					slowIndex = i + 1;
					break;
				}
			}
			if (fastIndex - slowIndex + 1 > maxLength)
			{
				maxLength = fastIndex - slowIndex + 1;
			}
		}

		return maxLength;
	}
}