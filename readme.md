Lightweight template engine for C#
=====================================

  * display model values
  * loop through any IEnumerable
  * if/else conditions on booleans or functions
  * limitless nesting.
  * custom functions for displaying values or conditions.
  * includes.
  * extend base templates.


How to use it:
-----------

	Template.Parse("Hello {{.}}!").Render("Joe"); 

	// returns -> Hello Joe!


Samples:
--------

Model can be any object. You access properties with a dot:

	{{ .name }}

You can chain properties. If any is null it simply returns null:

	{{ .Customer.Address.PostalCode }}

If a property implenents IDictionary you can access it by key. Any of this syntax are valid:

	{{ .Request.Cookies.sfxSession.Value }}
	{{ .Request.Cookies["sfxSession"].Value }}
	{{ .Request.Cookies['sfxSession'].Value }}

Loops
----------

	{{ foreach(item in .items) }} 
		{{.item}}
	{{end}}

You can also read the loop index:

	{{ foreach(item, i in .items) }} 
		{{.item}}{{.i}}
	{{end}}

Nesting:

	{{ foreach(item in .items) }} 
		{{ foreach(child in .item) }} 
			{{.child}}
		{{end}}
	{{end}}

Conditions
------------
Conditions can accept boolean values or functions:

	{{ if .User.IsAdmin }} 
	   you are admin
	{{ else }}
	   you are not admin
	{{ end }}

	{{ if isNotNull(.Name) }} 
	   Name: {{ .Name }}
	{{ end }}

Functions
---------
You can define custom functions and call them to display values:

	template.AddFunction("pow", (int a) => a * a);

And in the template:

	{{ pow(.) }} 

You can also use them in condtions:

	template.AddFunction("isvalid", (object a) => a != null);

And in the template:

	{{ if isvalid(.value) }} 

Includes
--------------------

	{{ include header.html }}
		 <h1>Hello</h1>
	{{ include  footer.html }}

Nested includes or includes inside extended templates are possible but please avoid them :)

Extend base templates
--------------------
Base template:

	<html>
	    <head>
	    {{ block header }}{{end}}
	    </head>
	  <body>   
	     {{ block body }}{{end}}
	  </body>
	</html>


Extend it:

	{{ extends base.html }}
	
	{{ block header }}
	    <link href="/css/foo.css" rel="stylesheet" type="text/css" />
	{{ end }}
	
	{{ block body }}
	      <div>
	            <h1>Hello</h1>
	      </div>
	{{ end }}

Renders:

	<html>
	    <head>
	    	<link href="/css/foo.css" rel="stylesheet" type="text/css" />
	    </head>
	  <body>   
	      <div>
	            <h1>Hello</h1>
	      </div>
	  </body>
	</html>



