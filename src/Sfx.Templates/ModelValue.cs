using System;

namespace Sfx.Templates
{
	// Una variable referente al modelo
	//
	//     * . significa el modelo en si
	//     * .name sería el valor de la clave "name"
	//
	sealed class ModelValue : IRenderizable
	{
		public string Key { get; set; }

		public void Render(RenderContext context)
		{
			var value = Template.GetValue(context.RenderModel, this.Key);
			if(value == null)
			{
				return;
			}

			var iRenderizable = value as IRenderizable;
			if(iRenderizable != null)
			{
				iRenderizable.Render(context);
			}
			else
			{
				context.Writer.Write(value.ToString());
			}
		}
	}
}

