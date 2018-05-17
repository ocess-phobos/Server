using System.Threading;
using Phobos.Server.Sockets;

namespace Phobos.Server
{
    public static class Program
    {
        public static void Main()
        {
            Thread thread = new Thread(() => new MainServer());
            thread.Start();

            AsynchronousSocketListener.StartListening();
        }
    }
}
