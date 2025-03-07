using System;

namespace Sirenix.OdinInspector.Editor
{
	public static class DrawerChainExtensions
	{
		public static BakedDrawerChain Bake(this DrawerChain chain)
		{
			if (chain == null)
			{
				throw new ArgumentNullException("chain");
			}
			BakedDrawerChain bakedDrawerChain = chain as BakedDrawerChain;
			if (bakedDrawerChain != null)
			{
				bakedDrawerChain.Rebake();
				return bakedDrawerChain;
			}
			return new BakedDrawerChain(chain);
		}
	}
}
