namespace DataCommander.Providers
{
    using System.Linq;
    using System.Text;

    internal sealed class TokenIterator
    {
        #region Private Fields

        private static readonly char[] operatorsOrPunctuators = new char[]
        {
            '{', '}', '[', ']', '(', ')', '.', ',', ':', ';', '+', '-', '*', '/', '%', '&', '|', '^', '!', '~', '=', '<', '>', '?'
        };

        private readonly string text;
        private int index = 0;
        private readonly int length;
        private int tokenIndex;
        private int lineIndex;

        #endregion

        public TokenIterator(string text)
        {
            this.text = text;

            if (text != null)
            {
                this.length = text.Length;
            }
            else
            {
                this.length = 0;
            }
        }

        public Token Next()
        {
            Token token = null;

            while (this.index < this.length)
            {
                int startPosition;
                int endPosition;
                string value;
                var c = this.text[this.index];

                if (c == 'N')
                {
                    startPosition = this.index;
                    if (this.index + 1 < this.length && this.text[this.index + 1] == '\'')
                    {
                        this.index++;
                        value = this.ReadString();
                        endPosition = this.index;
                        token = new Token(this.tokenIndex, startPosition, endPosition - 1, this.lineIndex, TokenType.String, value);
                    }
                    else
                    {
                        value = this.ReadKeyWord();
                        endPosition = this.index;
                        token = new Token(this.tokenIndex, startPosition, endPosition - 1, this.lineIndex, TokenType.KeyWord, value);
                    }
                    break;
                }
                else if (char.IsLetter(c) || c == '[' || c == '@')
                {
                    startPosition = this.index;
                    value = this.ReadKeyWord();
                    endPosition = this.index;
                    token = new Token(this.tokenIndex, startPosition, endPosition - 1, this.lineIndex, TokenType.KeyWord, value);
                    break;
                }
                else if (c == '"' || c == '\'')
                {
                    startPosition = this.index;
                    value = this.ReadString();
                    endPosition = this.index;
                    token = new Token(this.tokenIndex, startPosition, endPosition - 1, this.lineIndex, TokenType.String, value);
                    break;
                }
                else if (char.IsDigit(c) || c == '-')
                {
                    startPosition = this.index;
                    value = this.ReadDigit();
                    endPosition = this.index;
                    token = new Token(this.tokenIndex, startPosition, endPosition - 1, this.lineIndex, TokenType.Digit, value);
                    break;
                }
                else if (operatorsOrPunctuators.Contains(c))
                {
                    startPosition = this.index;
                    value = c.ToString();
                    endPosition = this.index;
                    token = new Token(this.tokenIndex, startPosition, endPosition, this.lineIndex, TokenType.OperatorOrPunctuator, value);
                    this.index++;
                    break;
                }
                else if (c == '\r')
                {
                    this.lineIndex++;
                    this.index += 2;
                }
                else
                {
                    this.index++;
                }
            }

            if (token != null)
            {
                this.tokenIndex++;
            }
            return token;
        }

        #region Private Methods

        private string ReadKeyWord()
        {
            var sb = new StringBuilder();

            while (this.index < this.length)
            {
                var c = this.text[this.index];
                if (char.IsWhiteSpace(c) || c == ',' || c == '(' || c == ')' || c == '=' || c == '+')
                {
                    break;
                }
                else
                {
                    this.index++;
                }
                sb.Append(c);
            }

            var keyWord = sb.ToString();
            return keyWord;
        }

        private string ReadString()
        {
            var sb = new StringBuilder();
            this.index++;
            var escape = false;

            while (this.index < this.length)
            {
                var c = this.text[this.index];
                this.index++;

                if (escape)
                {
                    if (c == 'n')
                    {
                        c = '\n';
                    }
                    else if (c == 'r')
                    {
                        c = '\r';
                    }
                    else if (c == 't')
                    {
                        c = '\t';
                    }

                    sb.Append(c);

                    escape = false;
                }
                else if (c == '"' || c == '\'')
                {
                    break;
                }
                else if (c == '\\')
                {
                    escape = true;
                }
                else
                {
                    sb.Append(c);
                }
            }

            return sb.ToString();
        }

        private string ReadDigit()
        {
            var sb = new StringBuilder();

            while (this.index < this.length)
            {
                var c = this.text[this.index];
                if (char.IsWhiteSpace(c) || c == ',')
                {
                    break;
                }
                else
                {
                    this.index++;
                }
                sb.Append(c);
            }

            return sb.ToString();
        }

        #endregion
    }
}