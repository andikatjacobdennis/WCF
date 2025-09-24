using System;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Text;

namespace WcfServer
{
    [ServiceContract]
    public interface ITestService
    {
        [OperationContract]
        [FaultContract(typeof(string))] // declare fault type
        string Echo(string text);
    }

    public class TestService : ITestService
    {
        public string Echo(string text)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(text))
                {
                    throw new ArgumentException("Input text cannot be null or empty.");
                }

                return $"Server received: {text}";
            }
            catch (Exception ex)
            {
                // Convert exception into a FaultException so client can handle gracefully
                throw new FaultException<string>($"Service error: {ex.Message}");
            }
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            // Force console to UTF-8
            Console.OutputEncoding = Encoding.UTF8;

            Uri baseAddress = new Uri("http://localhost:8080/TestService");

            using (ServiceHost host = new ServiceHost(typeof(TestService), baseAddress))
            {
                try
                {
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

                    host.AddServiceEndpoint(typeof(ITestService), binding, "");

                    // Enable WSDL publishing
                    ServiceMetadataBehavior smb = new ServiceMetadataBehavior
                    {
                        HttpGetEnabled = true
                    };
                    host.Description.Behaviors.Add(smb);

                    host.Open();

                    Console.WriteLine("✅ WCF Service running at " + baseAddress);
                    Console.WriteLine("Press ENTER to stop.");
                    Console.ReadLine();

                    host.Close();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("❌ Service host error: " + ex.Message);
                    if (host.State == CommunicationState.Faulted)
                    {
                        host.Abort();
                    }
                }
            }
        }
    }
}

