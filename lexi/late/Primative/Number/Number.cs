using late;
using lexi;

namespace late {
    namespace primative {
    namespace _number
{
    public class Number: Late {
        public Number(float value) {
            this.newInstance(null, value, new List<string>());
        } public Number(double value) {
            this.newInstance(null, value, new List<string>());
        }

        // BinOp:
            public RTResult BinOp(dynamic other, Delegate operation) {
                RTResult res = new RTResult();
                if (other.GetType() == typeof(Number)) {
                    return res.success(new Number(operation.DynamicInvoke(this.value, other.value)).setContext(this.context));
                } else return res.failure(this.illegalOperation(other));
            }

            public new RTResult BinOp_addedTo(dynamic other) {Delegate l = (double x, double y) => x + y; return this.BinOp(other, l);}
            public new RTResult BinOp_subbedBy(dynamic other) {Delegate l = (double x, double y) => x - y; return this.BinOp(other, l);}
            public new RTResult BinOp_multedBy(dynamic other) {Delegate l = (double x, double y) => x * y; return this.BinOp(other, l);}
            public new RTResult BinOp_divedBy(dynamic other) {Delegate l = (double x, double y) => x / y; return this.BinOp(other, l);}
            public new RTResult BinOp_powedBy(dynamic other) {Delegate l = (double x, double y) => Math.Pow(x, y); return this.BinOp(other, l);}
            public new RTResult BinOp_tetedBy(dynamic other) {
                double tet(double x, double y) {
                    if (y == 0) return 1;
                    return Math.Pow(x, tet(x, y-1));
                }
                Delegate l = (double x, double y) => tet(x, y);
                return this.BinOp(other, l);
            }

        public new string repr() {
            return $"{this.value}";
        }
    }
}}}