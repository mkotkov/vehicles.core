namespace vehicles.core
{
    public abstract class Vehicle
    {
        public string Make { get; }
        public string Model { get; }
        public double Odometer { get; private set; }

        protected Vehicle(string make, string model, double odometer = 0) { ... }

        public abstract string Move(double km);

        protected void AddDistance(double km)
        {
            if (double.IsNaN(km) || double.IsInfinity(km) || km <= 0)
                throw new ArgumentException("Vahemaa peab olema positiivne arv.");
            Odometer += km;
        }
    }
}
