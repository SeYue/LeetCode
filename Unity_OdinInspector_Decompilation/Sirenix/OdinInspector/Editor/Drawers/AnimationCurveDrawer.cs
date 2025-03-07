using System;
using System.Reflection;
using Sirenix.Utilities;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	/// <summary>
	/// Animation curve property drawer.
	/// </summary>
	public sealed class AnimationCurveDrawer : DrawWithUnityBaseDrawer<AnimationCurve>
	{
		private AnimationCurve[] curvesLastFrame;

		private static Action clearCache;

		private static IAtomHandler<AnimationCurve> atomHandler;

		static AnimationCurveDrawer()
		{
			atomHandler = AtomHandlerLocator.GetAtomHandler<AnimationCurve>();
			MethodInfo methodInfo = null;
			Type typeByCachedFullName = AssemblyUtilities.GetTypeByCachedFullName("UnityEditorInternal.AnimationCurvePreviewCache");
			if (typeByCachedFullName != null)
			{
				MethodInfo method = typeByCachedFullName.GetMethod("ClearCache", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
				ParameterInfo[] parameters = method.GetParameters();
				if (parameters != null && parameters.Length == 0)
				{
					methodInfo = method;
				}
			}
			if (methodInfo != null)
			{
				clearCache = EmitUtilities.CreateStaticMethodCaller(methodInfo);
			}
		}

		protected override void Initialize()
		{
			base.Initialize();
			if (clearCache != null)
			{
				clearCache();
				curvesLastFrame = (AnimationCurve[])(object)new AnimationCurve[base.ValueEntry.ValueCount];
				for (int i = 0; i < base.ValueEntry.ValueCount; i++)
				{
					AnimationCurve from = base.ValueEntry.Values[i];
					curvesLastFrame[i] = atomHandler.CreateInstance();
					atomHandler.Copy(ref from, ref curvesLastFrame[i]);
				}
			}
		}

		protected override void DrawPropertyLayout(GUIContent label)
		{
			if (clearCache != null)
			{
				for (int i = 0; i < base.ValueEntry.ValueCount; i++)
				{
					if (!atomHandler.Compare(curvesLastFrame[i], base.ValueEntry.Values[i]))
					{
						clearCache();
						break;
					}
				}
			}
			base.DrawPropertyLayout(label);
			if (clearCache != null)
			{
				for (int j = 0; j < base.ValueEntry.ValueCount; j++)
				{
					AnimationCurve from = base.ValueEntry.Values[j];
					atomHandler.Copy(ref from, ref curvesLastFrame[j]);
				}
			}
		}
	}
}
