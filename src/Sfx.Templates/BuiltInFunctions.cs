using System;

namespace Sfx.Templates
{
	static class BuiltInFunctions
	{
		public static Delegate GetFunction(string name, RenderContext context)
		{
			switch(name.ToLower())
			{
				case "equals":
					Func<object, object, bool> eq = (a, b) => Evaluator.ConvertToString(a) == Evaluator.ConvertToString(b);
					return eq;

				case "isnotnull":
					Func<object, bool> isNotNull = (v) => v != null;
					return isNotNull;

				case "isnull":
					Func<object, bool> isNull = (v) => v == null;
					return isNull;

				case "istrue":
					Func<bool, bool> isTrue = (v) => v;
					return isTrue;

				case "isfalse":
					Func<bool, bool> isFalse = (v) => !v;
					return isFalse;

				case "isempty":
					Func<object, bool> isEmpty = (v) => v == null || v.ToString() == string.Empty;
					return isEmpty;

				case "format":
					Func<object, string, RenderContext, string> format = (v, f, c) => {
						var formattable = v as IFormattable;
						if(formattable != null)
						{
							return formattable.ToString(f, c.Culture);
						}
						return null;
					};
					return format;
			}

			return null;
		}
	}
}

