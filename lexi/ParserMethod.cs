using lexi;
using System.Collections;

namespace UParser
{
    // Parser Methods
    public class ParserMethod {
        // Unique for ParserMethod
        public List<List<Token>> req = new List<List<Token>> {
            new List<Token>() {new Token(Token.BLNK)}
        };

        public bool match(List<Token> sample) {
            bool result = true;
            int i = 0;
            foreach (Token tok in sample) {
                i++;
                if (!result) break;
                bool output = false;
                for (int a = 0; a < req[i].Count - 1; a++) {
                    bool res = req[i][a].matches(tok.type, tok.value);
                    if (res) output = res;
                }
                result = output;
            }
            return result;
        }

        public object findMatch(List<Token> tokens, ArrayList rep) {
            List<Token> cache = new List<Token>();
            object method = null;

            for (int i = 0; method is null; i++) {
                int founds = 0;
                ArrayList found = new ArrayList() {};

                cache.Add(tokens[i]);
                foreach (dynamic m in rep) {
                    if (m.match(cache)) {
                        founds++;
                        found.Add(m);
                    }
                }
                if (founds < 2) method = found[0];
            }
            return method;
        }

        public object tokMatch(Parser parse, ArrayList rep) {
            List<Token> cache = new List<Token>();
            int oldTokIdx = parse.tokIdx;
            Token oldTok = parse.currentTok;
            object method = null;

            for (int i = 0; method is null; i++) {
                int founds = 0;
                ArrayList found = new ArrayList() {};

                parse.advance();
                cache.Add(parse.currentTok);
                foreach (dynamic m in rep) {
                    if (m.match(cache)) {
                        founds++;
                        found.Add(m);
                    }
                }
                if (founds < 2) method = found[0];
            }

            parse.tokIdx = oldTokIdx;
            parse.currentTok = oldTok;

            return method;
        }

        // For Children
        public ParseResult parse(Parser self) {
            return null;
        }
    }

    // Children
    public class stmt: ParserMethod {
        public new ParseResult parse(Parser self) {
            return null;
        }
    }

    public class atom: ParserMethod {
        public new List<List<Token>> req = new List<List<Token>> {
            new List<Token>() {new Token(Token.INT), new Token(Token.FLOAT), new Token(Token.IDENTIFIER)},
        };

        public new ParseResult parse(Parser self) {
            ParseResult res = new ParseResult();
            Token tok = self.currentTok;

            if (tok.type == Token.INT || tok.type == Token.FLOAT) {
                res.registerAdvance(self.advance());
                return res.success(new NumberNode(tok, tok.posStart, tok.posEnd));
            } else if (tok.type == Token.IDENTIFIER) {
                res.registerAdvance(self.advance());
            } else {
            }

            return res.failure(new lexi.Error(
                tok.posStart, tok.posEnd,
                lexi.Error.InvalidSyntaxError,
                "Expected int, float, identifier, string, var, while, if, late, '[', '+', '-', or '('"
            ));
        }
    }
}