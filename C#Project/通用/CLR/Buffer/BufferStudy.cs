using System;

public class BufferStudy
{
    public static void Test()
    {
        int[] arr1 = new[] { 1, 2, 3, 4, 5 };
        int[] arr2 = new int[10];

        int size = sizeof(int) * 5;

        Buffer.BlockCopy(arr1, 0, arr2, 0, size);

        Console.WriteLine($"sizeof(int):{sizeof(int)}");
        Console.WriteLine($"Buffer.ByteLength(arr1):{Buffer.ByteLength(arr1)}");
        Console.WriteLine($"打印字节:");
        for (int i = 0; i < Buffer.ByteLength(arr1); i++)
        {
            Console.WriteLine(Buffer.GetByte(arr1, i));
        }

        Console.WriteLine();

        Buffer.SetByte(arr1, 1, 23);
        foreach (var i in arr1)
        {
            Console.WriteLine(i);
        }

        Console.WriteLine();
        foreach (var i in arr2)
        {
            Console.WriteLine(i);
        }
    }
}