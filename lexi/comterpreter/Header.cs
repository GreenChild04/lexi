using lexi;

namespace comterpreter
{
    public class Header {
        public Dictionary<string, string> fdata;
        public dynamic node;

        public Header(dynamic node) {
            this.node = node;
            this.fdata = Position.fdata;
        }

        public static uint address = 2000;

        public Header(SeraLib.SeraData data) {
            this.fdata = data.data[0].toDict();
            this.node = data.data[1];
            Position.fdata = this.fdata;
        }

        public SeraLib.SeraBall seralib() {
            return new SeraLib.SeraBall(Header.address) {
                new SeraDict(this.fdata),
                this.node,
            };
        }
    }

    public class SeraDict {
        public List<dynamic> keys = new List<dynamic>();
        public List<dynamic> values = new List<dynamic>();

        public SeraDict(Dictionary<string, string> dict) {
            foreach (KeyValuePair<string, string> i in dict) {
                this.keys.Add(i.Key);
                this.values.Add(i.Value);
            }
        }

        public Dictionary<string, string> toDict() {
            Dictionary<string, string> result = new Dictionary<string, string>();
            for (int i = 0; i < keys.Count; i++) result[this.keys[i]] = this.values[i];
            return result;
        }

        public static uint address = 2001;

        public SeraDict(SeraLib.SeraData data) {
            this.keys = data.data[0];
            this.values = data.data[1];
        }

        public SeraLib.SeraBall seralib() {
            return new SeraLib.SeraBall(SeraDict.address) {
                keys,
                values,
            };
        }
    }
}