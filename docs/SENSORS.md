# Sensors Layer

The Sensors layer manages sensor registration, discovery, and data collection. It's a pure C# layer with no MonoBehaviour components.

## Overview

**Key Class**: `SensorManager`  
**Namespace**: `RobotSim.Sensors`  
**Type**: Pure C# (no MonoBehaviour)

### Purpose
- Register and manage multiple sensors
- Provide unified data collection interface
- Enable sensor swapping without code changes

## SensorManager

### API

```csharp
public class SensorManager
{
    // Registration
    public void RegisterSensor(ISensor sensor);
    public void UnregisterSensor(string sensorId);
    
    // Query
    public ISensor GetSensor(string sensorId);
    public int GetSensorCount();
    public IEnumerable<string> GetAllSensorIds();
    
    // Data retrieval
    public object GetSensorValue(string sensorId);
    public float GetSensorValueAsFloat(string sensorId, float defaultValue = 0f);
    public Dictionary<string, object> CollectAllSensorData();
    
    // Integration
    public SensorDataDTO Collect(RobotBody body, int tick);
}
```

### Initialization

```csharp
// In RobotBrain.Awake()
_sensorManager = new SensorManager();
_sensorManager.RegisterSensor(laserSensor);
```

### Data Collection

**Single Frame Flow**:

```
Collect() called
    │
    ├─ For each registered sensor:
    │   └─ sensor.GetValue()
    │
    ├─ Build Dictionary<sensorId, value>
    │
    └─ Return SensorDataDTO with:
        ├─ distanceFront (float)       ← LaserDistance value
        ├─ currentSpeed (float)        ← From RobotBody
        ├─ tick (int)                  ← Frame counter
        ├─ dt (float)                  ← Delta time
        └─ allSensorsData (Dictionary) ← All sensor values
```

### Example: Getting Sensor Data

```csharp
// Collect all sensor data
var sensorData = _sensorManager.Collect(body, tick);

// Use specific sensor
float distance = sensorData.distanceFront;

// Access via dictionary
var allData = sensorData.allSensorsData;
if (allData.ContainsKey("LaserDistance"))
{
    float value = (float)allData["LaserDistance"];
}

// Or use convenience method
float value = _sensorManager.GetSensorValueAsFloat("LaserDistance", 9999f);
```

---

## ISensor Interface

### Definition

```csharp
public interface ISensor
{
    string SensorId { get; }
    void Initialize();
    object GetValue();
}
```

### Implementation Contract

| Member | Purpose | Notes |
|--------|---------|-------|
| `SensorId` | Unique sensor identifier | Used as dictionary key |
| `Initialize()` | Setup/calibration | Called when registered |
| `GetValue()` | Read current value | Called every frame |

### Built-in Implementation: LaserDistanceSensor

```csharp
public class LaserDistanceSensor : MonoBehaviour, ISensor
{
    public string SensorId => "LaserDistance";
    
    public void Initialize()
    {
        // Optional setup
    }
    
    public object GetValue()
    {
        return GetDistance();  // Returns float
    }
}
```

---

## Creating Custom Sensors

### Step 1: Implement ISensor

```csharp
public class GyroSensor : MonoBehaviour, ISensor
{
    public string SensorId => "Gyroscope";
    
    private Vector3 _angularVelocity;
    
    public void Initialize()
    {
        Debug.Log("Gyro initialized");
    }
    
    public object GetValue()
    {
        return _angularVelocity;  // Return Vector3
    }
}
```

### Step 2: Register in RobotBrain

```csharp
private void Awake()
{
    _sensorManager = new SensorManager();
    
    // Register built-in sensor
    _sensorManager.RegisterSensor(laserSensor);
    
    // Register custom sensor
    var gyro = GetComponentInChildren<GyroSensor>();
    _sensorManager.RegisterSensor(gyro);
}
```

### Step 3: Use in Brain

```csharp
public BrainStepResultDTO Tick(SensorDataDTO sensors)
{
    // Get sensor data
    var allData = sensors.allSensorsData;
    
    if (allData.ContainsKey("Gyroscope"))
    {
        var angularVel = (Vector3)allData["Gyroscope"];
        // Use gyro data in decision logic
    }
    
    return new BrainStepResultDTO(...);
}
```

---

## Multiple Sensors Example

### Setup Scenario
```
Robot with multiple sensors:
├─ LaserDistanceSensor (front)
├─ LaserDistanceSensor (back)  ← Multiple same type
├─ GyroSensor
└─ TemperatureSensor
```

### Configuration

```csharp
public class LaserDistanceSensor : MonoBehaviour, ISensor
{
    [SerializeField]
    private string _sensorId = "LaserDistance_Front";  // Unique ID
    
    public string SensorId => _sensorId;
    // ...
}
```

### Registration

```csharp
private void Awake()
{
    _sensorManager = new SensorManager();
    
    // Register with different IDs
    var laserFront = GetComponentInChildren<LaserDistanceSensor>();
    laserFront._sensorId = "LaserDistance_Front";
    _sensorManager.RegisterSensor(laserFront);
    
    var laserBack = laserFront.transform.sibling?.GetComponent<LaserDistanceSensor>();
    laserBack._sensorId = "LaserDistance_Back";
    _sensorManager.RegisterSensor(laserBack);
}
```

### Usage

```csharp
var data = sensors.allSensorsData;
float frontDist = (float)data["LaserDistance_Front"];
float backDist = (float)data["LaserDistance_Back"];
```

---

## Data Structures

### SensorDataDTO

```csharp
public struct SensorDataDTO
{
    public float distanceFront;                      // Primary sensor
    public float currentSpeed;                       // From RobotBody
    public int tick;                                 // Frame counter
    public float dt;                                 // Delta time
    [NonSerialized]
    public Dictionary<string, object> allSensorsData; // All sensors
}
```

### Why SensorDataDTO?

✅ **Backward Compatibility**: `distanceFront` still available  
✅ **Extensibility**: `allSensorsData` for new sensors  
✅ **Serializable**: Can be sent over network  
✅ **Type-Safe**: `GetSensorValueAsFloat()` prevents casting errors  

---

## Performance Considerations

### Sensor Collection Overhead

```
Per Frame Cost:
├─ SensorManager.Collect()        ~0.01ms
├─ LaserDistanceSensor.GetValue() ~0.1ms  (Physics.Raycast)
└─ Total                          ~0.11ms
```

### Optimization Tips

1. **Cache results if reading multiple times**
   ```csharp
   var value = _sensorManager.GetSensorValue("LaserDistance");
   // Don't call again in same frame
   ```

2. **Use appropriate sensor refresh rates**
   ```csharp
   // Some sensors may not need every frame
   if (_tick % 2 == 0)  // Every 2 frames
   {
       // Update expensive sensor
   }
   ```

3. **Minimize active sensors**
   ```csharp
   // Only register what you need
   _sensorManager.RegisterSensor(laserSensor);
   // Don't register unused sensors
   ```

---

## Error Handling

### Missing Sensor

```csharp
// Safe - returns null
var sensor = _sensorManager.GetSensor("NonExistent");
if (sensor == null)
{
    Debug.LogWarning("Sensor not found");
}

// Safe - returns default
float value = _sensorManager.GetSensorValueAsFloat("Missing", 0f);
```

### Null Sensor ID

```csharp
// SensorManager validates
public void RegisterSensor(ISensor sensor)
{
    if (sensor == null)
    {
        Debug.LogError("Cannot register null sensor");
        return;  // Safely rejected
    }
}
```

---

## Testing Sensors

### Unit Test Example

```csharp
[Test]
public void TestSensorRegistration()
{
    var sensorManager = new SensorManager();
    var mockSensor = new MockSensor("Test");
    
    sensorManager.RegisterSensor(mockSensor);
    
    var retrieved = sensorManager.GetSensor("Test");
    Assert.IsNotNull(retrieved);
    Assert.AreEqual("Test", retrieved.SensorId);
}

[Test]
public void TestDataCollection()
{
    var sensorManager = new SensorManager();
    var mockSensor = new MockSensor("Value") { Value = 42f };
    
    sensorManager.RegisterSensor(mockSensor);
    var data = sensorManager.CollectAllSensorData();
    
    Assert.AreEqual(42f, (float)data["Value"]);
}
```

---

## Best Practices

### 1. Use Meaningful Sensor IDs
```csharp
// ✅ Good
public string SensorId => "LaserDistance_Front";

// ❌ Avoid
public string SensorId => "Sensor1";
```

### 2. Implement Initialize() for Setup
```csharp
// ✅ Good
public void Initialize()
{
    _calibrationValue = GetCurrentReading();
}

// ❌ Avoid - using Awake()
private void Awake()
{
    _calibrationValue = GetCurrentReading();
}
```

### 3. Handle Edge Cases in GetValue()
```csharp
// ✅ Good
public object GetValue()
{
    if (!isActive) return 0f;
    return ReadValue();
}

// ❌ Avoid
public object GetValue()
{
    return ReadValue();  // May throw
}
```

---

See also:
- [Components Layer](COMPONENTS.md) - LaserDistanceSensor implementation
- [Architecture](ARCHITECTURE.md) - System overview
