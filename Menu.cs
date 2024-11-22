using System.Windows;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace A_Level_Computer_Science_NEA
{
    public partial class Menu : Window
    {
        private List<string> scenarios;
        private int currentScenarioIndex = 0;

        public Menu()
        {
            InitializeComponent();
            scenarios = new List<string> { "Scenario 1", "Scenario 2", "Scenario 3" }; // Example scenarios
            updateScenario();
        }

        private void previousScenario(object sender, RoutedEventArgs e)
        {
            currentScenarioIndex = (currentScenarioIndex == 0) ? scenarios.Count - 1 : currentScenarioIndex - 1;
            updateScenario();
        }

        private void nextScenario(object sender, RoutedEventArgs e)
        {
            currentScenarioIndex = (currentScenarioIndex == scenarios.Count - 1) ? 0 : currentScenarioIndex + 1;
            updateScenario();
        }

        private void updateScenario()
        {
            scenarioSelector.Text = scenarios[currentScenarioIndex];
            scenarioDescription.Text = "Description for " + scenarios[currentScenarioIndex];
        }

        private void beginSimulation(object sender, RoutedEventArgs e)
        {
            Simulation mainWindow = new Simulation();
            mainWindow.Show();
            this.Close();
        }

        private void openSettings(object sender, RoutedEventArgs e)
        {
            Settings settingsWindow = new Settings(this);
            settingsWindow.ShowDialog();
        }
    }
}