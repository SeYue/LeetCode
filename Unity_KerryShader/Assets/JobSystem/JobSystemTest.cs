using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

public class JobSystemTest : MonoBehaviour
{
	private void Awake()
	{
		Debug.Log("Awake"); 
	}

	private void Start()
	{
		Debug.Log(111);

		NativeArray<float> result = new NativeArray<float>(1, Allocator.TempJob);
		MyJob jobData = new MyJob();
		jobData.a = 10;
		jobData.b = 10;
		jobData.result = result;

		JobHandle handle = jobData.Schedule();
		handle.Complete();
		float aPlusB = result[0];
		result.Dispose();

		Debug.Log(aPlusB);
	}
}

public struct MyJob : IJob
{
	public float a;
	public float b;
	public NativeArray<float> result;

	void IJob.Execute()
	{
		result[0] = a + b;
	}
}