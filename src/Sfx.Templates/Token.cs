
namespace Sfx.Templates
{	
	enum TokenType
	{
		Error,
		Text,
		Function,
		Parameter
	}

    sealed class Token
    {
		public TokenType Type { get; set; }
        public string Value { get; set; }
        public int Position { get; set; }

		public Token(TokenType type, int position, string value)
        {
            this.Type = type;
            this.Position = position;
            this.Value = value;
        }

        public override string ToString()
        {
            return this.Type.ToString() + " -> " + this.Value;
        }
    }
}
