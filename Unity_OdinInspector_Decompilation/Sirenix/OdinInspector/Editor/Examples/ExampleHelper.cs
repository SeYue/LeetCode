using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Examples
{
	public static class ExampleHelper
	{
		private static readonly Random random = new Random();

		private static readonly string[] shaderNames = new string[3] { "Standard", "Specular", "Skybox/Cubemap" };

		private static readonly string[] strings = new string[7] { "Hello World", "Sirenix", "Unity", "Lorem Ipsum", "Game Object", "Scriptable Objects", "Ramblings of a mad man" };

		private static readonly string[] meshNames = new string[4] { "Cube", "Sphere", "Cylinder", "Capsule" };

		private static Material[] materials;

		private static Mesh[] meshes;

		private static Texture2D[] textures;

		private static bool initialized;

		private static void InitializeExampleDataSafely()
		{
			//IL_0097: Unknown result type (might be due to invalid IL or missing references)
			//IL_009d: Expected O, but got Unknown
			//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00af: Expected O, but got Unknown
			//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c1: Expected O, but got Unknown
			//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d3: Expected O, but got Unknown
			//IL_00df: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e5: Expected O, but got Unknown
			//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f7: Expected O, but got Unknown
			//IL_0103: Unknown result type (might be due to invalid IL or missing references)
			//IL_0109: Expected O, but got Unknown
			//IL_0116: Unknown result type (might be due to invalid IL or missing references)
			//IL_011c: Expected O, but got Unknown
			if (initialized)
			{
				return;
			}
			initialized = true;
			materials = ((IEnumerable<string>)shaderNames).Select((Func<string, Material>)((string s) => new Material(Shader.Find(s)))).ToArray();
			meshes = meshNames.Select((string s) => Resources.FindObjectsOfTypeAll<Mesh>().FirstOrDefault((Mesh x) => ((Object)x).get_name() == s)).ToArray();
			textures = (Texture2D[])(object)new Texture2D[10]
			{
				EditorIcons.OdinInspectorLogo,
				EditorIcons.UnityLogo,
				(Texture2D)EditorIcons.Upload.Active,
				(Texture2D)EditorIcons.Pause.Active,
				(Texture2D)EditorIcons.Paperclip.Active,
				(Texture2D)EditorIcons.Pen.Active,
				(Texture2D)EditorIcons.Play.Active,
				(Texture2D)EditorIcons.SettingsCog.Active,
				(Texture2D)EditorIcons.ShoppingBasket.Active,
				(Texture2D)EditorIcons.Sound.Active
			};
		}

		public static T GetScriptableObject<T>(string name) where T : ScriptableObject
		{
			T val = ScriptableObject.CreateInstance<T>();
			((Object)(object)val).set_name(name ?? typeof(T).GetNiceName());
			return val;
		}

		public static Material GetMaterial()
		{
			InitializeExampleDataSafely();
			return PickRandom(materials);
		}

		public static Texture2D GetTexture()
		{
			InitializeExampleDataSafely();
			return PickRandom(textures);
		}

		public static Mesh GetMesh()
		{
			InitializeExampleDataSafely();
			return PickRandom(meshes);
		}

		public static string GetString()
		{
			return PickRandom(strings);
		}

		public static float RandomInt(int min, int max)
		{
			return random.Next(min, max);
		}

		public static float RandomFloat(float min, float max)
		{
			return (float)(random.NextDouble() * (double)(max - min) + (double)min);
		}

		private static T PickRandom<T>(IList<T> collection)
		{
			return collection[random.Next(collection.Count)];
		}
	}
}
