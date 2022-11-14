using System.Collections;

namespace lexi
{
    public class RTResult {
        public dynamic value;
        public Error error;
        public dynamic returnVal;

        public RTResult() {
            this.reset();
        }

        public void reset() {
            this.value = null;
            this.error = null;
            this.returnVal = null;
        }

        public object register(RTResult res) {
            if (res.error is not null) return res;
            this.returnVal = res.returnVal;
            return res.value;
        }

        public RTResult success(dynamic value) {
            this.reset();
            this.value = value;
            return this;
        }

        public RTResult successReturn(dynamic value) {
            this.reset();
            this.returnVal = value;
            return this;
        }

        public RTResult failure(Error error) {
            this.reset();
            this.error = error;
            return this;
        }
    }
}