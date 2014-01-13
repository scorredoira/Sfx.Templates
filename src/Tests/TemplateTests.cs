using System;
using System.Collections.Generic;
using System.Globalization;
using NUnit.Framework;

namespace Sfx.Templates.Tests
{
	[TestFixture]
    public static class TemplateTests
    {
		[Test]
		public static void ParseValue()
        {
            var template = Template.Parse("Hello {{.}}!");
			Assert.AreEqual(template.Render("Patrick"), "Hello Patrick!");
        }

		[Test]
		public static void ParseValue2()
		{
			var template = Template.Parse("Hello {{.name}}!");
			Assert.AreEqual(template.Render(new Dictionary<string, object>() {{"name", "Patrick" }}), "Hello Patrick!");
		}

		[Test]
		public static void ParseValue3()
		{
			var template = Template.Parse("Hello {{.customer.name}}!");
			var model = new { customer = new Dictionary<string, object>() {{"name", "Patrick" }} };
			Assert.AreEqual(template.Render(model), "Hello Patrick!");
		}

		[Test]
		public static void ParseValue4()
		{
			var template = Template.Parse("Hello {{ .customer.name }}!");
			Assert.AreEqual(template.Render(new { customer = new { name = "Patrick" } }), "Hello Patrick!");
		}

		[Test]
		public static void ParseValue5()
		{
			var template = Template.Parse("Hello {{.customer['name']}}!");
			var model = new { customer = new Dictionary<string, object>() {{"name", "Patrick" }} };
			Assert.AreEqual(template.Render(model), "Hello Patrick!");
		}

		[Test]
		public static void ParseValue6()
		{
			var template = Template.Parse("Hello {{.customer[2]}}!");
			var model = new { customer = new List<string>(){ "juan", "pedro", "luis" }};
			Assert.AreEqual(template.Render(model), "Hello luis!");
		}

		[Test]
		public static void ArgumentWithQuotes()
		{

			var template = Template.Parse("{{ upper(\"q\\\"a\") }}");
			template.AddFunction("upper", (string a) => a.ToUpper());
			Assert.AreEqual("Q\\\"A", template.Render());
		}

		[Test]
		public static void ParseValueWithFormat()
		{
			var template = Template.Parse("{{ format(., \"f\") }}");
			template.Culture = new CultureInfo("es-ES");
			Assert.AreEqual(template.Render(10.2M), "10,20");
		}

		[Test]
		public static void ParseValueWithFormat2()
		{
			var template = Template.Parse("{{ format(., f) }}");
			template.Culture = new CultureInfo("en-US");
			Assert.AreEqual(template.Render(10.2M), "10.20");
		}

		[Test]
		public static void ParseValueWithFormat3()
		{
			var template = Template.Parse("{{ format(., \"dd-MM-yyyy HH:mm\") }}");
			var now = DateTime.Now;
			Assert.AreEqual(template.Render(now), now.ToString("dd-MM-yyyy HH:mm"));
		}

		[Test]
		public static void TestFunc()
		{
			var template = Template.Parse("{{ isnull(.name) }}");
			Assert.AreEqual("False", template.Render(new { name = "bill" }));
		}

		[Test]
		public static void TestFunc2()
		{
			var template = Template.Parse("{{ isnull(.name) }}");
			Assert.AreEqual("True", template.Render(new { name = (string)null }));
		}

		[Test]
		public static void ParseForeach()
		{
			var template = Template.Parse("Models:{{ foreach(item in .) }} {{.item}}{{end}}");
			Assert.AreEqual("Models: Padelclick Golfgest", template.Render(new string[] { "Padelclick", "Golfgest" }));
		}
		
		[Test]
		public static void ParseForeachIndex()
		{
			var template = Template.Parse("Models:{{ foreach(item in .items) }} {{.item}}{{end}}");
			var model = new { items = new string[] { "Padelclick", "Golfgest" }};
			var value = template.Render(model);
			Assert.AreEqual(value, "Models: Padelclick Golfgest");
		}
		
		[Test]
		public static void ParseForeachIndex2()
		{
			var template = Template.Parse("Models:{{ foreach(item, i in .items) }} {{.i}}{{end}}");
			var model = new { items = new string[] { "Padelclick", "Golfgest" }};
			var value = template.Render(model);
			Assert.AreEqual(value, "Models: 0 1");
		}
		
		[Test]
		public static void ParseForeachIndex3()
		{
			var template = Template.Parse("Models:{{ foreach(item, i in .items) }} {{.item}}{{.i}}{{end}}");
			var model = new { items = new string[] { "Padelclick", "Golfgest" }};
			var value = template.Render(model);
			Assert.AreEqual(value, "Models: Padelclick0 Golfgest1");
		}
		
		[Test]
		public static void ParseForeachIndex4()
		{
			var template = Template.Parse("Models:{{ foreach(item, i in .items) }} {{.item}}{{.i}}{{end}}");
			var model = new { items = new string[] { "Padelclick", "Golfgest" }};
			var value = template.Render(model);
			Assert.AreEqual(value, "Models: Padelclick0 Golfgest1");
		}

		[Test]
		public static void ParseNestedForeach()
        {
			var template = Template.Parse(@"Organization:{{ foreach(org in .) }}
                                                {{.org.country}}:{{ foreach(worker in .org.workers) }} {{.worker.name}}{{end}}
                                            {{end}}");

            var model = new object[] {
                new { country = "Spain", workers = new object[] {new { name = "Juan" }, new { name = "Luis" }}},
                new { country = "UK", workers = new object[] { new { name = "John" }, new { name = "Bill" }}}
            };

			var value = template.Render(model);

			var expected = @"Organization:
                                                Spain: Juan Luis
                                            
                                                UK: John Bill
                                            ";

			Assert.AreEqual(value, expected);
        }

		[Test]
		public static void ParseIf()
        {
			var template = Template.Parse("IsAdmin:{{ if equals(.isAdmin, true) }}yes!{{ end }}");
			Assert.AreEqual(template.Render(new { isAdmin = true }), "IsAdmin:yes!");
		}

		[Test]
		public static void IfWithModelKey()
		{
			var template = Template.Parse("IsAdmin:{{ if .isAdmin }} yes!{{ end }}");
			Assert.AreEqual(template.Render(new { isAdmin = true }), "IsAdmin: yes!");
		}

		[Test]
		public static void ParseIfElse()
        {
			var template = Template.Parse("IsAdmin:{{ if equals(.isAdmin, true) }}yes!{{ else }}no!{{ end }}");
			Assert.AreEqual(template.Render(new { isAdmin = false }), "IsAdmin:no!");
        }

		[Test]
		public static void ParseIf2()
        {
			var template = Template.Parse("IsGolf:{{ if equals(., Golfgest) }}yes!{{end}}");
			Assert.AreEqual(template.Render("Golfgest"), "IsGolf:yes!");
        }
		
		[Test]
		public static void TestInclude()
		{
			var template = Template.Parse("{{ include header }} Its {{.time}}.");
			template.ParseInclude("header", "Hello {{.name}}!");
			var render = template.Render(new Dictionary<string, object>() {{"name", "Patrick" }, {"time", "10:00"}});
			Assert.AreEqual(render, "Hello Patrick! Its 10:00.");
		}

		[Test]
		public static void TestExtended()
		{
			var template = Template.Parse("{{ extends main }} {{ block name }}Bill{{ end }} ");
			template.ParseInclude("main", "Hello {{ block name }}{{ end }}!");
			Assert.AreEqual("Hello Bill!", template.Render());
		}

		[Test]
		public static void TestExtended2()
		{
			var template = Template.Parse("{{ extends main }} {{ block name }}Bill{{ end }}  {{ block lastname }}{{ .last }}{{ end }}");
			template.ParseInclude("main", "Hello {{ block name }}{{ end }} {{ block lastName }}{{ end }}!");
			Assert.AreEqual("Hello Bill Smith!", template.Render(new { last = "Smith" }));
		}

		[Test]
		public static void TestGenericFunc3()
		{
			var template = Template.Parse("{{ pow(.) }}");
			template.AddFunction("pow", (int a) => a * a);
			Assert.AreEqual("9", template.Render(3));
		}

		[Test]
		public static void TestGenericFunc33()
		{
			var template = Template.Parse("{{ pow(3) }}");
			template.AddFunction("pow", (int a) => a * a);
			Assert.AreEqual("9", template.Render());
		}

		[Test]
		public static void TestGenericFunc34()
		{
			var template = Template.Parse("{{ sum(3, 8) }}");
			template.AddFunction("sum", (int a, int b) => a + b);
			Assert.AreEqual("11", template.Render());
		}

		[Test]
		public static void TestGenericFunc4()
		{
			var template = Template.Parse("{{ sum(3, 8) }}");
			template.AddFunction("sum", (int a, int b, RenderContext c) => {
				Assert.NotNull(c);
				return a + b;
			});
			Assert.AreEqual("11", template.Render());
		}

		[Test]
		public static void TestGenericFunc5()
		{
			var template = Template.Parse("{{ foo(3, 8, 9, 10, 'ssss') }}");
			template.AddFunction("foo", (int a, int b, string[] args) => a + b);
			Assert.AreEqual("11", template.Render());
		}

		[Test]
		public static void TestGenericFunc6()
		{
			var template = Template.Parse("{{ foo(3, 8, 9, 10, 'ssss') }}");
			template.AddFunction("foo", (int a, string[] args) => {
				return args.Length;
			});
			Assert.AreEqual("4", template.Render());
		}

		[Test]
		public static void TestGenericFunc7()
		{
			var template = Template.Parse("{{ foo('alaslas', 3, 'ssss') }}");
			template.AddFunction("foo", (object a, string[] args) => {
				return args.Length;
			});
			Assert.AreEqual("2", template.Render());
		}

		[Test]
		public static void TestGenericFunc8()
		{
			var template = Template.Parse("{{ foo('alaslas', 3, 'ssss') }}");
			template.AddFunction("foo", (string[] args) => {
				return args.Length;
			});
			Assert.AreEqual("3", template.Render());
		}

		[Test]
		public static void TestGenericFunc9()
		{
			var template = Template.Parse("{{ foo(.a, .b, 3) }}");
			template.AddFunction("foo", (string[] args) => {
				return string.Concat(args);
			});
			Assert.AreEqual("123", template.Render(new { a = "1", b = "2" }));
		}

		[Test]
		public static void TestRenderValue()
		{
			var template = Template.Parse("{{ .name }}");

			template.RenderValue = (a, b, c) => { 
				c.Writer.Write("Bill"); 
				return true; 
			};

			Assert.AreEqual("Bill", template.Render());
		}

		[Test]
		public static void TestRenderValue2()
		{
			var template = Template.Parse("{{ .name }}");

			template.RenderValue = (a, b, c) => { 
				c.Writer.Write("Bill"); 
				return true; 
			};

			Assert.AreEqual("Bill", template.Render(new { name = "John" }));
		}

		[Test]
		public static void TestRenderValue3()
		{
			var template = Template.Parse("{{ .name }}");
			template.RenderValue = (a, b, c) => false;
			Assert.AreEqual("John", template.Render(new { name = "John" }));
		}
    }
}


































