namespace Sirenix.OdinInspector.Editor.Examples
{
	public class MyDockableGameDashboard : OdinEditorWindow
	{
		private const string DEFAULT_GROUP = "TabGroup/Default/BtnGroup";

		private const string UNIFORM_GROUP = "TabGroup/Uniform/BtnGroup";

		[TabGroup("TabGroup", "Default", false, 0f)]
		[TabGroup("TabGroup", "Uniform", false, 0f)]
		public bool Toggle;

		[TabGroup("TabGroup", "Default", false, 0f, Paddingless = false)]
		[ResponsiveButtonGroup("TabGroup/Default/BtnGroup", DefaultButtonSize = ButtonSizes.Large)]
		public void PepperPepperPepper()
		{
		}

		[ResponsiveButtonGroup("TabGroup/Default/BtnGroup")]
		public void Thud()
		{
		}

		[ResponsiveButtonGroup("TabGroup/Default/BtnGroup")]
		public void WaldoWaldo()
		{
		}

		[ResponsiveButtonGroup("TabGroup/Default/BtnGroup")]
		public void Fred()
		{
		}

		[DisableIf("Toggle")]
		[ResponsiveButtonGroup("TabGroup/Default/BtnGroup")]
		public void FooFoo()
		{
		}

		[ResponsiveButtonGroup("TabGroup/Default/BtnGroup")]
		public void BarBar()
		{
		}

		[ResponsiveButtonGroup("TabGroup/Default/BtnGroup")]
		public void BazBazBaz()
		{
		}

		[DisableIf("Toggle")]
		[ResponsiveButtonGroup("TabGroup/Default/BtnGroup")]
		public void QuxQux()
		{
		}

		[ResponsiveButtonGroup("TabGroup/Default/BtnGroup")]
		public void QuuxQuuxQuux()
		{
		}

		[EnableIf("Toggle")]
		[ResponsiveButtonGroup("TabGroup/Default/BtnGroup")]
		public void CorgeCorge()
		{
		}

		[ResponsiveButtonGroup("TabGroup/Default/BtnGroup")]
		public void Uier()
		{
		}

		[EnableIf("Toggle")]
		[Button(ButtonSizes.Small)]
		[ResponsiveButtonGroup("TabGroup/Default/BtnGroup")]
		public void A()
		{
		}

		[Button(ButtonSizes.Small)]
		[EnableIf("Toggle")]
		[ResponsiveButtonGroup("TabGroup/Default/BtnGroup")]
		public void B()
		{
		}

		[Button(ButtonSizes.Small)]
		[ShowIf("Toggle", false)]
		[ResponsiveButtonGroup("TabGroup/Default/BtnGroup")]
		public void C()
		{
		}

		[EnableIf("Toggle")]
		[ResponsiveButtonGroup("TabGroup/Default/BtnGroup")]
		public void Henk()
		{
		}

		[ResponsiveButtonGroup("TabGroup/Default/BtnGroup")]
		public void Def()
		{
		}

		[ResponsiveButtonGroup("TabGroup/Default/BtnGroup")]
		public void DefDefDef()
		{
		}

		[TabGroup("TabGroup", "Uniform", false, 0f)]
		[ResponsiveButtonGroup("TabGroup/Uniform/BtnGroup", UniformLayout = true)]
		public void FooPepper()
		{
		}

		[ResponsiveButtonGroup("TabGroup/Uniform/BtnGroup")]
		public void FooThud()
		{
		}

		[ResponsiveButtonGroup("TabGroup/Uniform/BtnGroup")]
		public void WaldoFoo()
		{
		}

		[ResponsiveButtonGroup("TabGroup/Uniform/BtnGroup")]
		public void FredFoo()
		{
		}

		[DisableIf("Toggle")]
		[ResponsiveButtonGroup("TabGroup/Uniform/BtnGroup")]
		public void Fooooo()
		{
		}

		[ResponsiveButtonGroup("TabGroup/Uniform/BtnGroup")]
		public void BarFoo()
		{
		}

		[ResponsiveButtonGroup("TabGroup/Uniform/BtnGroup")]
		public void BazFoo()
		{
		}

		[DisableIf("Toggle")]
		[ResponsiveButtonGroup("TabGroup/Uniform/BtnGroup")]
		public void FooQux()
		{
		}

		[ResponsiveButtonGroup("TabGroup/Uniform/BtnGroup")]
		public void QuuxFoo()
		{
		}

		[ResponsiveButtonGroup("TabGroup/Uniform/BtnGroup")]
		public void UierFoo()
		{
		}

		[EnableIf("Toggle")]
		[ResponsiveButtonGroup("TabGroup/Uniform/BtnGroup")]
		public void CorgeFoo()
		{
		}

		[EnableIf("Toggle")]
		[ResponsiveButtonGroup("TabGroup/Uniform/BtnGroup")]
		public void FooGrapl()
		{
		}

		[Button(ButtonSizes.Large)]
		[ResponsiveButtonGroup("TabGroup/Uniform/BtnGroup")]
		public void FooDef()
		{
		}

		[Button(ButtonSizes.Large)]
		[ResponsiveButtonGroup("TabGroup/Uniform/BtnGroup")]
		public void DefFoo()
		{
		}
	}
}
