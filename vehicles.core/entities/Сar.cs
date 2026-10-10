using vehicles.core.interfaces;
using vehicles.core.Properties;
using Vehicles.Core;

namespace vehicles.core.entities;

public class Car : Vehicle, IDriveable
{
    public Car(string make, string model, double odometer = 0)
        : base(make, model, odometer) { }

    public string Drive(double km)
    {
        AddDistance(km);
        return string.Format(Resources.Str_Car_AddDistance, Make, km);
    }

    public override string Move(double km) => Drive(km);

    public override string Signal() => string.Format(Resources.Str_Car_Signal, Name);
}