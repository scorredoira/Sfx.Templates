using System;
using System.IO;
using System.Collections.Generic;

namespace Sfx.Templates
{
    /// <summary>
    /// Sencillo parser para poder anidar plantillas fácilmente.
    /// </summary>
    static class Parser
    {
		static readonly char[] argumentSeparators = new char[] {',', ' '};

		public static Template ParseFile(string file)
		{
			var template = Parse(File.ReadAllText(file));
			template.Path = file;
			return template;
		}

        public static Template Parse(string text)
        {
			return Parse(text, null);
        }
		
		static Template Parse(string text, Template mainTemplate)
		{
			var index = 0;
			var lexer = new LexerBase(text);
			lexer.Run(Lexer.LexText);
			var template = Parse(mainTemplate, lexer.Tokens, ref index);
			Validate(template);
			return template;
		}

		static void Validate(Template template)
		{
			var extends = false;

			var index = 0;
			foreach(var item in template.Items)
			{
				var text = item as TextChunk;
				if(text != null && string.IsNullOrWhiteSpace(text.Value))
				{
					// ignorar el espacio blanco
					continue;
				}

				index++;
				if(extends)
				{
					var block = item as TemplateBlock;
					if(block == null)
					{
						throw new TemplateException("A template that extends another can only contain blocks.");
					}
				}
				else
				{
					var function = item as Function;
					if(function != null)
					{
						if(function.Name == "extends")
						{
							if(index > 1)
							{
								throw new TemplateException("extends must appear at the beggining of the template");
							}
							else
							{
								extends = true;
							}
						}
					}
				}
			}
		}

        static Template Parse(Template mainTemplate, List<Token> tokens, ref int index)
        {
            var template = new Template();

			if(mainTemplate != null)
			{
				template.RenderContext = mainTemplate.RenderContext;
			}

            for (var l = tokens.Count; index < l; index++)
            {
				var currentToken = tokens [index];

				if(currentToken.Type == TokenType.Error)
				{
					throw new TemplateException(currentToken.Value);
				}
				else if(currentToken.Type == TokenType.Text)
				{
					template.Items.Add(new TextChunk() { Value = currentToken.Value });
				}
				// si empieza por un punto es un valor.
                else if(currentToken.Value.StartsWith("."))
				{
					var value = ParseValue(currentToken.Value, tokens, ref index);
					template.Items.Add(value);
				}          
				else if (currentToken.Value ==  "include")
				{
					index++; // avanzar para que parsee a partir de aquí
					var value = ParseInclude(currentToken, tokens, ref index, template);
					template.Items.Add(value);
				}       
				else if (currentToken.Value == "block")
				{
					index++; // avanzar para que parsee a partir de aquí
					var value = ParseBlock(currentToken, tokens, ref index, template);
					template.Items.Add(value);
				}
                else if (currentToken.Value == "foreach")
                {
                    index++; // avanzar para que parsee a partir de aquí
					var value = ParseForeach(currentToken, tokens, ref index, mainTemplate);
					template.Items.Add(value);
                }
                else if (currentToken.Value == "if")
                {
                    index++; // avanzar para que parsee a partir de aquí
					var value = ParseIf(currentToken, tokens, ref index, mainTemplate);
					template.Items.Add(value);
				}
                else if (currentToken.Value == "end" || currentToken.Value == "else")
                {
                    var nested = index > 0; // si no es el inicio es que es una plantilla anidada
                    if (!nested)
                    {
                        throw new TemplateException("Unbounded {{end}}");
                    }

					// estaba parseando una plantilla anidada. Terminar y volver.
                    break;
                }
				else if (currentToken.Type == TokenType.Function)
				{
					template.Items.Add(ParseFunction(currentToken, tokens, ref index, template));
				}
            }

            return template;
        }
					
		static IRenderizable ParseFunction(Token currentToken, IList<Token> tokens, ref int index, Template mainTemplate)
		{
			var function = new Function();
			function.Name = currentToken.Value.Trim();
			function.Arguments = new List<string>();

			while(tokens.Count > index + 1)
			{				
				var nextToken = tokens [index + 1];	
				if(nextToken.Type != TokenType.Parameter)
				{
					break;
				}
								
				index++;
				var tokenValue = nextToken.Value;

				// si hay una coma para separar, ignorarla
				if (nextToken.Type == TokenType.Parameter && tokenValue.Trim() == ",")
				{
					index++;
					nextToken = tokens[index];
					tokenValue = nextToken.Value;
				}

				tokenValue = tokenValue.Trim(argumentSeparators).Trim('"');

				// añadir el argumento. Como pueden ir separados por comas, eliminarla.
				function.Arguments.Add(tokenValue);

				// En caso de que extienda a otra, el argumento es la ruta.
				if(function.Name == "extends")
				{
					mainTemplate.BaseTemplate = tokenValue;

					// añadir la clave como referenciada.
					mainTemplate.Templates[tokenValue] = null;
				}
			}
			
			return function;
		}

        static IRenderizable ParseValue(string value, List<Token> tokens, ref int index)
        {
            var v = new ModelValue();
			v.Key = value.Trim();
            return v;
		}

		static IRenderizable ParseInclude(Token currentToken, List<Token> tokens, ref int index, Template mainTemplate)
		{
			var item = new Include();

			if (tokens.Count <= index)
			{
				throw new TemplateException(string.Format("Invalid include at: {0}", currentToken.Position));
			}

			var nextToken = tokens [index];
			if (nextToken.Type != TokenType.Parameter)
			{
				throw new TemplateException(string.Format("Invalid include at: {0}. Name not specified", currentToken.Position));
			}

			item.Path = nextToken.Value.Trim();
			item.MainTemplate = mainTemplate; 

			// añadir la clave como referenciada.
			mainTemplate.Templates[item.Path] = null;
		      
			return item;
		}

		static IRenderizable ParseBlock(Token currentToken, List<Token> tokens, ref int index, Template mainTemplate)
		{
			var item = new TemplateBlock();

			var nextToken = tokens [index];
			if (nextToken.Type != TokenType.Parameter)
			{
				throw new TemplateException(string.Format("Invalid include at: {0}. Name not specified", currentToken.Position));
			}

			item.Path = nextToken.Value.Trim();
			item.MainTemplate = mainTemplate;    
			item.Body = Parse(mainTemplate, tokens, ref index);
			return item;
		}

		/// <summary>
		/// Parses the foreach. El formato es {{ foreach .posts -> post[, i] }}
		/// Donde .posts es el IEnumerable que se itera, post es cada elemento en la iteración y i es el índice opcional.
		/// </summary>
		static IRenderizable ParseForeach (Token currentToken, List<Token> tokens, ref int index, Template mainTemplate)
		{
			var item = new Foreach ();

			if (tokens.Count <= index)
			{
				throw new TemplateException(string.Format("Invalid foreach at: {0}", currentToken.Position));
			}

			// Parsear el nombre del elemento de la iteración
			var nextToken = tokens [index];
			if (nextToken.Type != TokenType.Parameter)
			{
				throw new TemplateException(string.Format("Invalid if foreach at: {0}. Iterate name not found", currentToken.Position));
			}

			item.IterateKey = nextToken.Value.Trim(argumentSeparators);
			index++;

			// si hay una coma para separar, ignorarla
			nextToken = tokens [index];
			if (nextToken.Type == TokenType.Parameter && nextToken.Value.Trim() == ",")
			{
				index++;
				nextToken = tokens [index];
			}

			// Parsear el indice de la iteración (es opcional)
			if (nextToken.Type == TokenType.Parameter && nextToken.Value.Trim() != "in")
			{
				item.IndexKey = nextToken.Value.Trim(argumentSeparators);
				index++;
			}

			// Parsear el separador "in"
			nextToken = tokens [index];
			if (nextToken.Type != TokenType.Parameter && nextToken.Value.Trim() != "in")
			{
				throw new TemplateException(string.Format("Invalid if foreach at: {0}. Iterate separator 'in' not found", currentToken.Position));
			}
			
			index++;
			nextToken = tokens [index];
			if (nextToken.Type != TokenType.Parameter)
			{
				throw new TemplateException(string.Format("Invalid foreach at: {0}. Model not found", currentToken.Position));
			}

			item.ModelKey = nextToken.Value.Trim ();
			index++;

			item.Body = Parse(mainTemplate, tokens, ref index);
            
			return item;
        }
	
		static IRenderizable ParseIf (Token currentToken, List<Token> tokens, ref int index, Template mainTemplate)
		{
			var ifCondition = new IfCondition();

			if(tokens.Count <= index)
			{
				throw new TemplateException(string.Format("Invalid if condition at: {0}", currentToken.Position));
			}

			var condition = tokens[index];
			if(condition.Type == TokenType.Parameter)
			{
				ifCondition.ModelKey = condition.Value;
			}
			else
			{
				var function = ParseFunction(condition, tokens, ref index, null) as Function;
				if(function == null)
				{
					throw new TemplateException(string.Format("Invalid if condition at: {0}", currentToken.Position));
				}
				ifCondition.Expression = function;
			}

			index++;
			ifCondition.Body = Parse(mainTemplate, tokens, ref index);

			// Parsear el else si existe
			if(tokens.Count >= index && tokens[index].Value == "else")
			{
				index++;
				ifCondition.ElseBody = Parse(mainTemplate, tokens, ref index);
			}
            
			return ifCondition;
		}
    }
}




























