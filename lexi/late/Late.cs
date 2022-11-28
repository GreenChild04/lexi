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
}