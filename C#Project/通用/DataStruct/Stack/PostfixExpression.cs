using System;
using System.Collections.Generic;

// 后缀表达式
public class PostfixExpression
{
	public static void Test()
	{
		Calculate("(1+2)*3+4");
	}

	// 输入中缀表达式，将中缀表达式转换为后缀表达式，然后计算后缀表达式的结果
	public static int Calculate(string expression)
	{
		List<string> postfix = MiddleToPostfix(expression);
		foreach (string i in postfix)
			Console.WriteLine(i);
		return 0;
	}

	static List<string> MiddleToPostfix(string middle)
	{
		List<string> postfix = new List<string>();
		// 符号栈
		Stack<char> operatorStack = new Stack<char>();

		for (int i = 0; i < middle.Length; i++)
		{
			char chr = middle[i];
			if (chr == ' ')
				continue;

			switch (chr)
			{
				case '+':
				case '-':
				case '*':
				case '/':
				case '(':
				case ')':
					if (operatorStack.Count == 0)
					{
						operatorStack.Push(chr);
					}
					else if (chr == '(')
					{
						operatorStack.Push(chr);
					}
					else if (chr == ')')
					{
						// 遇到右括号就一直出栈，直到把左括号出栈
						while (operatorStack.Count > 0)
						{
							char _opera = operatorStack.Pop();
							if (_opera == '(')
								break;
							else
								postfix.Add(_opera.ToString());
						}
					}
					else
					{
						char operatorChr = operatorStack.Peek();
						if (operatorChr == '(')
						{
							operatorStack.Push(chr);
						}
						else
						{
							int topPriority = GetOperatorPriority(operatorChr);
							int currentPriority = GetOperatorPriority(chr);
							if (topPriority < currentPriority)
							{
								operatorStack.Push(chr);
							}
							else
							{
								while (operatorStack.Count > 0)
								{
									operatorChr = operatorStack.Peek();
									topPriority = GetOperatorPriority(operatorChr);
									if (topPriority < currentPriority)
									{
										operatorStack.Push(chr);
										break;
									}

									char _opera = operatorStack.Pop();
									postfix.Add(_opera.ToString());
								}

								operatorStack.Push(chr);
							}
						}
					}
					break;
				default:
					postfix.Add(chr.ToString());
					break;
			}
		}

		while (operatorStack.Count > 0)
		{
			postfix.Add(operatorStack.Pop().ToString());
		}

		return postfix;
	}

	static int GetOperatorPriority(char operatorChr)
	{
		switch (operatorChr)
		{
			case '+':
			case '-':
				return 0;
			case '*':
			case '/':
				return 1;
			default:
				return int.MaxValue;
		}
	}
}
