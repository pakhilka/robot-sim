# Components Layer

The Components layer contains all MonoBehaviour scripts that directly interact with Unity and the scene.

## Overview

| Component | Purpose | Dependencies |
|-----------|---------|--------------|
| **RobotBrain** | Main orchestrator | RobotBody, LaserDistanceSensor, RobotController |
| **RobotBody** | Physics & motor control | Rigidbody |
| **LaserDistanceSensor** | Distance measurement | Physics.Raycast |

## RobotBrain

**Namespace**: `RobotSim.Components`  
**Base Class**: `MonoBehaviour`

### Purpose
- **Single** orchestrator for the entire robot
- Manages the FixedUpdate loop
- Bridges pure C# logic with Unity

### Configuration (Inspector)

```csharp
[Header("Physical Components")]
public RobotBody body;
public LaserDistanceSensor laserSensor;

[Header("Brain Configuration")]
public BrainType brainType = BrainType.LocalMock;
public string tcpHost = "127.0.0.1";
public int tcpPort = 9999;
```

### Lifecycle

```
Awake()
├─ Create SensorManager
├─ Register sensors
├─ Create RobotController (selects brain)
└─ Ready for FixedUpdate

FixedUpdate() [repeating]
├─ Collect sensor data
├─ Robot brain decides
├─ Execute command
└─ Continue until Destroy

OnDestroy() [implicit]
└─ TCP connection closes (destructor in services)
```

### Code Example

```csharp
public class RobotBrain : MonoBehaviour
{
    public RobotBody body;
    public LaserDistanceSensor laserSensor;
    public BrainType brainType = BrainType.LocalMock;
    
    private SensorManager _sensorManager;
    private RobotController _robotController;
    
    private void Awake()
    {
        // Initialize sensors
        _sensorManager = new SensorManager();
        _sensorManager.RegisterSensor(laserSensor);
        
        // Create controller (brain selection)
        _robotController = new RobotController(brainType, tcpHost, tcpPort);
    }
    
    private void FixedUpdate()
    {
        // 1. Collect
        var sensorData = _sensorManager.Collect(body, _tick++);
        
        // 2. Decide
        var command = _robotController.Tick(sensorData);
        
        // 3. Execute
        body.SetMotors(command.left, command.right);
    }
}
```

### Error Handling

```csharp
private void FixedUpdate()
{
    if (body == null || _robotController == null)
        return;  // Skip this frame safely
}
```

---

## RobotBody

**Namespace**: `RobotSim.Components`  
**Base Class**: `MonoBehaviour`  
**Requires**: `Rigidbody` component

### Purpose
- Apply motor commands to physics
- Track current speed
- Handle acceleration/braking

### Public API

```csharp
public void SetMotors(float left, float right)
{
    // left, right: normalized [-1, 1]
    // -1 = full reverse, 0 = stop, 1 = full forward
}

public float CurrentSpeed { get; }  // Average of both motors
```

### Configuration

```csharp
public float maxSpeed = 10f;              // m/s
public float motorAcceleration = 5f;      // m/s²
public float motorBrakingForce = 10f;     // m/s²
```

### Physics Integration

```
Motor Command [-1, 1]
    │
    ├─ Clamp to [-maxSpeed, maxSpeed]
    │
    ├─ Apply to Rigidbody.linearVelocity
    │   (only X/Z, preserve Y for gravity)
    │
    └─ Constrained by maxSpeed
```

### Example Scenario

```csharp
// Set motors to move forward
body.SetMotors(1f, 1f);     // Full speed forward
// body.CurrentSpeed = 10f

// Command immediate stop
body.SetMotors(0f, 0f);
// body.CurrentSpeed gradually decreases (inertia)

// Turn left (left slower than right)
body.SetMotors(0.5f, 1.0f);
// Robot curves left
```

---

## LaserDistanceSensor

**Namespace**: `RobotSim.Components`  
**Base Class**: `MonoBehaviour`  
**Implements**: `ISensor`

### Purpose
- Measure distance to obstacles using Physics.Raycast
- Implement sensor interface for SensorManager

### Public API

```csharp
public string SensorId => "LaserDistance";

public void Initialize()
{
    // Setup if needed
}

public object GetValue()
{
    return GetDistance();  // Returns float
}

public float GetDistance()
{
    // Returns distance in meters, or maxDistance if nothing hit
}
```

### Configuration

```csharp
public float maxDistance = 20f;        // Maximum check distance (m)
public LayerMask obstacleMask;         // Layers to raycast against
```

### How It Works

```
┌─────────────────────────────────────┐
│  Raycast from transform.position    │
│  Along transform.forward direction  │
├─────────────────────────────────────┤
│  Hit obstacle at 5.2m?              │
│  Return 5.2f                        │
│                                     │
│  No obstacle within 20m?            │
│  Return 20f (maxDistance)           │
└─────────────────────────────────────┘
```

### Integration with SensorManager

```csharp
// In RobotBrain.Awake()
var laserSensor = GetComponentInChildren<LaserDistanceSensor>();
_sensorManager.RegisterSensor(laserSensor);

// Later...
float distance = _sensorManager.GetSensorValueAsFloat("LaserDistance", 9999f);
```

### Visualization

Editor visualization (OnDrawGizmos):
```
Yellow line in Scene view showing raycast direction and max distance
```

---

## Component Interaction Diagram

```
┌──────────────────────────────┐
│      RobotBrain              │
│   (MonoBehaviour)            │
└────────┬─────────────────────┘
         │
    FixedUpdate
         │
    ┌────┴─────────────────┐
    │                      │
    ▼                      ▼
┌─────────────┐    ┌──────────────────────┐
│RobotBody    │    │SensorManager         │
│             │    │├─ LaserDistanceSensor│
│SetMotors()  │    │└─ GetSensorValue()   │
└─────────────┘    └──────────────────────┘
    │                      │
    ▼                      ▼
Rigidbody          Physics.Raycast
```

---

## Best Practices

### 1. Configure Layers for Sensors
```
Set LaserDistanceSensor.obstacleMask in Inspector
└─ Choose which layers count as obstacles
```

### 2. Adjust MaxSpeed for Physics
```
Higher maxSpeed = faster, requires more stable framerate
Lower maxSpeed = more stable, easier to control
```

### 3. Handle Missing Components
```csharp
if (body == null)
{
    Debug.LogError("RobotBody not assigned");
    return;  // Safely skip frame
}
```

### 4. Monitor Performance
```
Raycast: ~0.1ms per frame
Use sparingly if performance critical
```

---

## Common Issues

### Issue: Sensor always returns maxDistance
**Solution**: Check LayerMask - object might be on different layer

### Issue: Robot doesn't move
**Solution**: 
1. Check RobotBody has Rigidbody
2. Verify brain is sending commands
3. Check maxSpeed isn't 0

### Issue: Raycast misses obstacles
**Solution**:
1. Increase maxDistance if obstacle is far
2. Verify raycast direction is correct (forward)
3. Check obstacle has collider

---

See also:
- [Sensors Layer](SENSORS.md) - SensorManager details
- [Architecture](ARCHITECTURE.md) - System overview
