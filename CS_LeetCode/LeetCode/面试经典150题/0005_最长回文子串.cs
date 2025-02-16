// https://leetcode.cn/problems/longest-palindromic-substring/
public class 最长回文子串
{
	public 最长回文子串()
	{
		string originStr = "babad";
		var str = LongestPalindrome(originStr);
		Console.WriteLine($"{originStr}\t{str}");

		originStr = "cbbd";
		str = LongestPalindrome(originStr);
		Console.WriteLine($"{originStr}\t{str}");

		originStr = "abb";
		str = LongestPalindrome(originStr);
		Console.WriteLine($"{originStr}\t{str}");
	}

	// 暴力解法
	public string LongestPalindrome(string s)
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
}