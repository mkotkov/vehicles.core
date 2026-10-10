using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using vehicles.core.entities;
using vehicles.core.interfaces;
using Vehicles.Core;
using AppRes = vehicles.wpf.Properties.Resources;


namespace vehicles.wpf
{
    /// <summary>A single log entry. IsError controls the text color in XAML.</summary>
    public sealed record LogEntry(string Text, bool IsError);

    public partial class MainWindow : Window
    {
        public Garage CarGarage { get; }
        public Garage BoatGarage { get; }
        public ObservableCollection<LogEntry> LogEntries { get; } = new();

        // The selection can exist in only one of the two lists at a time
        private Vehicle? SelectedVehicle =>
            (CarList.SelectedItem ?? BoatList.SelectedItem) as Vehicle;

        private int _carCount, _boatCount, _amphibiousCount;

        public MainWindow()
        {
            // The garage rule is defined by an interface, not by a concrete class
            CarGarage = new Garage(AppRes.Str_CarGarage, v => v is IDriveable);
            BoatGarage = new Garage(AppRes.Str_BoatGarage, v => v is ISwimmable);

            // Initial data is added before subscribing to Logged, so the log starts empty
            CarGarage.Add(new Car("Volvo", "V60", 120));
            CarGarage.Add(new AmphibiousCar("Amphi", "X"));
            BoatGarage.Add(new Boat("Bella", "600"));

            CarGarage.Logged += AddLog;
            BoatGarage.Logged += AddLog;

            InitializeComponent();
            DataContext = this;

            CarList.SelectedIndex = 0;
        }

        // ---------- Selection (one selected vehicle across both garages) ----------

        private void CarList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Clear the other list's selection so the details panel always shows one object
            if (CarList.SelectedItem != null) BoatList.SelectedIndex = -1;
            RefreshDetails();
        }

        private void BoatList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (BoatList.SelectedItem != null) CarList.SelectedIndex = -1;
            RefreshDetails();
        }

        private void RefreshDetails()
        {
            Vehicle? v = SelectedVehicle;

            // Button visibility depends on interfaces, not on the concrete class
            DriveButton.Visibility = v is IDriveable ? Visibility.Visible : Visibility.Collapsed;
            SwimButton.Visibility = v is ISwimmable ? Visibility.Visible : Visibility.Collapsed;
            RemoveButton.IsEnabled = v != null;
            DistanceBox.IsEnabled = v != null;

            SelectedText.Text = v == null ? string.Empty
                : string.Format(AppRes.Str_SelectedFormat, v.Name);
            MileageText.Text = v == null ? string.Empty
                : string.Format(AppRes.Str_MileageFormat, v.Odometer);
        }

        // ---------- Actions ----------

        private void Drive_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedVehicle is IDriveable d) Run(d.Drive);
        }

        private void Swim_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedVehicle is ISwimmable s) Run(s.Swim);
        }

        // 1. read the input  2. call the Core method  3. show the result or the error  4. refresh the info
        private void Run(Func<double, string> action)
        {
            Vehicle? v = SelectedVehicle;
            if (v == null) return;

            if (!TryReadDistance(out double km))
            {
                AddError(AppRes.Str_InvalidDistance);
                return;
            }

            try
            {
                string message = action(km);
                AddLog(message + " " + string.Format(AppRes.Str_MileageFormat, v.Odometer));
            }
            catch (ArgumentException ex)
            {
                AddError(ex.Message);
            }

            RefreshDetails();
        }

        private bool TryReadDistance(out double km)
        {
            // Both comma and dot are accepted as the decimal separator
            string text = DistanceBox.Text.Trim().Replace(',', '.');
            return double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out km);
        }

        // ---------- Adding ----------

        private void AddCar_Click(object sender, RoutedEventArgs e)
            => AddTo(CarGarage, CarList,
                new Car(AppRes.Str_NewCar, (++_carCount).ToString()));

        private void AddBoat_Click(object sender, RoutedEventArgs e)
            => AddTo(BoatGarage, BoatList,
                new Boat(AppRes.Str_NewBoat, (++_boatCount).ToString()));

        private void AddAmphibiousToCars_Click(object sender, RoutedEventArgs e)
            => AddTo(CarGarage, CarList,
                new AmphibiousCar(AppRes.Str_NewAmphibious, (++_amphibiousCount).ToString()));

        private void AddAmphibiousToBoats_Click(object sender, RoutedEventArgs e)
            => AddTo(BoatGarage, BoatList,
                new AmphibiousCar(AppRes.Str_NewAmphibious, (++_amphibiousCount).ToString()));

        private void AddTo(Garage garage, ListBox list, Vehicle vehicle)
        {
            try
            {
                garage.Add(vehicle);   // the garage itself makes the other vehicles signal and writes to the log
                list.SelectedItem = vehicle;
                list.ScrollIntoView(vehicle);
            }
            catch (ArgumentException ex)
            {
                AddError(ex.Message);
            }
        }

        // ---------- Removing ----------

        private void Remove_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedVehicle is not Vehicle v) return;

            // Find out which garage the selected vehicle belongs to
            bool inCars = CarList.SelectedItem == v;
            Garage garage = inCars ? CarGarage : BoatGarage;
            ListBox list = inCars ? CarList : BoatList;

            int index = list.SelectedIndex;
            garage.Remove(v);      // the garage itself makes the remaining vehicles blink their lights

            // Select a neighbor so the details panel is not left without a selection
            if (garage.Vehicles.Count > 0)
                list.SelectedIndex = Math.Min(index, garage.Vehicles.Count - 1);

            RefreshDetails();
        }

        // ---------- Log ----------

        private void AddLog(string message) => Append(message, isError: false);

        private void AddError(string message)
            => Append(string.Format(AppRes.Str_ErrorFormat, message), isError: true);

        private void Append(string message, bool isError)
        {
            var entry = new LogEntry(
                string.Format(AppRes.Str_LogEntryFormat, DateTime.Now, message), isError);

            LogEntries.Add(entry);
            LogList.ScrollIntoView(entry);   // new entries go to the bottom, the list scrolls to the latest
        }
    }
}