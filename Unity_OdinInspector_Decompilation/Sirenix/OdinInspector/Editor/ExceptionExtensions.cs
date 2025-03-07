using System;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor
{
	internal static class ExceptionExtensions
	{
		public static bool IsExitGUIException(this Exception ex)
		{
			do
			{
				if (ex is ExitGUIException)
				{
					return true;
				}
				ex = ex.InnerException;
			}
			while (ex != null);
			return false;
		}

		public static ExitGUIException AsExitGUIException(this Exception ex)
		{
			do
			{
				if (ex is ExitGUIException)
				{
					return ex as ExitGUIException;
				}
				ex = ex.InnerException;
			}
			while (ex != null);
			return null;
		}
	}
}
