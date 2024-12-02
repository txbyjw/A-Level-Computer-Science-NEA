using System;
using System.Windows;

namespace A_Level_Computer_Science_NEA
{
    public class simulation
    {
        public neutronics core { get; private set; }
        public coolingSystem cooling { get; private set; }
        public fuelSystem fuel { get; private set; }
        public steamGenerator steamGen { get; private set; }
        public turbine turbine { get; private set; }
        public controlSystem control { get; private set; }

        public simulation()
        {
            try
            {
                core = new neutronics();
                cooling = new coolingSystem();
                fuel = new fuelSystem();
                steamGen = new steamGenerator();
                turbine = new turbine();
                control = new controlSystem(core, cooling, fuel, steamGen, turbine);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error initialising components: {ex.Message}.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Console.WriteLine("Error initialising the following components: " + ex.Message);
            }
        }

        public void updateComponents()
        {
            if (control.isShutdown)
            {
                Console.WriteLine("The reactor is shut down.");
                MessageBox.Show($"Cannot perform this action when the reactor is shutdown.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            double reactivityAdjustment = control.core.neutronFlux * 0.1;
            control.core.adjustRods(-reactivityAdjustment);

            double newCoreTemperature = calculateCoreTemperature(core.neutronFlux, fuel.fuelLevel, cooling.coolantTemperature, core.rodInsertion);
            core.updateCoreTemperature(newCoreTemperature);

            double newCoolantTemperature = calculateCoolantTemperature(control.core.neutronFlux, cooling.coolantFlowRate);
            cooling.updateCoolantTemperature(newCoolantTemperature);

            cooling.updateCoolantPressure(cooling.coolantFlowRate, steamGen.steamPressure);
            turbine.generatePower(steamGen.steamPressure);
            fuel.depleteFuel(turbine.powerOutput);

            Console.WriteLine($"Neutron Flux: {control.core.neutronFlux}, Coolant Temperature: {cooling.coolantTemperature}, Power Output: {turbine.powerOutput}");
        }

        private double calculateCoreTemperature(double neutronFlux, double fuelLevel, double coolantTemperature, double rodInsertion)
        {
            double fluxEffect = neutronFlux * 5;
            double fuelEffect = (fuelLevel / 100) * 50;
            double coolantEffect = coolantTemperature * 0.1;
            double rodEffect = (1 - rodInsertion) * 10;

            double temp = core.coreTemperature + fluxEffect + fuelEffect - coolantEffect - rodEffect;
            return Math.Clamp(temp, 0, 350);
        }

        private double calculateCoolantTemperature(double coreTemperature, double flowRate)
        {
            double temperatureIncrease = coreTemperature * 0.8;
            double flowEffect = flowRate * 0.5;
            return Math.Clamp(temperatureIncrease - flowEffect, 0, 350);
        }
    }

    public class neutronics
    {
        public double neutronFlux { get; private set; }
        public double coreTemperature { get; private set; }
        public double reactivity { get; private set; }
        public double rodInsertion { get; private set; } = 0.5;
        public double rodIncrement { get; set; }

        private const double maxNeutronFlux = 100.0;

        public neutronics()
        {
            neutronFlux = 50.0;
            coreTemperature = 280;
            reactivity = 0.0;
        }

        public void adjustRods(double positionChange)
        {
            rodInsertion = Math.Clamp(rodInsertion + positionChange, 0, 1);

            if (rodInsertion < 0.5)
            {
                neutronFlux = Math.Min(maxNeutronFlux, neutronFlux + (1 - rodInsertion) * (maxNeutronFlux * 2));
            }
            else
            {
                neutronFlux = Math.Max(0, neutronFlux - (rodInsertion * (maxNeutronFlux / 2)));
            }

            reactivity = neutronFlux - 50;
        }

        public void updateCoreTemperature(double temperature)
        {
            coreTemperature = Math.Clamp(temperature, 0, 350);
        }
    }

    public class coolingSystem
    {
        public double coolantTemperature { get; private set; }
        public double coolantPressure { get; private set; }
        public double coolantFlowRate { get; set; }

        private const double maxTemperature = 350.0;
        private const double minFlowRate = 0.0;
        private const double maxFlowRate = 10.0;

        public coolingSystem()
        {
            coolantTemperature = 20.0;
            coolantFlowRate = 5.0;
            coolantPressure = 1.0;
        }

        public void updateCoolantTemperature(double temperature)
        {
            coolantTemperature = Math.Clamp(temperature, 0, maxTemperature);
        }

        public void updateCoolantPressure(double flowRate, double steamPressure)
        {
            coolantPressure = flowRate * 0.1 + steamPressure;
        }

        public void adjustFlowRate(double adjustment)
        {
            coolantFlowRate += adjustment;
            coolantFlowRate = Math.Clamp(coolantFlowRate, minFlowRate, maxFlowRate);
        }
    }

    public class fuelSystem
    {
        public double fuelLevel { get; private set; }

        private const double depletionRate = 0.01;

        public fuelSystem()
        {
            fuelLevel = 100.0;
        }

        public void depleteFuel(double powerOutput)
        {
            if (powerOutput > 0)
            {
                fuelLevel -= depletionRate * powerOutput / 100;
            }

            fuelLevel = Math.Max(fuelLevel, 0);
        }

        public void refuel()
        {
            fuelLevel = 100.0;
        }
    }

    public class steamGenerator
    {
        public double steamTemperature { get; private set; }
        public double steamPressure { get; private set; }

        private const double maxSteamPressure = 150.0;

        public void generateSteam(double coolantTemperature)
        {
            steamPressure = Math.Clamp(coolantTemperature * 0.8, 0, maxSteamPressure);
            this.steamTemperature = coolantTemperature * 0.5;
        }
    }

    public class turbine
    {
        public double powerOutput { get; private set; }

        public void generatePower(double steamPressure)
        {
            powerOutput = steamPressure * 10;
            if (powerOutput > 0)
            {
                steamPressure = Math.Max(steamPressure - (powerOutput * 0.01), 0);
            }
        }
    }

    public class controlSystem
    {
        public bool isShutdown { get; private set; }

        public neutronics core { get; private set; }
        private coolingSystem cooling;
        private fuelSystem fuel;
        private steamGenerator steam;
        private turbine turbine;

        public controlSystem(neutronics core, coolingSystem cooling, fuelSystem fuel, steamGenerator steam, turbine turbine)
        {
            this.core = core;
            this.cooling = cooling;
            this.fuel = fuel;
            this.steam = steam;
            this.turbine = turbine;
        }

        public void startReactor()
        {
            isShutdown = false;
        }

        public void stopReactor()
        {
            isShutdown = true;
        }
    }
}