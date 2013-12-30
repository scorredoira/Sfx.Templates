using System;
using System.Globalization;
using NUnit.Framework;

namespace Sfx.Templates.Tests
{
	public class Program
	{
		public static void Main(string[] args)
		{    
			var template = Template.Parse("{{ format(., \"f\") }}");
			template.Culture = new CultureInfo("es-ES");
			Assert.AreEqual(template.Render(10.2M), "10,20");
		}
	}
}

