# Contributing Guide

Welcome! This guide explains how to contribute to RobotSim.

## Code Style and Standards

### C# Conventions

```csharp
// Namespacing
namespace RobotSim.Brains { ... }

// Class naming: PascalCase
public class MyCustomBrain { ... }

// Private fields: _camelCase with underscore
private readonly ITcpClientService _tcpService;
private int _frameCount;

// Public properties: PascalCase
public string SensorId => "MySensor";
public float CurrentSpeed { get; }

// Methods: PascalCase
public BrainStepResultDTO Tick(SensorDataDTO sensors) { ... }

// Constants: UPPER_CASE
private const float DEFAULT_MAX_SPEED = 10f;

// Local variables: camelCase
float distance = sensors.distanceFront;
```

### Documentation

**Every public class/method must have XML documentation:**

```csharp
/// <summary>
/// Manages sensor registration and data collection.
/// Pure C# class with no MonoBehaviour dependencies.
/// </summary>
public class SensorManager
{
    /// <summary>
    /// Register a new sensor for data collection.
    /// </summary>
    /// <param name="sensor">Sensor implementing ISensor interface</param>
    public void RegisterSensor(ISensor sensor)
    {
        // ...
    }
}
```

### File Organization

```
Each file contains ONE public class
├─ File name matches class name (MySensor.cs contains MySensor)
├─ Namespace matches folder structure (Sensors folder → RobotSim.Sensors)
└─ Imports at top, organized by namespace
```

---

## Architecture Principles

### 1. Pure C# for Business Logic

✅ **Good**:
```csharp
public class MyBrain : IRobotBrain
{
    public BrainStepResultDTO Tick(SensorDataDTO sensors)
    {
        // Pure logic, no MonoBehaviour dependencies
        return ComputeDecision(sensors);
    }
}
```

❌ **Avoid**:
```csharp
public class MyBrain : MonoBehaviour, IRobotBrain
{
    private void Update() { ... }  // Don't use lifecycle
}
```

### 2. Dependency Injection

✅ **Good**:
```csharp
public class WokwiTcpBrain : IRobotBrain
{
    private readonly ITcpClientService _tcpService;
    
    public WokwiTcpBrain(ITcpClientService tcpService, BrainConfig config)
    {
        _tcpService = tcpService;  // Injected
    }
}
```

❌ **Avoid**:
```csharp
public class WokwiTcpBrain : IRobotBrain
{
    private TcpClientService _tcpService = new();  // Hard dependency
}
```

### 3. Single Responsibility

✅ **Good**: Each class has one reason to change
```
RobotBrain      → Orchestration
SensorManager   → Sensor management
TcpClientService → TCP communication
WokwiTcpBrain   → Decision logic
```

❌ **Avoid**: God objects
```csharp
public class RobotManager
{
    // Sensor management
    // Motor control
    // TCP communication
    // Decision making
    // ...too many responsibilities
}
```

### 4. Interface-Based Design

✅ **Good**:
```csharp
public interface IRobotBrain { ... }
public class WokwiTcpBrain : IRobotBrain { ... }
public class LocalMockBrain : IRobotBrain { ... }
public class CustomBrain : IRobotBrain { ... }
```

❌ **Avoid**: Concrete inheritance
```csharp
public class BaseBrain { ... }
public class WokwiTcpBrain : BaseBrain { ... }  // Too coupled
```

---

## Adding Features

### Adding a New Brain Type

**Step 1: Implement IRobotBrain**

```csharp
namespace RobotSim.Brains
{
    /// <summary>
    /// My custom AI algorithm for robot decision-making.
    /// </summary>
    public class MyCustomBrain : IRobotBrain
    {
        private readonly BrainConfig _config;
        private int _frameCount = 0;
        
        public MyCustomBrain(BrainConfig config)
        {
            _config = config;
        }
        
        public BrainStepResultDTO Tick(SensorDataDTO sensors)
        {
            _frameCount++;
            
            // Your logic here
            float distance = sensors.distanceFront;
            var command = DecideMovement(distance);
            
            return new BrainStepResultDTO(
                BrainStatusDTO.Ready, 
                command, 
                "debug_info"
            );
        }
        
        private MotorCommandDTO DecideMovement(float distance)
        {
            return distance > 10f 
                ? _config.DriveCommand 
                : new MotorCommandDTO(0, 0);
        }
    }
}
```

**Step 2: Register in BrainSelector**

```csharp
// In BrainSelector.cs

public enum BrainType 
{ 
    WokwiTcp, 
    LocalMock,
    MyCustom  // ← Add here
}

public static IRobotBrain CreateBrain(BrainType type, ...)
{
    return type switch
    {
        BrainType.MyCustom => new MyCustomBrain(new BrainConfig()),
        // ... other cases
    };
}
```

**Step 3: Test**

```csharp
[Test]
public void TestMyCustomBrain()
{
    var brain = new MyCustomBrain(new BrainConfig());
    var sensors = new SensorDataDTO { distanceFront = 5f };
    
    var result = brain.Tick(sensors);
    
    Assert.AreEqual(0f, result.command.left);
}
```

### Adding a New Sensor Type

**Step 1: Implement ISensor (MonoBehaviour)**

```csharp
namespace RobotSim.Components
{
    /// <summary>
    /// Temperature sensor for environmental monitoring.
    /// </summary>
    public class TemperatureSensor : MonoBehaviour, ISensor
    {
        [SerializeField]
        private string _sensorId = "Temperature";
        
        public string SensorId => _sensorId;
        
        public void Initialize()
        {
            // Setup if needed
        }
        
        public object GetValue()
        {
            // Read temperature (example)
            return GetRandomTemperature();
        }
        
        private float GetRandomTemperature()
        {
            return Random.Range(15f, 35f);  // 15-35°C
        }
    }
}
```

**Step 2: Register in RobotBrain**

```csharp
// In RobotBrain.Awake()

private void Awake()
{
    _sensorManager = new SensorManager();
    _sensorManager.RegisterSensor(laserSensor);
    
    // Add new sensor
    var tempSensor = GetComponentInChildren<TemperatureSensor>();
    _sensorManager.RegisterSensor(tempSensor);
}
```

**Step 3: Use in Brain**

```csharp
public BrainStepResultDTO Tick(SensorDataDTO sensors)
{
    var allData = sensors.allSensorsData;
    
    if (allData.ContainsKey("Temperature"))
    {
        float temp = (float)allData["Temperature"];
        // Use temperature in decision logic
    }
    
    return new BrainStepResultDTO(...);
}
```

### Adding a New Service

**Step 1: Create Interface**

```csharp
namespace RobotSim.Services
{
    public interface IMyService
    {
        void Initialize();
        string ProcessCommand(string command);
    }
}
```

**Step 2: Implement Service**

```csharp
namespace RobotSim.Services
{
    /// <summary>
    /// Pure C# service with no MonoBehaviour.
    /// </summary>
    public class MyService : IMyService
    {
        public void Initialize()
        {
            Debug.Log("MyService initialized");
        }
        
        public string ProcessCommand(string command)
        {
            return $"Processed: {command}";
        }
    }
}
```

**Step 3: Inject into Brain**

```csharp
public class MyBrain : IRobotBrain
{
    private readonly IMyService _service;
    
    public MyBrain(IMyService service, BrainConfig config)
    {
        _service = service;
        _service.Initialize();
    }
    
    public BrainStepResultDTO Tick(SensorDataDTO sensors)
    {
        var result = _service.ProcessCommand("move");
        // Use result...
    }
}
```

---

## Testing Guidelines

### Unit Tests

**Location**: `Assets/Tests/`  
**Naming**: `*Tests.cs` suffix

```csharp
using NUnit.Framework;

[TestFixture]
public class MyBrainTests
{
    [Test]
    public void Tick_WhenObstacleNear_ReturnStop()
    {
        // Arrange
        var brain = new MyCustomBrain(new BrainConfig());
        var sensors = new SensorDataDTO { distanceFront = 5f };
        
        // Act
        var result = brain.Tick(sensors);
        
        // Assert
        Assert.AreEqual(0f, result.command.left);
        Assert.AreEqual(0f, result.command.right);
    }
    
    [Test]
    public void Tick_WhenDistanceFar_ReturnForward()
    {
        var brain = new MyCustomBrain(new BrainConfig());
        var sensors = new SensorDataDTO { distanceFront = 20f };
        
        var result = brain.Tick(sensors);
        
        Assert.AreEqual(1f, result.command.left);
        Assert.AreEqual(1f, result.command.right);
    }
}
```

### Integration Tests

```csharp
[Test]
public void RobotBrain_WithLocalMock_MovesForward()
{
    // Create scene
    var robot = new GameObject("Robot");
    var body = robot.AddComponent<RobotBody>();
    var rigidbody = robot.AddComponent<Rigidbody>();
    
    // Run test
    body.SetMotors(1f, 1f);
    
    // Assert
    Assert.Greater(rigidbody.linearVelocity.magnitude, 0);
}
```

### Running Tests

```bash
# Via Unity Editor
Window → General → Test Runner → Run All

# Via command line
unity -runTests -testPlatform editmode -testCategory "UnitTests"
```

---

## Pull Request Process

### Before Submitting

1. **Test locally**
   ```bash
   # Run all tests
   Window → Test Runner → Run All
   ```

2. **Follow code style**
   ```csharp
   // Use provided templates
   // Run code formatter (if available)
   ```

3. **Update documentation**
   ```
   If adding feature:
   ├─ Update ARCHITECTURE.md (if design change)
   ├─ Update relevant layer docs
   └─ Add code examples
   ```

4. **Commit messages**
   ```
   Good:  "Add GyroSensor implementation"
   Good:  "Fix TCP reconnection logic"
   Good:  "Improve SensorManager performance"
   
   Avoid: "fix bug"
   Avoid: "stuff"
   Avoid: "asdfgh"
   ```

### PR Description Template

```markdown
## Description
Brief description of changes

## Type of Change
- [ ] New feature
- [ ] Bug fix
- [ ] Performance improvement
- [ ] Documentation update

## Related Issue
Fixes #123

## Testing
- [ ] Unit tests added
- [ ] Integration tests added
- [ ] Manual testing done

## Documentation
- [ ] README updated
- [ ] Architecture docs updated
- [ ] Code comments added

## Checklist
- [ ] Code follows style guide
- [ ] No compilation errors
- [ ] All tests pass
```

---

## Common Mistakes to Avoid

### ❌ Mixing Concerns

```csharp
// Don't do this:
public class MyBrain : IRobotBrain
{
    // TCP logic mixed with decision logic
    private TcpClient _tcp;
    
    public BrainStepResultDTO Tick(SensorDataDTO sensors)
    {
        _tcp.SendData(...);  // Should be in service
        var decision = DecideMovement(sensors);  // Should be brain logic
    }
}

// Do this instead:
public class MyBrain : IRobotBrain
{
    private readonly ITcpClientService _tcpService;
    
    public MyBrain(ITcpClientService tcpService, ...)
    {
        _tcpService = tcpService;  // Injected
    }
    
    public BrainStepResultDTO Tick(SensorDataDTO sensors)
    {
        // Only decision logic here
        return new BrainStepResultDTO(...);
    }
}
```

### ❌ Unnecessary MonoBehaviours

```csharp
// Don't do this:
namespace RobotSim.Sensors
{
    public class SensorManager : MonoBehaviour  // ✗ No reason
    {
        private void Update() { }
    }
}

// Do this instead:
namespace RobotSim.Sensors
{
    public class SensorManager  // ✓ Pure C#
    {
        public void Collect() { }
    }
}
```

### ❌ Hard-coded Values

```csharp
// Don't do this:
public class MyBrain : IRobotBrain
{
    public BrainStepResultDTO Tick(SensorDataDTO sensors)
    {
        if (sensors.distanceFront < 10)  // ✗ Magic number
        {
            return new BrainStepResultDTO(...);
        }
    }
}

// Do this instead:
public class MyBrain : IRobotBrain
{
    private readonly BrainConfig _config;
    
    public BrainStepResultDTO Tick(SensorDataDTO sensors)
    {
        if (sensors.distanceFront < _config.StopDistance)  // ✓ Configurable
        {
            return new BrainStepResultDTO(...);
        }
    }
}
```

---

## Getting Help

- **Documentation**: See [Architecture](ARCHITECTURE.md) and layer guides
- **Examples**: Check `Assets/Scenes/` for example implementations
- **Issues**: Open GitHub issue with detailed description
- **Discussions**: Use GitHub Discussions for design questions

---

## License

By contributing, you agree your code will be under the same license as the project (MIT License).

---

Happy coding! 🚀
