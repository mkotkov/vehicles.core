using vehicles.core.Properties;

namespace Vehicles.Core;

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
}