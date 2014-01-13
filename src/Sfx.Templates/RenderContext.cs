using System;
using System.Globalization;
using System.IO;

namespace Sfx.Templates
{
	public sealed class RenderContext
	{
		RenderModel renderModel;
		Map items;
		Map<Delegate> functions;
		Map<Template> templates { get; set; }

		internal Map<Template> Templates
		{
			get
			{
				if(this.templates == null)
				{
					this.templates = new Map<Template>();
				}
				return templates;
			}
		}

		internal RenderModel RenderModel
		{
			get
			{
				if(this.renderModel == null)
				{
					this.renderModel = new RenderModel();
				}
				return renderModel;
			}
		}

		public Func<string, object, RenderContext, bool> RenderValue { get; set; }
		public TextWriter Writer { get; set; }
		public CultureInfo Culture { get; set; }

		public object Model 
		{
			get { return this.RenderModel.Model; }
			set { this.RenderModel.Model = value; }
		}

		internal Map Items
		{
			get
			{
				if(items == null)
				{
					items = new Map();
				}
				return items;
			}
		}

		internal Map<Delegate> Functions 
		{
			get
			{
				if(functions == null)
				{
					functions = new Map<Delegate>();
				}
				return functions;
			}
		}

		public RenderContext() : this(null, CultureInfo.CurrentCulture)
		{
		}

		public RenderContext(object model) : this(model, CultureInfo.CurrentCulture)
		{
		}

		public RenderContext(object model, CultureInfo culture)
		{
			this.Model = model;
			this.Culture = culture;
		}

		public RenderContext(object model, TextWriter w)
		{
			this.Model = model;
			this.Writer = w;
		}

		public void Add<TResult>(string name, Func<TResult> func)
		{
			this.Functions.Add(name, func);
		}

		public void Add<T1>(string name, Func<T1, object> func)
		{
			this.Functions.Add(name, func);
		}

		public void Add<T1, T2>(string name, Func<T1, T2, object> func)
		{
			this.Functions.Add(name, func);
		}

		public void Add<T1, T2, T3>(string name, Func<T1, T2, T3, object> func)
		{
			this.Functions.Add(name, func);
		}

		public void Add<T1, T2, T3, T4>(string name, Func<T1, T2, T3, T4, object> func)
		{
			this.Functions.Add(name, func);
		}
	}
}

























