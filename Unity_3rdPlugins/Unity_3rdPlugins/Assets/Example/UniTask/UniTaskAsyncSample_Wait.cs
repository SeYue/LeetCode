using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace UniTaskExample
{
	public class UniTaskAsyncSample_Wait
	{
		public async UniTask<int> WaitYiled(PlayerLoopTiming loopTiming)
		{
			await UniTask.Yield(loopTiming);
			return 0;
		}

		public async UniTask<object> LoadAsync<T>(string path)
		{
			var asyncOperation = Resources.LoadAsync(path);
			return await asyncOperation;
		}

		public async UniTask<int> WaitNextFrame()
		{
			await UniTask.NextFrame();
			return Time.frameCount;
		}

		//public async UniTask<int> WaitEndOfFrame()
		//{
		//await UniTask.WaitForEndOfFrame();
		//}
	}
}
