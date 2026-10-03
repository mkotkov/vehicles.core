# Vehicles

A small WPF application that demonstrates object-oriented design in C#: an abstract base class, interfaces, polymorphism and a thin UI on top of a separate Core library.

The user can add and select vehicles, enter a distance, run an action the selected vehicle supports, and see the result together with the new odometer value.

## Core design

| Type | Role |
|---|---|
| `Vehicle` (abstract) | `Make`, `Model` and `Odometer` are read-only from outside. Defines the abstract `Move(double km)`. |
| `IDriveable` | `Drive(double km)` |
| `ISwimmable` | `Swim(double km)` |
| `Car` | `Vehicle` + `IDriveable` |
| `Boat` | `Vehicle` + `ISwimmable` |
| `AmphibiousCar` | `Vehicle` + `IDriveable` + `ISwimmable` |

Key rules:

- **Encapsulation.** `Odometer` has a private setter. It is changed only by the protected `AddDistance` method inside the class hierarchy.
- **Validation.** `AddDistance` throws `ArgumentException` for zero, negative, `NaN` or infinite values. The odometer is changed only after the check passes, so invalid input never corrupts the object state.
- **Polymorphism.** `Move(km)` behaves differently in each subclass (`Car` drives, `Boat` swims, `AmphibiousCar` drives by default) and returns a message for the log.
- **Capabilities come from interfaces**, not from concrete types.

## UI design

- All vehicles live in one `ObservableCollection<Vehicle>`, bound to the list with `ItemsSource`, so added and removed items appear automatically.
- The event handlers only: read the selection and input, call a Core method, show the result or the error message, refresh the selected vehicle info. They contain no business logic.
- The **Sõida** and **Uju** buttons are shown depending on the interfaces the selected object implements (`is IDriveable`, `is ISwimmable`). There is no large `if/switch` on the concrete vehicle type.
- Invalid input (empty, text, `0`, negative) shows an error in the activity log and leaves the odometer unchanged.
- Both `.` and `,` are accepted as the decimal separator.

## Styling and localization

- Colors, radii, shadows, typography and button/input styles are defined as resource tokens at the top of `MainWindow.xaml` (based on the Product UI Styleguide).
- No UI text is hardcoded in XAML or code. Texts and format strings live in `Properties/Resources.resx` and are accessed with `{x:Static res:Resources.Str_...}` in XAML and `string.Format(Resources.Str_...)` in C#.
- To add another language, add a `Resources.<culture>.resx` file with the same keys.

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

1. Select a vehicle in the **Sõidukid** list. The right panel shows its name and odometer.
2. Enter a distance in **Vahemaa**.
3. Press **Sõida** (cars, amphibious cars) or **Uju** (boats, amphibious cars). The result and the new odometer value appear in **Tegevuste logi**.
4. Use **Lisa auto**, **Lisa paat** or **Lisa amfiib** to add a new vehicle, and **Eemalda** to remove the selected one.

## Manual test checklist

| Check | Expected result |
|---|---|
| Add a car, a boat and an amphibious car from the UI | All appear in the same list |
| Select a car | Only **Sõida** is visible |
| Select a boat | Only **Uju** is visible |
| Select an amphibious car | Both buttons are visible |
| Drive 10 km with Volvo V60 (odometer 120) | Log: "Volvo sõitis 10 km." and odometer 130 |
| Enter `0`, `-5` or `abc` | Error in the log, odometer unchanged |
| Remove the selected vehicle | It disappears and a neighbor is selected |

## Possible improvements

- Implement `INotifyPropertyChanged` in `Vehicle` so the odometer updates through binding instead of manual refresh.
- Move the window logic into a ViewModel (MVVM) with commands.
- Add unit tests for the Core library.