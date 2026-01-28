# Brains Layer

The Brains layer contains the decision-making logic of the robot. All classes are pure C# with no MonoBehaviour dependencies.

## Overview

**Key Classes**:
- `IRobotBrain` - Interface contract for all brains
- `RobotController` - Orchestrator that manages brain selection
- `BrainSelector` - Factory for creating brain instances
- `BrainConfig` - Configuration for brains
- `WokwiTcpBrain` - TCP-based decision logic
- `LocalMockBrain` - Simple mock for testing

**Namespace**: `RobotSim.Brains`  
**Type**: Pure C# (no MonoBehaviour)

## IRobotBrain Interface

### Contract

```csharp
public interface IRobotBrain
{
    BrainStepResultDTO Tick(SensorDataDTO sensors);
}
```

### Implementation Requirements

Every brain must:
1. Accept `SensorDataDTO` with sensor readings
2. Make decision based on sensor data
3. Return `BrainStepResultDTO` with command to execute

---

## Brain Structure

### Generic Brain Template

```csharp
public class MyBrain : IRobotBrain
{
    private readonly BrainConfig _config;
    private BrainStatusDTO _status;
    
    public MyBrain(BrainConfig config)
    {
        _config = config;
        _status = BrainStatusDTO.Ready;
    }
    
    public BrainStepResultDTO Tick(SensorDataDTO sensors)
    {
        // 1. Read sensor data
        float distance = sensors.distanceFront;
        
        // 2. Make decision
        MotorCommandDTO command = DecideMovement(distance);
        
        // 3. Return result
        return new BrainStepResultDTO(_status, command, "debug_info");
    }
    
    private MotorCommandDTO DecideMovement(float distance)
    {
        // Your AI logic here
        return new MotorCommandDTO(1f, 1f);
    }
}
```

---

## Built-in Brains

### LocalMockBrain

**Purpose**: Simple hardcoded logic for testing  
**Behavior**: Drive forward, stop when obstacle near

```csharp
public class LocalMockBrain : IRobotBrain
{
    private readonly BrainConfig _config;
    
    public LocalMockBrain(BrainConfig config)
    {
        _config = config;
    }
    
    public BrainStepResultDTO Tick(SensorDataDTO sensors)
    {
        MotorCommandDTO command;
        
        if (sensors.distanceFront <= _config.StopDistance)
        {
            command = new MotorCommandDTO(0f, 0f);  // Stop
        }
        else
        {
            command = _config.DriveCommand;        // Go forward
        }
        
        return new BrainStepResultDTO(
            BrainStatusDTO.Ready, 
            command, 
            "mock_brain"
        );
    }
}
```

**Configuration**:
```csharp
var config = new BrainConfig
{
    StopDistance = 10f,
    DriveCommand = new MotorCommandDTO(1f, 1f)
};
var brain = new LocalMockBrain(config);
```

### WokwiTcpBrain

**Purpose**: Communicate with TCP backend (wokwi-tcp docker)  
**Behavior**: Send distance, receive commands via TCP

```csharp
public class WokwiTcpBrain : IRobotBrain
{
    private readonly ITcpClientService _tcpService;
    private readonly BrainConfig _config;
    private BrainStatusDTO _status;
    private MotorCommandDTO _currentCmd;
    
    public WokwiTcpBrain(ITcpClientService tcpService, BrainConfig config)
    {
        _tcpService = tcpService;
        _config = config;
        Connect();
    }
    
    public BrainStepResultDTO Tick(SensorDataDTO sensors)
    {
        // Send current distance
        int distance = (int)sensors.distanceFront;
        string response = _tcpService.SendDistance(distance);
        
        // Decide based on response
        MotorCommandDTO targetCmd;
        if (response?.Contains(_config.AlarmToken) == true)
        {
            targetCmd = new MotorCommandDTO(0f, 0f);  // Brake
        }
        else
        {
            targetCmd = _config.DriveCommand;
        }
        
        // Smooth transition
        _currentCmd = SmoothTo(_currentCmd, targetCmd, _config.BrakeTime);
        
        return new BrainStepResultDTO(_status, _currentCmd, response ?? "");
    }
}
```

**Data Flow**:
```
Frame 1: Distance=50   ──TCP──> Backend  ◄──TCP── "forward"
Frame 2: Distance=25   ──TCP──> Backend  ◄──TCP── "forward"
Frame 3: Distance=8    ──TCP──> Backend  ◄──TCP── "alarm"  → Brake
```

---

## BrainConfig

**Central configuration for all brains**

```csharp
public class BrainConfig
{
    // TCP Handshake
    public string ReadyToken = "Parktronik ready";
    
    // Movement
    public string AlarmToken = "alarm";
    public MotorCommandDTO DriveCommand = new(1f, 1f);
    
    // Acceleration
    public float BrakeTime = 0.6f;      // Slow to stop
    public float AccelTime = 0.25f;    // Fast acceleration
    
    // Mock brain
    public float StopDistance = 10f;
}
```

**Why separate config?**
- ✅ Easy to adjust without code changes
- ✅ Reusable across brain types
- ✅ Centralized parameters

---

## RobotController

**Namespace**: `RobotSim.Brains`  
**Type**: Pure C# (no MonoBehaviour)

### Purpose
- Encapsulate brain selection and orchestration
- Simplify RobotBrain's interface

### API

```csharp
public class RobotController
{
    public RobotController(BrainType brainType, string tcpHost = "127.0.0.1", int tcpPort = 9999)
    {
        _brain = BrainSelector.CreateBrain(brainType, tcpHost, tcpPort);
    }
    
    public MotorCommandDTO Tick(SensorDataDTO sensorData)
    {
        var result = _brain.Tick(sensorData);
        return result.command;
    }
}
```

### Usage

```csharp
// In RobotBrain.Awake()
_robotController = new RobotController(BrainType.WokwiTcp, "127.0.0.1", 9999);

// In RobotBrain.FixedUpdate()
var command = _robotController.Tick(sensorData);
body.SetMotors(command.left, command.right);
```

---

## BrainSelector (Factory)

**Namespace**: `RobotSim.Brains`  
**Pattern**: Factory Method

### Purpose
- Create brain instances based on type
- Centralize brain instantiation logic
- Easy to add new brain types

### API

```csharp
public class BrainSelector
{
    public static IRobotBrain CreateBrain(
        BrainType type,
        string tcpHost = "127.0.0.1",
        int tcpPort = 9999)
    {
        return type switch
        {
            BrainType.WokwiTcp => new WokwiTcpBrain(
                new TcpClientService(tcpHost, tcpPort),
                new BrainConfig()
            ),
            BrainType.LocalMock => new LocalMockBrain(
                new BrainConfig()
            ),
            _ => throw new ArgumentException($"Unknown brain type: {type}")
        };
    }
}

public enum BrainType { WokwiTcp, LocalMock }
```

### Adding New Brain Type

**Step 1**: Add to enum
```csharp
public enum BrainType 
{ 
    WokwiTcp, 
    LocalMock,
    MyCustomBrain  // ← New type
}
```

**Step 2**: Add case in factory
```csharp
public static IRobotBrain CreateBrain(BrainType type, ...)
{
    return type switch
    {
        BrainType.MyCustomBrain => new MyCustomBrain(config),
        // ... other cases
    };
}
```

**Step 3**: Select in Inspector
```
Inspector: Brain Type = MyCustomBrain
```

---

## Decision-Making Patterns

### Pattern 1: Threshold-Based (LocalMockBrain)

```csharp
if (distance <= threshold)
    command = stop;
else
    command = forward;
```

**Pros**: Simple, fast  
**Cons**: Limited complexity

### Pattern 2: Response-Based (WokwiTcpBrain)

```csharp
string response = _tcpService.SendDistance(distance);
if (response == "alarm")
    command = stop;
else
    command = forward;
```

**Pros**: Flexible, backend-driven  
**Cons**: Network latency dependency

### Pattern 3: Smooth Interpolation

```csharp
// Get target command
MotorCommandDTO targetCmd = ComputeTarget(sensors);

// Smooth transition
_currentCmd = Vector2.Lerp(_currentCmd, targetCmd, smoothFactor);
```

**Pros**: Smooth motion, no jerky changes  
**Cons**: Requires state management

---

## Creating a Custom Brain

### Example: AI-Based Brain

```csharp
public class AIBrain : IRobotBrain
{
    private readonly BrainConfig _config;
    private int _frameCount = 0;
    
    public AIBrain(BrainConfig config)
    {
        _config = config;
    }
    
    public BrainStepResultDTO Tick(SensorDataDTO sensors)
    {
        _frameCount++;
        
        // Simple pattern: oscillate every 2 seconds
        float distance = sensors.distanceFront;
        float speed = Mathf.Sin(_frameCount * 0.1f);
        
        // Stop if too close
        if (distance < _config.StopDistance)
            speed = 0;
        
        var command = new MotorCommandDTO(speed, speed);
        return new BrainStepResultDTO(BrainStatusDTO.Ready, command, "");
    }
}
```

### Registration

```csharp
// 1. Add to enum
public enum BrainType { ..., AI }

// 2. Add to factory
case BrainType.AI => new AIBrain(new BrainConfig())

// 3. Select in Inspector
Brain Type = AI
```

---

## BrainStatusDTO

**State machine for brain status**

```csharp
public enum BrainStatusDTO
{
    Disconnected = 0,  // Not connected (TCP)
    Connecting = 1,    // Connecting (TCP)
    Ready = 2,         // Ready to execute
    Error = 3          // Error occurred
}
```

### Usage in Brains

```csharp
private BrainStatusDTO _status = BrainStatusDTO.Disconnected;

public BrainStepResultDTO Tick(SensorDataDTO sensors)
{
    // Try reconnect if not ready
    if (_status == BrainStatusDTO.Disconnected)
    {
        Connect();
    }
    
    // Only execute if ready
    MotorCommandDTO command = (_status == BrainStatusDTO.Ready)
        ? ComputeCommand(sensors)
        : new MotorCommandDTO(0, 0);  // Safe stop
    
    return new BrainStepResultDTO(_status, command, "");
}
```

---

## Thread Safety

### Design Principle
**All brains run on main thread (FixedUpdate)**

```csharp
// Safe: Called from FixedUpdate only
public BrainStepResultDTO Tick(SensorDataDTO sensors)
{
    // No threading needed
    return new BrainStepResultDTO(...);
}
```

### Exception: TCP Service

```csharp
// Background thread (safe via ConcurrentQueue)
Thread readLoop = new Thread(ReadTcpMessages)
{
    IsBackground = true
};

// Main thread (safe - uses Interlocked/ConcurrentQueue)
string message = _incomingQueue.Dequeue();  // Thread-safe
```

---

## Testing Brains

### Unit Test Example

```csharp
[Test]
public void TestLocalMockBrain_StopsNearObstacle()
{
    var brain = new LocalMockBrain(new BrainConfig { StopDistance = 10f });
    
    var sensors = new SensorDataDTO { distanceFront = 5f };
    var result = brain.Tick(sensors);
    
    Assert.AreEqual(0f, result.command.left);
    Assert.AreEqual(0f, result.command.right);
}

[Test]
public void TestWokwiTcpBrain_SendsDistance()
{
    var mockService = new MockTcpService();
    var brain = new WokwiTcpBrain(mockService, new BrainConfig());
    
    var sensors = new SensorDataDTO { distanceFront = 50f };
    brain.Tick(sensors);
    
    Assert.IsTrue(mockService.LastDistanceSent == 50);
}
```

---

## Best Practices

### 1. Pure Functions
```csharp
// ✅ Good - no side effects
private MotorCommandDTO ComputeCommand(SensorDataDTO sensors)
{
    return new MotorCommandDTO(sensors.distanceFront > 10 ? 1 : 0, 0);
}

// ❌ Avoid - side effects
private MotorCommandDTO ComputeCommand(SensorDataDTO sensors)
{
    Debug.Log("Deciding...");  // Side effect
    return new MotorCommandDTO(...);
}
```

### 2. Immutable State
```csharp
// ✅ Good
public class SimpleBrain : IRobotBrain
{
    public BrainStepResultDTO Tick(SensorDataDTO sensors)
    {
        // No state modification
        return ComputeResult(sensors);
    }
}

// ⚠️ Acceptable - clear state management
public class StatefulBrain : IRobotBrain
{
    private int _frameCount = 0;
    
    public BrainStepResultDTO Tick(SensorDataDTO sensors)
    {
        _frameCount++;  // Clear state change
        return ComputeResult(sensors, _frameCount);
    }
}
```

### 3. Fail-Safe Defaults
```csharp
// ✅ Good
public BrainStepResultDTO Tick(SensorDataDTO sensors)
{
    if (sensors == null)
        return new BrainStepResultDTO(BrainStatusDTO.Error, new(0, 0), "null_sensors");
    
    // Process...
}

// ❌ Avoid
public BrainStepResultDTO Tick(SensorDataDTO sensors)
{
    return new BrainStepResultDTO(..., sensors.distanceFront, ...);
    // Crash if sensors is null
}
```

---

See also:
- [Architecture](ARCHITECTURE.md) - System overview
- [Services Layer](SERVICES.md) - TCP integration
