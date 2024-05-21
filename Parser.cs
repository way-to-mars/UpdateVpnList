namespace UpdateVpnList
{
    internal class Parser
    {
        private static readonly string blockHeader = "<div class=\"server\"><p>";
        private static readonly string hrefHeader = "href=\"";

        public static IReadOnlyList<string> ServersList(string data) {
            var resultList = new List<string>();

            var index = 0;

            while (true)
            {
                var blockIndex = data.IndexOf(blockHeader, index);
                if (blockIndex == -1) break;

                index = blockIndex + blockHeader.Length;

                var hrefIndex = data.IndexOf(hrefHeader, index);
                if (hrefIndex == -1) break;

                index = hrefIndex + hrefHeader.Length;
                var secondQuoteIndex = data.IndexOf('\"', index);
                if (secondQuoteIndex == -1) break;

                string url = data.Substring(index, secondQuoteIndex - index);
                resultList.Add(url);
            }

            return resultList as IReadOnlyList<string>;
        }
    }
}
