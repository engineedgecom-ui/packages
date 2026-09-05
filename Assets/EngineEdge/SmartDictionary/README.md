# EngineEdge — Smart Dictionary Pro

> **A fully serializable, Inspector-friendly generic Dictionary for Unity.**  
> LINQ support · events · safe access API · merge operations · JSON/PlayerPrefs save-load · 5 bonus collections.

[![Unity 2021.3+](https://img.shields.io/badge/Unity-2021.3%2B-black?logo=unity)](https://unity.com)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE.md)
[![Version](https://img.shields.io/badge/version-1.0.0-blue)](CHANGELOG.md)

---

## Table of Contents

1. [Installation](#installation)
2. [Quick Start](#quick-start)
3. [API Reference](#api-reference)
4. [Bonus Collections](#bonus-collections)
5. [Samples](#samples)
6. [Requirements](#requirements)
7. [License](#license)

---

## Installation

### Via .unitypackage

1. Download `SmartDictionaryPro_1.0.0.unitypackage` from the Unity Asset Store.
2. Open your Unity project (2021.3 or higher).
3. Double-click the `.unitypackage` file — Unity will import everything automatically.

### Via Unity Package Manager (local)

1. Clone or copy the `com.engineedge.smartdictionary` folder into your project's `Packages/` directory.
2. In the Unity Editor open **Window → Package Manager**.
3. Click **＋ → Add package from disk…** and select `package.json`.

---

## Quick Start

### Declare and populate in C\#

```csharp
using EngineEdge.SmartDictionary;
using UnityEngine;

public class Example : MonoBehaviour
{
    // Appears in the Inspector with full key/value editing
    [SerializeField]
    private SerializableDictionary<string, int> inventory
        = new SerializableDictionary<string, int>();

    private void Start()
    {
        // Safe add
        inventory.TryAdd("sword", 1);

        // Safe read with fallback
        int arrows = inventory.GetOrDefault("arrow", 0);

        // Add-or-update in one call
        inventory.AddOrUpdate("potion", 5);

        // Deconstruct foreach
        foreach (var (item, qty) in inventory)
            Debug.Log($"{item}: {qty}");
    }
}
```

### LINQ extensions

```csharp
// Filter to high-value items
var expensive = inventory.WhereDict((k, v) => v > 50);

// Transform values
var doubled = inventory.SelectValues((k, v) => v * 2);

// Sort by key ascending
var sorted = inventory.OrderByKey();

// Aggregates
int   total   = inventory.SumValues();
int   highest = inventory.MaxValue();
double avg    = inventory.AverageValues();
```

### Events

```csharp
inventory.OnEntryAdded   += (k, v)      => Debug.Log($"Added {k}");
inventory.OnEntryRemoved += (k, v)      => Debug.Log($"Removed {k}");
inventory.OnEntryUpdated += (k, old, n) => Debug.Log($"{k}: {old} → {n}");
inventory.OnCleared      += ()          => Debug.Log("Cleared!");
inventory.OnCountChanged += c           => Debug.Log($"Count: {c}");

// Temporarily pause events without removing handlers
inventory.EventsEnabled = false;
```

### Save and Load

```csharp
// JSON file
inventory.SaveToFile(path);
inventory.LoadFromFile(path);

// PlayerPrefs
inventory.ToPlayerPrefs("MyKey");
inventory.FromPlayerPrefs("MyKey");
```

### Merge operations

```csharp
var a = new SerializableDictionary<string, int> { {"x", 1} };
var b = new SerializableDictionary<string, int> { {"y", 2} };

a.Merge(b);            // adds keys from b that are absent in a
a.MergeOverwrite(b);   // overwrites existing keys
var common  = a.Intersect(b);
var diff    = a.Except(b);
var all     = a.Union(b);
var copy    = a.Clone();
```

---

## API Reference

### Core — `SerializableDictionary<TKey, TValue>`

| Method | Description |
|--------|-------------|
| `Add(key, value)` | Adds an entry; throws on duplicate key |
| `TryAdd(key, value)` | Adds if absent; returns `false` on duplicate |
| `Remove(key)` | Removes the entry; throws if absent |
| `TryRemove(key)` | Removes if present; returns `false` if absent |
| `ContainsKey(key)` | Returns `true` if the key exists |
| `TryGetValue(key, out value)` | Standard TryGetValue pattern |
| `GetOrDefault(key, defaultVal)` | Returns value or fallback — never throws |
| `GetOrAdd(key, factory)` | Returns existing value or adds via factory |
| `AddOrUpdate(key, value)` | Inserts or replaces in one call |
| `FindKey(predicate)` | First key matching `(k, v) => bool` |
| `FindValue(predicate)` | First value matching `(k, v) => bool` |
| `FindAllByValue(predicate)` | All keys whose values satisfy predicate |
| `Filter(predicate)` | New dictionary with matching entries |
| `Map(selector)` | New dictionary with transformed values |
| `Clear()` | Removes all entries |
| `Count` | Number of entries |
| `Keys` / `Values` | Collection views |
| `EventsEnabled` | Toggle all events at once |

### LINQ Extensions — `SerializableDictionaryExtensions`

| Method | Description |
|--------|-------------|
| `ToSerializableDictionary(keySelector, valueSelector)` | Build from any `IEnumerable<T>` |
| `WhereDict(predicate)` | Filter entries |
| `SelectValues(selector)` | Project values to a new type |
| `OrderByKey()` | Ascending key sort |
| `OrderByValue()` | Ascending value sort |
| `SumValues()` | Sum of all int or float values |
| `AverageValues()` | Mean of all numeric values |
| `MinValue()` / `MaxValue()` | Min / Max of all values |

### Merge Extensions

| Method | Description |
|--------|-------------|
| `Merge(other)` | Add missing keys from other |
| `MergeOverwrite(other)` | Add / overwrite all keys from other |
| `Intersect(other)` | Keep only keys present in both |
| `Except(other)` | Remove keys present in other |
| `Union(other)` | Combine, source wins on conflicts |
| `Clone()` | Shallow copy |

### Persistence Extensions

| Method | Description |
|--------|-------------|
| `SaveToFile(path)` | Serialize to JSON file |
| `LoadFromFile(path)` | Deserialize from JSON file |
| `ToPlayerPrefs(key)` | Persist to `PlayerPrefs` |
| `FromPlayerPrefs(key)` | Restore from `PlayerPrefs` |

---

## Bonus Collections

| Type | Description |
|------|-------------|
| `SerializableHashSet<T>` | Serializable set — no duplicates |
| `SerializableStack<T>` | Serializable LIFO stack |
| `SerializableQueue<T>` | Serializable FIFO queue |
| `SerializableOrderedDictionary<TKey, TValue>` | Dictionary with preserved insertion order |
| `SerializableBiDictionary<TKey, TValue>` | Bidirectional dictionary — look up by key or value |

All five types:
- Appear in the Unity Inspector with custom drawers
- Support Undo/Redo
- Serialise correctly with `[SerializeField]`

---

## Samples

Import any sample via **Window → Package Manager → Smart Dictionary Pro → Samples**.

| Sample | Description |
|--------|-------------|
| **BasicUsage** | `DemoController.cs` + `GameConfig.cs` — core API walkthrough |
| **LinqAndEvents** | `LinqEventsDemo.cs` — LINQ extensions and event subscriptions |
| **SaveLoad** | `SaveLoadDemo.cs` — JSON file and PlayerPrefs persistence |
| **AdvancedCollections** | `AdvancedCollectionsDemo.cs` — all 5 bonus collection types |

---

## Requirements

| Requirement | Version |
|-------------|---------|
| Unity | **2021.3 LTS** or higher |
| .NET | **.NET Standard 2.1** |
| C# | 9.0+ (included with Unity 2021.3) |

> No third-party dependencies. No unsafe code.

---

## License

This package is released under the **MIT License**.  
See [LICENSE.md](LICENSE.md) for the full text.

Copyright © 2026 [EngineEdge](https://engineedge.dev)
