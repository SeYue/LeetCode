// https://leetcode.cn/problems/longest-palindromic-substring/
using System.Text;

public class 最长回文子串
{
	public 最长回文子串()
	{
		string originStr = "babad";
		var str = CenterExtensionMethod(originStr);
		Console.WriteLine($"{originStr}\t{str}");

		originStr = "cbbd";
		str = CenterExtensionMethod(originStr);
		Console.WriteLine($"{originStr}\t{str}");

		originStr = "abb";
		str = CenterExtensionMethod(originStr);
		Console.WriteLine($"{originStr}\t{str}");
	}

	// 暴力解法
	public string LongestPalindrome_1(string s)
	{
		if (s.Length == 0 || s.Length == 1)
		{
			return s;
		}

		int maxLength = 0;
		string maxLengthStr = string.Empty;

		for (int i = 0; i < s.Length; i++)
		{
			for (int j = i; j < s.Length; j++)
			{
				int childStrLength = j - i + 1;
				if (childStrLength % 2 == 0)
				{
					int center = i + childStrLength / 2;
					int centerL = center - 1;
					int centerR = center;

					bool hasEquals = false;
					while (i <= centerL && centerR <= j)
					{
						if (s[centerL] == s[centerR])
						{
							hasEquals = true;
							if (centerL - 1 >= i && centerR + 1 <= j && s[centerL - 1] == s[centerR + 1])
							{
								centerL--;
								centerR++;
							}
							else
							{
								break;
							}
						}
						else
						{
							break;
						}
					}

					if (hasEquals && centerR - centerL + 1 >= maxLength)
					{
						maxLength = centerR - centerL + 1;
						maxLengthStr = s.Substring(centerL, centerR - centerL + 1);
					}

				}
				else
				{
					if (childStrLength == 0 || childStrLength == 1)
					{
						if (1 > maxLength)
						{
							maxLength = 1;
							maxLengthStr = s.Substring(0, 1);
						}
					}
					else
					{
						int center = i + childStrLength / 2;
						int centerL = center;
						int centerR = center;

						bool hasEquals = false;
						while (i <= centerL && centerR <= j)
						{
							if (s[centerL] == s[centerR])
							{
								hasEquals = true;
								if (centerL - 1 >= i && centerR + 1 <= j && s[centerL - 1] == s[centerR + 1])
								{
									centerL--;
									centerR++;
								}
								else
								{
									break;
								}
							}
							else
							{
								break;
							}
						}

						if (hasEquals && centerR - centerL + 1 >= maxLength)
						{
							maxLength = centerR - centerL + 1;
							maxLengthStr = s.Substring(centerL, centerR - centerL + 1);
						}
					}
				}
			}
		}

		return maxLengthStr;
	}

	// 中心扩展法
	// 遍历到数组某一个元素的时候，向左右两边扩张，如果左边的元素和右边的元素相等，则继续扩张，否则停止扩张并返回。如果没有元素了，也一起返回。
	// 如果有单数长度的字串，则需要对数组进行扩张 abc->#a#b#c#
	string CenterExtensionMethod(string s)
	{
		if (string.IsNullOrEmpty(s) || s.Length == 1)
		{
			return s;
		}

		// 先扩张
		StringBuilder newStr = new StringBuilder();
		for (int i = 0; i < s.Length; i++)
		{
			newStr.Append("#");
			newStr.Append(s[i]);
		}
		newStr.Append("#");

		// 然后使用中心扩展法搜索回文子串
		string str = string.Empty;
		for (int i = 0; i < newStr.Length / 2; i++)
		{

		}
	}

	string CetnerExtension(StringBuilder sb, int i)
	{
		int len = 0;
		// 从中间往两边扩展
		for (int k = 0; k <= i && k < (sb.Length - i); k++)
		{
			if (sb[i - k] == sb[i + k])
			{
				len++;
			}
			else
			{
				break;
			}
		}
		return len - 1;
	}
}