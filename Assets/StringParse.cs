namespace Assets
{
    public static class StringParse
    {
        static string FirstLine(string passS)
        {
            return passS.Substring(0, passS.IndexOf("\r\n"));
        }

        public static string ChopLine(string passS)
        {
            return passS == "" ? "" : passS.Substring(passS.IndexOf(";") + 3);
        }

        public static string TextAfterChar(string passLine, string mark)
        {
            return passLine.Substring(passLine.IndexOf(mark) + 1, passLine.Length - passLine.IndexOf(mark) - 1);
        }

        public static string TextBeforeChar(string passLine, string mark)
        {
            return passLine.Substring(0, passLine.IndexOf(mark));
        }

        public static string Block(string passS, string start, string upTo)
        {
            return passS.IndexOf(start) < 0 
                ? "" 
                : passS.Substring(passS.IndexOf(start), passS.IndexOf(upTo) - passS.IndexOf(start));
        }

        public static string ChopBlock(string passS, string upTo)
        {
            return passS.Substring(passS.IndexOf(upTo) + upTo.Length, passS.Length - passS.IndexOf(upTo) - upTo.Length);
        }

        public static string GetLine(string passS, string tag)
        {
            string linePlusRest = passS.Substring(passS.IndexOf(tag), passS.Length - passS.IndexOf(tag));
            return FirstLine(linePlusRest);
        }

        public static int[] DateFromString(string passS)
        {
            int[] ret = new int[5];
            ret[0] = int.Parse(passS.Substring(0, 4)); // YYYY;
            ret[1] = int.Parse(passS.Substring(4, 2)); // MM;
            ret[2] = int.Parse(passS.Substring(6, 2)); // DD;
            ret[3] = int.Parse(passS.Substring(9, 2)); // HH;
            ret[4] = int.Parse(passS.Substring(11, 2)); // MM;

            return ret;
        }

        public static int[] ShortDateFromString(string passS)
        {
            int[] ret = new int[3];
            ret[0] = int.Parse(passS.Substring(0, 4)); // YYYY;
            ret[1] = int.Parse(passS.Substring(4, 2)); // MM;
            ret[2] = int.Parse(passS.Substring(6, 2)); // DD;

            return ret;
        }
    }
}