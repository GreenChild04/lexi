namespace lexi
{
    public class SymbolTable {
        public SymbolTable parent;
        Dictionary<string, dynamic> symbols;

        public SymbolTable(SymbolTable parent=null) {
            this.parent = parent;
        }

        public dynamic get(string name) {
            bool isfound = this.symbols.ContainsKey(name);
            if (!isfound && this.parent is not null) {
                return parent.get(name);
            }
            return this.symbols[name];
        }

        public void set(string name, dynamic value) {
            this.symbols[name] = value;
        }

        public void remove(string name) {
            this.symbols.Remove(name);
        }
    }
}