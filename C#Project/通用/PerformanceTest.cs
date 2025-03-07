using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

public class PerformanceTest : ExampleBase
{
	public override void TestMethod()
	{
		Stopwatch sw = new Stopwatch();
		sw.Start();
		Queue<string> queue = new Queue<string>();
		for (int i = 0; i < 10000000; i++)
		{
			queue.Enqueue(i.ToString());
		}
		sw.Stop();
		Console.WriteLine("初始化队列:" + sw.Elapsed.TotalSeconds);

		sw = new Stopwatch();
		sw.Start();
		while (queue.Count > 0)
		{
			queue.Dequeue();
		}
		sw.Stop();
		Console.WriteLine("遍历队列:" + sw.Elapsed.TotalSeconds);

		 sw = new Stopwatch();
		sw.Start();
		Stack<string> stack = new Stack<string>();
		for (int i = 0; i < 10000000; i++)
		{
			stack.Push(i.ToString());
		}
		sw.Stop();
		Console.WriteLine("初始化栈:" + sw.Elapsed.TotalSeconds);

		sw = new Stopwatch();
		sw.Start();
		while (stack.Count > 0)
		{
			stack.Pop();
		}
		sw.Stop();
		Console.WriteLine("遍历栈:" + sw.Elapsed.TotalSeconds);
	}
}
