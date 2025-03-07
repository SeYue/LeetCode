using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace UniTaskExample
{
	public class UniTaskExampleMain : MonoBehaviour
	{
		private async void Start()
		{
			string urlGoogle = "https://www.google.com.hk/search?hl=zh-CN&q=";
			string url2 = "https://www.baidu.com/";

			UniTaskExample_Web web = new UniTaskExample_Web();
			StartCoroutine(web.UnityCall(url2));

			var str = await web.GetRequest(urlGoogle, 5);
			Debug.Log(str);
		}
	}
}
