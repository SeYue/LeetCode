using System;

public abstract class State
{
	public abstract void Handle(Context context);
}

public class ConcreteStateA : State
{
	public override void Handle(Context context)
	{
		context.State = new ConcreteStateB();
	}
}

public class ConcreteStateB : State
{
	public override void Handle(Context context)
	{
		context.State = new ConcreteStateA();
	}
}

public class Context
{
	State state;
	public State State
	{
		get => state;
		set
		{
			state = value;
			Console.WriteLine("切换状态:" + state.GetType().ToString());
		}
	}

	public Context(State state)
	{
		this.state = state;
	}

	public void Request()
	{
		state.Handle(this);
	}
}

// 测试
public class StateModeTest : ExampleBase
{
	public override void TestMethod()
	{
		Context c = new Context(new ConcreteStateA());

		c.Request();
		c.Request();
		c.Request();
		c.Request();
	}
}