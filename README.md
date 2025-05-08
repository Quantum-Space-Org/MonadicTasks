# Quantum.MonadicTasks  
*A clean, expressive monadic toolkit for composing asynchronous tasks in .NET*

**Quantum.MonadicTasks** brings the power of functional composition to `Task<T>` by wrapping it in a monadic interface. With intuitive methods like `.Bind()`, `.Map()`, and `.Fail()`, you can structure complex async flows in a clean, modular, and testable way.

---

## 💡 What’s a Monad?

A **monad** is a design pattern from functional programming used to **wrap a value** and provide **rules for chaining computations** on that value—especially when those computations might include failure, side effects, or asynchronous behavior.

In this library:

- The **monad wraps `Task<T>`**, so you're working with asynchronous operations
- It allows you to **safely and predictably chain async operations**, just like method pipelines in LINQ
- Think of it like a smarter `Task<T>` with better composition support

If you've used LINQ with `IEnumerable<T>`, you’ve used a monad—you just didn’t know it had a name.

---

## 📦 Installation

Install via the .NET CLI:

```bash
dotnet add package Quantum.MonadicTasks
```

Or use NuGet Package Manager in Visual Studio.

---

## ✨ Features

| Method              | Description                                                 |
| ------------------- | ----------------------------------------------------------- |
| `Unit(value)`       | Wraps a value or a task into a monadic task (`Task<T>`)     |
| `Bind`              | Chains async operations (like `SelectMany`)                 |
| `Map`               | Transforms the result without awaiting manually             |
| `Fail`              | Creates a faulted monad with an exception                   |
| `Flatten`           | Converts nested monads into a single monadic task           |
| `Sequence`          | Runs multiple monadic tasks in parallel and gathers results |
| `PerformSideEffect` | Executes side effects without breaking monadic flow         |
| `GetResultAsync`    | Retrieves the result or throws if failed                    |


---


## 🧑‍💻 Example Usage

```csharp
using Quantum.MonadicTasks;

var result = await TaskMonad<int>.Unit(10)
    .Bind(x => TaskMonad<int>.Unit(x + 5))
    .Map(x => x * 2)
    .GetResultAsync();

Console.WriteLine(result); // Outputs: 30

```

## 🧑‍💻 Near-Real Example

### Order Processing Example
This example simulates an order processing scenario where we check if the order is valid, process the payment, and then ship the order, using monads for handling async operations and error handling.

```csharp

using Quantum.MonadicTasks;
using System;

public class Order
{
    public int Id { get; set; }
    public decimal Amount { get; set; }
    public bool IsValid { get; set; }
}

public class OrderProcessor
{
    public TaskMonad<Order> ValidateOrder(Order order)
    {
        if (order.IsValid)
        {
            return TaskMonad<Order>.Unit(order);
        }
        else
        {
            return TaskMonad<Order>.Fail(new InvalidOperationException("Order is not valid."));
        }
    }

    public TaskMonad<Order> ProcessPayment(Order order)
    {
        if (order.Amount > 0)
        {
            Console.WriteLine($"Processing payment of {order.Amount} for Order {order.Id}");
            return TaskMonad<Order>.Unit(order);
        }
        else
        {
            return TaskMonad<Order>.Fail(new InvalidOperationException("Payment failed due to invalid amount."));
        }
    }
  
    public TaskMonad<string> ShipOrder(Order order)
    {
        return TaskMonad<string>.Unit($"Order {order.Id} shipped successfully.");
    }
}
  
public async Task RunOrderProcessing()
{
    var order = new Order { Id = 123, Amount = 100.00m, IsValid = true };

    var result = await TaskMonad<Order>.Unit(order)
        .Bind(order => new OrderProcessor().ValidateOrder(order))
        .Bind(order => new OrderProcessor().ProcessPayment(order))
        .Bind(order => new OrderProcessor().ShipOrder(order))
        .GetResultAsync();

    Console.WriteLine(result); // Outputs: Order 123 shipped successfully.
}
```


In this example, the TaskMonad is used to chain various steps of the order processing, making it easy to handle both successful and failed operations in a structured way. If any step fails (e.g., invalid order or payment failure), the flow short-circuits and returns the error.

---

## ❌ Handling Errors

```csharp
var failure = MonadicTask<int>.Fail(new InvalidOperationException("Oops!"));

try
{
    var result = await failure.GetResultAsync();
}
catch (Exception ex)
{
    Console.WriteLine(ex.Message); // Outputs: Oops!
}
```

---

## 🔄 Run in Parallel

```csharp
var tasks = new[]
{
    MonadicTask<int>.Return(1),
    MonadicTask<int>.Return(2),
    MonadicTask<int>.Return(3)
};

var sequence = await MonadicTask<int>.Sequence(tasks);
var results = await sequence.GetResultAsync();

Console.WriteLine(string.Join(", ", results)); // Outputs: 1, 2, 3
```

---

## 🎭 Maybe Monad Interop
```csharp
var maybe = MaybeMonad<int>.Some(42);
var taskMonad = maybe.ToTaskMonad();

var result = await taskMonad.GetResultAsync();
Console.WriteLine(result); // Outputs: 42
```
---
## 🎯 Side Effects
Synchronous:

```csharp
await TaskMonad<int>.Unit(5)
    .PerformSideEffect(value => Console.WriteLine($"Value: {value}"))
    .GetResultAsync();
```


Asynchronous:
```csharp
await TaskMonad<int>.Unit(5)
    .PerformSideEffect(async value =>
    {
        await Task.Delay(100);
        Console.WriteLine($"Value: {value}");
    })
    .GetResultAsync();
```

---
## 🧪 Unit Tests

Tests are available under `/tests` and cover:
- Composing operations with `Bind`
- Transforming values with `Map`
- Failure handling
- Task flattening
- Sequencing async operations

Run with:

```bash
dotnet test
```

---

## 🤝 Contributing

Pull requests, suggestions, and issues are welcome!

1. Fork the repo
2. Create a new branch: `feature/your-feature-name`
3. Commit and test your changes
4. Submit a pull request

Please include tests for any new functionality.

---

## 🧠 About Quantum

**Quantum** is a modular, open-source framework from **Your Organization**, built to enhance productivity and composability in modern .NET applications.

**Quantum.MonadicTasks** is a focused utility library for composing `Task<T>` in a clear, predictable, and functional way—without sacrificing readability.
