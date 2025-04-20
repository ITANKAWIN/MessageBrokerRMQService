# MessageBrokerRMQService

A simple .NET 9-based microservice that demonstrates how to publish messages to RabbitMQ using MassTransit. This service sends messages to two separate queues (`sms-queue` and `noti-queue`) using a shared topic exchange with different routing keys.

---

## 🚀 Tech Stack

- [.NET 9](https://dotnet.microsoft.com/)
- [MassTransit](https://masstransit.io/)
- [RabbitMQ](https://www.rabbitmq.com/)
- ASP.NET Core

---

## 📦 Architecture Overview

This project uses a **Publisher** service that sends messages to a **Topic Exchange** in RabbitMQ. Two queues are bound to the exchange with distinct routing keys:

```
                      +---------------------+
                      | Publisher (.NET 9)  |
                      +---------------------+
                                |
                                v
                     +------------------------+
                     | Exchange: topic type   |
                     | Name: notification-ex  |
                     +------------------------+
                      |                      |
        RoutingKey: notification.sms   RoutingKey: notification.email
                      |                      |
                      v                      v
             +----------------+       +------------------+
             |   sms-queue    |       |   noti-queue     |
             +----------------+       +------------------+
```

---

## 🛠️ Getting Started

### Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download)
- [RabbitMQ Server](https://www.rabbitmq.com/download.html) (Docker or local install)

### RabbitMQ with Docker

```bash
docker run -d --hostname rmq --name rmq-dev -p 5672:5672 -p 15672:15672 rabbitmq:3-management
```

Login to RabbitMQ dashboard: [http://localhost:15672](http://localhost:15672)  
(Default credentials: `guest` / `guest`)

### Running the Project

1. Clone the repository

```bash
git clone https://github.com/tanapoomjaisabay/MessageBrokerRMQService.git
cd MessageBrokerRMQService
```

2. Restore and run the project

```bash
dotnet restore
dotnet run --project Publisher
```

---

## 📤 How It Works

The publisher sends two types of messages:

- **SMS Message** → routed with `notification.sms`
- **Notification Message** → routed with `notification.email`

Each message type is mapped to the same exchange (`notification-exchange`) but with different routing keys.

```csharp
await _publishEndpoint.Publish<SendSms>(new SendSms { ... }, context =>
{
    context.SetRoutingKey("notification.sms");
});
```

```csharp
await _publishEndpoint.Publish<SendNoti>(new SendNoti { ... }, context =>
{
    context.SetRoutingKey("notification.email");
});
```

---

## 🗂️ Project Structure

```
MessageBrokerRMQService/
├── Models/               # Message definitions (SendSms, SendNoti)
├── Services/             # PublisherService to publish messages
├── Program.cs            # MassTransit + RabbitMQ configuration
├── appsettings.json      # RabbitMQ connection options (if used)
```

---

## 📌 Notes

- Uses topic exchange for flexibility and scalability
- RoutingKey-based queue binding
- Easy to extend for additional queues and message types

---

## 👨‍💻 Author

Created by [Tanapoom Jaisabay](https://github.com/tanapoomjaisabay)  
Feel free to contribute or fork!
