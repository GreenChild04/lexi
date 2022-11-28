namespace lexi
{
    public class SymbolTable {
        public SymbolTable parent;
        Dictionary<string, Symbol> symbols;

        public SymbolTable(SymbolTable parent=null) {
            this.parent = parent;
        }

        public dynamic get(string name) {
            bool isfound = this.symbols.ContainsKey(name);
            if (!isfound && this.parent is not null) {
                return parent.get(name);
            }
            return isfound ? this.symbols[name]: null;
        }

        public object set(string name, dynamic value, bool constant=false) {
            Symbol found = this.get(name);
            if (found is null) return this.symbols[name] = new Symbol(value, constant);
            if (!found.constant) return this.symbols[name] = new Symbol(value, constant);
            return null;
        }

        public void remove(string name) {
            this.symbols.Remove(name);
        }

        public void extend(SymbolTable other) {
            foreach (KeyValuePair<string, Symbol> i in other.symbols)
                if (this.get(i.Key) is null) this.symbols[i.Key] = i.Value;
        }
    }

    public class Symbol {
        public dynamic value;
        public bool constant;

        public Symbol(dynamic value, bool constant=false) {
            this.value = value;
            this.constant = constant;
        }
    }
}