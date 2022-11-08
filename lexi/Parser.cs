using UParser;

namespace lexi
{
    // Main Parser Class
    public class Parser {
        public List<Token> tokens;
        public int tokIdx;
        public Token currentTok;

        public Parser(List<Token> tokens) {
            this.tokens = tokens;
            this.tokIdx = -1;
            this.currentTok = null;
            this.advance();
        }

        public object advance() {
            this.tokIdx++;
            if (this.tokIdx < this.tokens.Count)
                this.currentTok = this.tokens[this.tokIdx];
            return null;
        }

        public ParseResult parse() {
            ParseResult res = new atom().parse(this);
            if (res.error is null && this.currentTok.type != Token.EOF) {
                return res.failure(new Error(
                    this.currentTok.posStart, this.currentTok.posEnd,
                    Error.InvalidSyntaxError,
                    "Expected '+', '-', '*', '/', '^' or '^^'"
                ));
            }
            return res;
        }

        // Parser Methods
    }
}