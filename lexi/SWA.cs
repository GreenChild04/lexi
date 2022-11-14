using lexi;

namespace error
{
    public class SWA {
        public static string gen(string text, Position posStart, Position posEnd) {
            string result = "";

            // Calculate indices
            int idxStart = max(new List<int> {rfind(text, '\n', 0, posStart.idx), 0});
            int idxEnd = find(text, '\n', idxStart + 1);
            if (idxEnd < 0) idxEnd = text.Length;

            // Generate each line
            int lineCount = posEnd.ln - posStart.ln + 1;
            for (int i = 0; i < lineCount; i++) {
                // Calculate the line columns
                string line = text[idxStart..idxEnd];
                int colStart = i == 0 ? posStart.col: 0;
                int colEnd = i == lineCount - 1 ? posEnd.col: line.Length - 1;

                // Append to result
                result += line + "\n";
                result += mul(" ", colStart) + mul("^", colEnd - colStart);

                // Recalculate indices
                idxStart = idxEnd;
                idxEnd = find(text, '\n', idxStart + 1);
                if (idxEnd < 0) idxEnd = text.Length;
            }

            return result.Replace("\t", "");
        }

        static int max(List<int> list) {
            int largest = 0;

            foreach (int i in list) {
                if (i > largest) largest = i;
            }

            return largest;
        }

        static int rfind(string str, char c, int start, int end) {
            int found = -1;

            for (int i = start; i <= end - 1; i++) {
                if (str[i] == c)
                    if (i > found) found = i;
            }

            return found;
        }

        static int find(string str, char c, int start) {
            for (int i = start; i < str.Length - 1; i++) {
                if (str[i] == c) return i;
            }
            return -1;
        }

        static string mul(string str, int amt) {
            string result = "";

            for (int i = 0; i < amt; i++) {
                result += str;
            }

            return result;
        }
    }
}