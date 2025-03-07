using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Sirenix.Serialization;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor
{
	/// <summary>
	/// Base class for creating editor windows using Odin.
	/// </summary>
	/// <example>
	/// <code>
	/// public class SomeWindow : OdinEditorWindow
	/// {
	///     [MenuItem("My Game/Some Window")]
	///     private static void OpenWindow()
	///     {
	///         GetWindow&lt;SomeWindow&gt;().Show();
	///     }
	///
	///     [Button(ButtonSizes.Large)]
	///     public void SomeButton() { }
	///
	///     [TableList]
	///     public SomeType[] SomeTableData;
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// <code>
	/// public class DrawSomeSingletonInAnEditorWindow : OdinEditorWindow
	/// {
	///     [MenuItem("My Game/Some Window")]
	///     private static void OpenWindow()
	///     {
	///         GetWindow&lt;DrawSomeSingletonInAnEditorWindow&gt;().Show();
	///     }
	///
	///     protected override object GetTarget()
	///     {
	///         return MySingleton.Instance;
	///     }
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// <code>
	/// private void InspectObjectInWindow()
	/// {
	///     OdinEditorWindow.InspectObject(someObject);
	/// }
	///
	/// private void InspectObjectInDropDownWithAutoHeight()
	/// {
	///     var btnRect = GUIHelper.GetCurrentLayoutRect();
	///     OdinEditorWindow.InspectObjectInDropDown(someObject, btnRect, btnRect.width);
	/// }
	///
	/// private void InspectObjectInDropDown()
	/// {
	///     var btnRect = GUIHelper.GetCurrentLayoutRect();
	///     OdinEditorWindow.InspectObjectInDropDown(someObject, btnRect, new Vector2(btnRect.width, 100));
	/// }
	///
	/// private void InspectObjectInACenteredWindow()
	/// {
	///     var window = OdinEditorWindow.InspectObject(someObject);
	///     window.position = GUIHelper.GetEditorWindowRect().AlignCenter(270, 200);
	/// }
	///
	/// private void OtherStuffYouCanDo()
	/// {
	///     var window = OdinEditorWindow.InspectObject(this.someObject);
	///
	///     window.position = GUIHelper.GetEditorWindowRect().AlignCenter(270, 200);
	///     window.titleContent = new GUIContent("Custom title", EditorIcons.RulerRect.Active);
	///     window.OnClose += () =&gt; Debug.Log("Window Closed");
	///     window.OnBeginGUI += () =&gt; GUILayout.Label("-----------");
	///     window.OnEndGUI += () =&gt; GUILayout.Label("-----------");
	/// }
	/// </code>
	/// </example>
	/// <seealso cref="T:Sirenix.OdinInspector.Editor.OdinMenuEditorWindow" />
	[ShowOdinSerializedPropertiesInInspector]
	public class OdinEditorWindow : EditorWindow, ISerializationCallbackReceiver
	{
		private Action _onBeginGUI;

		private Action _onEndGUI;

		private static PropertyInfo materialForceVisibleProperty = typeof(MaterialEditor).GetProperty("forceVisible", BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);

		private static bool hasUpdatedOdinEditors = false;

		private static int inspectObjectWindowCount = 3;

		private static readonly object[] EmptyObjectArray = new object[0];

		[SerializeField]
		[HideInInspector]
		private SerializationData serializationData;

		[SerializeField]
		[HideInInspector]
		private Object inspectorTargetSerialized;

		[SerializeField]
		[HideInInspector]
		private float labelWidth = 0.33f;

		[NonSerialized]
		private object inspectTargetObject;

		[SerializeField]
		[HideInInspector]
		private Vector4 windowPadding = new Vector4(4f, 4f, 4f, 4f);

		[SerializeField]
		[HideInInspector]
		private bool useScrollView = true;

		[SerializeField]
		[HideInInspector]
		private bool drawUnityEditorPreview;

		[SerializeField]
		[HideInInspector]
		private int wrappedAreaMaxHeight = 1000;

		[NonSerialized]
		private int drawCountWarmup;

		[NonSerialized]
		private bool isInitialized;

		private GUIStyle marginStyle;

		private object[] currentTargets = new object[0];

		private ImmutableList<object> currentTargetsImm;

		private Editor[] editors = (Editor[])(object)new Editor[0];

		private PropertyTree[] propertyTrees = new PropertyTree[0];

		private Vector2 scrollPos;

		private int mouseDownId;

		private EditorWindow mouseDownWindow;

		private int mouseDownKeyboardControl;

		private Vector2 contenSize;

		private float defaultEditorPreviewHeight = 170f;

		private bool preventContentFromExpanding;

		private EditorTimeHelper timeHelper = new EditorTimeHelper();

		private bool isInsideOnGUI;

		/// <summary>
		/// Gets the label width to be used. Values between 0 and 1 are treated as percentages, and values above as pixels.
		/// </summary>
		public virtual float DefaultLabelWidth
		{
			get
			{
				return labelWidth;
			}
			set
			{
				labelWidth = value;
			}
		}

		/// <summary>
		/// Gets or sets the window padding. x = left, y = right, z = top, w = bottom.
		/// </summary>
		public virtual Vector4 WindowPadding
		{
			get
			{
				//IL_0001: Unknown result type (might be due to invalid IL or missing references)
				return windowPadding;
			}
			set
			{
				//IL_0001: Unknown result type (might be due to invalid IL or missing references)
				//IL_0002: Unknown result type (might be due to invalid IL or missing references)
				windowPadding = value;
			}
		}

		/// <summary>
		/// Gets a value indicating whether the window should draw a scroll view.
		/// </summary>
		public virtual bool UseScrollView
		{
			get
			{
				return useScrollView;
			}
			set
			{
				useScrollView = value;
			}
		}

		/// <summary>
		/// Gets a value indicating whether the window should draw a Unity editor preview, if possible.
		/// </summary>
		public virtual bool DrawUnityEditorPreview
		{
			get
			{
				return drawUnityEditorPreview;
			}
			set
			{
				drawUnityEditorPreview = value;
			}
		}

		/// <summary>
		/// Gets the default preview height for Unity editors.
		/// </summary>
		public virtual float DefaultEditorPreviewHeight
		{
			get
			{
				return defaultEditorPreviewHeight;
			}
			set
			{
				defaultEditorPreviewHeight = value;
			}
		}

		/// <summary>
		/// At the start of each OnGUI event when in the Layout event, the GetTargets() method is called and cached into a list which you can access from here.
		/// </summary>
		protected ImmutableList<object> CurrentDrawingTargets => currentTargetsImm;

		/// <summary>
		/// The Odin property tree drawn.
		/// </summary>
		[Obsolete("Support for non Odin drawn editors and drawing of multiple editors has been added, so there is no longer any guarantee that there will be a PropertyTree.")]
		public PropertyTree PropertyTree
		{
			get
			{
				if (propertyTrees != null)
				{
					return propertyTrees.FirstOrDefault();
				}
				return null;
			}
		}

		/// <summary>
		/// Occurs when the window is closed.
		/// </summary>
		public event Action OnClose;

		/// <summary>
		/// Occurs at the beginning the OnGUI method.
		/// </summary>
		public event Action OnBeginGUI;

		/// <summary>
		/// Occurs at the end the OnGUI method. 
		/// </summary>
		public event Action OnEndGUI;

		/// <summary>
		/// Gets the target which which the window is supposed to draw. By default it simply returns the editor window instance itself. By default, this method is called by <see cref="M:Sirenix.OdinInspector.Editor.OdinEditorWindow.GetTargets" />().
		/// </summary>
		protected virtual object GetTarget()
		{
			if (inspectTargetObject != null)
			{
				return inspectTargetObject;
			}
			if (inspectorTargetSerialized != (Object)null)
			{
				return inspectorTargetSerialized;
			}
			return this;
		}

		/// <summary>
		/// Gets the targets to be drawn by the editor window. By default this simply yield returns the <see cref="M:Sirenix.OdinInspector.Editor.OdinEditorWindow.GetTarget" /> method.
		/// </summary>
		protected virtual IEnumerable<object> GetTargets()
		{
			yield return GetTarget();
		}

		/// <summary>
		/// <para>
		/// Pops up an editor window for the given object in a drop-down window which closes when it loses its focus. 
		/// This particular overload uses a few frames to calculate the height of the content before showing the window with a height that matches its content.
		/// </para>
		/// <para>Protip: You can subscribe to OnClose if you want to know when that occurs.</para>
		/// </summary>
		public static OdinEditorWindow InspectObjectInDropDown(object obj, Rect btnRect, float windowWidth)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0008: Unknown result type (might be due to invalid IL or missing references)
			return InspectObjectInDropDown(obj, btnRect, new Vector2(windowWidth, 0f));
		}

		/// <summary>
		/// Measures the GUILayout content height and adjusts the window height accordingly.
		/// Note that this feature becomes pointless if any layout group expands vertically.
		/// </summary>
		/// <param name="maxHeight">The max height of the window.</param>
		/// <param name="retainInitialWindowPosition">When the window height expands below the screen bounds, it will move the window 
		/// upwards when needed, enabling this will move it back down when the window height is decreased. </param>
		protected void EnableAutomaticHeightAdjustment(int maxHeight, bool retainInitialWindowPosition)
		{
			//IL_002f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0034: Unknown result type (might be due to invalid IL or missing references)
			//IL_0046: Unknown result type (might be due to invalid IL or missing references)
			//IL_004b: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ea: Expected O, but got Unknown
			//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
			//IL_0104: Expected O, but got Unknown
			preventContentFromExpanding = true;
			wrappedAreaMaxHeight = maxHeight;
			Resolution currentResolution = Screen.get_currentResolution();
			int screenHeight = ((Resolution)(ref currentResolution)).get_height() - 40;
			Rect originalP = ((EditorWindow)this).get_position();
			((Rect)(ref originalP)).set_x((float)(int)((Rect)(ref originalP)).get_x());
			((Rect)(ref originalP)).set_y((float)(int)((Rect)(ref originalP)).get_y());
			((Rect)(ref originalP)).set_width((float)(int)((Rect)(ref originalP)).get_width());
			((Rect)(ref originalP)).set_height((float)(int)((Rect)(ref originalP)).get_height());
			Rect currentP = originalP;
			OdinEditorWindow wnd = this;
			int getGoodOriginalPounter = 0;
			int tmpFrameCount = 0;
			CallbackFunction callback = null;
			callback = (CallbackFunction)delegate
			{
				//IL_0010: Unknown result type (might be due to invalid IL or missing references)
				//IL_001a: Expected O, but got Unknown
				//IL_002a: Unknown result type (might be due to invalid IL or missing references)
				//IL_0034: Expected O, but got Unknown
				//IL_0095: Unknown result type (might be due to invalid IL or missing references)
				//IL_009a: Unknown result type (might be due to invalid IL or missing references)
				//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
				//IL_00af: Unknown result type (might be due to invalid IL or missing references)
				//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
				//IL_0109: Unknown result type (might be due to invalid IL or missing references)
				//IL_010e: Unknown result type (might be due to invalid IL or missing references)
				//IL_0137: Unknown result type (might be due to invalid IL or missing references)
				//IL_014c: Unknown result type (might be due to invalid IL or missing references)
				//IL_0162: Unknown result type (might be due to invalid IL or missing references)
				//IL_0177: Unknown result type (might be due to invalid IL or missing references)
				//IL_01c3: Unknown result type (might be due to invalid IL or missing references)
				//IL_01dd: Unknown result type (might be due to invalid IL or missing references)
				//IL_01e7: Expected O, but got Unknown
				EditorApplication.update = (CallbackFunction)Delegate.Remove((Delegate)(object)EditorApplication.update, (Delegate)(object)callback);
				EditorApplication.update = (CallbackFunction)Delegate.Remove((Delegate)(object)EditorApplication.update, (Delegate)(object)callback);
				if (!((Object)(object)wnd == (Object)null))
				{
					if (tmpFrameCount++ < 10)
					{
						((EditorWindow)wnd).Repaint();
					}
					if (getGoodOriginalPounter <= 1 && ((Rect)(ref originalP)).get_y() < 1f)
					{
						getGoodOriginalPounter++;
						originalP = ((EditorWindow)this).get_position();
					}
					else
					{
						Rect position = ((EditorWindow)this).get_position();
						((Rect)(ref originalP)).set_width(((Rect)(ref position)).get_width());
						if (!retainInitialWindowPosition)
						{
							((Rect)(ref originalP)).set_position(((Rect)(ref position)).get_position());
						}
						int num = (int)contenSize.y;
						if ((float)num != ((Rect)(ref currentP)).get_height())
						{
							tmpFrameCount = 0;
							currentP = originalP;
							((Rect)(ref currentP)).set_height((float)Math.Min(num, maxHeight));
							((EditorWindow)wnd).set_minSize(new Vector2(((EditorWindow)wnd).get_minSize().x, ((Rect)(ref currentP)).get_height()));
							((EditorWindow)wnd).set_maxSize(new Vector2(((EditorWindow)wnd).get_maxSize().x, ((Rect)(ref currentP)).get_height()));
							if (((Rect)(ref currentP)).get_yMax() >= (float)screenHeight)
							{
								float num2 = ((Rect)(ref currentP)).get_yMax() - (float)screenHeight;
								ref Rect reference = ref currentP;
								((Rect)(ref reference)).set_y(((Rect)(ref reference)).get_y() - num2);
							}
							((EditorWindow)wnd).set_position(currentP);
						}
					}
					EditorApplication.update = (CallbackFunction)Delegate.Combine((Delegate)(object)EditorApplication.update, (Delegate)(object)callback);
				}
			};
			EditorApplication.update = (CallbackFunction)Delegate.Combine((Delegate)(object)EditorApplication.update, (Delegate)(object)callback);
		}

		/// <summary>
		/// <para>
		/// Pops up an editor window for the given object in a drop-down window which closes when it loses its focus. 
		/// </para>
		/// <para>Protip: You can subscribe to OnClose if you want to know when that occurs.</para>
		/// </summary>
		public static OdinEditorWindow InspectObjectInDropDown(object obj, Rect btnRect, Vector2 windowSize)
		{
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			//IL_002d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0088: Unknown result type (might be due to invalid IL or missing references)
			//IL_0097: Unknown result type (might be due to invalid IL or missing references)
			//IL_0134: Unknown result type (might be due to invalid IL or missing references)
			//IL_0139: Unknown result type (might be due to invalid IL or missing references)
			//IL_0143: Unknown result type (might be due to invalid IL or missing references)
			//IL_0152: Unknown result type (might be due to invalid IL or missing references)
			//IL_0153: Unknown result type (might be due to invalid IL or missing references)
			//IL_015e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0181: Unknown result type (might be due to invalid IL or missing references)
			//IL_0182: Unknown result type (might be due to invalid IL or missing references)
			OdinEditorWindow window = CreateOdinEditorWindowInstanceForObject(obj);
			if (windowSize.x <= 1f)
			{
				windowSize.x = ((Rect)(ref btnRect)).get_width();
			}
			if (windowSize.x <= 1f)
			{
				windowSize.x = 400f;
			}
			((Rect)(ref btnRect)).set_x((float)(int)((Rect)(ref btnRect)).get_x());
			((Rect)(ref btnRect)).set_width((float)(int)((Rect)(ref btnRect)).get_width());
			((Rect)(ref btnRect)).set_height((float)(int)((Rect)(ref btnRect)).get_height());
			((Rect)(ref btnRect)).set_y((float)(int)((Rect)(ref btnRect)).get_y());
			windowSize.x = (int)windowSize.x;
			windowSize.y = (int)windowSize.y;
			try
			{
				EditorWindow curr = GUIHelper.CurrentWindow;
				if ((Object)(object)curr != (Object)null)
				{
					window.OnBeginGUI += delegate
					{
						curr.Repaint();
					};
				}
			}
			catch
			{
			}
			if (!EditorGUIUtility.get_isProSkin())
			{
				window.OnBeginGUI += delegate
				{
					//IL_0010: Unknown result type (might be due to invalid IL or missing references)
					//IL_0015: Unknown result type (might be due to invalid IL or missing references)
					//IL_0023: Unknown result type (might be due to invalid IL or missing references)
					//IL_0028: Unknown result type (might be due to invalid IL or missing references)
					//IL_0030: Unknown result type (might be due to invalid IL or missing references)
					//IL_0035: Unknown result type (might be due to invalid IL or missing references)
					Rect position2 = ((EditorWindow)window).get_position();
					float width2 = ((Rect)(ref position2)).get_width();
					position2 = ((EditorWindow)window).get_position();
					SirenixEditorGUI.DrawSolidRect(new Rect(0f, 0f, width2, ((Rect)(ref position2)).get_height()), SirenixGUIStyles.MenuBackgroundColor);
				};
			}
			window.OnEndGUI += delegate
			{
				//IL_0010: Unknown result type (might be due to invalid IL or missing references)
				//IL_0015: Unknown result type (might be due to invalid IL or missing references)
				//IL_0023: Unknown result type (might be due to invalid IL or missing references)
				//IL_0028: Unknown result type (might be due to invalid IL or missing references)
				//IL_0030: Unknown result type (might be due to invalid IL or missing references)
				Rect position = ((EditorWindow)window).get_position();
				float width = ((Rect)(ref position)).get_width();
				position = ((EditorWindow)window).get_position();
				SirenixEditorGUI.DrawBorders(new Rect(0f, 0f, width, ((Rect)(ref position)).get_height()), 1);
			};
			window.labelWidth = 0.33f;
			window.DrawUnityEditorPreview = true;
			((Rect)(ref btnRect)).set_position(GUIUtility.GUIToScreenPoint(((Rect)(ref btnRect)).get_position()));
			if ((int)windowSize.y == 0)
			{
				((EditorWindow)window).ShowAsDropDown(btnRect, new Vector2(windowSize.x, 10f));
				window.EnableAutomaticHeightAdjustment(600, retainInitialWindowPosition: true);
			}
			else
			{
				((EditorWindow)window).ShowAsDropDown(btnRect, windowSize);
			}
			return window;
		}

		/// <summary>
		/// <para>
		/// Pops up an editor window for the given object in a drop-down window which closes when it loses its focus. 
		/// </para>
		/// <para>Protip: You can subscribe to OnClose if you want to know when that occurs.</para>
		/// </summary>
		public static OdinEditorWindow InspectObjectInDropDown(object obj, Vector2 position)
		{
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_0008: Unknown result type (might be due to invalid IL or missing references)
			//IL_001e: Unknown result type (might be due to invalid IL or missing references)
			Rect btnRect = default(Rect);
			((Rect)(ref btnRect))._002Ector(position.x, position.y, 1f, 1f);
			return InspectObjectInDropDown(obj, btnRect, 350f);
		}

		/// <summary>
		/// <para>
		/// Pops up an editor window for the given object in a drop-down window which closes when it loses its focus. 
		/// </para>
		/// <para>Protip: You can subscribe to OnClose if you want to know when that occurs.</para>
		/// </summary>
		public static OdinEditorWindow InspectObjectInDropDown(object obj, float windowWidth)
		{
			//IL_0005: Unknown result type (might be due to invalid IL or missing references)
			//IL_000a: Unknown result type (might be due to invalid IL or missing references)
			//IL_000d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			//IL_0029: Unknown result type (might be due to invalid IL or missing references)
			Vector2 mousePosition = Event.get_current().get_mousePosition();
			Rect btnRect = default(Rect);
			((Rect)(ref btnRect))._002Ector(mousePosition.x, mousePosition.y, 1f, 1f);
			return InspectObjectInDropDown(obj, btnRect, windowWidth);
		}

		/// <summary>
		/// <para>
		/// Pops up an editor window for the given object in a drop-down window which closes when it loses its focus. 
		/// </para>
		/// <para>Protip: You can subscribe to OnClose if you want to know when that occurs.</para>
		/// </summary>
		public static OdinEditorWindow InspectObjectInDropDown(object obj, Vector2 position, float windowWidth)
		{
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_0008: Unknown result type (might be due to invalid IL or missing references)
			//IL_001e: Unknown result type (might be due to invalid IL or missing references)
			Rect btnRect = default(Rect);
			((Rect)(ref btnRect))._002Ector(position.x, position.y, 1f, 1f);
			return InspectObjectInDropDown(obj, btnRect, windowWidth);
		}

		/// <summary>
		/// <para>
		/// Pops up an editor window for the given object in a drop-down window which closes when it loses its focus. 
		/// </para>
		/// <para>Protip: You can subscribe to OnClose if you want to know when that occurs.</para>
		/// </summary>
		public static OdinEditorWindow InspectObjectInDropDown(object obj, float width, float height)
		{
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			//IL_001a: Unknown result type (might be due to invalid IL or missing references)
			Rect btnRect = default(Rect);
			((Rect)(ref btnRect))._002Ector(Event.get_current().get_mousePosition(), Vector2.get_one());
			return InspectObjectInDropDown(obj, btnRect, new Vector2(width, height));
		}

		/// <summary>
		/// <para>
		/// Pops up an editor window for the given object in a drop-down window which closes when it loses its focus. 
		/// </para>
		/// <para>Protip: You can subscribe to OnClose if you want to know when that occurs.</para>
		/// </summary>
		public static OdinEditorWindow InspectObjectInDropDown(object obj)
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			return InspectObjectInDropDown(obj, Event.get_current().get_mousePosition());
		}

		/// <summary>
		/// Pops up an editor window for the given object.
		/// </summary>
		public static OdinEditorWindow InspectObject(object obj)
		{
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			//IL_002e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0033: Unknown result type (might be due to invalid IL or missing references)
			//IL_0035: Unknown result type (might be due to invalid IL or missing references)
			//IL_0044: Unknown result type (might be due to invalid IL or missing references)
			//IL_0049: Unknown result type (might be due to invalid IL or missing references)
			//IL_004a: Unknown result type (might be due to invalid IL or missing references)
			OdinEditorWindow odinEditorWindow = CreateOdinEditorWindowInstanceForObject(obj);
			((EditorWindow)odinEditorWindow).Show();
			Vector2 move = new Vector2(30f, 30f) * (float)(inspectObjectWindowCount++ % 6 - 3);
			((EditorWindow)odinEditorWindow).set_position(GUIHelper.GetEditorWindowRect().AlignCenter(400f, 300f).AddPosition(move));
			return odinEditorWindow;
		}

		/// <summary>
		/// Inspects the object using an existing OdinEditorWindow.
		/// </summary>
		public static OdinEditorWindow InspectObject(OdinEditorWindow window, object obj)
		{
			//IL_004b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0055: Expected O, but got Unknown
			//IL_0066: Unknown result type (might be due to invalid IL or missing references)
			//IL_0070: Expected O, but got Unknown
			//IL_0079: Unknown result type (might be due to invalid IL or missing references)
			//IL_0083: Expected O, but got Unknown
			Object val = obj as Object;
			if (Object.op_Implicit(val))
			{
				window.inspectTargetObject = null;
				window.inspectorTargetSerialized = val;
			}
			else
			{
				window.inspectorTargetSerialized = null;
				window.inspectTargetObject = obj;
			}
			if (Object.op_Implicit((Object)(val as Component)))
			{
				((EditorWindow)window).set_titleContent(new GUIContent(((Object)(val as Component).get_gameObject()).get_name()));
			}
			else if (Object.op_Implicit(val))
			{
				((EditorWindow)window).set_titleContent(new GUIContent(val.get_name()));
			}
			else
			{
				((EditorWindow)window).set_titleContent(new GUIContent(obj.ToString()));
			}
			EditorUtility.SetDirty((Object)(object)window);
			return window;
		}

		/// <summary>
		/// Creates an editor window instance for the specified object, without opening the window.
		/// </summary>
		public static OdinEditorWindow CreateOdinEditorWindowInstanceForObject(object obj)
		{
			//IL_004f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0059: Expected O, but got Unknown
			//IL_006a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0074: Expected O, but got Unknown
			//IL_007d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0087: Expected O, but got Unknown
			//IL_0088: Unknown result type (might be due to invalid IL or missing references)
			//IL_0097: Unknown result type (might be due to invalid IL or missing references)
			OdinEditorWindow odinEditorWindow = ScriptableObject.CreateInstance<OdinEditorWindow>();
			GUIUtility.set_hotControl(0);
			GUIUtility.set_keyboardControl(0);
			Object val = obj as Object;
			if (Object.op_Implicit(val))
			{
				odinEditorWindow.inspectorTargetSerialized = val;
			}
			else
			{
				odinEditorWindow.inspectTargetObject = obj;
			}
			if (Object.op_Implicit((Object)(val as Component)))
			{
				((EditorWindow)odinEditorWindow).set_titleContent(new GUIContent(((Object)(val as Component).get_gameObject()).get_name()));
			}
			else if (Object.op_Implicit(val))
			{
				((EditorWindow)odinEditorWindow).set_titleContent(new GUIContent(val.get_name()));
			}
			else
			{
				((EditorWindow)odinEditorWindow).set_titleContent(new GUIContent(obj.ToString()));
			}
			((EditorWindow)odinEditorWindow).set_position(GUIHelper.GetEditorWindowRect().AlignCenter(600f, 600f));
			EditorUtility.SetDirty((Object)(object)odinEditorWindow);
			return odinEditorWindow;
		}

		void ISerializationCallbackReceiver.OnAfterDeserialize()
		{
			UnitySerializationUtility.DeserializeUnityObject((Object)(object)this, ref serializationData);
			OnAfterDeserialize();
		}

		void ISerializationCallbackReceiver.OnBeforeSerialize()
		{
			UnitySerializationUtility.SerializeUnityObject((Object)(object)this, ref serializationData);
			OnBeforeSerialize();
		}

		/// <summary>
		/// Draws the Odin Editor Window.
		/// </summary>
		protected virtual void OnGUI()
		{
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0011: Invalid comparison between Unknown and I4
			//IL_0062: Unknown result type (might be due to invalid IL or missing references)
			//IL_0067: Unknown result type (might be due to invalid IL or missing references)
			//IL_0077: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c6: Expected O, but got Unknown
			//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d6: Invalid comparison between Unknown and I4
			//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
			//IL_0100: Unknown result type (might be due to invalid IL or missing references)
			//IL_011c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0138: Unknown result type (might be due to invalid IL or missing references)
			//IL_0153: Unknown result type (might be due to invalid IL or missing references)
			//IL_0158: Unknown result type (might be due to invalid IL or missing references)
			//IL_015e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0192: Unknown result type (might be due to invalid IL or missing references)
			//IL_019d: Unknown result type (might be due to invalid IL or missing references)
			//IL_01a2: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ba: Unknown result type (might be due to invalid IL or missing references)
			//IL_01bf: Unknown result type (might be due to invalid IL or missing references)
			//IL_01c3: Unknown result type (might be due to invalid IL or missing references)
			//IL_01c8: Unknown result type (might be due to invalid IL or missing references)
			//IL_01d2: Unknown result type (might be due to invalid IL or missing references)
			//IL_01d7: Unknown result type (might be due to invalid IL or missing references)
			//IL_01db: Unknown result type (might be due to invalid IL or missing references)
			//IL_01e0: Unknown result type (might be due to invalid IL or missing references)
			//IL_01e3: Unknown result type (might be due to invalid IL or missing references)
			//IL_01e8: Unknown result type (might be due to invalid IL or missing references)
			//IL_01f9: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ff: Invalid comparison between Unknown and I4
			//IL_0202: Unknown result type (might be due to invalid IL or missing references)
			//IL_0204: Unknown result type (might be due to invalid IL or missing references)
			//IL_0299: Unknown result type (might be due to invalid IL or missing references)
			//IL_029e: Unknown result type (might be due to invalid IL or missing references)
			//IL_02ae: Unknown result type (might be due to invalid IL or missing references)
			//IL_02b4: Invalid comparison between Unknown and I4
			//IL_0302: Unknown result type (might be due to invalid IL or missing references)
			//IL_0308: Invalid comparison between Unknown and I4
			//IL_0329: Unknown result type (might be due to invalid IL or missing references)
			//IL_0330: Invalid comparison between Unknown and I4
			InitializeIfNeeded();
			if ((int)Event.get_current().get_type() == 8)
			{
				_onBeginGUI = this.OnBeginGUI;
				_onEndGUI = this.OnEndGUI;
			}
			EditorTimeHelper time = EditorTimeHelper.Time;
			EditorTimeHelper.Time = timeHelper;
			EditorTimeHelper.Time.Update();
			try
			{
				isInsideOnGUI = true;
				bool flag = preventContentFromExpanding;
				Rect val;
				if (flag)
				{
					val = ((EditorWindow)this).get_position();
					GUILayout.BeginArea(new Rect(0f, 0f, ((Rect)(ref val)).get_width(), (float)wrappedAreaMaxHeight));
				}
				if (_onBeginGUI != null)
				{
					_onBeginGUI();
				}
				if (!hasUpdatedOdinEditors)
				{
					GlobalConfig<InspectorConfig>.Instance.EnsureEditorsHaveBeenUpdated();
					hasUpdatedOdinEditors = true;
				}
				object obj = marginStyle;
				if (obj == null)
				{
					GUIStyle val2 = new GUIStyle();
					obj = (object)val2;
					val2.set_padding(new RectOffset());
				}
				marginStyle = (GUIStyle)obj;
				if ((int)Event.get_current().get_type() == 8)
				{
					marginStyle.get_padding().set_left((int)WindowPadding.x);
					marginStyle.get_padding().set_right((int)WindowPadding.y);
					marginStyle.get_padding().set_top((int)WindowPadding.z);
					marginStyle.get_padding().set_bottom((int)WindowPadding.w);
					UpdateEditors();
				}
				EventType type = Event.get_current().get_type();
				if ((int)Event.get_current().get_type() == 0)
				{
					mouseDownId = GUIUtility.get_hotControl();
					mouseDownKeyboardControl = GUIUtility.get_keyboardControl();
					mouseDownWindow = EditorWindow.get_focusedWindow();
				}
				bool flag2 = UseScrollView;
				if (flag2)
				{
					scrollPos = EditorGUILayout.BeginScrollView(scrollPos, (GUILayoutOption[])(object)new GUILayoutOption[0]);
				}
				Vector2 size;
				if (preventContentFromExpanding)
				{
					val = EditorGUILayout.BeginVertical((GUILayoutOption[])GUILayoutOptions.ExpandHeight(expand: false));
					size = ((Rect)(ref val)).get_size();
				}
				else
				{
					val = EditorGUILayout.BeginVertical((GUILayoutOption[])(object)new GUILayoutOption[0]);
					size = ((Rect)(ref val)).get_size();
				}
				if (contenSize == Vector2.get_zero() || (int)Event.get_current().get_type() == 7)
				{
					contenSize = size;
				}
				GUIHelper.PushHierarchyMode(hierarchyMode: false);
				float num = ((!(DefaultLabelWidth < 1f)) ? DefaultLabelWidth : (contenSize.x * DefaultLabelWidth));
				GUIHelper.PushLabelWidth(num);
				OnBeginDrawEditors();
				GUILayout.BeginVertical(marginStyle, (GUILayoutOption[])(object)new GUILayoutOption[0]);
				DrawEditors();
				GUILayout.EndVertical();
				OnEndDrawEditors();
				GUIHelper.PopLabelWidth();
				GUIHelper.PopHierarchyMode();
				EditorGUILayout.EndVertical();
				if (flag2)
				{
					EditorGUILayout.EndScrollView();
				}
				if (_onEndGUI != null)
				{
					_onEndGUI();
				}
				if (Event.get_current().get_type() != type)
				{
					mouseDownId = -2;
				}
				if ((int)Event.get_current().get_type() == 1 && GUIUtility.get_hotControl() == mouseDownId && (Object)(object)EditorWindow.get_focusedWindow() == (Object)(object)mouseDownWindow && GUIUtility.get_keyboardControl() == mouseDownKeyboardControl)
				{
					GUIHelper.RemoveFocusControl();
					GUI.FocusControl((string)null);
				}
				if (drawCountWarmup < 10)
				{
					((EditorWindow)this).Repaint();
					if ((int)Event.get_current().get_type() == 7)
					{
						drawCountWarmup++;
					}
				}
				if (Event.get_current().get_isMouse() || (int)Event.get_current().get_type() == 12 || currentTargets == null || currentTargets.Length == 0)
				{
					((EditorWindow)this).Repaint();
				}
				((EditorWindow)(object)this).RepaintIfRequested();
				if (flag)
				{
					GUILayout.EndArea();
				}
			}
			finally
			{
				isInsideOnGUI = false;
				EditorTimeHelper.Time = time;
			}
		}

		/// <summary>
		/// Calls DrawEditor(index) for each of the currently drawing targets.
		/// </summary>
		protected virtual void DrawEditors()
		{
			for (int i = 0; i < currentTargets.Length; i++)
			{
				DrawEditor(i);
			}
		}

		protected void EnsureEditorsAreReady()
		{
			InitializeIfNeeded();
			UpdateEditors();
		}

		protected void UpdateEditors()
		{
			currentTargets = currentTargets ?? new object[0];
			editors = (Editor[])(((object)editors) ?? ((object)new Editor[0]));
			propertyTrees = propertyTrees ?? new PropertyTree[0];
			IEnumerable<object> targets = GetTargets();
			IList<object> list = (IList<object>)((targets is IList<object>) ? ((IList<object>)targets) : ((targets != null) ? ((object)targets.ToList()) : ((object)EmptyObjectArray)));
			if (currentTargets.Length != list.Count)
			{
				if (editors.Length > list.Count)
				{
					int num = editors.Length - list.Count;
					for (int i = 0; i < num; i++)
					{
						Editor val = editors[editors.Length - i - 1];
						if (Object.op_Implicit((Object)(object)val))
						{
							Object.DestroyImmediate((Object)(object)val);
						}
					}
				}
				if (propertyTrees.Length > list.Count)
				{
					int num2 = propertyTrees.Length - list.Count;
					for (int j = 0; j < num2; j++)
					{
						propertyTrees[propertyTrees.Length - j - 1]?.Dispose();
					}
				}
				Array.Resize(ref currentTargets, list.Count);
				Array.Resize(ref editors, list.Count);
				Array.Resize(ref propertyTrees, list.Count);
				((EditorWindow)this).Repaint();
				currentTargetsImm = new ImmutableList<object>(currentTargets);
			}
			for (int k = 0; k < list.Count; k++)
			{
				object obj = list[k];
				object obj2 = currentTargets[k];
				if (obj == obj2)
				{
					continue;
				}
				GUIHelper.RequestRepaint();
				currentTargets[k] = obj;
				if (obj == null)
				{
					if (propertyTrees[k] != null)
					{
						propertyTrees[k].Dispose();
					}
					propertyTrees[k] = null;
					if (Object.op_Implicit((Object)(object)editors[k]))
					{
						Object.DestroyImmediate((Object)(object)editors[k]);
					}
					editors[k] = null;
					continue;
				}
				EditorWindow val2 = obj as EditorWindow;
				if (obj.GetType().InheritsFrom<Object>() && !Object.op_Implicit((Object)(object)val2))
				{
					Object val3 = obj as Object;
					if (Object.op_Implicit(val3))
					{
						if (propertyTrees[k] != null)
						{
							propertyTrees[k].Dispose();
						}
						propertyTrees[k] = null;
						if (Object.op_Implicit((Object)(object)editors[k]))
						{
							Object.DestroyImmediate((Object)(object)editors[k]);
						}
						editors[k] = Editor.CreateEditor(val3);
						Editor obj3 = editors[k];
						MaterialEditor val4 = obj3 as MaterialEditor;
						if ((Object)(object)val4 != (Object)null && materialForceVisibleProperty != null)
						{
							materialForceVisibleProperty.SetValue(val4, true, null);
						}
					}
					else
					{
						if (propertyTrees[k] != null)
						{
							propertyTrees[k].Dispose();
						}
						propertyTrees[k] = null;
						if (Object.op_Implicit((Object)(object)editors[k]))
						{
							Object.DestroyImmediate((Object)(object)editors[k]);
						}
						editors[k] = null;
					}
				}
				else
				{
					if (propertyTrees[k] != null)
					{
						propertyTrees[k].Dispose();
					}
					if (Object.op_Implicit((Object)(object)editors[k]))
					{
						Object.DestroyImmediate((Object)(object)editors[k]);
					}
					editors[k] = null;
					if (obj is IList)
					{
						propertyTrees[k] = PropertyTree.Create(obj as IList);
					}
					else
					{
						propertyTrees[k] = PropertyTree.Create(obj);
					}
				}
			}
		}

		private void InitializeIfNeeded()
		{
			if (!isInitialized)
			{
				isInitialized = true;
				if (((EditorWindow)this).get_titleContent() != null && ((EditorWindow)this).get_titleContent().get_text() == ((object)this).GetType().FullName)
				{
					((EditorWindow)this).get_titleContent().set_text(((object)this).GetType().GetNiceName().SplitPascalCase());
				}
				((EditorWindow)this).set_wantsMouseMove(true);
				Selection.selectionChanged = (Action)Delegate.Remove(Selection.selectionChanged, new Action(SelectionChanged));
				Selection.selectionChanged = (Action)Delegate.Combine(Selection.selectionChanged, new Action(SelectionChanged));
				Initialize();
			}
		}

		/// <summary>
		/// Initialize get called by OnEnable and by OnGUI after assembly reloads 
		/// which often happens when you recompile or enter and exit play mode.
		/// </summary>
		protected virtual void Initialize()
		{
		}

		private void SelectionChanged()
		{
			((EditorWindow)this).Repaint();
		}

		/// <summary>
		/// Called when the window is enabled. Remember to call base.OnEnable();
		/// </summary>
		protected virtual void OnEnable()
		{
			InitializeIfNeeded();
		}

		/// <summary>
		/// Draws the editor for the this.CurrentDrawingTargets[index].
		/// </summary>
		protected virtual void DrawEditor(int index)
		{
			if (!isInsideOnGUI)
			{
				EnsureEditorsAreReady();
			}
			PropertyTree propertyTree = propertyTrees[index];
			Editor val = editors[index];
			if (propertyTree != null || ((Object)(object)val != (Object)null && val.get_target() != (Object)null))
			{
				if (propertyTree != null)
				{
					object obj = propertyTree.WeakTargets.FirstOrDefault();
					bool applyUndo = Object.op_Implicit(obj as Object);
					propertyTree.Draw(applyUndo);
				}
				else
				{
					OdinEditor.ForceHideMonoScriptInEditor = true;
					try
					{
						val.OnInspectorGUI();
					}
					finally
					{
						OdinEditor.ForceHideMonoScriptInEditor = false;
					}
				}
			}
			if (DrawUnityEditorPreview)
			{
				DrawEditorPreview(index, defaultEditorPreviewHeight);
			}
		}

		/// <summary>
		/// Uses the <see cref="M:UnityEditor.Editor.DrawPreview(UnityEngine.Rect)" /> method to draw a preview for the this.CurrentDrawingTargets[index].
		/// </summary>
		protected virtual void DrawEditorPreview(int index, float height)
		{
			//IL_0030: Unknown result type (might be due to invalid IL or missing references)
			//IL_0035: Unknown result type (might be due to invalid IL or missing references)
			//IL_0037: Unknown result type (might be due to invalid IL or missing references)
			if (!isInsideOnGUI)
			{
				EnsureEditorsAreReady();
			}
			Editor val = editors[index];
			if ((Object)(object)val != (Object)null && val.HasPreviewGUI())
			{
				Rect controlRect = EditorGUILayout.GetControlRect(false, height, (GUILayoutOption[])(object)new GUILayoutOption[0]);
				val.DrawPreview(controlRect);
			}
		}

		/// <summary>
		/// Called when the window is destroyed. Remember to call base.OnDestroy();
		/// </summary>
		protected virtual void OnDestroy()
		{
			if (editors != null)
			{
				for (int i = 0; i < editors.Length; i++)
				{
					if (Object.op_Implicit((Object)(object)editors[i]))
					{
						Object.DestroyImmediate((Object)(object)editors[i]);
						editors[i] = null;
					}
				}
			}
			if (propertyTrees != null)
			{
				for (int j = 0; j < propertyTrees.Length; j++)
				{
					if (propertyTrees[j] != null)
					{
						propertyTrees[j].Dispose();
						propertyTrees[j] = null;
					}
				}
			}
			Selection.selectionChanged = (Action)Delegate.Remove(Selection.selectionChanged, new Action(SelectionChanged));
			Selection.selectionChanged = (Action)Delegate.Remove(Selection.selectionChanged, new Action(SelectionChanged));
			if (this.OnClose != null)
			{
				this.OnClose();
			}
		}

		/// <summary>
		/// Called before starting to draw all editors for the <see cref="P:Sirenix.OdinInspector.Editor.OdinEditorWindow.CurrentDrawingTargets" />.
		/// </summary>
		protected virtual void OnEndDrawEditors()
		{
		}

		/// <summary>
		/// Called after all editors for the <see cref="P:Sirenix.OdinInspector.Editor.OdinEditorWindow.CurrentDrawingTargets" /> has been drawn.
		/// </summary>
		protected virtual void OnBeginDrawEditors()
		{
		}

		/// <summary>
		/// See ISerializationCallbackReceiver.OnBeforeSerialize for documentation on how to use this method.
		/// </summary>
		protected virtual void OnAfterDeserialize()
		{
		}

		/// <summary>
		/// Implement this method to receive a callback after unity serialized your object.
		/// </summary>
		protected virtual void OnBeforeSerialize()
		{
		}

		public OdinEditorWindow()
			: this()
		{
		}//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)

	}
}
