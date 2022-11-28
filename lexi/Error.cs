using error;

namespace lexi
{
    public class Error {
        // Class
        public static string InvalidCharError = "Illegal Character";
        public static string InvalidSyntaxError = "Invalid Syntax";
        public static string ExpectedCharError = "Expected Character";
        public static string RTError = "Runtine Error";
        public static string IllegalOpError = "Illegal Operation Error";

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

    //Holds all of the Error Codes
    public class ErrorMsg {
        // Invalid Syntax Error
        public static string ISE001 = "Expected '+', '-', '*', '/', '^' or '^^'";
        public static string ISE002 = "String wasn't closed";
        public static string ISE003 = "'=' (after '!')";
        public static string ISE004 = "Expected int, float, identifier, string, '[', '{', '+', '-', or '('";
        public static string ISE005 = "Expected 'col' keyword to start collection";
        public static string ISE006 = "Expected ')', int, float, identifier, string, '[', '{', '+', '-', or '('";
        public static string ISE007 = ISE006.Replace(")", "]");
        public static string ISE008 = ISE006.Replace(")", "}");
        public static string ISE009 = "Expected tuple, list or curl";
        public static string ISE010 = "Expected 'live' keyword";
        public static string ISE011 = "Expected 'late' keyword";
        public static string ISE012 = "Expected identifier after comma";
        public static string ISE013 = "Expected 'while'";
        public static string ISE014 = "Expected 'if'";
        public static string ISE015 = "Expected 'encap'";
        public static string ISE016 = "Expected identifier";
        public static string ISE017 = "Unexpected keyword used here";
        public static string ISE018 = "Expected boolean operation";

        // Expected Character Error
        public static string ICE001 = "Expected ':'";
        public static string ICE002 = "Expected '{'";
        public static string ICE003 = "Expected '}'";
        public static string ICE004 = "Expected '('";
        public static string ICE005 = "Expected ',' or ')'";
        public static string ICE006 = "Expected ',' or ']'";
        public static string ICE007 = "Expected ';' or '}'";
        public static string ICE008 = "Expected '['";
        public static string ICE009 = "Expected ':', ';' or '='";
        public static string ICE010 = "Expected '='";
        public static string ICE011 = "Expected identifier or ')'";
        
        // Illegal Operation Error
        public static string IOE001 = "Cannot run this operation between these objects";
    }
}