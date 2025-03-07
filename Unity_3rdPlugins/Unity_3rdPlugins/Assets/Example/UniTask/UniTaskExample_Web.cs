using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace UniTaskExample
{
	public class UniTaskExample_Web
	{
		public IEnumerator UnityCall(string url)
		{
			UnityWebRequest webRequest = UnityWebRequest.Get(url);
			yield return webRequest.SendWebRequest();
			if (webRequest.result != UnityWebRequest.Result.Success)
			{
				Debug.Log("网络异常:" + webRequest.error);
			}
			else
			{
				Debug.Log("网络正常");
				Debug.Log(webRequest.downloadHandler.text);
			}
		}

		public async UniTask<string> GetRequest(string url, float timeout)
		{
			CancellationTokenSource cts = new CancellationTokenSource();
			cts.CancelAfterSlim(TimeSpan.FromSeconds(timeout));

			(bool IsCanceled, UnityWebRequest Result) i = await UnityWebRequest.Get(url)
				.SendWebRequest()               // 发送数据
				.WithCancellation(cts.Token)    // 设置取消token
				.SuppressCancellationThrow();   // 不抛出异常,而是通过返回值的方式返回取消的token
			if (!i.IsCanceled)
			{
				return i.Result.downloadHandler.text.Substring(0, 100);
			}
			return "取消或超时";
		}
	}
}
