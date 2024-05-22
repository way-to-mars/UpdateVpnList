namespace UpdateVpnList
{
    internal class Logger
    {
        public static void Log(string msg)
        {
        #if DEBUG
            string time = DateTime.Now.ToString("hh:mm:ss.ffffff");
            string threadName = $"id:{Environment.CurrentManagedThreadId}-'{Thread.CurrentThread.Name ?? ""}'";

            Console.WriteLine($"{time} [{threadName}] {msg}");
        #else
            Console.WriteLine(msg);
        #endif
        }

    }
}
