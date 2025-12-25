# How to Use MoqHttp VCR - Simple Guide

A quick, beginner-friendly guide to using MoqHttp's VCR (Video Cassette Recorder) features.

---

## Table of Contents

1. [What is VCR?](#what-is-vcr)
2. [Getting Started](#getting-started)
3. [Simple Examples](#simple-examples)
4. [Advanced Features (Phase 2)](#advanced-features-phase-2)
5. [Common Scenarios](#common-scenarios)
6. [Troubleshooting](#troubleshooting)

---

## What is VCR?

VCR lets you **record real API responses** and **replay them** in tests. Think of it like a DVR for HTTP calls!

### Why Use VCR?

✅ **No more manual mocking** - Use real API data  
✅ **Tests run faster** - No network calls after recording  
✅ **Works offline** - Perfect for airplanes or coffee shops  
✅ **Free API calls** - Record once, replay forever  
✅ **Great for teams** - Share recorded data via Git  

---

## Getting Started

### Step 1: Install MoqHttp

```bash
dotnet add package MoqHttp
```

### Step 2: Choose Your Mode

MoqHttp VCR has **three modes**:

| Mode | When to Use | What It Does |
|------|-------------|--------------|
| **Record** | First time setup | Hits real API, saves responses |
| **Playback** | After recording | Uses saved responses, no network |
| **Auto** | Recommended! | Smart: records if missing, plays back if exists |

---

## Simple Examples

### Example 1: Auto Mode (Easiest!)

```csharp
[Fact]
public async Task Test_GitHub_User()
{
    var mockServer = new HttpServer(5000);
    mockServer.Config
        .Auto()
        .FromFile("fixtures/github_user.json")
        .ProxyTo("https://api.github.com");
    
    mockServer.Run();

    var httpClient = new HttpClient();
    var response = await httpClient.GetAsync("http://localhost:5000/users/octocat");
    var json = await response.Content.ReadAsStringAsync();

    // First run: hits GitHub, saves response
    // Next runs: instant replay from file
    
    Assert.Contains("octocat", json);
    mockServer.Dispose();
}
```

### Example 2: Recording

```csharp
[Fact]
public async Task Record_Weather_Data()
{
    var mockServer = new HttpServer(5000);
    mockServer.Config
        .Record()  // Explicitly record
        .ToFile("fixtures/weather.json")
        .ProxyTo("https://api.open-meteo.com");
    
    mockServer.Run();

    var httpClient = new HttpClient();
    await httpClient.GetAsync("http://localhost:5000/v1/forecast?latitude=40.7&longitude=-74");

    mockServer.Dispose(); // File is saved here
}
```

### Example 3: Playback

```csharp
[Fact]
public async Task Playback_Weather_Data()
{
    var mockServer = new HttpServer(5000);
    mockServer.Config
        .Playback()  // Use recorded data
        .FromFile("fixtures/weather.json");
    
    mockServer.Run();

    var httpClient = new HttpClient();
    var response = await httpClient.GetAsync("http://localhost:5000/v1/forecast?latitude=40.7&longitude=-74");
    
    // Lightning fast - no internet needed!
    
    Assert.Equal(200, (int)response.StatusCode);
    mockServer.Dispose();
}
```

---

## Advanced Features (Phase 2)

### Selective Recording - Only Record What You Need

```csharp
// Only record GET requests
mockServer.Config
    .Record()
    .OnlyMethods("GET")
    .ToFile("fixtures/api.json")
    .ProxyTo("https://api.example.com");
```

```csharp
// Only record API calls (skip /health, /status, etc.)
mockServer.Config
    .Record()
    .OnlyPaths("/api/")
    .ToFile("fixtures/api.json")
    .ProxyTo("https://api.example.com");
```

```csharp
// Custom filter
mockServer.Config
    .Record()
    .Filter(request => request.Path.ToString().Contains("important"))
    .ToFile("fixtures/important.json")
    .ProxyTo("https://api.example.com");
```

### Advanced Matching - Match Specific Requests

```csharp
// Match by authorization header (different users = different responses)
mockServer.Config
    .Playback()
    .FromFile("fixtures/api.json")
    .MatchHeaders("Authorization");
```

```csharp
// Match by query string
mockServer.Config
    .Playback()
    .FromFile("fixtures/search.json")
    .MatchQueryString();
```

```csharp
// Match by request body (for POST/PUT)
mockServer.Config
    .Playback()
    .FromFile("fixtures/posts.json")
    .MatchBody();
```

### Response Transformation - Dynamic Data

```csharp
// Inject current timestamp
mockServer.Config
    .Playback()
    .FromFile("fixtures/api.json")
    .Transform(response => {
        var json = JObject.Parse(response.Body);
        json["server_time"] = DateTime.UtcNow.ToString("o");
        response.Body = json.ToString();
        return response;
    });
```

```csharp
// Add custom headers
mockServer.Config
    .Playback()
    .FromFile("fixtures/api.json")
    .Transform(response => {
        response.Headers["X-Custom-Header"] = "test-value";
        return response;
    });
```

---

## Common Scenarios

### Scenario 1: Testing Payment Gateway

```csharp
[Fact]
public async Task Test_Successful_Payment()
{
    var mockServer = new HttpServer(8080);
    mockServer.Config
        .Auto()
        .FromFile("fixtures/payment_success.json")
        .ProxyTo("https://api.stripe.com");
    
    mockServer.Run();

    var paymentService = new PaymentService("http://localhost:8080");
    var result = await paymentService.ChargeCard(amount: 99.99, token: "tok_visa");

    Assert.True(result.Success);
    mockServer.Dispose();
}
```

### Scenario 2: Testing with Different Users

```csharp
[Fact]
public async Task Test_Admin_User()
{
    var mockServer = new HttpServer(5000);
    mockServer.Config
        .Playback()
        .FromFile("fixtures/users.json")
        .MatchHeaders("Authorization"); // Match by auth header!
    
    mockServer.Run();

    var client = new HttpClient();
    client.DefaultRequestHeaders.Add("Authorization", "Bearer ADMIN_TOKEN");
    
    var response = await client.GetAsync("http://localhost:5000/api/admin");
    
    // Gets admin response from cassette
    Assert.Equal(200, (int)response.StatusCode);
    mockServer.Dispose();
}
```

### Scenario 3: Pagination Testing

```csharp
[Fact]
public async Task Test_Pagination()
{
    var mockServer = new HttpServer(5000);
    mockServer.Config
        .Playback()
        .FromFile("fixtures/products.json")
        .MatchQueryString(); // Match by query params!
    
    mockServer.Run();

    var client = new HttpClient();
    
    // Page 1
    var page1 = await client.GetAsync("http://localhost:5000/products?page=1");
    // Page 2
    var page2 = await client.GetAsync("http://localhost:5000/products?page=2");
    
    // Each gets correct page from cassette
    Assert.NotEqual(
        await page1.Content.ReadAsStringAsync(),
        await page2.Content.ReadAsStringAsync()
    );
    
    mockServer.Dispose();
}
```

### Scenario 4: Filter Out Health Checks

```csharp
[Fact]
public async Task Record_Only_Important_Endpoints()
{
    var mockServer = new HttpServer(5000);
    mockServer.Config
        .Record()
        .Filter(r => !r.Path.ToString().Contains("/health"))
        .Filter(r => !r.Path.ToString().Contains("/ping"))
        .ToFile("fixtures/api_important.json")
        .ProxyTo("https://api.example.com");
    
    mockServer.Run();

    var client = new HttpClient();
    await client.GetAsync("http://localhost:5000/health"); // Not recorded
    await client.GetAsync("http://localhost:5000/api/data"); // Recorded!
    
    mockServer.Dispose();
    
    // Cassette only has /api/data
}
```

---

## Troubleshooting

### Problem: "No matching interaction found"

**Cause:** Your request doesn't match any recorded interaction.

**Solutions:**
```csharp
// 1. Make sure query strings match
.MatchQueryString()

// 2. Check headers
.MatchHeaders("Authorization", "Content-Type")

// 3. Re-record the cassette
// Delete the file and run in Record or Auto mode
```

### Problem: Tests fail after API changes

**Solution:** Re-record your cassettes
```bash
# Delete old cassettes
rm -rf fixtures/

# Run tests - they'll record fresh data
dotnet test

# Commit new cassettes
git add fixtures/
git commit -m "Update cassettes for new API"
```

### Problem: Cassette file not created

**Cause:** `Dispose()` not called (file is saved on disposal)

**Solution:**
```csharp
// Use using statement
using (var mockServer = new HttpServer(5000))
{
    mockServer.Config.Record()...;
    mockServer.Run();
    // ... tests ...
} // Dispose() called automatically

// OR explicitly call Dispose()
mockServer.Dispose();
```

### Problem: Want to see cassette contents

**Solution:** Cassettes are just JSON files!
```bash
# View with any text editor
cat fixtures/api.json

# Or format with jq
cat fixtures/api.json | jq .
```

---

## Tips & Tricks

### Tip 1: Use Auto Mode for Everything

```csharp
// Recommended pattern
mockServer.Config
    .Auto()  // Smart!
    .FromFile("fixtures/my_test.json")
    .ProxyTo("https://api.example.com");
```

**Why?** Works for new developers + CI/CD + you. No thinking required!

### Tip 2: Organize Cassettes by Feature

```
fixtures/
├── authentication/
│   ├── login_success.json
│   ├── login_failure.json
│   └── token_refresh.json
├── users/
│   ├── get_user.json
│   └── update_user.json
└── payments/
    ├── charge_success.json
    └── charge_declined.json
```

### Tip 3: Commit Cassettes to Git

```bash
git add fixtures/
git commit -m "Add API cassettes for user tests"
git push
```

**Why?** Team members get instant test data. No API keys needed!

### Tip 4: Combine Multiple Features

```csharp
mockServer.Config
    .Record()
    .OnlyMethods("GET", "POST")            // Filter
    .OnlyPaths("/api/")                     // Filter
    .ToFile("fixtures/filtered.json")
    .ProxyTo("https://api.example.com");
```

```csharp
mockServer.Config
    .Playback()
    .FromFile("fixtures/api.json")
    .MatchHeaders("Authorization")          // Advanced matching
    .MatchQueryString()                     // Advanced matching
    .Transform(r => { /* modify */ });      // Transformation
```

### Tip 5: One Cassette Per Test

```csharp
[Fact]
public async Task Test_Feature_A()
{
    mockServer.Config.Auto().FromFile("fixtures/feature_a.json")...;
    // Test feature A
}

[Fact]
public async Task Test_Feature_B()
{
    mockServer.Config.Auto().FromFile("fixtures/feature_b.json")...;
    // Test feature B
}
```

**Why?** Clearer, easier to debug, easier to update.

---

## Quick Reference Card

```csharp
// BASIC VCR
.Auto()                      // Smart record/playback
.Record()                    // Force recording
.Playback()                  // Force playback
.FromFile("path.json")       // Cassette file
.ToFile("path.json")         // Where to save
.ProxyTo("https://api.com")  // Real API URL

// PHASE 2: FILTERING
.Filter(r => ...)            // Custom filter
.OnlyMethods("GET", "POST")  // Filter by method
.OnlyPaths("/api/", "/v1/")  // Filter by path

// PHASE 2: MATCHING
.MatchHeaders("Auth", ...)   // Match by headers
.MatchBody()                 // Match by body
.MatchQueryString()          // Match by query
.MatchOn((req, rec) => ...)  // Custom matcher

// PHASE 2: TRANSFORMATION
.Transform(response => ...)  // Modify response
```

---

## Next Steps

1. **Try it out** - Start with Auto mode
2. **Read examples** - Check `VCR_EXAMPLES.md` for more
3. **Advanced features** - See Phase 2 in implementation plan
4. **Contribute** - Found a bug? Open an issue on GitHub!

---

## Need Help?

- **Examples**: See `VCR_EXAMPLES.md`
- **Issues**: Open a GitHub issue
- **Questions**: Check existing issues or ask the community

Happy testing! 🎉
