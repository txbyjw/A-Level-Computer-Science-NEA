using System.Windows;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace A_Level_Computer_Science_NEA
{
    public partial class Menu : Window
    {
        private List<string> scenarios;
        private Dictionary<string, string> scenarioDescriptions;
        private int currentScenarioIndex = 0;

        public Menu()
        {
            InitializeComponent();
            scenarios = new List<string>
            {
                "Scenario 1: Power Surge",
                "Scenario 2: Coolant Failure",
                "Scenario 3: Full Power Test",
                "Scenario 4: Free Mode"
            };

            scenarioDescriptions = new Dictionary<string, string>
            {
                { "Scenario 1: Power Surge", "A sudden power surge has caused an instability in the reactor, which may lead to a potential shutdown. Ensure that all systems are functioning properly and the reactor is safe." },
                { "Scenario 2: Coolant Failure", "A fault has occured in the cooling system, causing the core temperature to rise. Safely control the reactor to prevent a meltdown." },
                { "Scenario 3: Full Power Test", "Test the reactor under full power conditions. Ensure you can hold the reactor in this maximum output state without any failures." },
                { "Scenario 4: Free Mode", "The reactor starts in the shut-down state, and you can do whatever you like with no goals or restrictions!" }
            };

            updateScenario();

            SettingsData settingsData = SettingsData.Load();
            SettingsManager.applySettings(settingsData, this);
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
            scenarioDescription.Text = scenarioDescriptions[scenarios[currentScenarioIndex]];
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

        private void quit(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}