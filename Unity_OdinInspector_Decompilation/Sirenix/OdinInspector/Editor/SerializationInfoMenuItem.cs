using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor
{
	internal class SerializationInfoMenuItem : OdinMenuItem
	{
		private MemberSerializationInfo info;

		private string typeName;

		public const int IconSize = 20;

		public const int IconSpacing = 4;

		public SerializationInfoMenuItem(OdinMenuTree tree, string name, MemberSerializationInfo instance)
			: base(tree, name, instance)
		{
			info = instance;
			typeName = instance.MemberInfo.GetReturnType().GetNiceName();
		}

		protected override void OnDrawMenuItem(Rect rect, Rect labelRect)
		{
			//IL_0005: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Invalid comparison between Unknown and I4
			//IL_0033: Unknown result type (might be due to invalid IL or missing references)
			//IL_004e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0059: Unknown result type (might be due to invalid IL or missing references)
			//IL_0071: Unknown result type (might be due to invalid IL or missing references)
			//IL_007d: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00da: Unknown result type (might be due to invalid IL or missing references)
			//IL_00df: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
			//IL_0119: Unknown result type (might be due to invalid IL or missing references)
			if ((int)Event.get_current().get_type() == 7)
			{
				((Rect)(ref labelRect)).set_width(((Rect)(ref labelRect)).get_width() - 10f);
				float x = SirenixGUIStyles.Label.CalcSize(GUIHelper.TempContent(base.Name)).x;
				float x2 = SirenixGUIStyles.RightAlignedGreyMiniLabel.CalcSize(GUIHelper.TempContent(typeName)).x;
				GUI.Label(labelRect.SetX(Mathf.Max(((Rect)(ref labelRect)).get_xMin() + x, ((Rect)(ref labelRect)).get_xMax() - x2)).SetXMax(((Rect)(ref labelRect)).get_xMax()), typeName, base.IsSelected ? SirenixGUIStyles.LeftAlignedWhiteMiniLabel : SirenixGUIStyles.LeftAlignedGreyMiniLabel);
				((Rect)(ref rect)).set_x(((Rect)(ref rect)).get_x() + 4f);
				((Rect)(ref rect)).set_x(((Rect)(ref rect)).get_x() + 4f);
				rect = rect.AlignLeft(20f);
				rect = rect.AlignMiddle(20f);
				DrawTheIcon(rect, info.Info.HasAll(SerializationFlags.SerializedByOdin), info.OdinMessageType);
				((Rect)(ref rect)).set_x(((Rect)(ref rect)).get_x() + 28f);
				DrawTheIcon(rect, info.Info.HasAll(SerializationFlags.SerializedByUnity), info.UnityMessageType);
			}
		}

		private void DrawTheIcon(Rect rect, bool serialized, InfoMessageType messageType)
		{
			//IL_0004: Unknown result type (might be due to invalid IL or missing references)
			//IL_000a: Unknown result type (might be due to invalid IL or missing references)
			//IL_001f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0025: Unknown result type (might be due to invalid IL or missing references)
			//IL_003a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0040: Unknown result type (might be due to invalid IL or missing references)
			//IL_0054: Unknown result type (might be due to invalid IL or missing references)
			//IL_0060: Unknown result type (might be due to invalid IL or missing references)
			//IL_008c: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
			switch (messageType)
			{
			case InfoMessageType.Error:
				GUI.DrawTexture(rect.AlignCenterXY(22f), (Texture)(object)EditorIcons.ConsoleErroricon, (ScaleMode)2);
				return;
			case InfoMessageType.Warning:
				GUI.DrawTexture(rect.AlignCenterXY(20f), (Texture)(object)EditorIcons.ConsoleWarnicon, (ScaleMode)2);
				return;
			case InfoMessageType.Info:
				GUI.DrawTexture(rect.AlignCenterXY(20f), (Texture)(object)EditorIcons.ConsoleInfoIcon, (ScaleMode)2);
				return;
			}
			if (serialized)
			{
				GUI.DrawTexture(rect.AlignCenterXY(((Texture)EditorIcons.TestPassed).get_width()), (Texture)(object)EditorIcons.TestPassed, (ScaleMode)2);
				return;
			}
			GUI.set_color(EditorGUIUtility.get_isProSkin() ? new Color(1f, 1f, 1f, 0.2f) : new Color(0.15f, 0.15f, 0.15f, 0.2f));
			EditorIcons.X.Draw(rect.Padding(2f));
			GUI.set_color(Color.get_white());
		}
	}
}
