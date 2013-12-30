using System;
using System.Collections.Generic;

namespace Sfx.Templates
{
	/// <summary>
	/// Una condición en las plantillas.
	/// </summary>
	sealed class Function : IRenderizable
	{
		public string Name { get; set; }		
		public List<string> Arguments { get; set; }

		/// <summary>
		/// El valor asignado desde fuera como resultado de evaluar la función. Este valor
		/// aparecerá en lugar de la función cuando se ejecute Render.
		/// </summary>
		public string Value { get; set; }	

		public void Render(RenderContext context)
		{
			object value = this.Value;
			if(value == null)
			{
				value = Evaluator.Eval(this.Name, this.Arguments, context.RenderModel, context);
			}

			context.Writer.Write(value);
		}
	}
}

