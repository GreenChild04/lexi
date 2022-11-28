using System.Collections;
using lexi;

namespace dom
{
    public class Dom {
        public static char key = ' ';
        public Position posStart;
        public Position posEnd;

        public Dom newInstance(Position posStart, Position posEnd) {
            this.posStart = posStart;
            this.posEnd = posEnd;
            return this;
        }

        public string wrap(char wrapper, string value) {
            return wrapper + value + wrapper;
        }
    }

    public class Number: Dom {
        public new static char key = '$';
        public Token value;

        public Number(Token value, Position posStart, Position posEnd) {
            this.value = value;
            this.newInstance(posStart, posEnd);
        }

        public string write() {
            return wrap(Number.key, value.repr());
        }
    }

    public class String: Dom {
        public new static char key = '$';
        public Token value;
        
        public String(Token value, Position posStart, Position posEnd) {
            this.value = value;
            this.newInstance(posStart, posEnd);
        }

        public string write() {
            return wrap(String.key, value.type + ":" + wrap('\'', value.value));
        }
    }

    public class BinOp: Dom {
        public new static char key = '#';
        public dynamic left;
        public dynamic right;
        public Token opTok;

        public BinOp(dynamic left, dynamic right, Token opTok) {
            this.left = left;
            this.right = right;
            this.opTok = opTok;
            this.newInstance(posStart, posEnd);
        }

        public string write() {
            string contents = $"{this.left.write()};{this.right.write()};{this.opTok.repr()}";
            return wrap(BinOp.key, contents);
        }
    }

    public class UnaryOp: Dom {
        public new static char key = '!';
        public dynamic node;
        public Token opTok;

        public UnaryOp(dynamic node, Token opTok, Position posStart, Position posEnd) {
            this.node = node;
            this.opTok = opTok;
            this.newInstance(posStart, posEnd);
        }
        
        public string write() {
            string contents = $"{this.opTok.repr()};{this.node.write()}";
            return wrap(UnaryOp.key, contents);
        }
    }
}