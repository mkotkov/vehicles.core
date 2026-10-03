using vehicles.core.Properties;

namespace Vehicles.Core;

public class Boat : Vehicle, ISwimmable
{
    public Boat(string make, string model, double odometer = 0)
        : base(make, model, odometer) { }

    public string Swim(double km)
    {
        AddDistance(km);
        return string.Format(Resources.Str_Boat_AddDistance, Make, km);
    }

    public override string Move(double km) => Swim(km);
}