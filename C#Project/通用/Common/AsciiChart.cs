using System;

public class AsciiChart
{
	public static void TestMethod()
	{
		LogManager.Log("Ascii码的数值:");
		for (int i = byte.MinValue; i < byte.MaxValue; i++)
		{
			LogManager.LogFormat("数字:{0}  \t字符:{1}", i, (char)i);
		}

		LogManager.Log(GetAsciiCode('+').ToString());
		LogManager.Log(GetAsciiCode('-').ToString());
		LogManager.Log(GetAsciiCode('*').ToString());
		LogManager.Log(GetAsciiCode('/').ToString());
		LogManager.Log(GetAsciiCode('(').ToString());
		LogManager.Log(GetAsciiCode(')').ToString());
	}

	public static byte GetAsciiCode(char chr)
	{
		return (byte)chr;
	}
}
