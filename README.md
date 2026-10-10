# Vehicles

A small WPF application that demonstrates object-oriented design in C#: an abstract base class, interfaces, polymorphism, events and a thin UI on top of a separate Core library.

Vehicles are split between two garages: a **car garage** and a **boat garage**. The user can add, select and remove vehicles in both garages, enter a distance, run an action the selected vehicle supports, and follow everything that happens in an activity log.

## Core design

| Type | Role |
|---|---|
| `Vehicle` (abstract) | `Make`, `Model` and `Odometer` are read-only from outside. Defines the abstract `Move(double km)` and `Signal()`, and the virtual `BlinkLights()`. |
| `IDriveable` | `Drive(double km)` |
| `ISwimmable` | `Swim(double km)` |
| `Car` | `Vehicle` + `IDriveable` |
| `Boat` | `Vehicle` + `ISwimmable` |
| `AmphibiousCar` | `Vehicle` + `IDriveable` + `ISwimmable` |
| `Garage` | Holds a collection of vehicles, accepts only the vehicles its rule allows, and notifies the others when a vehicle is added or removed. |

Key rules:

- **Encapsulation.** `Odometer` has a private setter. It is changed only by the protected `AddDistance` method inside the class hierarchy.
- **Validation.** `AddDistance` throws `ArgumentException` for zero, negative, `NaN` or infinite values. The odometer is changed only after the check passes, so invalid input never corrupts the object state.
- **Polymorphism.** `Move(km)` behaves differently in each subclass (`Car` drives, `Boat` swims, `AmphibiousCar` drives by default). `Signal()` is also different per type (cars "piip", boats "tuut", amphibious cars "piip-tuut"). Both return a message for the log.
- **Capabilities come from interfaces**, not from concrete types.

### Garages

- Each `Garage` is created with a name and an acceptance rule: the car garage accepts `IDriveable`, the boat garage accepts `ISwimmable`. Because of this, an `AmphibiousCar` can be added to **either** garage, and no check on the concrete type is needed.
- **Adding** a vehicle makes the *other* vehicles in the same garage give a signal (`Signal()`). The new vehicle does not signal itself.
- **Removing** a vehicle makes the *remaining* vehicles in the same garage blink their lights (`BlinkLights()`).
- A garage exposes its vehicles as a `ReadOnlyObservableCollection<Vehicle>`. Changes are possible only through `Add` and `Remove`, so the signals cannot be bypassed.
- Every step (added, removed, signal, blink) is reported through the `Logged` event. The garages do not know anything about the UI.
- Adding a vehicle the garage does not accept, or the same vehicle twice, throws `ArgumentException`.

## UI design

- Two cards show the garages side by side. Each list is bound to its garage's `Vehicles` collection, so added and removed items appear automatically.
- Only one vehicle can be selected at a time. Selecting a vehicle in one garage clears the selection in the other, and the details panel on the right always shows the selected vehicle.
- The event handlers only: read the selection and input, call a Core method, show the result or the error message, refresh the selected vehicle info. They contain no business logic.
- The **Sõida** and **Uju** buttons are shown depending on the interfaces the selected object implements (`is IDriveable`, `is ISwimmable`). There is no large `if/switch` on the concrete vehicle type.
- The activity log is a history: all garage events and action results are appended in chronological order, with a timestamp, and the list scrolls to the newest entry. Errors are shown in red.
- Invalid input (empty, text, `0`, negative) shows an error in the log and leaves the odometer unchanged.
- Both `.` and `,` are accepted as the decimal separator.

## Styling and localization

- Colors, radii, shadows, typography and button/input styles are defined as resource tokens at the top of `MainWindow.xaml` (based on the Product UI Styleguide).
- No UI text is hardcoded in XAML or in the window code. Texts and format strings live in `Properties/Resources.resx` and are accessed with `{x:Static res:Resources.Str_...}` in XAML and `string.Format(Resources.Str_...)` in C#.
- The messages produced by the Core library (signals, blinking, "added/removed") are written inside Core, because Core must not depend on the WPF project and its resources.
- To add another language to the UI, add a `Resources.<culture>.resx` file with the same keys.

## Requirements

- Windows
- .NET SDK matching the solution's target framework
- Visual Studio 2022 (recommended) or the `dotnet` CLI

## Build and run

```bash
dotnet build
dotnet run --project vehicles.wpf
```

Or open `Vehicles.sln` in Visual Studio and press **F5**.

## How to use

1. Select a vehicle in the **Autode garaaž** or **Paatide garaaž** list. The right panel shows its name and odometer.
2. Enter a distance in **Vahemaa**.
3. Press **Sõida** (cars, amphibious cars) or **Uju** (boats, amphibious cars). The result and the new odometer value appear in **Tegevuste logi**.
4. Use **Lisa auto** or **Lisa paat** to add a vehicle to the matching garage, and **Lisa amfiib** to add an amphibious car to either garage. The other vehicles in that garage give a signal.
5. Press **Eemalda** to remove the selected vehicle. The remaining vehicles in the same garage blink their lights.

## Manual test checklist

| Check | Expected result |
|---|---|
| Add a car, a boat and an amphibious car (to both garages) from the UI | Each appears in the right garage |
| Select a car | Only **Sõida** is visible |
| Select a boat | Only **Uju** is visible |
| Select an amphibious car | Both buttons are visible |
| Select a vehicle in the other garage | The previous selection is cleared |
| Add a vehicle to a garage with other vehicles | Log: "added" entry, then a signal from each other vehicle of that garage |
| Add a vehicle to a garage with no other vehicles | Log: only the "added" entry |
| Remove a vehicle | Log: "removed" entry, then a blink from each remaining vehicle of that garage |
| Add or remove in one garage | The other garage's vehicles do not react |
| Drive 10 km with Volvo V60 (odometer 120) | Log: "Volvo sõitis 10 km." and odometer 130 |
| Enter `0`, `-5` or `abc` | Red error in the log, odometer unchanged |

## Possible improvements

- Implement `INotifyPropertyChanged` in `Vehicle` so the odometer updates through binding instead of manual refresh.
- Move the window logic into a ViewModel (MVVM) with commands.
- Add unit tests for the Core library, including the garage rules and signals.
- Move the Core messages into a localizable form (for example, return message codes or use a formatter supplied by the UI).