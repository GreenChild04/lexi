using late;
using lexi;

namespace late {
    namespace primative {
    namespace _iteration
{
    public class List: Late {
        public List(List<dynamic> elements) {
            this.newInstance(null, elements, new List<string>());
        }

        public new string repr() {
            string res = $"[{this.value[0].repr()}";
            for (int i = 1; i < this.value.Count; i++) {
                res += ", " + this.value[i].repr();
            } res += "]";
            return res;
        }
    }
}}}