using vehicles.core.Properties;

namespace Vehicles.Core;

public abstract class Vehicle
{
    public string Make { get; }
    public string Model { get; }
    public double Odometer { get; private set; } 

    public string Name => string.Format(Resources.Str_Vehicle_Name, Make, Model);

    protected Vehicle(string make, string model, double odometer = 0)
    {
        if (string.IsNullOrWhiteSpace(make))
            throw new ArgumentException(Resources.Err_Make_IsNullOrWhiteSpace, nameof(make));
        if (string.IsNullOrWhiteSpace(model))
            throw new ArgumentException(Resources.Err_Model_IsNullOrWhiteSpace, nameof(model));
        if (double.IsNaN(odometer) || double.IsInfinity(odometer) || odometer < 0)
            throw new ArgumentException(Resources.Err_Negativ_Odometer, nameof(odometer));

        Make = make;
        Model = model;
        Odometer = odometer;
    }

    public abstract string Move(double km);

    protected void AddDistance(double km)
    {
        if (double.IsNaN(km) || double.IsInfinity(km) || km <= 0)
            throw new ArgumentException(Resources.Err_Positive_Distance, nameof(km));

        Odometer += km;
    }
}