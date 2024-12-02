using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Threading;

namespace A_Level_Computer_Science_NEA
{
    public partial class Simulation : Window
    {
        private simulation reactorSimulation;
        private DispatcherTimer updateTimer;

        public Simulation()
        {
            InitializeComponent();
            setupTimer();
            updateDisplay();

            try
            {
                reactorSimulation = new simulation();
                Console.WriteLine("Successfully initialised reactorSimulation.");

                sldCoolantFlowRate.ValueChanged += coolantFlowRateChanged;
                rodInsertionSlider.ValueChanged += setRodIncrement;
                //aiCheckbox.Checked += aiControlChecked;
                //aiCheckbox.Unchecked += aiControlUnchecked;
            }

            catch (Exception ex)
            {
                Console.WriteLine("Failed to initialise reactorSimulation: " + ex.Message);
            }
        }

        private void increaseRodInsertion(object sender, RoutedEventArgs e)
        {
            reactorSimulation.core.adjustRods(-reactorSimulation.core.rodIncrement);
            updateDisplay();
        }

        private void decreaseRodInsertion(object sender, RoutedEventArgs e)
        {
            reactorSimulation.core.adjustRods(reactorSimulation.core.rodIncrement);
            updateDisplay();
        }

        private void setRodIncrement(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (reactorSimulation.core != null)
            {
                reactorSimulation.core.rodIncrement = rodInsertionSlider.Value;
            }
        }

        private void startReactorClick(object sender, RoutedEventArgs e)
        {
            reactorSimulation.control.startReactor();
            updateDisplay();
        }

        private void shutdownClick(object sender, RoutedEventArgs e)
        {
            reactorSimulation.control.stopReactor();
            updateDisplay();
        }

        private void refuelClick(object sender, RoutedEventArgs e)
        {
            reactorSimulation.fuel.refuel();
            updateDisplay();
        }

        private void coolantFlowRateChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (reactorSimulation.cooling != null)
            {
                reactorSimulation.cooling.adjustFlowRate(sldCoolantFlowRate.Value);
                updateDisplay();
            }
        }

        // private void aiControlChecked(object sender, RoutedEventArgs e)
        // {
            // if (reactorSimulation.core != null)
            // {
                // reactorSimulation.core.aiControl = true;
            // }
        // }

        //private void aiControlUnchecked(object sender, RoutedEventArgs e)
        //{
            //if (reactorSimulation.core != null)
            //{
                //reactorSimulation.core.aiControl = false;
            //}
        //}

        private void setupTimer()
        {
            updateTimer = new DispatcherTimer();
            updateTimer.Interval = TimeSpan.FromMilliseconds(500);
            updateTimer.Tick += updateTimerTick;
            updateTimer.Start();
        }

        private void updateTimerTick(object sender, EventArgs e)
        {
            reactorSimulation.updateComponents();
            updateDisplay();
        }

        private void updateDisplay()
        {
            if (reactorSimulation != null)
            {
                txtCoreTemperature.Text = $"Core Temperature: {reactorSimulation.core.coreTemperature} °C";
                txtReactorPressure.Text = $"Reactor Pressure: {reactorSimulation.cooling.coolantPressure} bar";
                txtReactivity.Text = $"Reactivity: {reactorSimulation.core.reactivity}";
                txtCoolantTemperature.Text = $"Coolant Temperature: {reactorSimulation.cooling.coolantTemperature} °C";
                txtFuelLevel.Text = $"Fuel Level: {reactorSimulation.fuel.fuelLevel}%";
                txtShutdownStatus.Text = $"Reactor Status: {(reactorSimulation.control.isShutdown ? "Shutdown" : "Operational")}";
                txtPowerOutput.Text = $"Power Output: {reactorSimulation.turbine.powerOutput} MW";
                txtRodInsertionLevel.Text = $"Rod Insertion: {reactorSimulation.core.rodInsertion * 100}%";
            }
            else
            {
                Console.WriteLine("Reactor Simulation is null; display update skipped!"); // Adding because I keep getting stupid null errors
            }
        }
    }
}