using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

public class ThreadTest
{
	public static void Test()
	{
		CancellationTokenSource cancelToken = new CancellationTokenSource(1000);
		Thread_01.Test12();
		Console.ReadLine();
		Console.WriteLine(1);
	}
}

class Thread_01
{
	public static void Test1()
	{
		Thread t1 = new Thread(WriteY);
		t1.Name = "Y Thread";
		t1.Start();

		for (int i = 0; i < 1000; i++)
		{
			Console.Write("x");
		}
		Console.WriteLine(Thread.CurrentThread.Name);
	}

	static void WriteY()
	{
		Console.WriteLine(Thread.CurrentThread.Name);
		Console.WriteLine();
		for (int i = 0; i < 1000; i++)
		{
			Console.Write("y");
		}
	}

	static readonly object _locker = new object();
	public static void Test2()
	{
		bool isOpen1 = false;
		int i = 0;
		ThreadStart lan = () =>
		{
			lock (_locker)
			{
				Console.WriteLine(i++);
				if (!isOpen1)
				{
					Console.WriteLine("输出我了");
					Thread.Sleep(100);

					isOpen1 = true;
				}
			}
		};

		new Thread(lan).Start();
		lan();
	}

	public static void Test3()
	{
		ManualResetEvent signal = new ManualResetEvent(false);

		new Thread(() =>
		{
			Console.WriteLine("wait for signal");
			signal.WaitOne();
			signal.Dispose();
			Console.WriteLine("got signal");
		}).Start();

		Thread.Sleep(3000);
		signal.Set();
	}

	public static void Test4()
	{
		Task<int> t = Task.Run(() =>
		{
			Thread.Sleep(5000);
			Console.WriteLine("Task Console");
			return 1;
		});
		Console.WriteLine(t.IsCompleted);
		t.Wait(1000);

		Console.WriteLine(t.IsCompleted);
	}

	public static void Test5()
	{
		Task<int> t = Task.Run(() =>
		{
			Thread.Sleep(5000);
			Console.WriteLine("Task Console");
			return 1;
		});
		Console.WriteLine(t.Result);
		Console.WriteLine("finish");
	}

	public static void Test6()
	{
		Task t = Task.Run(() =>
		{
			throw new NullReferenceException();
		});
		Console.WriteLine(t.IsCanceled);
		Console.WriteLine(t.IsFaulted);
		Console.WriteLine(t.Exception.ToString());
		Console.WriteLine("finish");
	}

	public static void Test7()
	{
		Task<int> t = Task.Run(() =>
		{
			Thread.Sleep(2000);
			Console.WriteLine("Running Finish");
			throw null;
			return 10089;
		});
		TaskAwaiter<int> awaiter = t.GetAwaiter();
		awaiter.OnCompleted(() =>
		{
			int result = awaiter.GetResult();
			Console.WriteLine(result);
		});

		Thread.Sleep(3000);
		Console.WriteLine("finish");
	}

	public static void Test8()
	{
		TaskCompletionSource<int> tcs = new TaskCompletionSource<int>();
		new Thread(() =>
		{
			Thread.Sleep(5000);
			tcs.SetResult(42);
		})
		{
			IsBackground = true
		}.Start();
		Task<int> task = tcs.Task;
		Console.WriteLine(task.Result);
	}

	public static void Test9()
	{
		Task t = Delay();
		t.GetAwaiter().OnCompleted(() =>
		{
			Console.WriteLine(t);
		});
		Console.WriteLine();
		Console.ReadLine();
	}

	static Task Delay()
	{
		var tcs = new TaskCompletionSource<object>();
		var timer = new System.Timers.Timer(3000)
		{
			AutoReset = false
		};
		timer.Elapsed += delegate
		{
			timer.Dispose();
			tcs.SetResult(43);
		};
		timer.Start();
		return tcs.Task;
	}

	public static async Task Test10()
	{
		Console.WriteLine(1);
		await Foreach(2);
		await Foreach(3);
		await Foreach(4);
		Console.WriteLine(5);
	}

	static Task Foreach(int v)
	{
		return Task.Run(() =>
		{
			for (int i = 0; i < v; i++)
			{
				Console.Write(i);
			}
			Console.WriteLine(v);
		});
	}

	static event EventHandler<TaskExceptionEventArgs> taskExceptionEventHandler;
	public static void Test11(CancellationToken token)
	{
		taskExceptionEventHandler = (sender, aeArgs) =>
		{
			Console.WriteLine(aeArgs.a.Message);
		};


		Task t = Task.Run(async () =>
		{
			try
			{
				for (int i = 0; i < 100; i++)
				{
					await Task.Delay(100, token);
					Console.WriteLine(i);
				}
			}
			catch (Exception ex)
			{
				taskExceptionEventHandler.Invoke(null, new TaskExceptionEventArgs() { a = ex });
			}
		}, token);
		//.ContinueWith((t) =>
		//{
		//	Console.WriteLine(t.Exception?.InnerException.Message);
		//});

		// 同步式
		//try
		//{
		//	t.Wait();
		//}
		//catch (Exception e)
		//{
		//	Console.WriteLine(e.ToString());
		//}
	}

	class TaskExceptionEventArgs : EventArgs
	{
		public Exception a { get; set; }
	}

	public static async void Test12()
	{
		Task t1 = Task.Run(async () =>
		{
			await Task.Delay(1000);
			Console.WriteLine(1);
		});
		Task t2 = Task.Run(async () =>
		{
			await Task.Delay(2000);
			Console.WriteLine(2);
		});
		Task t3 = Task.Run(async () =>
		{
			await Task.Delay(3000);
			Console.WriteLine(3);
		});
		await Task.WhenAll(t1, t2, t3);
	}
}
