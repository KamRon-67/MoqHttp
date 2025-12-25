# MoqHttp Transformation Plan: Adding VCR Capabilities

## Executive Summary

Transform MoqHttp from a basic HTTP mocking library into a comprehensive testing tool by adding VCR (Video Cassette Recorder) functionality. This will increase the pain point value from **6.5/10 to 8.5/10** and market fit from **3/10 to 7.5/10**.

---

## Current State Analysis

### Strengths
- ✅ Simple, focused API
- ✅ Low learning curve
- ✅ Actual HTTP server (realistic testing)
- ✅ Clean codebase (.NET 6+)

### Weaknesses
- ❌ Competes with established solutions (MockHttp, WireMock.Net)
- ❌ Limited adoption (2 stars, 15 open issues)
- ❌ No unique differentiator
- ❌ Maintenance concerns

### Pain Point Rating
- **Current**: 6.5/10 - Real problem, but solved elsewhere
- **With VCR**: 8.5/10 - Addresses a more painful, underserved need

---

## The VCR Opportunity

### What is VCR?
VCR (Video Cassette Recorder) pattern allows you to:
1. **Record** real HTTP interactions during first test run
2. **Replay** recorded responses in subsequent runs
3. **Never mock manually** - use real API data
4. **Speed up tests** - no actual HTTP calls after recording
5. **Catch API changes** - detect when upstream services break contracts

### Why It's Powerful
- **Integration testing without pain** - Test against real data without API costs
- **Onboarding friendly** - New developers get realistic test data instantly
- **Documentation** - Cassettes serve as API usage examples
- **Debugging** - See exactly what APIs return
- **Cost savings** - Record once, replay infinitely (no API rate limits)

---

## Proposed Feature Set

### Phase 1: Core VCR Functionality (MVP)

#### 1.1 Record Mode
```csharp
var mockServer = new HttpServer(8080);
mockServer.Config
    .Record()
    .ToFile("fixtures/weather_api.json")
    .ProxyTo("https://api.weather.com");

mockServer.Run();

// Test runs, hits real API, saves responses
await httpClient.GetAsync("http://localhost:8080/forecast");

mockServer.Dispose();
```

#### 1.2 Playback Mode
```csharp
var mockServer = new HttpServer(8080);
mockServer.Config
    .Playback()
    .FromFile("fixtures/weather_api.json");

mockServer.Run();

// Test runs, uses recorded responses (no real API call)
await httpClient.GetAsync("http://localhost:8080/forecast");

mockServer.Dispose();
```

#### 1.3 Auto Mode (Record if Missing)
```csharp
mockServer.Config
    .Auto()
    .FromFile("fixtures/users.json")
    .ProxyTo("https://api.example.com");
// Records if file doesn't exist, plays back if it does
```

#### 1.4 Cassette Format (JSON)
```json
{
  "version": "1.0",
  "interactions": [
    {
      "request": {
        "method": "GET",
        "uri": "https://api.weather.com/forecast",
        "headers": {
          "Accept": "application/json"
        },
        "body": null
      },
      "response": {
        "status": 200,
        "headers": {
          "Content-Type": "application/json"
        },
        "body": "{\"temp\": 72, \"condition\": \"sunny\"}",
        "recordedAt": "2025-12-24T10:30:00Z"
      }
    }
  ]
}
```

---

### Phase 2: Advanced Matching & Filtering

#### 2.1 Request Matching
```csharp
mockServer.Config
    .Playback()
    .MatchOn(r => r.Method)
    .MatchOn(r => r.Path)
    .MatchOn(r => r.Headers["Authorization"])
    .MatchOn(r => r.QueryString["userId"])
    .FromFile("fixtures/api.json");
```

#### 2.2 Request Filtering (Record Selectively)
```csharp
mockServer.Config
    .Record()
    .Filter(r => r.Path.StartsWith("/api/"))
    .Filter(r => r.Method == "GET" || r.Method == "POST")
    .ToFile("fixtures/filtered.json")
    .ProxyTo("https://api.example.com");
```

#### 2.3 Dynamic Responses
```csharp
mockServer.Config
    .Playback()
    .FromFile("fixtures/users.json")
    .Transform(response => {
        // Inject dynamic timestamps
        var json = JObject.Parse(response.Body);
        json["timestamp"] = DateTime.UtcNow;
        response.Body = json.ToString();
        return response;
    });
```

---

### Phase 3: Security & Privacy

#### 3.1 Sensitive Data Scrubbing
```csharp
mockServer.Config
    .Record()
    .ScrubHeader("Authorization")
    .ScrubHeader("X-API-Key")
    .ScrubBody(@"api_key=[\w]+", "api_key=REDACTED")
    .ScrubBody(@"""password""\s*:\s*""[^""]+""", @"""password"":""REDACTED""")
    .ToFile("fixtures/safe.json")
    .ProxyTo("https://api.example.com");
```

#### 3.2 Before Record Hooks
```csharp
mockServer.Config
    .Record()
    .BeforeSave(interaction => {
        // Custom scrubbing logic
        interaction.Request.Headers.Remove("Cookie");
        interaction.Response.Headers.Remove("Set-Cookie");
        
        // Mask PII
        if (interaction.Response.Body.Contains("ssn")) {
            interaction.Response.Body = MaskSensitiveData(interaction.Response.Body);
        }
        
        return interaction;
    })
    .ToFile("fixtures/secure.json")
    .ProxyTo("https://api.example.com");
```

---

### Phase 4: Validation & Maintenance

#### 4.1 Expiration & Auto-Refresh
```csharp
mockServer.Config
    .Playback()
    .FromFile("fixtures/products.json")
    .RefreshIfOlderThan(TimeSpan.FromDays(7))
    .ProxyTo("https://api.store.com");
```

#### 4.2 Diff Detection
```csharp
mockServer.Config
    .Playback()
    .FromFile("fixtures/api.json")
    .ValidateAgainst("https://api.example.com")
    .OnDifference((recorded, live) => {
        _testOutput.WriteLine($"API Response Changed!");
        _testOutput.WriteLine($"Recorded: {recorded.Body}");
        _testOutput.WriteLine($"Live: {live.Body}");
        // Option to fail test or just warn
    });
```

#### 4.3 Cassette Management
```csharp
// Re-record specific interactions
mockServer.Config
    .Playback()
    .FromFile("fixtures/api.json")
    .ReRecordMatching(r => r.Path.Contains("/users/123"))
    .ProxyTo("https://api.example.com");

// Prune old interactions
CassetteManager.Prune("fixtures/api.json", olderThan: TimeSpan.FromDays(30));
```

---

## Technical Implementation

### Architecture Changes

```
MoqHttp/
├── Core/
│   ├── HttpServer.cs (existing)
│   └── ServerConfig.cs (existing)
├── Recording/
│   ├── RecordingMode.cs
│   ├── PlaybackMode.cs
│   ├── AutoMode.cs
│   ├── ProxyHandler.cs
│   └── RequestMatcher.cs
├── Cassettes/
│   ├── Cassette.cs
│   ├── Interaction.cs
│   ├── CassetteReader.cs
│   ├── CassetteWriter.cs
│   └── CassetteManager.cs
├── Security/
│   ├── DataScrubber.cs
│   └── ScrubRule.cs
└── Validation/
    ├── DiffDetector.cs
    └── ExpirationChecker.cs
```

### Key Components

#### 1. Cassette Model
```csharp
public class Cassette
{
    public string Version { get; set; } = "1.0";
    public List<Interaction> Interactions { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public Dictionary<string, string> Metadata { get; set; } = new();
}

public class Interaction
{
    public RecordedRequest Request { get; set; }
    public RecordedResponse Response { get; set; }
    public DateTime RecordedAt { get; set; }
}
```

#### 2. Recording Handler
```csharp
public class RecordingHandler : DelegatingHandler
{
    private readonly string _targetUri;
    private readonly Cassette _cassette;
    private readonly List<IScrubRule> _scrubRules;
    
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestRequest request, 
        CancellationToken cancellationToken)
    {
        // Proxy to real API
        var response = await ProxyRequest(request, cancellationToken);
        
        // Record interaction
        var interaction = await CaptureInteraction(request, response);
        
        // Apply scrubbing
        foreach (var rule in _scrubRules)
        {
            interaction = rule.Apply(interaction);
        }
        
        _cassette.Interactions.Add(interaction);
        
        return response;
    }
}
```

#### 3. Playback Handler
```csharp
public class PlaybackHandler : DelegatingHandler
{
    private readonly Cassette _cassette;
    private readonly IRequestMatcher _matcher;
    
    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, 
        CancellationToken cancellationToken)
    {
        // Find matching interaction
        var interaction = _matcher.FindMatch(request, _cassette.Interactions);
        
        if (interaction == null)
        {
            throw new CassetteException($"No matching interaction for {request.Method} {request.RequestUri}");
        }
        
        // Replay recorded response
        return Task.FromResult(BuildResponse(interaction.Response));
    }
}
```

---

## Competitive Positioning

### Comparison Matrix

| Feature | MoqHttp (Current) | EasyVCR | WireMock.Net | **MoqHttp + VCR** |
|---------|-------------------|---------|--------------|-------------------|
| Simple mocking | ✅ Excellent | ❌ No | ✅ Good | ✅ Excellent |
| Record/Playback | ❌ No | ✅ Yes | ⚠️ Complex | ✅ Yes |
| Easy to use | ✅ Very | ✅ Very | ❌ Steep curve | ✅ Very |
| Active maintenance | ❌ Unclear | ✅ Yes | ✅ Yes | ❓ TBD |
| .NET 6+ focused | ✅ Yes | ✅ Yes | ✅ Yes | ✅ Yes |
| Data scrubbing | ❌ No | ✅ Yes | ⚠️ Limited | ✅ Yes |
| Diff detection | ❌ No | ❌ No | ❌ No | ✅ Yes (unique!) |
| In-memory option | ✅ Yes | ❌ No | ✅ Yes | ✅ Yes |

### Unique Selling Points

1. **Dual Mode**: Manual mocking OR VCR recording - best of both worlds
2. **Diff Detection**: Only library to validate cassettes against live APIs
3. **Simplicity**: Easiest API in the .NET ecosystem
4. **Lightweight**: No heavy dependencies
5. **Modern**: Built for .NET 6+, async-first

---

## Migration Path for Existing Users

### Backward Compatibility
```csharp
// Old way still works
_mockServer.Config.Get("/test/123").Send("It Really Works!");

// New VCR features are additive
_mockServer.Config
    .Record()
    .ToFile("fixtures/test.json")
    .ProxyTo("https://api.example.com");
```

---

## Implementation Roadmap

### Phase 1: Core VCR (MVP) - Week 1-2
- [ ] Create cassette data models (`Cassette`, `Interaction`, `RecordedRequest`, `RecordedResponse`)
- [ ] Implement `CassetteReader` and `CassetteWriter` for JSON serialization
- [ ] Create `ProxyHandler` for forwarding requests to real APIs
- [ ] Implement basic `RecordingHandler` to capture and save interactions
- [ ] Implement basic `PlaybackHandler` to replay recorded responses
- [ ] Add fluent API methods: `.Record()`, `.Playback()`, `.ToFile()`, `.FromFile()`, `.ProxyTo()`
- [ ] Create integration tests for record and playback scenarios
- [ ] Update documentation with basic usage examples

### Phase 2: Advanced Matching - Week 3
- [ ] Create `IRequestMatcher` interface and default implementation
- [ ] Implement configurable matching strategies (method, path, headers, query strings, body)
- [ ] Add `.MatchOn()` fluent API
- [ ] Implement request filtering for selective recording
- [ ] Add `.Filter()` fluent API
- [ ] Create tests for different matching scenarios
- [ ] Add response transformation capabilities
- [ ] Add `.Transform()` fluent API

### Phase 3: Security & Privacy - Week 4
- [ ] Create `IScrubRule` interface
- [ ] Implement header scrubbing
- [ ] Implement body scrubbing with regex support
- [ ] Add `.ScrubHeader()` and `.ScrubBody()` fluent API
- [ ] Implement `.BeforeSave()` hook system
- [ ] Create common scrubbing rules (auth tokens, API keys, passwords, PII)
- [ ] Add security documentation and best practices guide
- [ ] Create tests for scrubbing functionality

### Phase 4: Validation & Maintenance - Week 5
- [ ] Implement expiration checking for cassettes
- [ ] Add `.RefreshIfOlderThan()` fluent API
- [ ] Create `DiffDetector` to compare recorded vs live responses
- [ ] Add `.ValidateAgainst()` and `.OnDifference()` fluent API
- [ ] Implement `CassetteManager` utility class
- [ ] Add cassette pruning functionality
- [ ] Implement selective re-recording with `.ReRecordMatching()`
- [ ] Create comprehensive tests for all validation features

### Phase 5: Polish & Release - Week 6
- [ ] Comprehensive documentation update
- [ ] Create migration guide for existing users
- [ ] Write getting started tutorial
- [ ] Create example projects demonstrating all features
- [ ] Performance optimization
- [ ] Add comprehensive error messages
- [ ] Create comparison documentation vs competitors
- [ ] Prepare release notes and announcement
- [ ] Update README with new features
- [ ] Release v2.0.0 with VCR functionality

---

## Success Metrics

### Technical Metrics
- All existing tests continue to pass (backward compatibility)
- 90%+ code coverage for new VCR features
- Performance: Recording overhead < 10ms per request
- Performance: Playback 100x faster than real API calls

### Adoption Metrics
- GitHub stars increase from 2 to 50+ within 3 months
- At least 5 community-contributed issues/PRs
- Featured in .NET testing articles/blogs
- 100+ weekly NuGet downloads

### Quality Metrics
- Zero critical bugs in first month after release
- Comprehensive documentation with examples
- Positive feedback on ease of use
- Clear differentiation from competitors

---

## Risks & Mitigation

| Risk | Impact | Mitigation |
|------|--------|------------|
| Breaking changes for existing users | High | Ensure all existing APIs continue to work; add new features as extensions |
| Complex implementation delays release | Medium | Focus on MVP first; release incrementally |
| Limited adoption despite improvements | High | Marketing: write blog posts, create videos, engage .NET community |
| Competing libraries add similar features | Medium | Focus on simplicity and unique features (diff detection) |
| Maintenance burden increases | Medium | Write comprehensive tests; create clear contribution guidelines |

---

## Marketing Plan

### Launch Strategy
1. **Pre-launch** (2 weeks before release)
   - Write detailed blog post explaining VCR pattern and benefits
   - Create comparison guide with competitors
   - Prepare demo videos

2. **Launch Day**
   - Announce on Reddit (r/dotnet, r/csharp)
   - Post on Twitter/X with hashtags #dotnet #testing
   - Submit to .NET newsletters (e.g., .NET Weekly)
   - Create announcement on GitHub Discussions

3. **Post-launch** (ongoing)
   - Write tutorial articles on dev.to and Medium
   - Create YouTube tutorials
   - Engage with users asking for feedback
   - Monitor and respond to issues quickly

### Content Ideas
- "Introducing VCR Testing for .NET: Record Real API Calls Once, Replay Forever"
- "How to Test Third-Party APIs Without Hitting Rate Limits"
- "MoqHttp vs WireMock.Net vs EasyVCR: A Comprehensive Comparison"
- "5 Ways VCR Testing Improves Your Integration Tests"

---

## Conclusion

Adding VCR functionality to MoqHttp transforms it from "yet another HTTP mocking library" into a **unique, powerful testing tool** that solves real pain points in modern .NET development. The combination of simplicity, VCR capabilities, and unique features like diff detection creates a compelling value proposition that can attract significant adoption.

**Next Steps:**
1. Review and approve this implementation plan
2. Begin Phase 1 implementation
3. Set up project tracking (GitHub Projects or similar)
4. Schedule regular progress reviews
