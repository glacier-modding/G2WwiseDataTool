using System.Diagnostics;

namespace G2WwiseDataTool
{
    public class BufferedTraceListener : TraceListener
    {
        private List<string> buffer = new List<string>();

        public override void Write(string message)
        {
            buffer.Add(message);
        }

        public override void WriteLine(string message)
        {
            buffer.Add(message);
        }

        public void WriteBufferedMessages()
        {
            foreach (var message in buffer)
            {
                Console.WriteLine(message);
            }
        }
    }
}
