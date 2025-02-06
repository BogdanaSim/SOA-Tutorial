# **Integrating RabbitMQ with MassTransit in .NET 8 Microservices**  

## **Introduction**  

RabbitMQ is a message broker that allows **asynchronous communication** between microservices. Instead of manually managing RabbitMQ connections and consumers, we will use **MassTransit**, a popular .NET library that simplifies RabbitMQ integration.  

### **What We’ll Build**  

We’ll create two microservices inside a **.NET 8 Solution (`MicroservicesSolution.sln`)**:  

1. **OrderService (Producer)** → Publishes an order message.  
2. **PaymentService (Consumer)** → Listens for order messages and processes payments.  

This setup simulates a **real-world e-commerce workflow**, where an order is placed, and payment is processed asynchronously.  
---

## **Step 1: Install Prerequisites**  

Ensure you have the following installed:  

- **.NET 8 SDK**  
- **RabbitMQ** (Installed locally or via Docker)  
- **Erlang** (Required for RabbitMQ)  
- **Visual Studio / VS Code**  

💡 **Run RabbitMQ with Docker** (Skip installation by using a container):  

```sh
docker run -d --name rabbitmq -p 5672:5672 -p 15672:15672 rabbitmq:3-management
```

RabbitMQ will be available at: **[http://localhost:15672](http://localhost:15672)** (Username/Password: `guest/guest`).  

---

## **Step 2: Create a .NET Solution and Microservices**  

### **1️⃣ Create a .NET Solution**  

```sh
dotnet new sln -n MicroservicesSolution
```

### **2️⃣ Create the OrderService and PaymentService Projects**  

```sh
dotnet new webapi -n OrderService
dotnet new webapi -n PaymentService
```

### **3️⃣ Add the Projects to the Solution**  

```sh
dotnet sln add OrderService/OrderService.csproj
dotnet sln add PaymentService/PaymentService.csproj
```

### **4️⃣ Install MassTransit in Both Projects**  

```sh
dotnet add OrderService/OrderService.csproj package MassTransit
dotnet add OrderService/OrderService.csproj package MassTransit.AspNetCore
dotnet add OrderService/OrderService.csproj package MassTransit.RabbitMQ

dotnet add PaymentService/PaymentService.csproj package MassTransit
dotnet add PaymentService/PaymentService.csproj package MassTransit.AspNetCore
dotnet add PaymentService/PaymentService.csproj package MassTransit.RabbitMQ
```

---

## **Step 3: Implement the Producer (OrderService)**  

The **OrderService** will publish messages to RabbitMQ when an order is placed.  

🔹 **Modify `OrderController.cs` (Located in `OrderService/Controllers/OrderController.cs`)**  

```csharp
using MassTransit;
using Microsoft.AspNetCore.Mvc;

[Route("api/orders")]
[ApiController]
public class OrderController : ControllerBase
{
    private readonly IPublishEndpoint _publishEndpoint;

    public OrderController(IPublishEndpoint publishEndpoint)
    {
        _publishEndpoint = publishEndpoint;
    }

    [HttpPost]
    public async Task<IActionResult> PlaceOrder([FromBody] Order order)
    {
        await _publishEndpoint.Publish(order);
        return Ok("Order placed successfully");
    }
}

public record Order(int Id, string Product, decimal Price);
```

🔹 **Configure MassTransit in `Program.cs` (Located in `OrderService/Program.cs`)**  

```csharp
using MassTransit;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMassTransit(config =>
{
    config.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host("localhost", "/", h =>
        {
            h.Username("guest");
            h.Password("guest");
        });
    });
});

builder.Services.AddControllers();
var app = builder.Build();
app.MapControllers();
app.Run();
```

---

## **Step 4: Implement the Consumer (PaymentService)**  

The **PaymentService** will listen for messages from the `orderQueue` and process payments.  

🔹 **Create `OrderConsumer.cs` (Located in `PaymentService/Consumers/OrderConsumer.cs`)**  

```csharp
using MassTransit;

public class OrderConsumer : IConsumer<Order>
{
    public async Task Consume(ConsumeContext<Order> context)
    {
        var order = context.Message;
        Console.WriteLine($"[x] Processing payment for Order ID: {order.Id}, Product: {order.Product}");
        await Task.CompletedTask;
    }
}

public record Order(int Id, string Product, decimal Price);
```

🔹 **Register MassTransit and the Consumer in `Program.cs` (Located in `PaymentService/Program.cs`)**  

```csharp
using MassTransit;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMassTransit(config =>
{
    config.AddConsumer<OrderConsumer>();

    config.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host("localhost", "/", h =>
        {
            h.Username("guest");
            h.Password("guest");
        });

        cfg.ReceiveEndpoint("orderQueue", ep =>
        {
            ep.ConfigureConsumer<OrderConsumer>(context);
        });
    });
});

var app = builder.Build();
app.Run();
```

---

## **Step 5: Running the Microservices**  

1. **Start the PaymentService (Consumer) First:**  

```sh
dotnet run --project PaymentService
```

2. **Start the OrderService (Producer):**  

```sh
dotnet run --project OrderService
```

3. **Place an Order (via Postman or cURL):**  

```sh
curl -X POST "http://localhost:5138/api/orders" -H "Content-Type: application/json" -d '{"id":1,"product":"Laptop","price":999.99}'
```

🎉 **Expected Output in PaymentService Console:**  

```
[x] Processing payment for Order ID: 1, Product: Laptop
```

---

## **Project Structure**  

```
MicroservicesSolution/
│-- MicroservicesSolution.sln
│
├── OrderService/
│   ├── Controllers/
│   │   ├── OrderController.cs
│   ├── Program.cs
│   ├── OrderService.csproj
│
├── PaymentService/
│   ├── Consumers/
│   │   ├── OrderConsumer.cs
│   ├── Program.cs
│   ├── PaymentService.csproj
```

---

## **Why Use MassTransit with RabbitMQ?**  

✅ **Simplifies RabbitMQ Integration** → No need for manual connection management.  
✅ **Built-in Resilience** → Automatic retries and error handling.  
✅ **Easy Scalability** → Multiple consumers can process messages concurrently.  
✅ **Supports Additional Features** → Event-driven architecture, scheduling, and more.  

---
