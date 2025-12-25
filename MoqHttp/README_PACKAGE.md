# MoqHttp

**MoqHttp** is a lightweight, fluent HTTP mocking and VCR library for **.NET 10+**. It allows you to easily spin up a local HTTP server to mock API responses or record real API interactions for consistent, fast, and offline-capable testing.

---

## 🚀 Quick Start (Traditional Mocking)

Mock any HTTP endpoint with a few lines of code:

```csharp
using MoqHttp;

// 1. Create and start the server
using var mockServer = new HttpServer(5000);
mockServer.Run();

// 2. Configure an endpoint
mockServer.Config.Get("/api/data").Send("{\"status\": \"ok\"}", 200);

// 3. Your code makes a request to http://localhost:5000/api/data
var httpClient = new HttpClient();
var response = await httpClient.GetAsync("http://localhost:5000/api/data");
var contents = await response.Content.ReadAsStringAsync(); // "{\"status\": \"ok\"}"
```

---

## 📼 VCR Functionality (Record & Replay)

Eliminate network flakiness by recording real API responses once and replaying them in your tests.

### Auto Mode (Recommended)
Automatically records if the cassette file is missing, otherwise replays from disk.

```csharp
mockServer.Config
    .Auto()
    .FromFile("fixtures/github_user.json")
    .ProxyTo("https://api.github.com");

mockServer.Run();
// First run: Captures real data from GitHub
// Subsequent runs: Instant playback from local file!
```

### Advanced VCR Features
- **Filtering**: Record only specific paths or HTTP verbs.
- **Transformation**: Modify recorded responses on the fly (e.g., inject current timestamps).
- **Advanced Matching**: Match by query strings, headers, or request bodies.

---

## ✨ Key Features
- **Zero Configuration**: Starts in-process with minimal boilerplate.
- **Fluent API**: Intuitive, chainable syntax for configuring mocks.
- **Standard-Compliant**: Built on top of ASP.NET Core Kestrel.
- **No Dependencies**: Pure .NET library using `System.Text.Json`.
- **Fast**: VCR playback is 100x faster than real network calls.

---

## 📦 Installation

```bash
dotnet add package MoqHttp
```

For more documentation and examples, visit the [GitHub Repository](https://github.com/KamRon-67/MoqHttp).
