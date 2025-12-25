# MoqHttp Project Review & Feature List

## Overview
**MoqHttp** is a lightweight, fluent HTTP mocking library for .NET 6+. It allows developers to easily spin up a local HTTP server to mock API responses or record real API interactions for consistent, fast, and offline-capable testing.

The project is divided into two main areas: **Core Mocking** and **VCR (Video Cassette Recorder) Functionalit**.

---

## Core Features: HTTP Mocking

The foundation of MoqHttp is its ability to simulate an HTTP server with a simple, readable API.

### 🚀 Lightweight Server
- **In-Process Hosting**: Uses ASP.NET Core Kestrel to host a real HTTP server within your test process.
- **Configurable Binding**: Easily specify hostname and port (defaults to localhost:5000).
- **Easy Lifecycle**: Start with `.Run()` and clean up with `.Dispose()`.

### 🛠 Fluent Mocking API
- **Readable Syntax**: Configure expectations using a natural, chainable language.
- **Verb Support**: Built-in support for `GET`, `POST`, `PUT`, and `DELETE`.
- **Generic Requests**: Match any custom HTTP method using `.Request("METHOD", "url")`.
- **Status Codes**: Set any HTTP status code (e.g., `200 OK`, `404 Not Found`, `500 Internal Server Error`).
- **Headers**: Add custom response headers.

### 🎭 Flexible Responses
- **Static Strings**: Return simple text or serialized JSON strings.
- **JSON Objects**: Support for reading JSON directly from a file into a `JObject` for easy response configuration.
- **Dynamic Actions**: Use `context => { ... }` to gain full control over the `HttpContext`, allowing for complex logic, streaming, or custom middleware-like behavior.

---

## Technical Details
- **Target Framework**: .NET 6.0
- **Server Engine**: ASP.NET Core Kestrel
- **Core Dependencies**:
  - `Microsoft.AspNetCore` (version 2.2.0)
  - `Newtonsoft.Json` (version 13.0.1)
  - `Newtonsoft.Json.Schema` (version 3.0.14)
- **Note**: While targeting .NET 6, the library currently utilizes legacy ASP.NET Core 2.2 packages for its HTTP abstractions and hosting, providing a stable but older-style foundation.

---

## Advanced Features: VCR (Video Cassette Recorder)

The VCR module allows you to record real API interactions and "replay" them later, eliminating the need for manual mocking and external network dependencies.

### 📼 Recording & Playback Modes
- **Record Mode**: Proxies requests to a real API, captures the interaction (request/response), and saves it to a "cassette" (JSON file).
- **Playback Mode**: Replays responses from a saved cassette. No network calls are made to the real API.
- **Auto Mode (Smart)**: Automatically records if the cassette file is missing and switches to playback if it exists. Perfect for CI/CD and team environments.

### 🔍 Advanced Request Matching (Phase 2)
- **Method & Path**: Default matching by HTTP verb and URL path.
- **Query Strings**: Match requests based on their query parameters.
- **Headers**: Match specific headers (e.g., `Authorization`, `Accept`).
- **Body Matching**: Match based on the exact content of the request body (crucial for POST/PUT).
- **Custom Matchers**: Plug in your own logic to determine if a request matches a recorded interaction.

### ✂️ Selective Recording (Filtering)
- **Method Filters**: Record only specific verbs (e.g., record only `GET`s).
- **Path Filters**: Record only specific URL patterns (e.g., only paths starting with `/api/v1/`).
- **Custom Predicates**: Define complex rules for which interactions should be saved to the cassette.

### 🧪 Response Transformation
- **Dynamic Injection**: Modify a recorded response before it's replayed (e.g., injecting a current `DateTime` or a unique `RequestId`).
- **Header/Status Modification**: Change replayed headers or status codes on the fly without editing the cassette file.

### 📦 Cassette Management
- **JSON Format**: Cassettes are human-readable JSON files, making them easy to debug and version control.
- **Atomic Writes**: Ensures cassettes are saved safely without corruption.
- **Directory Creation**: Automatically creates necessary folders when saving cassettes.

---

## Why Use MoqHttp?

1. **Speed**: Replaying from disk is magnitudes faster than hitting real network APIs.
2. **Reliability**: Eliminates flaky tests caused by external API downtime or rate limiting.
3. **Realistic Data**: VCR captures real headers and wire-format data, leading to more accurate tests than manual mocks.
4. **Developer Experience**: The fluent API is designed to be intuitive and requires minimal setup.
5. **No External Dependencies**: Everything runs in-process, making it portable and easy to use in any .NET testing framework (xUnit, NUnit, MSTest).
