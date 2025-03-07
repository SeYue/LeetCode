using System;
using System.Reflection;
using Sirenix.OdinInspector.Internal;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor
{
	/// <summary>
	/// Not yet documented.
	/// </summary>
	[InitializeOnLoad]
	[CanEditMultipleObjects]
	public class OdinEditor : Editor
	{
		private static readonly GUIContent networkChannelLabel = new GUIContent("Network Channel", "QoS channel used for updates. Use the [NetworkSettings] class attribute to change this.");

		private static readonly GUIContent networkSendIntervalLabel = new GUIContent("Network Send Interval", "Maximum update rate in seconds. Use the [NetworkSettings] class attribute to change this, or implement GetNetworkSendInterval");

		private static Type AudioFilterGUIType;

		private static Func<MonoBehaviour, int> AudioUtil_GetCustomFilterChannelCount;

		private static Func<MonoBehaviour, bool> AudioUtil_HaveAudioCallback;

		private static Action<object, MonoBehaviour> DrawAudioFilterGUI;

		private static bool HasReflectedAudioFilter;

		private static bool Initialized = false;

		private int warmupRepaintCount;

		private float labelWidth;

		private object audioFilterGUIInstance;

		[NonSerialized]
		private PropertyTree tree;

		public static bool ForceHideMonoScriptInEditor { get; set; }

		/// <summary>
		/// Not yet documented.
		/// </summary>
		public PropertyTree Tree
		{
			get
			{
				if (tree == null)
				{
					try
					{
						tree = PropertyTree.Create(((Editor)this).get_serializedObject());
					}
					catch (ArgumentException)
					{
					}
				}
				return tree;
			}
		}

		/// <summary>
		/// Draws the default Odin inspector.
		/// </summary>
		public void DrawDefaultInspector()
		{
			DrawOdinInspector();
		}

		/// <summary>
		/// Draws the default Unity inspector.
		/// </summary>
		public void DrawUnityInspector()
		{
			((Editor)this).DrawDefaultInspector();
		}

		/// <summary>
		/// Not yet documented.
		/// </summary>
		public override void OnInspectorGUI()
		{
			DrawOdinInspector();
		}

		/// <summary>
		/// Draws the property tree.
		/// </summary>
		protected virtual void DrawTree()
		{
			Tree.Draw();
		}

		[Obsolete("This method will be removed, use GUIHelper.CurrentWindow.")]
		protected EditorWindow GetInspectorWindow()
		{
			Type type = typeof(EditorWindow).Assembly.GetType("UnityEditor.InspectorWindow");
			try
			{
				EditorWindow currentWindow = GUIHelper.CurrentWindow;
				if ((Object)(object)currentWindow != (Object)null && ((object)currentWindow).GetType() != type)
				{
					return null;
				}
				return currentWindow;
			}
			catch
			{
			}
			return null;
		}

		protected void MockUnityGenericInspector()
		{
			if (IsMissingMonoBehaviourTarget() && MissingMonoBehaviourGUI())
			{
				return;
			}
			((Editor)this).OnInspectorGUI();
			if (HasReflectedAudioFilter && ((Editor)this).get_target() is MonoBehaviour && AudioUtil_HaveAudioCallback(((Editor)this).get_target() as MonoBehaviour) && AudioUtil_GetCustomFilterChannelCount(((Editor)this).get_target() as MonoBehaviour) > 0)
			{
				if (audioFilterGUIInstance == null)
				{
					audioFilterGUIInstance = Activator.CreateInstance(AudioFilterGUIType);
				}
				Action<object, MonoBehaviour> drawAudioFilterGUI = DrawAudioFilterGUI;
				object arg = audioFilterGUIInstance;
				Object target = ((Editor)this).get_target();
				drawAudioFilterGUI(arg, target as MonoBehaviour);
			}
		}

		/// <summary>        
		/// Called by Unity.
		/// </summary>
		protected virtual void OnDisable()
		{
			EnsureInitialized();
			if (tree != null)
			{
				tree.Dispose();
			}
			tree = null;
		}

		/// <summary>
		/// Called by Unity.
		/// </summary>
		protected virtual void OnEnable()
		{
			if (tree != null)
			{
				tree.Dispose();
			}
			tree = null;
			warmupRepaintCount = 0;
			EnsureInitialized();
		}

		private static void EnsureInitialized()
		{
			if (!Initialized)
			{
				Initialized = true;
				try
				{
					string name = (UnityVersion.IsVersionOrGreater(5, 6) ? "HasAudioCallback" : "HaveAudioCallback");
					AudioUtil_HaveAudioCallback = (Func<MonoBehaviour, bool>)Delegate.CreateDelegate(typeof(Func<MonoBehaviour, bool>), typeof(Editor).Assembly.GetType("UnityEditor.AudioUtil").GetMethod(name, BindingFlags.Static | BindingFlags.Public));
					AudioUtil_GetCustomFilterChannelCount = (Func<MonoBehaviour, int>)Delegate.CreateDelegate(typeof(Func<MonoBehaviour, int>), typeof(Editor).Assembly.GetType("UnityEditor.AudioUtil").GetMethod("GetCustomFilterChannelCount", BindingFlags.Static | BindingFlags.Public));
					AudioFilterGUIType = typeof(Editor).Assembly.GetType("UnityEditor.AudioFilterGUI");
					DrawAudioFilterGUI = EmitUtilities.CreateWeakInstanceMethodCaller<MonoBehaviour>(AudioFilterGUIType.GetMethod("DrawAudioFilterGUI", BindingFlags.Instance | BindingFlags.Public));
					HasReflectedAudioFilter = true;
				}
				catch (Exception)
				{
					Debug.LogWarning((object)"The internal Unity class AudioFilterGUI has been changed; cannot properly mock a generic Unity inspector. This probably won't be very noticeable.");
				}
			}
		}

		private void DrawOdinInspector()
		{
			//IL_004c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0052: Invalid comparison between Unknown and I4
			EnsureInitialized();
			if (Tree == null)
			{
				((Editor)this).OnInspectorGUI();
				return;
			}
			if (Tree.RootPropertyCount == 0)
			{
				AssemblyTypeFlags assemblyTypeFlag = ((object)((Editor)this).get_target()).GetType().Assembly.GetAssemblyTypeFlag();
				if (assemblyTypeFlag == AssemblyTypeFlags.UnityTypes || assemblyTypeFlag == AssemblyTypeFlags.UnityEditorTypes)
				{
					MockUnityGenericInspector();
					return;
				}
			}
			if ((int)Event.get_current().get_type() == 8)
			{
				tree.DrawMonoScriptObjectField = !ForceHideMonoScriptInEditor && tree.GetUnitySerializedObjectNoUpdate() != null && tree.TargetType != null && GlobalConfig<GeneralDrawerConfig>.Instance.ShowMonoScriptInEditor && !tree.TargetType.IsDefined(typeof(HideMonoScriptAttribute), inherit: true);
			}
			DrawTree();
			if (UnityNetworkingUtility.NetworkBehaviourType != null && UnityNetworkingUtility.NetworkBehaviourType.IsAssignableFrom(((object)((Editor)this).get_target()).GetType()) && !((object)((Editor)this).get_target()).GetType().IsDefined<HideNetworkBehaviourFieldsAttribute>(inherit: true))
			{
				GUIContent obj = networkChannelLabel;
				Object target = ((Editor)this).get_target();
				EditorGUILayout.LabelField(obj, GUIHelper.TempContent(UnityNetworkingUtility.GetNetworkChannel(target as MonoBehaviour).ToString()), (GUILayoutOption[])(object)new GUILayoutOption[0]);
				GUIContent obj2 = networkSendIntervalLabel;
				Object target2 = ((Editor)this).get_target();
				EditorGUILayout.LabelField(obj2, GUIHelper.TempContent(UnityNetworkingUtility.GetNetworkingInterval(target2 as MonoBehaviour).ToString()), (GUILayoutOption[])(object)new GUILayoutOption[0]);
			}
			RepaintWarmup();
			((Editor)(object)this).RepaintIfRequested();
			if (Object.op_Implicit((Object)(object)GUIHelper.CurrentWindow))
			{
				GUIHelper.CurrentWindow.set_wantsMouseMove(true);
			}
		}

		private void RepaintWarmup()
		{
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			//IL_001a: Invalid comparison between Unknown and I4
			if (warmupRepaintCount < 1)
			{
				((Editor)this).Repaint();
				if ((int)Event.get_current().get_type() == 7)
				{
					warmupRepaintCount++;
				}
			}
		}

		private bool IsMissingMonoBehaviourTarget()
		{
			if (((object)((Editor)this).get_target()).GetType() != typeof(MonoBehaviour))
			{
				return ((object)((Editor)this).get_target()).GetType() == typeof(ScriptableObject);
			}
			return true;
		}

		private bool MissingMonoBehaviourGUI()
		{
			((Editor)this).get_serializedObject().Update();
			SerializedProperty val = ((Editor)this).get_serializedObject().FindProperty("m_Script");
			if (val == null)
			{
				return false;
			}
			EditorGUILayout.PropertyField(val, (GUILayoutOption[])(object)new GUILayoutOption[0]);
			Object objectReferenceValue = val.get_objectReferenceValue();
			MonoScript val2 = objectReferenceValue as MonoScript;
			bool flag = true;
			if ((Object)(object)val2 != (Object)null)
			{
				flag = false;
			}
			if (flag)
			{
				SirenixEditorGUI.WarningMessageBox("The associated script can not be loaded.\nPlease fix any compile errors\nand assign a valid script.");
			}
			if (((Editor)this).get_serializedObject().ApplyModifiedProperties())
			{
				ActiveEditorTracker.get_sharedTracker().ForceRebuild();
			}
			return true;
		}

		public OdinEditor()
			: this()
		{
		}
	}
}
