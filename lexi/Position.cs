namespace lexi
{
    public class Position {
        // File Data
        public static Dictionary<string, string> fdata = new Dictionary<string, string>();
        public string fn;
        public string ftxt {
            get {return fdata[this.fn];}
            set {fdata[this.fn] = value;}
        }

        // Position
        public int idx;
        public int ln;
        public int col;

        public Position(int idx, int ln, int col, string fn, string ftxt) {
            this.idx = idx;
            this.ln = ln;
            this.col = col;
            this.fn = fn;
            this.ftxt = ftxt;
        }

        public Position advance(string currentChar=null) {
            this.idx += 1;
            this.col += 1;

            if (currentChar == "\n") {
                this.ln += 1;
                this.col = 0;
            }

            return this;
        }

        public Position copy() {
            return new Position(this.idx, this.ln, this.col, this.fn, this.ftxt);
        }

        /////////////////////
        // SeraLib Stuff
        /////////////////////

        public static uint address = 1002;

        public Position (SeraLib.SeraData data) {
            this.idx = int.Parse(data.data[0]);
            this.ln = int.Parse(data.data[1]);
            this.col = int.Parse(data.data[2]);
            this.fn = data.data[3];
        }

        public SeraLib.SeraBall seralib() {
            return new SeraLib.SeraBall(Position.address) {
                this.idx.ToString(),
                this.ln.ToString(),
                this.col.ToString(),
                this.fn,
            };
        }
    }
}