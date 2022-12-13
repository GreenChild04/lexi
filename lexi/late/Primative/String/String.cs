using late;
using lexi;

namespace late {
    namespace primative {
    namespace _string
{
    public class String: Late {
        public String(string value) {
            this.newInstance(null, value, new List<string>());
        }

        // BinOp:
            public new RTResult BinOp_addedTo(dynamic other) {
                RTResult res = new RTResult();
                if (other.GetType() == typeof(String)) {
                    return res.success(new String(this.value + other.value).setContext(this.context));
                } else return res.failure(this.illegalOperation(other));
            }
            
            public new RTResult BinOp_multedBy(dynamic other) {
                string mul(string x, double y) {
                    string output = ""; 
                    for (int i = 0; i < y; i++) {
                        output += x;
                    } return output;
                }
                RTResult res = new RTResult();
                if (other.GetType() == typeof(late.primative._number.Number)) {
                    return res.success(new String(mul(this.value, (double) other.value)).setContext(this.context));
                } else return res.failure(this.illegalOperation(other));
            }

        public new string repr() {
            return $"\"{this.value}\"";
        }
    }
}}}