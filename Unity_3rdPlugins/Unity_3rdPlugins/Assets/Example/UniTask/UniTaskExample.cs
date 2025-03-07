using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.LowLevel;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace UniTaskExample
{
	/*
	 主要分4个小节介绍UniTask
	 1.异步读取文本
	 2.异步读取文本
	 3.网络异步请求
	 4.有关于delay,例如等待多少,直接在unity对应时间点上加上回调,非常好用



	 */
	public class UniTaskExample : MonoBehaviour
	{
		public Button m_loadTextBtn;
		public Text m_text;

		public Button m_changeScene;
		public Slider m_changeSceneSlider;

		public Button m_webRequest;
		public Image m_tex;
		public int frameCount = 2;

		// Delay
		public Button m_delayBtn;
		public Button m_delayFrameBtn;

		// 生命周期
		public Button m_yieldBtn;
		bool _showUpdateLog;
		public PlayerLoopTiming playerLoopTiming;
		List<PlayerLoopSystem.UpdateFunction> _injectUpdateFunctions = new List<PlayerLoopSystem.UpdateFunction>();

		UniTaskAsyncSample_Wait m_example = new UniTaskAsyncSample_Wait();

		void Start()
		{
			m_loadTextBtn.onClick.AddListener(OnClickLoadText);
			m_changeScene.onClick.AddListener(OnClickChangeScene);
			m_webRequest.onClick.AddListener(OnClickWebRequest);

			m_delayBtn.onClick.AddListener(OnClickDelayBtn);
			m_delayFrameBtn.onClick.AddListener(OnClickDelayNextFrame);

			// 生命周期
			m_yieldBtn.onClick.AddListener(OnClickDelay);

			InjectFunction();

			OnClickWhenAll();
			v.value = 1;
		}

		// 加载文本
		async void OnClickLoadText()
		{
			ResourceRequest loadOperation = Resources.LoadAsync<TextAsset>("Text");
			UnityEngine.Object text = await loadOperation;
			m_text.text = ((TextAsset)text).text;
		}

		// 加载场景
		private async void OnClickChangeScene()
		{
			string targetScene = "UniTaskScene2";
			await SceneManager.LoadSceneAsync(targetScene)
				.ToUniTask(             // 使用这个方法显示进度
				Progress.Create<float>( // 创建一个回调
				p =>
				{
					Debug.Log($"当前进度:{p}");
					m_changeSceneSlider.value = p;
				}));
		}

		// 下载贴图
		async void OnClickWebRequest()
		{
			var webRequest = UnityWebRequestTexture.GetTexture("https://s1.hdslb.com/bfs/static/jinkela/video/asserts/33-coin-ani.png");
			var result = await webRequest.SendWebRequest();
			var texture = ((DownloadHandlerTexture)result.downloadHandler).texture;
			int totalSpriteCount = 24;
			int perSpriteWidth = texture.width / totalSpriteCount;
			Sprite[] sprites = new Sprite[totalSpriteCount];
			for (int i = 0; i < totalSpriteCount; i++)
			{
				sprites[i] = Sprite.Create(texture, new Rect(i * perSpriteWidth, 0, perSpriteWidth, texture.height), Vector2.one * 0.5f);
			}

			while (true)
			{
				for (int i = 0; i < totalSpriteCount; i++)
				{
					m_tex.sprite = sprites[i];
					await UniTask.DelayFrame(frameCount); //UniTask.Delay(TimeSpan.FromSeconds(perFrameTime));
				}
			}
		}

		// 延迟一秒
		async void OnClickDelayBtn()
		{
			Debug.Log($"执行Delay开始,当前时间{Time.time}");
			await UniTask.Delay(TimeSpan.FromSeconds(1), ignoreTimeScale: true);
			Debug.Log($"执行Delay结束,当前时间{Time.time}");
		}

		async void OnClickDelayNextFrame()
		{
			Debug.Log($"当前帧数:{Time.frameCount}");
			await UniTask.DelayFrame(5);
			Debug.Log($"当前帧数:{Time.frameCount}");
		}

		// 往unity底层注册log
		void InjectFunction()
		{
			PlayerLoopSystem playerLoop = PlayerLoop.GetCurrentPlayerLoop();
			PlayerLoopSystem[] subSystems = playerLoop.subSystemList;
			playerLoop.updateDelegate += OnUpdate;
			for (int i = 0; i < subSystems.Length; i++)
			{
				int index = i;
				PlayerLoopSystem.UpdateFunction injectFunction = () =>
				{
					if (_showUpdateLog)
						Debug.Log($"执行子系统{subSystems[index]},当前帧,{Time.frameCount}");
				};
				_injectUpdateFunctions.Add(injectFunction);
				subSystems[i].updateDelegate += injectFunction;
			}

			PlayerLoop.SetPlayerLoop(playerLoop);
		}

		void UnInjectFunction()
		{
			PlayerLoopSystem playerLoop = PlayerLoop.GetCurrentPlayerLoop();
			playerLoop.updateDelegate -= OnUpdate;
			var subsystems = playerLoop.subSystemList;
			for (int i = 0; i < subsystems.Length; i++)
			{
				subsystems[i].updateDelegate -= _injectUpdateFunctions[i];
			}
			_injectUpdateFunctions.Clear();
			PlayerLoop.SetPlayerLoop(playerLoop);
		}

		void OnUpdate()
		{
			//Debug.Log("11");
		}

		// 查看执行时机
		async void OnClickDelay()
		{
			Debug.Log($"当前帧数:{Time.frameCount}");
			_showUpdateLog = true;
			await UniTask.Yield(playerLoopTiming);
			_showUpdateLog = false;
			Debug.Log($"当前帧数:{Time.frameCount}");
		}

		void OnDestroy()
		{
			UnInjectFunction();
		}

		ValueTest v = new ValueTest();
		async void OnClickWhenAll()
		{
			Debug.Log(1);
			await UniTask.Delay(TimeSpan.FromSeconds(1));
			Debug.Log(2);
			await UniTask.WaitUntilValueChanged(v, x =>
			{
				Debug.Log($"校验中,{x.value}");
				return x.value;
			});
			Debug.Log(3);
		}
	}

	public class ValueTest
	{
		public int value = 0;
	}
}
