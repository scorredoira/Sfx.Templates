using System;
using System.Collections;
using System.Collections.Generic;

namespace Sfx.Templates
{	
	/// <summary>
	/// Un bucle.
	/// </summary>
	sealed class Foreach : IRenderizable, IContainer
	{
		public Template Body { get; set; }
		public string ModelKey { get; set; }
		public string IterateKey { get; set; }
		public string IndexKey { get; set; }

		public void Render(RenderContext context)
		{
			var iterable = Template.GetValue(context.RenderModel, this.ModelKey) as IEnumerable;

			if(iterable != null)
			{
				var index = 0;
				foreach(var modelItem in iterable)
				{
					context.RenderModel.RenderValues[IterateKey] = modelItem;

					if(this.IndexKey != null)
					{
						// incluir el indice en el que está la iteración
						context.RenderModel.RenderValues[this.IndexKey] = index;
					}

					this.Body.Render(context);
					index++;
				}
			}

			context.RenderModel.RenderValues[IterateKey] = null;
		}

		public List<IRenderizable> Items 
		{
			get { return this.Body.Items; }
		}
	}
}

