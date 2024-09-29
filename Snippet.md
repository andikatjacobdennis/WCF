### Comprehensive WCF Tutorial

This tutorial will guide you through creating a **Windows Communication Foundation (WCF)** service that includes:

1. A basic WCF service.
2. Exception handling using `FaultContract`.
3. Two-way (duplex) communication using callbacks.

We will cover creating a service, configuring it, hosting it, and consuming it in a client application. This tutorial assumes you're using **Visual Studio** and **.NET Framework**.

---

### **Step 1: Create a WCF Service Application**

1. Open **Visual Studio**.
2. Create a new **WCF Service Application**:
   - Go to **File → New → Project**.
   - Select **WCF Service Application**.
   - Name the project `WcfServiceApp` and click **OK**.

When the project is created, you'll see some default files including `IService1.cs` and `Service1.svc`. We will modify these to create our own service.

---

### **Step 2: Define the Service Contract**

A **service contract** defines the operations that the service will expose to the client.

1. Open the `IService1.cs` file and modify it to define an interface for a simple **calculator service** with `Add` and `Subtract` operations. Also, we will include **exception handling** using `FaultContract`.

```csharp
using System.ServiceModel;

[ServiceContract]
public interface ICalculatorService
{
    [OperationContract]
    [FaultContract(typeof(CalculationFault))]
    int Add(int a, int b);

    [OperationContract]
    [FaultContract(typeof(CalculationFault))]
    int Subtract(int a, int b);
}
```

2. Define a **fault contract** class that will be used to pass error details to the client in case of an exception.

```csharp
using System.Runtime.Serialization;

[DataContract]
public class CalculationFault
{
    [DataMember]
    public string Error { get; set; }

    [DataMember]
    public string Details { get; set; }
}
```

---

### **Step 3: Implement the Service**

Next, implement the `ICalculatorService` interface in `Service1.svc.cs` (or create a new class).

1. Open the `Service1.svc.cs` file and modify it as follows:

```csharp
public class CalculatorService : ICalculatorService
{
    public int Add(int a, int b)
    {
        try
        {
            if (a < 0 || b < 0)
            {
                throw new ArgumentException("Negative numbers are not allowed.");
            }
            return a + b;
        }
        catch (Exception ex)
        {
            CalculationFault fault = new CalculationFault
            {
                Error = "Invalid Operation",
                Details = ex.Message
            };
            throw new FaultException<CalculationFault>(fault);
        }
    }

    public int Subtract(int a, int b)
    {
        return a - b;
    }
}
```

In this code:
- We're throwing a `FaultException<CalculationFault>` in case of an invalid operation.
- If any of the numbers are negative, an exception is thrown and handled using a fault contract.

---

### **Step 4: Configure the Service**

We need to configure the WCF service in `Web.config` so it can be hosted. WCF services use bindings to communicate with clients, and the configuration defines how the service behaves.

1. Open the `Web.config` file and update the `system.serviceModel` section as follows:

```xml
<system.serviceModel>
  <services>
    <service name="WcfServiceApp.CalculatorService">
      <endpoint address="" binding="basicHttpBinding" contract="WcfServiceApp.ICalculatorService" />
      <endpoint address="mex" binding="mexHttpBinding" contract="IMetadataExchange" />
      <host>
        <baseAddresses>
          <add baseAddress="http://localhost:8080/CalculatorService"/>
        </baseAddresses>
      </host>
    </service>
  </services>
  <behaviors>
    <serviceBehaviors>
      <behavior>
        <serviceMetadata httpGetEnabled="true" />
        <serviceDebug includeExceptionDetailInFaults="false" />
      </behavior>
    </serviceBehaviors>
  </behaviors>
</system.serviceModel>
```

This configuration:
- Defines an HTTP endpoint using `basicHttpBinding`.
- Enables metadata exchange (MEX) so the client can discover the service.
- Sets the base address for the service at `http://localhost:8080/CalculatorService`.

---

### **Step 5: Implement Two-Way (Duplex) Communication**

Next, we'll implement **duplex communication**. In this model, the client can send requests to the service, and the service can call back to the client.

1. **Define a Callback Contract**: A callback contract allows the service to notify the client about results or events.

```csharp
[ServiceContract]
public interface ICalculatorCallback
{
    [OperationContract(IsOneWay = true)]
    void ResultReady(int result);
}
```

2. **Modify the Service Contract**: Update the `ICalculatorService` interface to support duplex communication by using the `CallbackContract` attribute.

```csharp
[ServiceContract(CallbackContract = typeof(ICalculatorCallback))]
public interface ICalculatorService
{
    [OperationContract]
    void Add(int a, int b);
}
```

3. **Implement the Callback in the Service**: In the service implementation, we will use `OperationContext` to send data back to the client through the callback channel.

```csharp
public class CalculatorService : ICalculatorService
{
    public void Add(int a, int b)
    {
        int result = a + b;
        ICalculatorCallback callback = OperationContext.Current.GetCallbackChannel<ICalculatorCallback>();
        callback.ResultReady(result);
    }
}
```

---

### **Step 6: Hosting the WCF Service**

The WCF service can be hosted using:
- **IIS** (included in the project by default).
- **Self-hosting** using a console application.

For self-hosting, create a new **Console Application** and use the following code:

```csharp
static void Main(string[] args)
{
    using (ServiceHost host = new ServiceHost(typeof(CalculatorService), new Uri("http://localhost:8080/CalculatorService")))
    {
        host.Open();
        Console.WriteLine("Service is running at http://localhost:8080/CalculatorService");
        Console.ReadLine();
    }
}
```

---

### **Step 7: Create a Client for Duplex Communication**

The client needs to support duplex communication to receive callbacks from the service.

1. **Implement the Callback Interface**: The client must implement the callback interface to handle responses from the service.

```csharp
public class CalculatorCallback : ICalculatorCallback
{
    public void ResultReady(int result)
    {
        Console.WriteLine("Result received from service: " + result);
    }
}
```

2. **Create a Duplex Channel**: Use the `InstanceContext` class to create a duplex channel between the client and the service.

```csharp
class Program
{
    static void Main(string[] args)
    {
        InstanceContext instanceContext = new InstanceContext(new CalculatorCallback());
        CalculatorServiceClient client = new CalculatorServiceClient(instanceContext);

        client.Add(5, 10);
        Console.WriteLine("Waiting for result...");
        Console.ReadLine();

        client.Close();
    }
}
```

---

### **Step 8: Running the Service and Client**

1. **Run the Service**: If using self-hosting, run the console application to start the service. If using IIS, just start debugging the service project.
2. **Run the Client**: Start the client application, which will communicate with the WCF service and receive callbacks.

---

### **Summary**

1. **Define a service contract** (`ICalculatorService`) with methods and a callback contract for duplex communication.
2. **Implement the service** with error handling using `FaultContract` and support for two-way communication using a callback.
3. **Configure the service** in `Web.config` to use HTTP binding, and enable metadata exchange.
4. **Host the service** using either IIS or a self-hosted console application.
5. **Create a duplex client** that can send requests to the service and receive callbacks.

With this tutorial, you've built a fully functional WCF service that includes basic operations, exception handling with `FaultContract`, and two-way communication through duplex channels. Let me know if you'd like to add more advanced topics!
