using System;
using System.Collections.Generic;

namespace Sfx.Templates
{
	/// <summary>
	/// Una condición en las plantillas.
	/// </summary>
	sealed class IfCondition : IRenderizable, IContainer
	{
		// si la condicion es un valor del modelo en vez de una función.
		public string ModelKey { get; set; } 
		public Function Expression { get; set; }
		public Template Body { get; set; }
		public Template ElseBody { get; set; }

		public List<IRenderizable> Items 
		{
			get 
			{
				var items = new List<IRenderizable> (this.Body.Items);
				if (this.ElseBody != null)
				{
					items.AddRange (this.ElseBody.Items);
				}

				return items; 
			}
		}

		public void Render(RenderContext context)
		{
			bool bResult;
			if(this.ModelKey != null)
			{
				var value = Template.GetValue(context.RenderModel, this.ModelKey);
				bResult = Convert.ToBoolean(value);
			}
			else
			{
				var result = Evaluator.Eval(this.Expression.Name, this.Expression.Arguments, context.RenderModel, context);
				bResult = Convert.ToBoolean(result);
			}

			if(bResult)
			{
				this.Body.Render(context);
			}
			else if(this.ElseBody != null)
			{
				this.ElseBody.Render(context);
			}
		}
	}
}

