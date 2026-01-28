# Services Layer

The Services layer provides infrastructure functionality. All classes are pure C# with no MonoBehaviour dependencies.

## Overview

**Key Classes**:
- `ITcpClientService` - Interface contract
- `TcpClientService` - TCP communication implementation

**Namespace**: `RobotSim.Services`  
**Type**: Pure C# (no MonoBehaviour)

## Why Separate Services?

```
Good:  Brains don't know about TCP
Bad:   Brains contain TCP logic
```

**Benefits**:
- ✅ Brains testable without TCP
- ✅ Swappable implementations (TCP, HTTP, WebSocket, etc.)
- ✅ Single Responsibility Principle
- ✅ Reusable across projects

---

## ITcpClientService Interface

### Contract

```csharp
public interface ITcpClientService
{
    void Connect();
    void Disconnect();
    string SendDistance(int distance);
}
```

### Implementation Requirements

Every TCP service must:
1. Manage connection lifecycle (Connect/Disconnect)
2. Send data (SendDistance)
3. Return responses

### Why Interface?

```csharp
// Brain depends on interface, not concrete class
public class WokwiTcpBrain : IRobotBrain
{
    private readonly ITcpClientService _tcpService;  // ← Interface, not class
    
    public WokwiTcpBrain(ITcpClientService tcpService, BrainConfig config)
    {
        _tcpService = tcpService;  // Can be any implementation
    }
}

// Easy to swap implementations
var service = new TcpClientService("host", 9999);           // Real TCP
// or
var service = new MockTcpService();                          // Testing
// or
var service = new WebSocketTcpService("ws://host:9999");    // WebSocket
```

---

## TcpClientService

### Overview

```csharp
public class TcpClientService : ITcpClientService
{
    private readonly string _host;
    private readonly int _port;
    private TcpClient _tcpClient;
    private NetworkStream _networkStream;
    private StreamReader _streamReader;
    private StreamWriter _streamWriter;
    private Thread _readThread;
    private ConcurrentQueue<string> _incomingQueue;
}
```

### Connection Lifecycle

```
┌─────────────┐
│ Disconnected│
└──────┬──────┘
       │
    Connect()
       │
       ▼
┌──────────────┐
│  Connecting  │
└──────┬───────┘
       │
       ▼ (TCP successful)
┌──────────────┐
│   Connected  │
└──────┬───────┘
       │
  Disconnect()
       │
       ▼
┌─────────────┐
│ Disconnected│
└─────────────┘
```

### Public API

#### Connect()

```csharp
public void Connect()
{
    try
    {
        _tcpClient = new TcpClient();
        _tcpClient.Connect(_host, _port);
        _networkStream = _tcpClient.GetStream();
        
        // Start background read thread
        _readThread = new Thread(ReadLoop) { IsBackground = true };
        _readThread.Start();
        
        Debug.Log($"Connected to {_host}:{_port}");
    }
    catch (Exception ex)
    {
        Debug.LogError($"Connection failed: {ex.Message}");
        throw;
    }
}
```

#### Disconnect()

```csharp
public void Disconnect()
{
    // Signal read thread to stop
    if (_cancellationTokenSource != null)
        _cancellationTokenSource.Cancel();
    
    // Wait for thread to finish
    if (_readThread?.IsAlive == true)
        _readThread.Join(TimeSpan.FromSeconds(1));
    
    // Clean up resources
    CleanupConnection();
}
```

#### SendDistance(int distance)

```csharp
public string SendDistance(int distance)
{
    if (!_tcpClient?.Connected == true)
        throw new InvalidOperationException("Not connected");
    
    try
    {
        // Send distance value
        _streamWriter.WriteLine(distance);
        
        // Get latest response from queue
        string lastMessage = null;
        while (_incomingQueue.TryDequeue(out string message))
            lastMessage = message;
        
        return lastMessage;
    }
    catch (Exception ex)
    {
        Debug.LogError($"Send failed: {ex.Message}");
        CleanupConnection();
        throw;
    }
}
```

### Thread Safety

#### Background Read Thread

```csharp
private void ReadLoop()
{
    try
    {
        while (!_cancellationTokenSource.Token.IsCancellationRequested)
        {
            // This runs on background thread
            string line = _streamReader.ReadLine();
            
            if (line != null)
            {
                // Thread-safe queue
                _incomingQueue.Enqueue(line);
            }
        }
    }
    catch (Exception ex)
    {
        Debug.LogWarning($"Read error: {ex.Message}");
    }
}
```

#### Thread Communication

```
┌──────────────────┐           ┌──────────────────┐
│   Main Thread    │           │ Background Thread│
│  (FixedUpdate)   │           │  (ReadLoop)      │
└────────┬─────────┘           └────────┬─────────┘
         │                              │
         │ SendDistance(50)             │
         ├─────────TCP────────────────┤
         │                              │
         │                   Read from TCP
         │                   Enqueue("alarm")
         │                              │
         │ Dequeue ◄──────────────────┤
         │
     Process "alarm"
```

#### Why ConcurrentQueue?

```csharp
private ConcurrentQueue<string> _incomingQueue;

// Thread-safe without locks
_incomingQueue.Enqueue(line);        // From background thread
while (_incomingQueue.TryDequeue(out var msg)) // From main thread
{
    // No lock needed - atomic operations
}
```

---

## Typical Connection Sequence

### TCP Connection Setup

```
Frame 0: User selects WokwiTcp brain
         └─ RobotBrain.Awake() creates WokwiTcpBrain
         └─ WokwiTcpBrain constructor calls tcpService.Connect()

Frame 1: Connect() starts background thread
         └─ Background thread tries: tcpClient.Connect("127.0.0.1", 9999)
         └─ Success: Ready to send/receive

Frame 2: FixedUpdate
         └─ Collect sensor: distance = 50m
         └─ Send to TCP: _tcpService.SendDistance(50)
         └─ TCP Response: "forward"
         └─ Brain makes decision: command = forward

Frame 3: FixedUpdate (next frame)
         └─ Collect sensor: distance = 8m
         └─ Send to TCP: _tcpService.SendDistance(8)
         └─ TCP Response: "alarm"
         └─ Brain makes decision: command = stop
```

---

## Error Handling

### Connection Errors

```csharp
public void Connect()
{
    try
    {
        _tcpClient.Connect(_host, _port);
        // ...
    }
    catch (SocketException ex)
    {
        Debug.LogError($"Socket error: {ex.Message}");
        throw;  // Let caller handle
    }
    catch (Exception ex)
    {
        Debug.LogError($"Connection error: {ex.Message}");
        throw;
    }
}
```

### Send Errors

```csharp
public string SendDistance(int distance)
{
    if (!_tcpClient?.Connected == true)
        throw new InvalidOperationException("Not connected");
    
    try
    {
        _streamWriter.WriteLine(distance);
    }
    catch (IOException ex)
    {
        // Connection broken during send
        CleanupConnection();
        throw;
    }
}
```

### Brain Response

```csharp
public BrainStepResultDTO Tick(SensorDataDTO sensors)
{
    if (_status == BrainStatusDTO.Error)
    {
        // Retry connection
        Connect();
    }
    
    try
    {
        var response = _tcpService.SendDistance(distance);
        // Process response
    }
    catch (Exception ex)
    {
        _status = BrainStatusDTO.Error;
        return new BrainStepResultDTO(BrainStatusDTO.Error, new(0, 0), ex.Message);
    }
}
```

---

## Resource Cleanup

### Destructor

```csharp
~TcpClientService()
{
    Disconnect();  // Ensure cleanup on garbage collection
}
```

### Manual Cleanup (OnDestroy)

```csharp
// In RobotBrain (if needed)
private void OnDestroy()
{
    _robotController.Dispose();  // Cleanup services
}
```

### CleanupConnection()

```csharp
private void CleanupConnection()
{
    try
    {
        _streamWriter?.Close();
        _streamReader?.Close();
        _networkStream?.Close();
        _tcpClient?.Close();
    }
    catch { }  // Ignore cleanup errors
    
    // Set to null
    _streamWriter = null;
    _streamReader = null;
    _networkStream = null;
    _tcpClient = null;
}
```

---

## Creating Alternative Implementations

### Mock Service (Testing)

```csharp
public class MockTcpService : ITcpClientService
{
    public int LastDistanceSent { get; private set; }
    public string ResponseToReturn { get; set; } = "forward";
    
    public void Connect() { }
    public void Disconnect() { }
    
    public string SendDistance(int distance)
    {
        LastDistanceSent = distance;
        return ResponseToReturn;
    }
}
```

**Usage**:
```csharp
[Test]
public void TestBrainWithMockService()
{
    var mockService = new MockTcpService { ResponseToReturn = "alarm" };
    var brain = new WokwiTcpBrain(mockService, config);
    
    var result = brain.Tick(new SensorDataDTO { distanceFront = 5 });
    
    Assert.AreEqual(0, result.command.left);  // Should brake
}
```

### WebSocket Service

```csharp
public class WebSocketTcpService : ITcpClientService
{
    private WebSocket _webSocket;
    
    public void Connect()
    {
        _webSocket = new ClientWebSocket();
        _webSocket.ConnectAsync(new Uri("ws://127.0.0.1:9999"), CancellationToken.None);
    }
    
    public string SendDistance(int distance)
    {
        var json = JsonConvert.SerializeObject(new { distance });
        _webSocket.SendAsync(Encoding.UTF8.GetBytes(json), WebSocketMessageType.Text, true, CancellationToken.None);
        
        // Receive response...
        return "forward";
    }
    
    public void Disconnect()
    {
        _webSocket?.CloseAsync(WebSocketCloseStatus.NormalClosure, null, CancellationToken.None);
        _webSocket?.Dispose();
    }
}
```

### Usage

```csharp
// Change service without changing brain
var service = new WebSocketTcpService();
var brain = new WokwiTcpBrain(service, config);
```

---

## Performance Considerations

### Message Queue Overflow

```csharp
// If receiving faster than processing...
while (_incomingQueue.TryDequeue(out string message))
{
    lastMessage = message;  // Keep only latest
}
```

### Connection Latency

```
Latency = Network delay + Backend processing

If latency > FixedDeltaTime (16.6ms @ 60 FPS):
└─ Responses arrive late
└─ Use cached response from previous frame
```

### Thread CPU Usage

```
Background read thread:
├─ Blocks on streamReader.ReadLine()
├─ Wakes only when data arrives
└─ Minimal CPU while idle
```

---

## Testing Services

### Connection Test

```csharp
[Test]
public void TestConnect()
{
    var service = new TcpClientService("127.0.0.1", 9999);
    
    // This will fail without actual server running
    Assert.Throws<SocketException>(() => service.Connect());
}
```

### Mock Test

```csharp
[Test]
public void TestSendWithMockService()
{
    var mock = new MockTcpService();
    
    mock.Connect();
    var response = mock.SendDistance(50);
    
    Assert.AreEqual("forward", response);
    Assert.AreEqual(50, mock.LastDistanceSent);
}
```

---

## Best Practices

### 1. Dependency Injection
```csharp
// ✅ Good
public WokwiTcpBrain(ITcpClientService service, BrainConfig config)
{
    _tcpService = service;  // Accept interface
}

// ❌ Avoid
public WokwiTcpBrain(BrainConfig config)
{
    _tcpService = new TcpClientService(...);  // Hard dependency
}
```

### 2. Error Recovery
```csharp
// ✅ Good
if (_status == BrainStatusDTO.Error)
{
    Connect();  // Retry
}

// ❌ Avoid
throw new Exception("Fatal error");  // Give up
```

### 3. Resource Cleanup
```csharp
// ✅ Good
public void Disconnect()
{
    _cancellationTokenSource.Cancel();
    _readThread.Join();  // Wait for thread
    CleanupConnection();
}

// ❌ Avoid
public void Disconnect()
{
    _tcpClient.Close();  // Might crash background thread
}
```

---

See also:
- [Brains Layer](BRAINS.md) - Using services in brains
- [Architecture](ARCHITECTURE.md) - System overview
