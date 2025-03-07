using Cysharp.Threading.Tasks;
using System;
using System.Threading;

namespace UniTaskExample
{
	public class UniTaskExample_Cancel
	{


		CancellationTokenSource firstToken;
		CancellationTokenSource seconds;
		CancellationTokenSource linkedCancelToken;

		public UniTaskExample_Cancel()
		{
			firstToken = new CancellationTokenSource();
			seconds = new CancellationTokenSource();
			linkedCancelToken = CancellationTokenSource.CreateLinkedTokenSource(firstToken.Token, seconds.Token);
		}

		public async void WaitA()
		{
			(bool IsCanceled, int Result) j = await WaitB().SuppressCancellationThrow();
			if (j.IsCanceled) { }
			if (j.Result == 1) { }
		}

		public async UniTask<int> WaitB()
		{
			await UniTask.Delay(TimeSpan.FromSeconds(1), cancellationToken: firstToken.Token);
			return 1;
		}

		public (int, int) ReturnTest()
		{
			return (1, 1);
		}
	}
}
