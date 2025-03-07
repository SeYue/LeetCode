using System;

public class LogManager
{
	// 当前使用的输出器
	static ILogBase m_currentLog;

	public LogManager()
	{
		m_currentLog = new CSLog();
	}

	public static void Log(string str)
	{
		m_currentLog.Log(str);
	}

	public static void LogFormat(string format, params object[] args)
	{
		m_currentLog.LogFormat(format, args);
	}

	public static void LogError(string str)
	{
		m_currentLog.LogError(str);
	}

	public static void LogErrorFormat(string format, params object[] args)
	{
		m_currentLog.LogErrorFormat(format, args);
	}

	// 抛出异常
	public static void ThrowException(string str)
	{
		throw new Exception(str);
	}
}

// 不同平台输出接口
public interface ILogBase
{
	void Log(string str);
	void LogFormat(string format, params object[] args);
	void LogError(string str);
	void LogErrorFormat(string format, params object[] args);
}

// C#控制台输出
public class CSLog : ILogBase
{
	public void Log(string str)
	{
		Console.WriteLine(str);
	}

	public void LogFormat(string format, params object[] args)
	{
		Console.WriteLine(string.Format(format, args));
	}

	public void LogError(string str)
	{
		CSLogHelepr.StartRedFornt();
		Console.WriteLine(str);
		CSLogHelepr.EndRedFront();
	}

	public void LogErrorFormat(string format, params object[] args)
	{
		CSLogHelepr.StartRedFornt();
		Console.WriteLine(string.Format(format, args));
		CSLogHelepr.EndRedFront();
	}

	// 控制台显示辅助类
	static class CSLogHelepr
	{
		static ConsoleColor defaultColor;

		// 将控制台字体改为红色
		public static void StartRedFornt()
		{
			defaultColor = Console.ForegroundColor;
			Console.ForegroundColor = ConsoleColor.Red;
		}

		// 将控制台字体改为默认颜色
		public static void EndRedFront()
		{
			Console.ForegroundColor = defaultColor;
		}
	}
}