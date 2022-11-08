namespace lexi
{
    public class NumberNode {
        public Token tok;
        public Position posStart;
        public Position posEnd;

        public NumberNode(Token tok, Position posStart, Position posEnd) {
            this.tok = tok;
            this.posStart = posStart;
            this.posEnd = posEnd;
        }

        public string repr() {
            return $"{this.tok.repr()}";
        }
    }
}