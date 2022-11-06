using Error;

namespace lexi
{
    public class Error {
        // Class
        public static string InvalidCharError = "Illegal Character";
        public static string InvalidSyntaxError = "Invalid Syntax";
        public static string ExpectedCharError = "Expected Character";
        public static string RTError = "Runtine Error";

        // Object
        public Position posStart;
        public Position posEnd;
        public string type;
        public string details;

        public Error(Position posStart, Position posEnd, string type, string details) {
            this.posStart = posStart;
            this.posEnd = posEnd;
            this.type = type;
            this.details = details;
        }

        public string repr() {
            string result = $"Error[ {this.type}: {this.details} ]\n";
            result += $"File[ {posStart.fn} ], Line[ {this.posStart.ln + 1} ]";
            result += $"\n\n{SWA.gen(this.posStart.ftxt, this.posStart, this.posEnd)}";
            return result;
        }
    }
}