using System;
using System.Collections.Generic;

namespace Sfx.Templates
{
    // Una función de estado que devuelve una función de estado. 
    // De esta forma de un estado se pasa diréctamente al siguiente en vez de volver
    // a un switch en el que recalcular cual es el próximo en función del anterior.
    delegate LexStateFunc LexStateFunc(LexerBase l);

    /// <summary>
    /// Divide input en tokens.
    /// Inspirado en la charla de Rob Pike sobre Go: http://rspace.googlecode.com/hg/slide/lex.html
    /// </summary>
    sealed class LexerBase
    {
        public const char EOF = '\0';
        public string Input; // the string being scanned
        public int Start;
        public int Pos;
        public List<Token> Tokens { get; set; } // En vez del channel acumula aqui los items escaneados.

        public LexerBase(string input)
        {
            this.Input = input;
            this.Tokens = new List<Token>();
        }

        public int Width
        {
            get { return this.Pos - this.Start; }
		}

		// lo que tiene por delante para procesar
		public string Ahead
		{
			get { return this.Input.Substring(this.Pos); }
		}

        public void Run(LexStateFunc initialState)
        {
            LexStateFunc state = initialState;

            while (state != null)
            {
                state = state(this);
            }
        }

        /// <summary>
        /// Genera un nuevo token y avanza.
        /// </summary>
		public Token Emit(TokenType t)
        {
            var token = new Token(t, this.Start, this.Input.Substring(this.Start, this.Pos - this.Start));
            this.Tokens.Add(token);
            this.Start = this.Pos;

			if(Template.Debug)
			{
				Console.WriteLine("{0}. {1} -> {2}", this.Tokens.IndexOf(token), token.Type, token.Value);
			}

			return token;
        }

        /// <summary>
        /// Avanza un caracter
        /// </summary>
        public char Next()
        {
            if (this.Pos >= this.Input.Length)
            {
                return EOF;
            }

            var c = this.Input[this.Pos];
            this.Pos++;
            return c;
        }

        public void Ignore()
        {
            this.Start = this.Pos;
        }

        public void Backup()
        {
            this.Pos--;
        }

        public char Peek()
        {
            return this.Peek(0);
        }

        public char Peek(int index)
        {
            var position = this.Pos + index;
            if (this.Input.Length <= position)
            {
                return EOF;
            }
            return this.Input[position];
        }

        // Devuelve si los próximos caracteres sons igual a prefix
        public bool HasPrefix(string prefix)
        {
            return StartsWith(ref this.Input, this.Pos, prefix);
        }

		/// <summary>
		/// Comprueba si input empieza por value a partir de la posición start.
		/// </summary>
		static bool StartsWith(ref string input, int start, string value)
		{
			if (input.Length < start + value.Length)
			{
				return false;
			}

			for (int i = 0, l = value.Length; i < l; i++)
			{
				if (input[start + i] != value[i])
				{
					return false;
				}
			}

			return true;
		}

        // Consume toda la palabra si existe a continuación
        public bool AcceptWord(string word)
        {
            if (this.HasPrefix(word))
            {
                this.Pos += word.Length;
                return true;
            }

            return false;
        }

        // Consume el proximo caracter si esta dentro del conjunto valid.
        public bool Accept(string valid)
        {
            var c = this.Next();
            if (c == EOF)
            {
                return false;
            }

            this.Backup();
            return valid.IndexOf(c) >= 0;
        }

        // Consume los próximos caracteres si están dentro del conjunto valid.
        public void AcceptAnyChar(string valid)
        {
            while (true)
            {
                var c = this.Next();

                if (c == EOF)
                {
                    return;
                }
                else if (valid.IndexOf(c) == -1)
                {
                    this.Backup();
                    return;
                }
            }
        }

		public override string ToString()
		{
			return this.Input.Substring(this.Start, this.Pos - this.Start);
		}
    }
}
