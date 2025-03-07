using System;
using System.Collections.Generic;

public class ExampleManager : Singleton_Class<ExampleManager>
{
	public HashSet<ExampleBase> m_examples = new HashSet<ExampleBase>();

	public void AddExample(ExampleBase exampleBase)
	{
		m_examples.Add(exampleBase);
	}
}

public abstract class ExampleBase
{
	public ExampleBase()
	{
		ExampleManager.Instance.AddExample(this);
	}

	public abstract void TestMethod();
}
