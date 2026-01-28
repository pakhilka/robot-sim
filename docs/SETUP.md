# Setup and Installation Guide

## Prerequisites

- **Unity 6.0** or higher
- **C# 11** support
- **.NET Framework 4.x** or **.NET Core 3.1+**
- **Git** (optional, for cloning)

## Installation Steps

### Option 1: Clone Repository

```bash
git clone https://github.com/yourusername/RobotSim.git
cd RobotSim
```

### Option 2: Manual Setup

1. Create new Unity 6 project
2. Copy `Assets/Scripts/Robot/` folder
3. Copy `Assets/Prefabs/` folder (if using prefabs)
4. Copy `Assets/Scenes/` folder (if using scenes)

---

## Project Setup

### 1. Create Robot GameObject

**In Unity Scene:**

```
Create GameObject "Robot"
```

### 2. Add Physics Components

```
Select "Robot" GameObject
├─ Add Component: Rigidbody
│  └─ Body Type: Dynamic
│  └─ Use Gravity: Enabled
│  └─ Constraints: Freeze Rotation (XYZ)
│
└─ Add Component: Collider (Box)
   └─ Size: 1x1x2 (or your robot dimensions)
```

### 3. Add Robot Body Component

```
Select "Robot" GameObject
Add Component: RobotBody
├─ Max Speed: 10 (m/s)
├─ Motor Acceleration: 5 (m/s²)
└─ Motor Braking Force: 10 (m/s²)
```

### 4. Add Laser Sensor

**Create child object:**

```
Robot/
└─ LaserSensorPoint (Empty)
   ├─ Position: (0, 0.5, 1)
   ├─ Add Component: LaserDistanceSensor
   │  ├─ Max Distance: 20 (m)
   │  └─ Obstacle Mask: Select layers (e.g., "Environment")
   └─ Add Component: Collider (Box) [optional, for visualization]
```

### 5. Add Robot Brain (Main Orchestrator)

```
Select "Robot" GameObject
Add Component: RobotBrain
├─ Body: Drag "Robot" here
├─ Laser Sensor: Drag "LaserSensorPoint" here
├─ Brain Type: LocalMock (or WokwiTcp)
├─ TCP Host: 127.0.0.1 (if using WokwiTcp)
└─ TCP Port: 9999 (if using WokwiTcp)
```

### Scene Hierarchy

```
Scene
├─ Robot (GameObject)
│  ├─ Rigidbody (component)
│  ├─ BoxCollider (component)
│  ├─ RobotBody (script component)
│  ├─ RobotBrain (script component) ← Main orchestrator
│  │
│  └─ LaserSensorPoint (child GameObject)
│     ├─ BoxCollider (component)
│     └─ LaserDistanceSensor (script component)
│
├─ Ground (optional)
│  ├─ MeshCollider or BoxCollider
│  └─ On layer "Environment"
│
└─ Obstacles (optional)
   ├─ BoxColliders on layer "Environment"
   └─ For LaserDistanceSensor to detect
```

---

## Configuring Brain Type

### Local Mock Brain (Default)

**Best for**: Testing, prototyping, offline development

```
Inspector (RobotBrain component):
├─ Brain Type: LocalMock
│  └─ Behavior: Drive forward, stop when obstacle near
└─ Config: Built-in defaults
```

**No additional setup needed!**

### TCP Backend Brain (Wokwi)

**Best for**: Real backend integration, remote decision-making

#### Prerequisites

- Docker (for running wokwi-tcp)
- Backend server running on desired host/port

#### Setup

**1. Start Backend Server**

```bash
# Using Docker (example)
docker run -p 9999:9999 your-backend-image

# Or direct
python your_backend_server.py --port 9999
```

**2. Configure in Inspector**

```
Inspector (RobotBrain component):
├─ Brain Type: WokwiTcp
├─ TCP Host: 127.0.0.1 (or your server IP)
├─ TCP Port: 9999 (match your server port)
└─ Config: Connected to backend
```

**3. Check Connection**

Console output:
```
[WokwiTcpBrain] Подключение инициировано
[TcpClientService] Подключено к 127.0.0.1:9999
[WokwiTcpBrain] Получен handshake, статус Ready
```

If error:
```
[TcpClientService] Ошибка подключения: Connection refused
```
→ Check if backend is running and port is correct

---

## Environment Setup

### Layer Configuration

**Create layers for sensor detection:**

```
Edit → Project Settings → Tags and Layers
├─ Add Layer: "Environment"
├─ Add Layer: "Obstacles"
└─ Add Layer: "Robot"
```

**Assign to GameObjects:**

```
Ground GameObject
└─ Layer: Environment

Obstacle GameObject
├─ Layer: Environment
└─ Add Collider component

Robot GameObject
└─ Layer: Robot (optional)
```

**Configure LaserDistanceSensor:**

```
Inspector (LaserDistanceSensor component):
└─ Obstacle Mask: Environment + Obstacles
```

### Physics Configuration

```
Edit → Project Settings → Physics

├─ Gravity: (0, -9.8, 0)
├─ Default Material: (Default friction, bounce)
├─ Layer Collision Matrix:
│  └─ Robot ↔ Environment: Enabled
│  └─ Robot ↔ Robot: Disabled (optional)
└─ Iteration Count: 7 (default)
```

---

## Testing Your Setup

### Test 1: Robot Movement

```
Steps:
1. Select RobotBrain component
2. Play scene
3. Observe console: "[RobotBrain] Инициализирован с мозгом..."
4. Watch robot move forward
5. Place obstacle in front
6. Robot should stop when obstacle is 10m away (default)
```

### Test 2: Sensor Data

```
Steps:
1. Add to your script or console:
   
   var sensorManager = robotBrain._sensorManager;
   var distance = sensorManager.GetSensorValueAsFloat("LaserDistance", 9999f);
   Debug.Log($"Distance: {distance}m");

2. Check console output as robot approaches obstacles
```

### Test 3: TCP Connection (if using WokwiTcp)

```
Steps:
1. Set Brain Type: WokwiTcp
2. Start TCP server: python backend_server.py --port 9999
3. Play scene
4. Check console for handshake message
5. Verify robot responds to backend commands
```

---

## Troubleshooting

### Issue: Robot doesn't move

**Diagnostics:**

```
Check ✓ RobotBody.cs component exists
Check ✓ Rigidbody is Dynamic
Check ✓ Gravity is enabled
Check ✓ Use Gravity is enabled
Check ✓ Constraints don't freeze position
```

**Solution:**
```
1. Select Robot GameObject
2. Inspector → RobotBody component
3. Set Max Speed > 0
4. Press Play and verify movement
```

### Issue: Sensor never detects obstacles

**Diagnostics:**

```
Check ✓ Obstacle has Collider
Check ✓ Obstacle layer matches Obstacle Mask
Check ✓ Max Distance >= obstacle distance
Check ✓ Laser sensor is child of Robot
```

**Solution:**
```
1. Scene view: position laser sensor at front of robot
2. Select laser sensor: Green gizmo shows raycast direction
3. Adjust Obstacle Mask to include obstacle layer
```

### Issue: TCP connection fails

**Diagnostics:**

```
Check ✓ Backend server is running
Check ✓ Backend listening on correct port
Check ✓ Host/port match in Inspector
Check ✓ Firewall allows connection
Check ✓ No other process using port
```

**Solution:**
```
1. Console → See error message
2. Verify backend: netstat -an | grep 9999
3. Try localhost: 127.0.0.1:9999
4. Check backend logs for handshake
```

### Issue: Compilation errors

**Diagnostics:**

```
Check ✓ Using Unity 6.0+
Check ✓ All Scripts under Assets/Scripts/Robot/
Check ✓ Namespaces consistent
Check ✓ No circular references
```

**Solution:**
```
1. Assets → Reimport All
2. Edit → Clear Console (clears cache)
3. Restart Unity
```

---

## Performance Optimization

### Frame Rate

Target **60 FPS** for smooth motion:

```
Edit → Project Settings → Time
└─ Fixed Timestep: 0.0167 (1/60)
```

### Draw Distance

Optimize sensor raycast:

```
Inspector (LaserDistanceSensor):
└─ Max Distance: 20 (increase = more overhead)
```

### Background Thread

TCP read thread runs safely in background:

```
Monitor via:
1. Window → Analysis → Profiler
2. Check CPU usage (should be minimal)
```

---

## Next Steps

### Learn More

- [Architecture Overview](ARCHITECTURE.md) - System design
- [Components Layer](COMPONENTS.md) - MonoBehaviour components
- [Brains Layer](BRAINS.md) - Decision logic
- [Services Layer](SERVICES.md) - Backend integration

### Extend Functionality

1. **Add custom brain**: See [Brains Layer](BRAINS.md)
2. **Add custom sensor**: See [Sensors Layer](SENSORS.md)
3. **Add custom service**: See [Services Layer](SERVICES.md)

### Run Examples

```
Scenes/
├─ LocalMock_Simple.unity       ← Start here
├─ LocalMock_Obstacles.unity    ← More complex
├─ WokwiTcp_Integration.unity   ← With backend
└─ Custom_Example.unity         ← Your modifications
```

---

## Development Tips

### Iterative Testing

```
1. Test with LocalMock (fast, no backend needed)
2. Switch to WokwiTcp once LocalMock works
3. Debug backend issues separately
```

### Debug Visualization

```
In Scene View:
├─ Yellow line = Laser raycast (OnDrawGizmos)
├─ Red box = Detected obstacle
└─ Green arrow = Robot forward direction
```

### Console Logging

```csharp
// Enable debug logging:
Debug.Log($"[RobotBrain] Frame {_tick}");
Debug.Log($"[WokwiTcpBrain] Status: {_status}");
Debug.Log($"[TcpClientService] Sent: {distance}m");
```

---

See also:
- [Architecture](ARCHITECTURE.md)
- [Contributing Guide](CONTRIBUTING.md)
