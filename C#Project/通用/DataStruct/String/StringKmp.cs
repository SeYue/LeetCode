using System;

public class StringKmp
{
	// 朴素模式匹配算法
	public static int Index(string mainStr, string temp)
	{
		int i = 0;  // i表示主串当前的下标值
		int j = 0;  // j表示子串当前的下标值

		while (i < mainStr.Length && j < temp.Length)
		{
			Console.WriteLine($"i:{i} \tj:{j} \t S[i]:{mainStr[i]} \t T[i]:{temp[j]} ");
			if (mainStr[i] == temp[j])
			{
				i++;
				j++;
				Console.WriteLine("同");
			}
			else
			{
				i = i - j + 1;
				j = 0;
				Console.WriteLine("不同");
			}
		}

		if (j >= temp.Length)
		{
			return i - temp.Length;
		}
		return -1;
		/*
		for (int i = 0; i < mainStr.Length; i++)
		{
			int pos = i;
			for (int j = 0; j < temp.Length; j++)
			{
				if (mainStr[pos] == temp[j])
				{
					if (j == temp.Length - 1)
					{
						return pos - temp.Length + 1;
					}
					else
					{
						pos++;
						continue;
					}
				}
				else
				{
					break;
				}
			}
		}
		return -1;
		*/
	}

	// 求next数组,暴力解法1
	public static int[] GetNext1(string T)
	{
		int[] next = new int[T.Length];
		next[0] = 0;
		next[1] = 1;

		for (int i = 2; i < T.Length; i++)
		{
			next[i] = 1;
			for (int subStrLength = i - 1; subStrLength >= 1; subStrLength--)
			{
				string prefix = T.Substring(0, subStrLength);
				string postfix = T.Substring(i - subStrLength, subStrLength);

				if (prefix == postfix)
				{
					next[i] = prefix.Length + 1;
					break;
				}
			}
		}

		foreach (var p in next)
		{
			Console.WriteLine(p);
		}
		return next;
	}

	// 求next数组,KMP模式匹配算法,这些人真他妈聪明
	public static int[] GetNext2(string T)
	{
		int[] next = new int[T.Length];
		next[0] = -1;

		int i = 0;
		int k = -1;

		while (i < T.Length - 1)
		{
			Console.WriteLine($"i:{i} \t k:{k}");
			if (k == -1)
			{
				// k==0表示回溯到了起点，已经没有相同的前缀和后缀了
				i++;
				k++;
				next[i] = k;
				Console.WriteLine($"执行了第1步,next[{i}]={k}");
			}
			else if (T[i] == T[k])
			{
				// T[i]==T[k]表示当前位置的前后缀相同
				i++;
				k++;
				next[i] = k;
				Console.WriteLine($"执行了第2步,next[{i}]={k}");
			}
			else
			{
				// 当前位置的前缀和后缀不相等时，回溯到i-1位置的k值
				k = next[k];
				Console.WriteLine($"执行了第3步,k = next[{k}]={k}");
			}
			//Console.Write(T.Substring(0, i) + " : ");
			//for (int j = 0; j <= i; j++)
			//{
			//	int p = next[j];
			//	Console.Write(p + " ");
			//}
			//Console.WriteLine();
			Console.WriteLine();
		}

		//for (int j = 0; j < next.Length; j++)
		//{
		//	next[j]++;
		//}

		Console.WriteLine(T);
		foreach (var p in next)
		{
			Console.Write(p + " ");
		}
		Console.WriteLine();
		Console.WriteLine();
		return next;
	}

	public static int KMP(string mainStr, string temp)
	{
		int i = 0;  // i表示主串当前的下标值
		int j = 0;  // j表示子串当前的下标值
		int[] next = GetNext2(temp);
		while (i < mainStr.Length && j < temp.Length)
		{
			if (j != -1)
				Console.WriteLine($"i:{i} \tj:{j} \t S[i]:{mainStr[i]} \t T[j]:{temp[j]} ");
			else
				Console.WriteLine($"j:{j}");
			if (j == -1 || mainStr[i] == temp[j])
			{
				i++;
				j++;
				Console.WriteLine("同");
			}
			else
			{
				j = next[j];
				if (j == -1)
				{
					i++;
					j++;
				}
				Console.WriteLine("不同");
			}
			Console.WriteLine();
		}

		if (j >= temp.Length)
		{
			return i - temp.Length;
		}
		return -1;
	}

	public static int[] GetNextval(string temp)
	{
		int[] nextval = new int[temp.Length];
		nextval[0] = -1;

		int i = 0;
		int k = -1;

		while (i < temp.Length - 1)
		{
			if (k == -1 || temp[i] == temp[k])
			{
				i++;
				k++;

				if (temp[i] != temp[k])
					nextval[i] = k;
				else
					nextval[i] = nextval[k]; // 多了这一步
			}
			else
			{
				k = nextval[k];
			}
		}
		Console.WriteLine(temp);
		foreach (var p in nextval)
		{
			Console.Write(p + " ");
		}
		Console.WriteLine();
		Console.WriteLine();
		return nextval;
	}
}

public class StringKmpTest
{
	public static void Test()
	{
		StringKmp.GetNext2("11211345");
		StringKmp.GetNextval("ababaaaba");
		//StringKmp.GetNext2("1231234");
	}
}
