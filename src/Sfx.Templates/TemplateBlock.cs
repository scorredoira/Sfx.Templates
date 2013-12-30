
namespace Sfx.Templates
{
	sealed class TemplateBlock : IRenderizable
	{
		public string Path { get; set; }
		public Template MainTemplate { get; set; }
		public Template Body { get; set; }

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

