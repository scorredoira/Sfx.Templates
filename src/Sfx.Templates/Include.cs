using System;

namespace Sfx.Templates
{
	// Una plantilla que se inserta en otra plantilla.
	sealed class Include : IRenderizable
	{
		public string Path { get; set; }
		public Template MainTemplate { get; set; }

		public void Render(RenderContext context)
		{
			var template = context.Templates [this.Path];
			if (template != null)
			{
				template.Render(context);
			}
		}
	}
}

