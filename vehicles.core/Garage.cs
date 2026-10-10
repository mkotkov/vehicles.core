using System.Collections.ObjectModel;

namespace Vehicles.Core;

public class Garage
{
    private readonly ObservableCollection<Vehicle> _vehicles = new();
    private readonly Func<Vehicle, bool> _accepts;

    public string Name { get; }

    public ReadOnlyObservableCollection<Vehicle> Vehicles { get; }

    public event Action<string>? Logged;

    public Garage(string name, Func<Vehicle, bool> accepts)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Garaaži nimi ei tohi olla tühi.");

        Name = name;
        _accepts = accepts ?? throw new ArgumentNullException(nameof(accepts));
        Vehicles = new ReadOnlyObservableCollection<Vehicle>(_vehicles);
    }

    public void Add(Vehicle vehicle)
    {
        if (vehicle == null)
            throw new ArgumentNullException(nameof(vehicle));
        if (!_accepts(vehicle))
            throw new ArgumentException($"{Name} ei võta vastu: {vehicle.Name}.");
        if (_vehicles.Contains(vehicle))
            throw new ArgumentException($"{vehicle.Name} on juba garaažis.");

        var others = _vehicles.ToList();
        _vehicles.Add(vehicle);

        Log($"{Name}: lisati {vehicle.Name}.");
        foreach (Vehicle other in others)
            Log(other.Signal());
    }

    public bool Remove(Vehicle vehicle)
    {
        if (vehicle == null || !_vehicles.Remove(vehicle))
            return false;

        Log($"{Name}: eemaldati {vehicle.Name}.");
        foreach (Vehicle remaining in _vehicles)
            Log(remaining.BlinkLights());

        return true;
    }

    private void Log(string message) => Logged?.Invoke(message);
}