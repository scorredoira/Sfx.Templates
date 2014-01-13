using System;

namespace Sfx.Templates
{
    static class Lexer
    {      
        public static LexStateFunc LexText(LexerBase l)
        {
            while (true)
            {
                if(l.HasPrefix("{{"))
                {
                    if(l.Width > 0)
                    {
                        l.Emit(TokenType.Text);
                    }
                    return LexKeyword;
                }

                if(l.Next() == LexerBase.EOF)
                {
                    if(l.Width > 0)
                    {
                        l.Emit(TokenType.Text);
                    }
                    return null;
                }
            }
        }

        public static LexStateFunc LexKeyword(LexerBase l)
        {
            l.AcceptWord("{{");
            l.AcceptAnyChar(" ");
            l.Ignore();

			if(l.Accept("."))
			{
				return LexModelKey;
			}

            while (true)
            {
                var c = l.Next();
				if (!char.IsLetterOrDigit(c))
                {
                    l.Backup();
                    break;
                }
            }

			var token = l.Emit(TokenType.Function);
			l.AcceptAnyChar(" (");
			l.Ignore();

			if(token.Value == "if")
			{
				// despues de un if viene una función
				return LexKeyword;
			}

			switch(token.Value)
			{		
				case "end":
				case "else":
					return LexInstruction;	

				case "include":
					return LexParameter;
			}

            return LexParameters;
		}

		public static LexStateFunc LexParameter(LexerBase lexer)
		{
			lexer.AcceptAnyChar(" ");
			lexer.Ignore();

			while (true)
			{
				if (lexer.HasPrefix("}}"))
				{
					if(lexer.Width > 0)
					{
						lexer.Emit(TokenType.Parameter);
					}
					lexer.AcceptWord("}}");
					lexer.Ignore();
					return LexText;
				}

				var c = lexer.Next();
				if(c == LexerBase.EOF)
				{
					throw new TemplateException(string.Format("Unfinished parameter line {0}", lexer.Line));
				}
			}
		}

		public static LexStateFunc LexInstruction(LexerBase lexer)
		{
			lexer.AcceptAnyChar(" ");
			if(!lexer.AcceptWord("}}"))
			{				
				throw new TemplateException(string.Format("Error after model key line {0}", lexer.Line));
			}
			lexer.Ignore();
			return LexText;
		}

		public static LexStateFunc LexModelKey(LexerBase lexer)
		{
			while (true)
			{
				var c = lexer.Next();

				// las keys son letras, numeros, corchetes (para indices) y comillas dentro de los indices.
				if (c != '.' && 
				    !char.IsLetterOrDigit(c) && 
				    c != '[' && 
				    c != ']' && 
				    c != '"' && 
				    c != '\'')
				{
					lexer.Backup();
					break;
				}
			}

			lexer.Emit(TokenType.Parameter);
			
			lexer.AcceptAnyChar(" ");
			if(!lexer.AcceptWord("}}"))
			{				
				throw new TemplateException(string.Format("Error after model key line {0}", lexer.Line));
			}

			lexer.Ignore();

			return LexText;
		}

        public static LexStateFunc LexParameters(LexerBase lexer)
		{
			lexer.AcceptAnyChar(" ,");
			lexer.Ignore();

            while (true)
			{
				if (lexer.HasPrefix(")"))
				{
					if(lexer.Width > 0)
					{
						lexer.Emit(TokenType.Parameter);
					}
					lexer.AcceptAnyChar(") ");
					lexer.AcceptWord("}}");
					lexer.Ignore();
					return LexText;
				}

				if (lexer.HasPrefix("}}"))
				{
					if(lexer.Width > 0)
					{
						lexer.Emit(TokenType.Parameter);
					}
					lexer.AcceptWord("}}");
					lexer.Ignore();
					return LexText;
				}

                var c = lexer.Next();
                switch(c)
				{
					case '"':
						return LexQuotes;

					case ' ':
					case ',':
						lexer.Backup();
                        if(lexer.Width > 0)
                        {
                            lexer.Emit(TokenType.Parameter);
						}
						lexer.AcceptAnyChar(" ,");
						lexer.Ignore();
                        break;

					case LexerBase.EOF:
						throw new TemplateException(string.Format("Unfinished parameter at line {0}", lexer.Line));
                }
            }
        }
				
		public static LexStateFunc LexQuotes(LexerBase lexer)
		{ 
			while(true)
			{
				var c = lexer.Next();
				switch(c)
				{
					case '\\':
						if(lexer.Peek() == '"')
						{
							lexer.Next();
						}
						break;

					case '"':
						lexer.Emit(TokenType.Parameter);
						return LexParameters;

					case LexerBase.EOF:
						throw new TemplateException(string.Format("Unfinished parameter at line {0}", lexer.Line));
				}
			}
		}
    }
}





































