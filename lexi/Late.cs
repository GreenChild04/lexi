using lexi;

namespace late
{
    ///////////////////////
    // Origin
    ///////////////////////

    public class Late {
        ///////////////////////
        // Values
        ///////////////////////

        // Child Mod
        public Position posStart;
        public Position posEnd;
        public Context context;

        // Instance Mod
        public string name;
        public dynamic value;
        public List<string> argNames;
        
        ///////////////////////
        // Constant Methods
        ///////////////////////

        public Late() {
            this.setPos();
            this.setContext();
        }

        public Late newInstance(string name, dynamic value, List<string> argNames) {
            this.name = name is null ? "<anonymous>": name;
            this.value = value;
            this.argNames = argNames;

            return this;
        }

        public Late setPos(Position posStart=null, Position posEnd=null) {
            this.posStart = posStart;
            this.posEnd = posEnd;
            return this;
        }

        public Late setContext(Context context=null) {
            this.context = context;
            return this;
        }

        public Error illegalOperation(dynamic other = null) {
            if (other is null) other = this;
            return new Error(
                this.posStart, other.posEnd,
                Error.IllegalOpError,
                ErrorMsg.IOE001
            );
        }

        public string repr() {
            return $"Late[{this.GetType().Name}]";
        }

        ///////////////////////
        // Modifiable Methods
        ///////////////////////

        // BinOp:
            public RTResult BinOp_addedTo(dynamic other) {return new RTResult().failure(this.illegalOperation(other));}
            public RTResult BinOp_subbedBy(dynamic other) {return new RTResult().failure(this.illegalOperation(other));}
            public RTResult BinOp_multedBy(dynamic other) {return new RTResult().failure(this.illegalOperation(other));}
            public RTResult BinOp_divedBy(dynamic other) {return new RTResult().failure(this.illegalOperation(other));}
            public RTResult BinOp_andedBy(dynamic other) {return new RTResult().failure(this.illegalOperation(other));}
            public RTResult BinOp_oredBy(dynamic other) {return new RTResult().failure(this.illegalOperation(other));}
            public RTResult BinOp_powedBy(dynamic other) {return new RTResult().failure(this.illegalOperation(other));}
            public RTResult BinOp_tetedBy(dynamic other) {return new RTResult().failure(this.illegalOperation(other));}
    }

    ///////////////////////
    // Built-in
    ///////////////////////

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
    }

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
                if (other.GetType() == typeof(Number)) {
                    return res.success(new String(mul(this.value, (double) other.value)).setContext(this.context));
                } else return res.failure(this.illegalOperation(other));
            }

    }
}