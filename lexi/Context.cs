namespace lexi
{
    public class Context {
        public string displayName;
        public Context parent;
        public Position parentEntryPos;
        public SymbolTable symbolTable = null;

        public Context(string displayName, Context parent=null, Position parentEntryPos=null) {
            this.displayName = displayName;
            this.parent = parent;
            this.parentEntryPos = parentEntryPos;
        }
    }
}