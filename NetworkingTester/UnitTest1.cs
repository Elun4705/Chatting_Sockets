using Communications;
using Microsoft.Extensions.Logging.Abstractions;

namespace NetworkingTester
{
    /// <summary>
    /// A test class for our Networking object which contains but a single Testmethod provided to us
    /// by our teacher.  This test method went unused.
    /// </summary>
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            Networking server = new(NullLogger.Instance,
            new_client,
            (s) => {; },
            receive_message, '\n');
            server.WaitForClients(11000, infinite: false);

            void receive_message(Networking server, string message)
            { Assert.AreEqual(message, "hello"); }
            void new_client(Networking client)
            { client.AwaitMessagesAsync(true); }

            Networking client = new(NullLogger.Instance,
            client_on_connect,
            (s) => {; }, (a, b) => {; }, '\n');
            client.Connect("localhost", 11000);

            void client_on_connect(Networking channel) { channel.Send("hello"); }

        }
    }
}