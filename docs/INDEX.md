# Documentation Index

Complete documentation for RobotSim project (~3400 lines, 74KB).

## 📖 Main Entry Point

Start here: **[README.md](README.md)** - Project overview, quick start, architecture diagram

---

## 🏗️ Architecture & Design

- **[ARCHITECTURE.md](docs/ARCHITECTURE.md)** ← Start here for design understanding
  - Design principles (layered architecture, dependency inversion, etc.)
  - System diagram with data flow
  - Key architectural decisions and rationale
  - Error handling strategy
  - Thread safety model
  - Extension points

---

## 📚 Layer Documentation

Each layer documented with examples and API reference:

### 1. **[COMPONENTS.md](docs/COMPONENTS.md)** - MonoBehaviour Layer
   - RobotBrain (main orchestrator)
   - RobotBody (physics & motors)
   - LaserDistanceSensor (distance measurement)
   - Component interaction diagram
   - Best practices & troubleshooting

### 2. **[SENSORS.md](docs/SENSORS.md)** - Sensor Management Layer
   - SensorManager (registration & collection)
   - ISensor interface contract
   - Creating custom sensors (step-by-step)
   - Multiple sensor example
   - Performance considerations

### 3. **[BRAINS.md](docs/BRAINS.md)** - Decision-Making Layer
   - IRobotBrain interface
   - Built-in brains: LocalMockBrain, WokwiTcpBrain
   - RobotController (orchestrator)
   - BrainSelector (factory pattern)
   - Creating custom brains (with examples)
   - Decision patterns

### 4. **[SERVICES.md](docs/SERVICES.md)** - Infrastructure Layer
   - ITcpClientService interface
   - TcpClientService implementation
   - Thread safety & background threads
   - Creating alternative implementations
   - Error handling & recovery

---

## 🚀 Setup & Development

### Getting Started

- **[SETUP.md](docs/SETUP.md)** - Installation and configuration guide
  - Prerequisites & installation
  - Scene setup step-by-step
  - Brain type configuration (LocalMock vs TCP)
  - Environment setup (layers, physics)
  - Testing your setup
  - Troubleshooting

### Contributing

- **[CONTRIBUTING.md](docs/CONTRIBUTING.md)** - Developer guide
  - Code style & conventions
  - Architecture principles to follow
  - Adding new brains (tutorial)
  - Adding new sensors (tutorial)
  - Adding new services (tutorial)
  - Testing guidelines
  - Pull request process
  - Common mistakes to avoid

---

## 🎯 Quick Navigation by Task

### "I want to..."

#### Understand the system
→ [README.md](README.md) → [ARCHITECTURE.md](docs/ARCHITECTURE.md)

#### Set up a robot
→ [SETUP.md](docs/SETUP.md)

#### Add a custom brain
→ [BRAINS.md](docs/BRAINS.md#creating-a-custom-brain) → [CONTRIBUTING.md](docs/CONTRIBUTING.md#adding-a-new-brain-type)

#### Add a new sensor
→ [SENSORS.md](docs/SENSORS.md#creating-custom-sensors) → [CONTRIBUTING.md](docs/CONTRIBUTING.md#adding-a-new-sensor-type)

#### Integrate with backend (TCP)
→ [SERVICES.md](docs/SERVICES.md) → [SETUP.md](docs/SETUP.md#tcp-backend-brain-wokwi)

#### Optimize performance
→ [SERVICES.md](docs/SERVICES.md#performance-considerations) → [SETUP.md](docs/SETUP.md#performance-optimization)

#### Debug issues
→ [SETUP.md](docs/SETUP.md#troubleshooting)

#### Write tests
→ [CONTRIBUTING.md](docs/CONTRIBUTING.md#testing-guidelines)

#### Contribute code
→ [CONTRIBUTING.md](docs/CONTRIBUTING.md)

---

## 📊 Documentation Statistics

| Document | Lines | Size | Topic |
|----------|-------|------|-------|
| README.md | 150 | 6.1K | Project overview |
| ARCHITECTURE.md | 280 | 7.6K | System design |
| COMPONENTS.md | 280 | 7.4K | MonoBehaviour layer |
| SENSORS.md | 330 | 8.6K | Sensor management |
| BRAINS.md | 420 | 12K | Decision logic |
| SERVICES.md | 420 | 12K | Backend integration |
| SETUP.md | 320 | 8.6K | Installation guide |
| CONTRIBUTING.md | 340 | 12K | Developer guide |
| **Total** | **~3400** | **~74KB** | Complete reference |

---

## 🔍 Key Concepts Reference

### Architecture Pattern
- **Layered Architecture**: Clear separation between presentation, business logic, services, and data
- **Dependency Inversion**: High-level modules depend on abstractions, not concrete implementations
- **Pure C# for Logic**: Business logic in pure C#, MonoBehaviour only for Unity integration

### Design Patterns Used
- **Factory Pattern**: BrainSelector creates brain instances
- **Dependency Injection**: Services injected via constructors
- **Interface-Based Design**: IRobotBrain, ISensor, ITcpClientService contracts
- **Producer-Consumer**: Background thread for TCP, main thread for processing

### Thread Model
- **Main Thread**: FixedUpdate, sensor collection, motor control
- **Background Thread**: TCP read (non-blocking)
- **Thread-Safe Queue**: ConcurrentQueue<string> for message passing

---

## 💡 Learning Path

**Beginner** (New to project)
1. Read [README.md](README.md)
2. Follow [SETUP.md](docs/SETUP.md) to configure scene
3. Run LocalMock brain example
4. Read [ARCHITECTURE.md](docs/ARCHITECTURE.md) for design overview

**Intermediate** (Understanding components)
1. Read [COMPONENTS.md](docs/COMPONENTS.md) for scene integration
2. Read [SENSORS.md](docs/SENSORS.md) for data collection
3. Read [BRAINS.md](docs/BRAINS.md) for decision logic
4. Read [SERVICES.md](docs/SERVICES.md) for backend integration

**Advanced** (Extending framework)
1. Read [CONTRIBUTING.md](docs/CONTRIBUTING.md) for best practices
2. Follow tutorials for [adding brains](docs/CONTRIBUTING.md#adding-a-new-brain-type), [sensors](docs/CONTRIBUTING.md#adding-a-new-sensor-type), [services](docs/CONTRIBUTING.md#adding-a-new-service)
3. Write unit tests
4. Submit PR

---

## 📝 Code Examples by Layer

### Components (MonoBehaviour)
→ See [COMPONENTS.md](docs/COMPONENTS.md#robotbrain) for RobotBrain example

### Sensors (Manager + Interface)
→ See [SENSORS.md](docs/SENSORS.md#creating-custom-sensors) for custom sensor tutorial

### Brains (Decision Logic)
→ See [BRAINS.md](docs/BRAINS.md#creating-a-custom-brain) for custom brain tutorial

### Services (Infrastructure)
→ See [SERVICES.md](docs/SERVICES.md#creating-alternative-implementations) for custom service examples

---

## 🔗 Cross-References

| Topic | Documents |
|-------|-----------|
| System Design | ARCHITECTURE, all layer docs |
| Setup | SETUP, COMPONENTS |
| Custom Brain | BRAINS, CONTRIBUTING |
| Custom Sensor | SENSORS, CONTRIBUTING |
| TCP Integration | SERVICES, SETUP |
| Performance | SETUP, SERVICES, ARCHITECTURE |
| Testing | CONTRIBUTING, all layer docs |
| Troubleshooting | SETUP, ARCHITECTURE |

---

## 📞 Support

- **Questions**: See relevant layer documentation
- **Issues**: Check [SETUP.md troubleshooting](docs/SETUP.md#troubleshooting)
- **Contributing**: See [CONTRIBUTING.md](docs/CONTRIBUTING.md)
- **Examples**: See code examples in each documentation file

---

## 📄 File Locations

```
RobotSim/
├── README.md                 ← Project overview
├── docs/
│   ├── ARCHITECTURE.md       ← System design
│   ├── COMPONENTS.md         ← MonoBehaviour layer
│   ├── SENSORS.md            ← Sensor management
│   ├── BRAINS.md             ← Decision logic
│   ├── SERVICES.md           ← Backend services
│   ├── SETUP.md              ← Installation guide
│   └── CONTRIBUTING.md       ← Developer guide
└── Assets/Scripts/Robot/
    ├── Components/           ← RobotBrain, RobotBody, sensors
    ├── Sensors/              ← SensorManager
    ├── Brains/               ← Brain implementations
    ├── Services/             ← TCP services
    ├── Interfaces/           ← Contracts
    └── Data/                 ← DTOs and results
```

---

**Last Updated**: January 28, 2026  
**Total Documentation**: 3400+ lines covering all aspects of RobotSim architecture and usage
