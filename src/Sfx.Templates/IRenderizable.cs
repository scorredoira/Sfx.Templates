using System;
using System.IO;
using System.Collections.Generic;

namespace Sfx.Templates
{	
	public interface IRenderizable
	{
		void Render(RenderContext context);
	}

	public interface IContainer
	{
		List<IRenderizable> Items { get; }
	}
}

