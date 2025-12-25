# VCR Examples

This document provides practical examples of using MoqHttp's VCR (Video Cassette Recorder) functionality.

## Table of Contents

- [Quick Start](#quick-start)
- [Recording Mode](#recording-mode)
- [Playback Mode](#playback-mode)
- [Auto Mode](#auto-mode)
- [Real-World Examples](#real-world-examples)

---

## Quick Start

VCR allows you to record real HTTP interactions once and replay them in tests, eliminating network calls and making tests faster and more reliable.

### Basic Recording Example

```csharp
using MoqHttp;
using System.Net.Http;

// Record real API responses
var mockServer = new HttpServer(8080);
mockServer.Config
    .Record()
    .ToFile("fixtures/weather_api.json")
    .ProxyTo("https://api.weather.com");

mockServer.Run();

// Make requests - they hit the real API and are recorded
var httpClient = new HttpClient();
var response = await httpClient.GetAsync("http://localhost:8080/forecast");

// Cassette is saved automatically on disposal
mockServer.Dispose();
```

### Basic Playback Example

```csharp
// Replay recorded responses (no network calls)
var mockServer = new HttpServer(8080);
mockServer.Config
    .Playback()
    .FromFile("fixtures/weather_api.json");

mockServer.Run();

// Same request now uses recorded data
var httpClient = new HttpClient();
var response = await httpClient.GetAsync("http://localhost:8080/forecast");
// Returns the recorded response instantly

mockServer.Dispose();
```

---

## Recording Mode

Recording mode proxies requests to a real API and saves all interactions to a cassette file.

### Example: Recording GitHub API Calls

```csharp
[Fact]
public async Task Record_GitHub_User_Data()
{
    // Arrange
    var mockServer = new HttpServer(5000);
    mockServer.Config
        .Record()
        .ToFile("fixtures/github_user.json")
        .ProxyTo("https://api.github.com");
    
    mockServer.Run();

    // Act
    var httpClient = new HttpClient();
    httpClient.DefaultRequestHeaders.Add("User-Agent", "MoqHttp-Test");
    
    var response = await httpClient.GetAsync("http://localhost:5000/users/octocat");
    var content = await response.Content.ReadAsStringAsync();

    // Assert
    Assert.Equal(200, (int)response.StatusCode);
    Assert.Contains("octocat", content);

    mockServer.Dispose();
    
    // fixtures/github_user.json now contains the recorded interaction
}
```

### Cassette Format

After recording, the cassette file contains JSON like this:

```json
{
  "Version": "1.0",
  "Interactions": [
    {
      "Request": {
        "Method": "GET",
        "Uri": "https://api.github.com/users/octocat",
        "Headers": {
          "User-Agent": "MoqHttp-Test"
        },
        "Body": null,
        "QueryParameters": {}
      },
      "Response": {
        "Status": 200,
        "Headers": {
          "Content-Type": "application/json; charset=utf-8"
        },
        "Body": "{\"login\":\"octocat\",\"id\":583231,...}",
        "RecordedAt": "2025-12-24T20:00:00Z"
      },
      "RecordedAt": "2025-12-24T20:00:00Z"
    }
  ],
  "CreatedAt": "2025-12-24T20:00:00Z",
  "Metadata": {}
}
```

---

## Playback Mode

Playback mode replays recorded interactions without making real network calls.

### Example: Testing with Recorded Data

```csharp
[Fact]
public async Task Test_With_Recorded_GitHub_Data()
{
    // Arrange
    var mockServer = new HttpServer(5000);
    mockServer.Config
        .Playback()
        .FromFile("fixtures/github_user.json");
    
    mockServer.Run();

    // Act - No real API call made
    var httpClient = new HttpClient();
    var response = await httpClient.GetAsync("http://localhost:5000/users/octocat");
    var content = await response.Content.ReadAsStringAsync();

    // Assert
    Assert.Equal(200, (int)response.StatusCode);
    Assert.Contains("octocat", content);

    mockServer.Dispose();
}
```

### Handling Unmatched Requests

If a request doesn't match any recorded interaction, VCR returns a 404:

```csharp
[Fact]
public async Task Unmatched_Request_Returns_404()
{
    var mockServer = new HttpServer(5000);
    mockServer.Config
        .Playback()
        .FromFile("fixtures/github_user.json");
    
    mockServer.Run();

    var httpClient = new HttpClient();
    var response = await httpClient.GetAsync("http://localhost:5000/users/different-user");

    Assert.Equal(404, (int)response.StatusCode);
    
    var content = await response.Content.ReadAsStringAsync();
    Assert.Contains("No matching interaction", content);

    mockServer.Dispose();
}
```

---

## Auto Mode

Auto mode is the smartest option - it automatically records if the cassette doesn't exist, otherwise it plays back.

### Example: Auto Mode for CI/CD

```csharp
[Fact]
public async Task Auto_Mode_Example()
{
    var mockServer = new HttpServer(5000);
    mockServer.Config
        .Auto()
        .FromFile("fixtures/api_data.json")
        .ProxyTo("https://api.example.com");
    
    mockServer.Run();

    var httpClient = new HttpClient();
    var response = await httpClient.GetAsync("http://localhost:5000/v1/data");

    // First run: Records the interaction
    // Subsequent runs: Replays from cassette
    
    Assert.Equal(200, (int)response.StatusCode);

    mockServer.Dispose();
}
```

### Workflow Benefits

**First Run** (cassette doesn't exist):
1. Requests are proxied to the real API
2. Responses are recorded
3. Cassette file is created

**Subsequent Runs** (cassette exists):
1. Requests use recorded responses
2. No network calls made
3. Tests run 100x faster

This is perfect for:
- New developers cloning the repo
- CI/CD pipelines
- Offline development

---

## Real-World Examples

### Example 1: Testing Payment Gateway Integration

```csharp
public class PaymentTests : IDisposable
{
    private HttpServer _mockServer;
    private HttpClient _httpClient;
    private const int Port = 8080;

    public PaymentTests()
    {
        _mockServer = new HttpServer(Port);
        _httpClient = new HttpClient();
    }

    [Fact]
    public async Task Process_Payment_Success()
    {
        // Use auto mode - records once, replays forever
        _mockServer.Config
            .Auto()
            .FromFile("fixtures/payment_success.json")
            .ProxyTo("https://api.stripe.com");
        
        _mockServer.Run();

        // Your payment service makes API calls via the mock server
        var paymentService = new PaymentService($"http://localhost:{Port}");
        var result = await paymentService.ChargeCard(
            amount: 49.99,
            token: "tok_visa"
        );

        Assert.True(result.Success);
        Assert.Equal("ch_123456", result.ChargeId);
    }

    [Fact]
    public async Task Process_Payment_Declined()
    {
        _mockServer.Config
            .Auto()
            .FromFile("fixtures/payment_declined.json")
            .ProxyTo("https://api.stripe.com");
        
        _mockServer.Run();

        var paymentService = new PaymentService($"http://localhost:{Port}");
        var result = await paymentService.ChargeCard(
            amount: 49.99,
            token: "tok_chargeDeclined"
        );

        Assert.False(result.Success);
        Assert.Equal("card_declined", result.ErrorCode);
    }

    public void Dispose()
    {
        _mockServer?.Dispose();
        _httpClient?.Dispose();
    }
}
```

### Example 2: Testing Weather Service

```csharp
[Fact]
public async Task Get_Weather_Forecast()
{
    var mockServer = new HttpServer(5000);
    mockServer.Config
        .Record()  // First time: record from real API
        .ToFile("fixtures/weather_sf.json")
        .ProxyTo("https://api.open-meteo.com");
    
    mockServer.Run();

    var httpClient = new HttpClient();
    var response = await httpClient.GetAsync(
        "http://localhost:5000/v1/forecast?latitude=37.7749&longitude=-122.4194&current_weather=true"
    );

    var weather = await response.Content.ReadAsStringAsync();
    
    Assert.Contains("current_weather", weather);
    Assert.Contains("temperature", weather);

    mockServer.Dispose();
    
    // Next run: change .Record() to .Playback() or use .Auto()
}
```

### Example 3: Backward Compatibility

VCR doesn't interfere with normal mocking:

```csharp
[Fact]
public async Task VCR_And_Normal_Mocking_Together()
{
    var mockServer = new HttpServer(5000);
    
    // Normal mocking still works
    mockServer.Config.Get("/status").Send("OK");
    
    // Can use VCR for other endpoints if needed
    // (Note: in current implementation, VCR takes precedence if enabled)
    
    mockServer.Run();

    var httpClient = new HttpClient();
    var response = await httpClient.GetAsync("http://localhost:5000/status");
    var content = await response.Content.ReadAsStringAsync();

    Assert.Equal("OK", content);

    mockServer.Dispose();
}
```

---

## Tips & Best Practices

### 1. Organize Cassettes by Test

```
fixtures/
  ├── github/
  │   ├── user_octocat.json
  │   └── repos_list.json
  ├── payment/
  │   ├── charge_success.json
  │   └── charge_declined.json
  └── weather/
      └── san_francisco.json
```

### 2. Use Auto Mode for Most Tests

```csharp
// Recommended: Auto mode
mockServer.Config
    .Auto()
    .FromFile("fixtures/api.json")
    .ProxyTo("https://api.example.com");
```

### 3. Record Once, Review, Then Commit

1. Run tests with `.Record()` to capture real API responses
2. Review the cassette JSON files
3. Commit cassettes to version control
4. Team members get instant test data without API keys

### 4. Re-record When APIs Change

```bash
# Delete old cassettes
rm -rf fixtures/

# Run tests to re-record
dotnet test

# Commit updated cassettes
git add fixtures/
git commit -m "Update API cassettes"
```

---

## Migration from Normal Mocking

If you're using normal MoqHttp mocking:

**Before (manual mocking):**
```csharp
mockServer.Config
    .Get("/api/users/123")
    .Send("{\"id\": 123, \"name\": \"John Doe\"}");
```

**After (VCR recording):**
```csharp
mockServer.Config
    .Auto()
    .FromFile("fixtures/users.json")
    .ProxyTo("https://api.example.com");
    
// First run: hits real API, gets actual data including:
// - Correct response format
// - Real headers
// - Actual error messages
// - Edge cases you didn't think of
```

### Benefits

✅ **Realistic data** - Use actual API responses  
✅ **Less maintenance** - No manual JSON construction  
✅ **Better coverage** - Capture real edge cases  
✅ **Faster tests** - Network calls eliminated after first run  
✅ **Team collaboration** - Share cassettes via version control  

---

## Next Steps

- Explore Phase 2 features (coming soon): Advanced matching, filtering, response transformation
- Check out Phase 3 features (coming soon): Data scrubbing for sensitive information
- Learn about Phase 4 features (coming soon): Diff detection, cassette expiration
