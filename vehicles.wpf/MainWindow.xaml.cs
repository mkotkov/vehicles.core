using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Vehicles.Core;
using AppRes = vehicles.wpf.Properties.Resources;


namespace vehicles.wpf
{
    public partial class MainWindow : Window
    {
        public ObservableCollection<Vehicle> Vehicles { get; } = new();
        private Vehicle? SelectedVehicle => VehicleList.SelectedItem as Vehicle;
        private int _carCount, _boatCount, _amphibiousCount;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;

            Vehicles.Add(new Car("Volvo", "V60", 120));
            Vehicles.Add(new Boat("Bella", "600"));
            Vehicles.Add(new AmphibiousCar("Amphi", "X"));

            VehicleList.SelectionChanged += VehicleList_SelectionChanged;
            VehicleList.SelectedIndex = 0;
        }

        private void VehicleList_SelectionChanged(object sender, SelectionChangedEventArgs e) => RefreshDetails();

        private void RefreshDetails()
        {
            Vehicle? v = SelectedVehicle;

            DriveButton.Visibility = v is IDriveable ? Visibility.Visible : Visibility.Collapsed;
            SwimButton.Visibility = v is ISwimmable ? Visibility.Visible : Visibility.Collapsed;
            RemoveButton.IsEnabled = v != null;
            DistanceBox.IsEnabled = v != null;

            SelectedText.Text = v == null ? string.Empty
                : string.Format(AppRes.Str_SelectedFormat, v.Name);
            MileageText.Text = v == null ? string.Empty
                : string.Format(AppRes.Str_MileageFormat, v.Odometer);

        }

        private void Run(Func<double, string> action)
        {
            if (!TryReadDistance(out double km))
            {
                ShowError(AppRes.Str_InvalidDistance);
                return;
            }

            try
            {
                string message = action(km);
                ShowResult(message);
            }
            catch (ArgumentException ex)
            {
                ShowError(ex.Message);
            }

            RefreshDetails();
        }


        private bool TryReadDistance(out double km)
        {
            string text = DistanceBox.Text.Trim().Replace(',', '.');
            return double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out km);
        }

        private void ShowResult(string message)
        {
            LogText.Foreground = (Brush)FindResource("TextPrimary");
            LogText.Text = message + Environment.NewLine
                + string.Format(AppRes.Str_MileageFormat, SelectedVehicle!.Odometer);
        }

        private void ShowError(string message)
        {
            LogText.Foreground = (Brush)FindResource("Danger");
            LogText.Text = message;
        }

        private void Drive_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedVehicle is IDriveable d) Run(d.Drive);
        }

        private void Swim_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedVehicle is ISwimmable s) Run(s.Swim);
        }


        private void AddCar_Click(object sender, RoutedEventArgs e)
            => AddAndSelect(new Car(AppRes.Str_NewCar, (++_carCount).ToString()));

        private void AddBoat_Click(object sender, RoutedEventArgs e)
            => AddAndSelect(new Boat(AppRes.Str_NewBoat, (++_boatCount).ToString()));

        private void AddAmphibious_Click(object sender, RoutedEventArgs e)
            => AddAndSelect(new AmphibiousCar(AppRes.Str_NewAmphibious, (++_amphibiousCount).ToString()));

        private void AddAndSelect(Vehicle vehicle)
        {
            Vehicles.Add(vehicle);
            VehicleList.SelectedItem = vehicle;
            VehicleList.ScrollIntoView(vehicle);
        }

        private void Remove_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedVehicle is not Vehicle v) return;

            int index = VehicleList.SelectedIndex;
            Vehicles.Remove(v);
            VehicleList.SelectedIndex = Math.Min(index, Vehicles.Count - 1);
            RefreshDetails();
        }
    }
}