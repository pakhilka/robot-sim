# Architecture Overview

## Design Principles

RobotSim follows several key architectural principles:

### 1. **Layered Architecture**
Clear separation of concerns with distinct responsibility boundaries:
- **Presentation Layer**: MonoBehaviour components (RobotBrain, RobotBody, LaserDistanceSensor)
- **Business Logic Layer**: Pure C# brains and controllers
- **Service Layer**: Infrastructure (TCP, sensor management)
- **Data Layer**: DTOs and Results

### 2. **Dependency Inversion**
High-level modules don't depend on low-level modules. Both depend on abstractions:
- `WokwiTcpBrain` depends on `ITcpClientService` interface, not concrete implementation
- `RobotBrain` depends on `IRobotBrain` interface, allowing brain swapping

### 3. **Single Responsibility**
Each class has one reason to change:
- `RobotBrain`: Orchestrates components
- `RobotBody`: Manages physics and motors
- `SensorManager`: Manages sensor registration and collection
- `WokwiTcpBrain`: Implements TCP-based decision logic

### 4. **Composition Over Inheritance**
Uses composition for flexibility:
- `RobotController` composes `IRobotBrain` implementations
- `BrainSelector` factory pattern for creating different brains

## System Diagram

```
┌─────────────────────────────────────────────────┐
│              Unity Scene (FixedUpdate)          │
├─────────────────────────────────────────────────┤
│                  RobotBrain                     │
│          (MonoBehaviour Orchestrator)           │
└────────────────┬──────────────────┬─────────────┘
                 │                  │
        ┌────────▼──────┐    ┌──────▼────────┐
        │  SensorManager│    │ RobotController
        │   (Pure C#)   │    │   (Pure C#)    │
        └────────┬──────┘    └──────┬────────┘
                 │                  │
        ┌────────▼──────┐    ┌──────▼────────────┐
        │ LaserDistance │    │  IRobotBrain     │
        │  Sensor       │    │  (Abstract)      │
        │(MonoBehaviour)│    ├──────┬──────┬────┤
        └───────────────┘    │      │      │    │
                       ┌─────┘      │      └──┐ │
                       │            │         │ │
              ┌────────▼───┐  ┌────▼──┐  ┌──▼──▼──┐
              │LocalMock   │  │WokwiTcp│  │Custom  │
              │Brain       │  │Brain   │  │Brain   │
              └────────────┘  └───┬────┘  └────────┘
                                  │
                        ┌─────────▼────────┐
                        │ TcpClientService │
                        │   (Pure C#)      │
                        └──────────────────┘
```

## Data Flow

### Single Frame Execution

```
FixedUpdate (RobotBrain)
    │
    ├─► 1. Collect Sensor Data
    │       └─► SensorManager.Collect()
    │           └─► LaserDistanceSensor.GetValue()
    │               └─► Physics.Raycast()
    │
    ├─► 2. Make Decision
    │       └─► RobotController.Tick(sensorData)
    │           └─► Brain.Tick(sensorData)
    │               └─► TCP Send (if WokwiTcp)
    │
    └─► 3. Execute Command
            └─► RobotBody.SetMotors(left, right)
                └─► Rigidbody.linearVelocity += command
```

## Key Design Decisions

### Decision 1: Single MonoBehaviour (RobotBrain)

**Why**: Minimize Unity lifecycle overhead while keeping orchestration simple.

**Alternative Considered**: One MonoBehaviour per component (RobotBrain, RobotSensors, RobotMotor)
**Rejected**: Unnecessary coupling and harder to debug

### Decision 2: Pure C# Brains

**Why**: 
- Testable without Unity
- Can run on backend servers
- Fast iteration and performance
- No lifecycle dependencies

**Example**:
```csharp
// No MonoBehaviour, no OnEnable/OnDisable
public class WokwiTcpBrain : IRobotBrain
{
    public BrainStepResultDTO Tick(SensorDataDTO sensors)
    {
        // Pure logic, can run anywhere
    }
}
```

### Decision 3: Separate SensorManager (Not MonoBehaviour)

**Why**: 
- Sensor management is a service, not a component
- Can be tested without scene
- Same pattern as TcpClientService
- Clear separation: SensorManager = logic, LaserDistanceSensor = hardware interface

**Pattern**:
```
MonoBehaviour (hardware): LaserDistanceSensor, RobotBody, RobotBrain
Pure C#       (service):  SensorManager, TcpClientService, Brains
```

### Decision 4: Factory Pattern (BrainSelector)

**Why**:
- Centralized brain creation logic
- Easy to add new brain types
- Configuration isolated in BrainConfig

**Example**:
```csharp
var brain = BrainSelector.CreateBrain(BrainType.WokwiTcp, host, port);
```

## Error Handling

### Levels

1. **Component Level** (RobotBrain)
   - Null checks before accessing components
   - Falls back to stopped state if any component fails

2. **Service Level** (TcpClientService)
   - Connection retries with exponential backoff
   - Graceful degradation if TCP fails

3. **Brain Level** (WokwiTcpBrain)
   - Tries reconnection on errors
   - Falls back to safe state (motors off)

### Example: TCP Failure Recovery

```
Frame 1: TCP Connected ✓ → Send distance → Receive command
Frame 2: TCP Fails     ✗ → Status = Error → Motors = 0
Frame 3: TCP Retry     ✓ → Reconnect → Status = Connecting
Frame 4: TCP Ready     ✓ → Resume normal operation
```

## Extension Points

### Adding a New Brain Type

1. Implement `IRobotBrain`
2. Add to `BrainType` enum
3. Add case in `BrainSelector.CreateBrain()`

### Adding a New Sensor Type

1. Create class inheriting `MonoBehaviour, ISensor`
2. Implement `SensorId`, `Initialize()`, `GetValue()`
3. Register in `RobotBrain.Awake()`
4. Access via `SensorManager.GetSensorValue()`

### Adding a New Service

1. Create interface `IMyService`
2. Implement pure C# class
3. Inject into brain via constructor
4. No MonoBehaviour needed

## Performance Profile

| Operation | Time | Notes |
|-----------|------|-------|
| Sensor Data Collection | ~0.1ms | Single Raycast per frame |
| Brain Decision | ~0.5ms | Depends on brain complexity |
| TCP Send/Receive | ~1ms | Background thread, async |
| Total per Frame | ~1.6ms | At 60 FPS |

## Thread Safety

### Background Threads
- **TcpClientService**: Background thread for reading TCP data
- **Thread-Safe Queue**: `ConcurrentQueue<string>` for message passing
- **Main Thread**: All Unity operations happen in FixedUpdate

### Pattern
```
Main Thread              Background Thread
    │                          │
    ├─ TCP Send ──────────────┤
    │                     Read from TCP
    │                     Enqueue message
    │                          │
    └─ Dequeue message ◄───────┘
```

---

See also:
- [Components Layer](COMPONENTS.md)
- [Sensors Layer](SENSORS.md)
- [Brains Layer](BRAINS.md)
- [Services Layer](SERVICES.md)
