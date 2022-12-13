using late;
using lexi;

namespace late {
    namespace primative {
    namespace _iteration
{
    public class Curl: Late {
        public Curl(List<dynamic> elements) {
            this.newInstance(null, elements, new List<string>());
        }

        public new string repr() {
            return "<curl container>";
        }
    }
}}}