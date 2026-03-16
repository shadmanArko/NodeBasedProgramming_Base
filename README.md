# NodeBasedProgramming_Base


A node-based programming system built in Unity 6000.3.2f1. Blocks are connected by ports to form an executable graph. The graph is serializable to JSON.

The task required node-based object manipulation, logic blocks, and JSON export/import — implemented here with a full MVC architecture using Zenject, UniRx, and UniTask.

---

## Architecture

The system follows strict MVC with dependency inversion throughout. The only `MonoBehaviour` in the MVC stack is `GraphView`.

```
NodeGraphInstaller (Zenject)
│
├── NodeGraphConfig          ScriptableObject — all configurable values
├── IEventBus                decoupled cross-system messaging (UniRx Subject per type)
│
├── IGraphModel              single source of truth — all state mutations go here
│   └── GraphModel           ReactiveProperty<T> + ReactiveCollection<T>
│
├── IGraphExecutionService   
│   └── GraphExecutionService       runs the graph — also implements IGraphRuntime
├── IGraphSerializationService
│   └── GraphSerializationService   async file I/O via UniTask.RunOnThreadPool
├── IBlockFactoryService
│   └── BlockFactoryService         Open/Closed registry — add types without modifying
│
├── IGraphController (IInitializable + IDisposable)
│   └── GraphController     pure C# — binds View intents to services and model
│
├── GraphView                sole MonoBehaviour — exposes IObservable<T> intents
└── GraphRuntimeBridge       mirrors model state to Inspector fields (read-only)
```

### Execution model

Blocks communicate through two port types:

- **Flow ports** — `FlowIn` / `FlowOut` control execution order. Entry blocks are those with a `FlowIn` port and no incoming flow connection.
- **Data ports** — values are pulled lazily. When a block needs an input, it follows the connection back to the upstream block and calls `GetOutputValue()`.

`GraphExecutionService` builds an `O(1)` index of all connections before each run. It implements `IGraphRuntime`, which is the only interface blocks depend on.

### Design patterns

| Pattern | Where |
|---|---|
| MVC | `GraphModel` / `GraphController` / `GraphView` |
| Dependency inversion | every class depends on an interface, never a concrete |
| Observer (UniRx) | model reactive properties push changes to controller and view |
| Factory (Open/Closed) | `BlockFactoryService` registry — extend without modifying |
| Facade | `IGraphModel` hides all internal collection management |
| Command (implicit) | view fires intents as `IObservable<T>`; controller handles them |
| Mediator | `IEventBus` decouples publishers from subscribers across layers |

### Data flow at runtime

```
User intent (Inspector button)
    -> GraphView fires IObservable intent
        -> GraphController subscribes, calls service or mutates model
            -> IGraphModel notifies subscribers via ReactiveProperty
                -> GraphController pushes update back to GraphView render method
                    -> Inspector reflects new state
```

---

## Block reference

### Object blocks

| Block | Ports in | Ports out | Notes |
|---|---|---|---|
| `SpawnObject` | FlowIn, PosX, PosY, PosZ | FlowOut, SpawnedObject | primitive: Cube / Sphere / Cylinder / Capsule |
| `MoveObject` | FlowIn, Target, X, Y, Z | FlowOut | mode: Absolute or Relative |
| `RotateObject` | FlowIn, Target, Pitch, Yaw, Roll | FlowOut | mode: Absolute or Relative |
| `ScaleObject` | FlowIn, Target, ScaleX, ScaleY, ScaleZ | FlowOut | mode: Absolute or Multiply |

### Logic blocks

| Block | Ports in | Ports out | Notes |
|---|---|---|---|
| `Variable` | FlowIn, Operand | FlowOut, Value | operations: None / Add / Subtract / Multiply / Divide |
| `Compare` | FlowIn, A, B | FlowOut, Result (bool) | operators: Equal / NotEqual / GT / GTE / LT / LTE |
| `Branch` | FlowIn, Condition (bool) | True (flow), False (flow) | standard if/else gate |
| `Log` | FlowIn, Message, FloatValue | FlowOut | prints to Unity Console |

---

## JSON format

```json
{
  "version": "2.0",
  "exportedAt": "2025-03-16T...",
  "blocks": [
    {
      "blockId": "a1b2c3d4",
      "blockType": "SpawnObject",
      "gameObjectName": "SpawnCube",
      "position": { "x": 0.0, "y": 0.0, "z": 0.0 },
      "propertiesJson": "{\"primitiveType\":\"Cube\",\"objectName\":\"DemoCube\",\"defaultX\":0,\"defaultY\":0,\"defaultZ\":0}"
    }
  ],
  "connections": [
    {
      "fromBlockId": "a1b2c3d4",
      "fromPortName": "FlowOut",
      "toBlockId": "e5f6a7b8",
      "toPortName": "FlowIn"
    }
  ],
  "runtimeObjects": [
    {
      "name": "DemoCube",
      "primitiveType": "Cube",
      "position": { "x": 0.0, "y": 0.0, "z": 0.0 },
      "rotation": { "x": 0.0, "y": 0.0, "z": 0.0 },
      "scale": { "x": 2.0, "y": 2.0, "z": 2.0 }
    }
  ]
}
```

---

## Project structure

```
Assets/
├── _Scripts/
│   ├── Config/
│   │   └── NodeGraphConfig.cs          ScriptableObject — all runtime settings
│   ├── Core/
│   │   ├── BaseBlock.cs                abstract MonoBehaviour, uses IGraphRuntime
│   │   ├── IGraphRuntime.cs            interface blocks depend on during execution
│   │   ├── BlockConnection.cs          serializable port connection data
│   │   └── PortDefinition.cs           port metadata (name, type, direction)
│   ├── Blocks/
│   │   ├── SpawnObjectBlock.cs
│   │   ├── MoveObjectBlock.cs
│   │   ├── RotateObjectBlock.cs
│   │   ├── ScaleObjectBlock.cs
│   │   ├── VariableBlock.cs
│   │   ├── CompareBlock.cs
│   │   ├── BranchBlock.cs
│   │   └── LogBlock.cs
│   ├── Events/
│   │   ├── IEventBus.cs
│   │   ├── SimpleEventBus.cs
│   │   └── GraphEvents.cs              all event structs
│   ├── Models/
│   │   ├── IGraphModel.cs
│   │   └── GraphModel.cs
│   ├── Services/
│   │   ├── Execution/
│   │   │   ├── IGraphExecutionService.cs
│   │   │   └── GraphExecutionService.cs
│   │   ├── Serialization/
│   │   │   ├── IGraphSerializationService.cs
│   │   │   └── GraphSerializationService.cs
│   │   └── Factory/
│   │       ├── IBlockFactoryService.cs
│   │       └── BlockFactoryService.cs
│   ├── Controllers/
│   │   ├── IGraphController.cs
│   │   └── GraphController.cs
│   ├── Views/
│   │   ├── GraphView.cs                sole MonoBehaviour in the MVC stack
│   │   └── GraphRuntimeBridge.cs       Inspector mirror — read-only
│   ├── Serialization/
│   │   ├── GraphData.cs
│   │   └── GraphSerializer.cs
│   ├── Installers/
│   │   └── NodeGraphInstaller.cs       Zenject MonoInstaller
│   └── Demo/
└──     └── DemoSetup.cs
```

---

## Setup

### Requirements

- Unity 6000.3.2f1

### Installation

1. **Clone the repository**

2. **Open in Unity**
    - Launch Unity Hub
    - Add project from disk
    - Select the cloned repository folder

3. **Install Dependencies**
    - Dependencies are managed through Unity Package Manager
    - All required packages should auto-resolve

### Quick Start

1. Open the **MainScene** scene
2. Press Play in Unity Editor
3. Game will automatically load the software
4. Use `Graphview_prefab(clone)` gameobject from the inspector to create and connect blocks

---

## Using the Inspector

Select `RuntimeBridge` in the Hierarchy while in Play Mode.

| Control | Action |
|---|---|
| Run Graph | executes the full graph from all entry blocks |
| Clear Scene | destroys all runtime-spawned objects and resets variable blocks |
| Export JSON | writes graph to the path set in `NodeGraphConfig` |
| Import JSON | reads and rebuilds the graph from that path |
| Add Block (buttons) | creates a new block as a child of `GraphView.BlockParent` |
| Connections panel | shows all live connections; each row has a remove button |
| Add Connection form | dropdown + port name fields; port hints update from block definitions |

`Clear on Run` in `NodeGraphConfig` controls whether the previous run's objects are destroyed before each execution.

---

## Extending

### Adding a new block type

1. Create a class extending `BaseBlock` in `Assets/NodeGraph/Blocks/`.
2. Override `BlockType`, `Execute(IGraphRuntime)`, `GetOutputValue(string, IGraphRuntime)`, and `GetPortDefinitions()`.
3. Register it in `BlockFactoryService`:

```csharp
_registry["MyBlock"] = go => go.AddComponent<MyBlock>();
```

4. Add serialization cases in `GraphSerializer.SerializeProps()` and `ApplyProps()`.

No other files need to change. The factory, editor, and serializer are all open for extension via the registry pattern.

---

## Dependencies

| Library | Purpose |
|---|---|
| Zenject | dependency injection — construction, binding, IInitializable lifecycle |
| UniRx | reactive properties, collections, observable subscriptions |
| UniTask | async/await for file I/O and graph execution without coroutines |