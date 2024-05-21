namespace UpdateVpnList
{
    internal class Web : IDisposable
    {
        private HttpClient client = new() { Timeout = TimeSpan.FromSeconds(20) };

        private readonly List<string> CheckList = [
                                                    "https://ya.ru",
                                                    "https://mail.ru",
                                                    "https://rambler.ru",
                                                    "https://gismeteo.ru",
                                                    "https://habr.ru",
                                                    "https://google.com",
                                                  ];

        public async Task<bool> CheckUrl(string url)
        {
            try
            {
                var checkingResponse = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);
                return checkingResponse.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }            
        }

        /// <summary>
        /// Checks a list of sites for availability
        /// </summary>
        /// <returns>The percentage of available sites </returns>
        public int CheckInternetConnection() {
            var tasks = CheckList.Select(CheckUrl);

            var results = Task.WhenAll(tasks).Result;

            return (100 * results.Where(x => x).Count()) / results.Length;
        }

        public string LoadUrlAsString(string url, out string notification) {
            string content = String.Empty;
            string error = String.Empty;
            try
            {
                content = client.GetStringAsync(url).Result;
            }
            catch (Exception ex)
            {
                error = ex.Message;
            }
            notification = error;
            return content;
        }

        public void Dispose()
        {
            client.Dispose();
        }
    }
}
