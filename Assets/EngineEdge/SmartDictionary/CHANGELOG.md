# Changelog

All notable changes to **Smart Dictionary Pro** will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

---

## [1.0.0] — 2026-09-05

### Added

#### Core — `SerializableDictionary<TKey, TValue>`
- Full `IDictionary<TKey, TValue>` implementation backed by a serializable list of key-value pairs.
- Lightweight, zero-overhead base collection with no event delegates or serialized UnityEvent allocations.
- Custom Unity property drawer with collapsible foldout and entry-count label.
- Case-insensitive search / filter bar in the Inspector.
- Two-column (key | value) table layout with alternating row colours.
- Per-row duplicate-key detection with ⚠ warning icon.
- Per-row remove (−) button with `Undo.RecordObject` support.
- Add (+) button at the bottom of the list.
- Automatic sync between the serialized list and the runtime `Dictionary<TKey, TValue>` on `OnBeforeSerialize` / `OnAfterDeserialize`.
- `TryAdd(key, value)` — returns `false` on duplicate, never throws.
- `TryRemove(key)` — returns `false` if absent, never throws.
- `GetOrDefault(key, defaultValue)` — safe read with a compile-time fallback.
- `GetOrAdd(key, factory)` — factory only called when key is absent.
- `AddOrUpdate(key, value)` — insert-or-replace in a single call.
- `FindKey(predicate)` — first key matching `(k, v) => bool`.
- `FindValue(predicate)` — first value matching `(k, v) => bool`.
- `FindAllByValue(predicate)` — all keys whose values satisfy the predicate.
- `Filter(predicate)` — returns a new dictionary with only matching entries.
- `Map(selector)` — returns a new dictionary with projected values.
- `foreach` deconstruct syntax: `foreach (var (k, v) in dict)`.

#### Reactive Collections — `ObservableDictionary<TKey, TValue>` & `ObservableHashSet<T>`
- Inherits from `SerializableDictionary` / `SerializableHashSet` with zero code duplication.
- C# `Action` events: `OnEntryAdded`, `OnEntryRemoved`, `OnEntryUpdated`, `OnCleared`, `OnCountChanged`.
- Serialized UnityEvents configurable in the Inspector (`OnEntryAddedEvent`, etc.).
- Modern Inspector Events toolbar at the bottom of the collection with `Active` / `Muted` pill toggle.
- `EventsEnabled` property to toggle all events at runtime.

#### LINQ Extensions (`SmartDictionaryLinqExtensions`)
- `ToSerializableDictionary(keySelector, valueSelector)` — build from `IEnumerable<T>`.
- `WhereDict(predicate)` — filter entries.
- `SelectValues(selector)` — project values to a new type.
- `OrderByKey()` — ascending key sort.
- `OrderByValue()` — ascending value sort.
- `SumValues()` — sum of all `int` values.
- `SumValues()` — sum of all `float` values.
- `AverageValues()` — arithmetic mean.
- `MinValue()` — minimum value.
- `MaxValue()` — maximum value.

#### Merge / Set Operations (`SmartDictionaryMergeExtensions`)
- `Merge(other)` — adds keys from `other` that are absent in the source.
- `MergeOverwrite(other)` — adds or overwrites all keys from `other`.
- `Intersect(other)` — new dictionary with only keys present in both.
- `Except(other)` — new dictionary with keys from `other` removed.
- `Union(other)` — combines both, source value wins on conflict.
- `Clone()` — shallow copy of the dictionary.

#### Persistence (`SmartDictionarySerializationExtensions`)
- `SaveToFile(path)` — serializes to a UTF-8 JSON file.
- `LoadFromFile(path)` — deserializes from a UTF-8 JSON file.
- `ToPlayerPrefs(prefsKey)` — stores as a JSON string in `PlayerPrefs`.
- `FromPlayerPrefs(prefsKey)` — restores from a `PlayerPrefs` JSON string.

#### Bonus Collections
- `SerializableHashSet<T>` — serializable set with duplicate rejection and custom drawer.
- `SerializableStack<T>` — serializable LIFO stack with Push / Pop / Peek.
- `SerializableQueue<T>` — serializable FIFO queue with Enqueue / Dequeue / Peek.
- `SerializableOrderedDictionary<TKey, TValue>` — insertion-order-preserving dictionary.
- `SerializableBiDictionary<TKey, TValue>` — bidirectional lookup by key or value.

#### Editor
- `InspectorStyles` — lazily-initialised `GUIStyle` cache (header, row alternation, add/remove buttons, search field, warning icon).
- `SerializableDictionaryDrawer` — full property drawer (foldout, search, two-column table, alternating rows, dupe detection, remove button, add button, Undo).
- `SerializableHashSetDrawer` — single-column property drawer with dupe detection, remove, add, Undo.
- `EngineEdge.SmartDictionary.Editor` assembly definition (Editor-only, references runtime asmdef).

#### Interactive Example & Runtime Suite
- Modular, dedicated example scripts in `Example/`: `DictionaryExample.cs`, `HashSetExample.cs`, `OrderedDictionaryExample.cs`, `BiDictionaryExample.cs`, `StackAndQueueExample.cs`, and `NestedStructuresExample.cs`.
- `SmartDictionaryExample.cs` + `SampleScene.unity` — comprehensive interactive testing suite with on-screen runtime GUI, tabbed navigation, live mutation log, and models demonstrating primitives, custom classes, custom structs, nested structures, and 6-layer deep keys.

#### Samples
- **BasicUsage** — `DemoController.cs` (MonoBehaviour) + `GameConfig.cs` (ScriptableObject).
- **LinqAndEvents** — `LinqEventsDemo.cs` (MonoBehaviour).
- **SaveLoad** — `SaveLoadDemo.cs` (MonoBehaviour) with OnGUI buttons.
- **AdvancedCollections** — `AdvancedCollectionsDemo.cs` (MonoBehaviour) showcasing all five bonus types.

#### Package
- UPM-compatible `package.json` manifest (`com.engineedge.smartdictionary`, v1.0.0).
- `README.md` with installation guide, quick-start examples, full API table, and sample descriptions.
- `CHANGELOG.md` (this file).
- `LICENSE.md` (MIT).

---

[1.0.0]: https://github.com/engineedge/smart-dictionary/releases/tag/v1.0.0
