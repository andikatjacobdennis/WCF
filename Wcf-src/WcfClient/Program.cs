using System;
using System.ServiceModel;
using System.Text;

namespace WcfClient
{
    [ServiceContract]
    public interface ITestService
    {
        [OperationContract]
        [FaultContract(typeof(string))]
        string Echo(string text);
    }

    class Program
    {
        static void Main(string[] args)
        {
            // Force console to UTF-8
            Console.OutputEncoding = Encoding.UTF8;

            WSHttpBinding binding = new WSHttpBinding(SecurityMode.None)
            {
                MaxReceivedMessageSize = int.MaxValue,
                MaxBufferPoolSize = int.MaxValue,
                ReaderQuotas = new System.Xml.XmlDictionaryReaderQuotas
                {
                    MaxArrayLength = int.MaxValue,
                    MaxBytesPerRead = int.MaxValue,
                    MaxDepth = int.MaxValue,
                    MaxNameTableCharCount = int.MaxValue,
                    MaxStringContentLength = int.MaxValue
                }
            };

            EndpointAddress address = new EndpointAddress("http://localhost:8080/TestService");

            ChannelFactory<ITestService> factory = null;
            ITestService proxy = null;

            try
            {
                factory = new ChannelFactory<ITestService>(binding, address);
                proxy = factory.CreateChannel();

                Console.WriteLine("Enter text to send to service (or empty to trigger error):");
                string input = Console.ReadLine();

                string response = proxy.Echo(input);
                Console.WriteLine("✅ Response from server: " + response);

                ((IClientChannel)proxy).Close();
                factory.Close();
            }
            catch (FaultException<string> faultEx)
            {
                Console.WriteLine("⚠️ Service reported error: " + faultEx.Detail);
                SafeClose((IClientChannel)proxy, factory);
            }
            catch (CommunicationException commEx)
            {
                Console.WriteLine("❌ Communication error: " + commEx.Message);
                SafeClose((IClientChannel)proxy, factory);
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Unexpected error: " + ex.Message);
                SafeClose((IClientChannel)proxy, factory);
            }

            Console.WriteLine("Press ENTER to exit.");
            Console.ReadLine();
        }

        private static void SafeClose(IClientChannel proxy, ChannelFactory factory)
        {
            try
            {
                proxy?.Abort();
                factory?.Abort();
            }
            catch { /* swallow exceptions during cleanup */ }
        }
    }
}
