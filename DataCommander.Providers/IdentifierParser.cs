namespace DataCommander.Providers
{
    using System.Collections.Generic;
    using System.IO;
    using System.Text;

    public sealed class IdentifierParser
    {
        private readonly TextReader textReader;

        public IdentifierParser(TextReader textReader)
        {
            this.textReader = textReader;
        }

        public IEnumerable<string> Parse()
        {
            char peekChar = default(char);

            while (true)
            {
                int peek = this.textReader.Peek();

                if (peek == -1)
                    break;

                peekChar = (char)peek;

                if (peekChar == '.')
                {
                    this.textReader.Read();
                }
                else if (peekChar == '[')
                {
                    yield return this.ReadQuotedIdentifier();
                }
                else
                {
                    yield return this.ReadUnquotedIdentifier();
                }
            }

            if (peekChar == '.')
            {
                yield return null;
            }
        }

        private string ReadQuotedIdentifier()
        {
            this.textReader.Read();
            var identifier = new StringBuilder();

            while (true)
            {
                int peek = this.textReader.Peek();

                if (peek == -1)
                    break;

                char peekChar = (char)peek;

                if (peekChar == ']')
                {
                    this.textReader.Read();
                    break;
                }
                else
                {
                    identifier.Append(peekChar);
                    this.textReader.Read();
                }
            }

            return identifier.ToString();
        }

        private string ReadUnquotedIdentifier()
        {
            var identifier = new StringBuilder();

            while (true)
            {
                int peek = this.textReader.Peek();

                if (peek == -1)
                    break;

                char peekChar = (char)peek;

                if (peekChar == '.')
                {
                    break;
                }
                else
                {
                    identifier.Append(peekChar);
                    this.textReader.Read();
                }
            }

            return identifier.ToString();
        }
    }
}