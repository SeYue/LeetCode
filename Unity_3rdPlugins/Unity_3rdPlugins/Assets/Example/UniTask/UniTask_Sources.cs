using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UniTask_Sources : MonoBehaviour
{
	public Button callBackBtn;
	public GameObject Target;
	public const float g = -9.8f;
	public float fallTime = 0.5f;   // µôÂäÊ±¼ä

	void Start()
	{
		callBackBtn.onClick.AddListener(UniTask.UnityAction(OnClickCallback));
	}

	async UniTaskVoid OnClickCallback()
	{
		float time = Time.time;
		UniTaskCompletionSource source = new UniTaskCompletionSource();
		FallTarget(Target.transform, fallTime, null, source).Forget();
		await source.Task;
		Debug.Log($"ºÄÊ±{Time.time - time}");
	}

	async UniTask FallTarget(Transform targetTf, float fallTime, Action onHalf, UniTaskCompletionSource source)
	{
		float startTime = Time.time;
		Vector3 startPosition = targetTf.position;
		float lastElapsedTime = 0;
		while (Time.time - startTime <= fallTime)
		{
			float elapsedTime = Mathf.Min(Time.time - startTime, fallTime);
			if (lastElapsedTime < fallTime * 0.5f && elapsedTime >= fallTime * 0.5f)
			{
				onHalf?.Invoke();
				source.TrySetResult();
			}

			lastElapsedTime = elapsedTime;
			float fallY = 0 + 0.5f * g * elapsedTime * elapsedTime;
			targetTf.position = startPosition + Vector3.down * fallY;
			await UniTask.Yield(this.GetCancellationTokenOnDestroy());
		}
	}
}
