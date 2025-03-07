using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace UniTaskExample
{
	public class UniTask_Thread : MonoBehaviour
	{
		public Text Text;

		void Start()
		{
			UniTask.UnityAction(OnClickStandardRun2)();
		}

		async UniTaskVoid OnClickStandardRun()
		{
			int result = 0;
			await UniTask.RunOnThreadPool(() => { result = 1; });
			await UniTask.SwitchToMainThread();
			Text.text = $"结束了,result={result}";
		}

		async UniTaskVoid OnClickStandardRun2()
		{
			int result = 0;
			await UniTask.RunOnThreadPool(() => { result = 1; });
			await UniTask.Yield(PlayerLoopTiming.PreUpdate);
			Text.text = $"结束了,result={result}";
		}
	}
}
