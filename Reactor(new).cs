using System;
using System.Windows;
using System.Windows.Navigation;
using System.Windows.Threading;
using Microsoft.Win32;

namespace A_Level_Computer_Science_NEA
{
    // Holds the "clock" to repeatedly call updates.
    public class Reactor 
    {
        private DispatcherTimer timer;
        public double timeElapsed = 0;
        public event Action dangerStateChanged;

        public UI ui = new UI(this);
        public Core core = new Core();
        public Pressuriser pressuriser = new Pressuriser();
        public PrimaryCoolingLoop primaryCoolingLoop = new PrimaryCoolingLoop();
        public SecondaryCoolingLoop secondaryCoolingLoop = new SecondaryCoolingLoop();
        public PowerGeneration powerGeneration = new PowerGeneration(this);

        public Reactor(UI ui)
        {
            this.ui = ui;
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(100); // Temporarily, this will be user-set later.
            timer.Tick += (sender, e) => updateSimulation();
            dangerStateChanged += ui.updateUI;
            timer.Start();
        }

        private void updateSimulation() // Called every 100ms to update each class and their attributes.
        {
            core.updateCore(timeElapsed);
            pressuriser.updatePressuriser(timeElapsed);
            primaryCoolingLoop.updatePrimaryCoolingLoop(core.getTemperature, timeElapsed);
            secondaryCoolingLoop.updateSecondaryCoolingLoop(primaryCoolingLoop.getCoolantTemperature, timeElapsed);
            powerGeneration.updatePowerGeneration(timeElapsed);
            checkIndicators();

            timeElapsed += 0.1;
        }

        private void checkIndicators()
        {
            bool previousState = ui.isDanger;
            ui.isDanger = core.getTemperature > 350 || core.getPressure > 165 || core.getCoolantFlow < 250 || core.getReactivity > 95 || core.getIntegrity < 30;

            if (ui.isDanger != previousState)
            {
                dangerStateChanged?.Invoke();
            }
        }
    }

    public class UI
    {
        // Indicators
        private bool criticalMass = true;
        private bool reactive = true;
        private bool danger = false;
        private bool overheating = false;
        private bool criticalOverheating = false;
        private bool operational = true;

        public bool isCriticalMass => criticalMass;
        public bool isReactive => reactive;
        public bool isDanger => danger;
        public bool isOverheating => overheating;
        public bool isCriticalOverheating => criticalOverheating;
        public bool isOperational => operational;

        // Switches
        private bool shutdown = false;

        public bool isShutdown => shutdown;

        Reactor reactor = new Reactor();

        public UI(Reactor reactor)
        {
            {
                this.reactor = reactor;
                this.reactor.dangerStateChanged += updateUI;
            }
        }

        public void updateUI()
        {
            // Do later
        }
    }

    // Main class, contains most of the calculations.
    public class Core
    {
        // Properties (Encapsulated)
        private double temperature = 330.0; // deg c
        private double pressure = 155.0; // bar
        private double coolantFlow = 400.0; // l min
        private double reactivity = 50.0; // %
        private double integrity = 100.0; // %
        private double fuelIntegrity = 100.0; // %

        public double getTemperature() => temperature;
        public double getPressure() => pressure;
        public double getCoolantFlow() => coolantFlow;
        public double getReactivity() => reactivity;
        public double getIntegrity() => integrity;
        public double getFuelIntegrity() => fuelIntegrity;

        // Properties (Control Rods) - Unsure as to whether or not these will be used yet.
        private double rodTemperature = 330.0; // deg c
        private double rodIntegrity = 100.0; // %, possibly irrelevant as likely will just use same integrity as the core.

        public double getRodTemperature() => rodTemperature;
        public double getRodIntegrity() => rodIntegrity;

        // Indicators and Switches
        private bool operatingPowerIndicator = true;
        private bool dangerIndicator = false;
        private bool overheatingIndicator = false;
        private bool criticalOverheatingIndicator = false;
        private bool criticalMassIndicator = true;
        private bool reactiveIndicator = true;
        private bool emergencyShutdownSwitch = false;

        public bool isOperatingPowerIndicator() => operatingPowerIndicator;
        public bool isDangerIndicator() => dangerIndicator;
        public bool isCriticalMassIndicator() => criticalMassIndicator;
        public bool isReactiveIndicator() => reactiveIndicator;
        public bool isEmergencyShutdownSwitch() => emergencyShutdownSwitch;

        // Constants
        private const double pressureTemperatureCoefficient = 0.5; // This is the (approximate) pressure increase per deg c above 300 degrees.

        public double getPTCoefficient() => pressureTemperatureCoefficient;

        // Reference Classes
        private Pressuriser pressuriser = new Pressuriser();
        private PrimaryCoolingLoop primaryCoolingLoop = new PrimaryCoolingLoop();
        private SecondaryCoolingLoop secondaryCoolingLoop = new SecondaryCoolingLoop();
        private PowerGeneration powerGeneration = new PowerGeneration();

        public Pressuriser GetPressuriser() => pressuriser;

        public class ControlRods
        {
            private double insertionLevel = 50;
            private double neutronAbsorbtionRate = 50;

            public double getInsertionLevel => insertionLevel;
            public double getNeutronAbsorbtionRate => neutronAbsorbtionRate;

            public void adjustInsertion(double adjustment)
            {
                insertionLevel = Math.Clamp(insertionLevel + adjustment, 0, 100);
                neutronAbsorbtionRate = Math.Pow(insertionLevel / 100, 2) * 100;
            }
        }

        public ControlRods controlRods = new ControlRods();

        public void updateCore(double timeElapsed)
        {
            calculateReactivity(controlRods.getNeutronAbsorbtionRate, coolantModerationFactor, getFuelIntegrity, getTemperature, timeElapsed);
            calculateTemperature(getReactivity, primaryCoolingLoop.getCoolantFlow, primaryCoolingLoop.getCoolantTemperature, powerGeneration.getThermalEfficiency, powerGeneration.getPowerOutput);
            calculatePressure(getTemperature, primaryCoolingLoop.getCoolantFlow, powerGeneration.getPowerOutput, getPTCoefficient);
            calculateIntegrity(getTemperature, getPressure, timeElapsed);
        }

        public void calculateReactivity(double neutronAbsorbtionRate, double coolantModerationFactor, double fuelIntegrity, double temperature, double timeElapsed)
        {
            double controlRodEffect = 1 - (neutronAbsorbtionRate / 100);
            double coolantEffect = coolantModerationFactor * (1 - 0.01 * (temperature - 300)); // More simple calculation for the effect of coolant, should still provide realistic results.
            double fuelEffect = fuelIntegrity * (1 - 0.005 * (timeElapsed)); // Always decreasing over time.

            double tempReactivity = reactivity - controlRodEffect * coolantEffect * fuelEffect;

            if (tempReactivity < 0 || tempReactivity > 100)
            {
                MessageBox.Show($"Reactivity out of range. Calculated reactivity was {tempReactivity}%, clamped value to be within range. (0% - 100%)", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            reactivity = Math.Clamp(tempReactivity, 0, 100);

            reactiveIndicator = reactivity > 0;
            criticalMassIndicator = reactivity >= 70;

            if (!dangerIndicator) // Checks to see if danger indicator is already lit, don't want it to override with a false value if there is danger.
            {
                dangerIndicator = reactivity >= 98;
            }
        }

        public void calculateTemperature(double reactivity, double coolantFlow, double coolantTemperature, double thermalConductivity, double powerOutput)
        {
            double heatGenerated = powerOutput * reactivity / 100;
            double coolantEffect = coolantFlow * (coolantTemperature - 300);
            double heatTransferred = thermalConductivity * (heatGenerated - coolantEffect);
            double tempTemperature = temperature + (heatGenerated - heatTransferred) * 0.01;

            if (tempTemperature < 15 || tempTemperature > 500)
            {
                MessageBox.Show($"Temperature out of range. Calculated temperature was {tempTemperature}°C, clamped value to be within range (15°C - 500°C)", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            temperature = Math.Clamp(tempTemperature, 15, 500);

            if (!dangerIndicator)
            {
                dangerIndicator = temperature >= 340;
            }

            overheatingIndicator = temperature >= 340;
            criticalOverheatingIndicator = temperature >= 400;
        }

        public void calculatePressure(double temperature, double coolantFlow, double powerOutput, double pressureTemperatureCoefficient, double timeElapsed)
        {
            pressuriser.updatePressuriser(timeElapsed);
            double tempPressure = pressuriser.getPressure;

            tempPressure = getPressure + (temperature - 300) * pressureTemperatureCoefficient;

            double powerEffect = powerOutput * 0.05;
            double coolantEffect = coolantFlow * 0.01;

            tempPressure += powerEffect;
            tempPressure -= coolantEffect;

            if (tempPressure < 1 || tempPressure > 180)
            {
                MessageBox.Show($"Pressure out of range. Calculated pressure was {tempPressure} bar, clamped value to be within range (1 bar - 180 bar)", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            pressure = Math.Clamp(tempPressure, 1, 180);

            if (!dangerIndicator)
            {
                dangerIndicator = pressure > 160;
            }
        }

        public void calculateIntegrity(double temperature, double pressure, double timeElapsed)
        {
            double tempIntegrity = getIntegrity;

            if (temperature >= 350 || pressure >= 165) // 10 degrees / bar over where warnings start
            {
                double overTemperature = temperature - 350;
                double overPressure = pressure - 165;

                dangerIndicator = true;

                if (overTemperature < 0)
                {
                    tempIntegrity -= 0.01 * overTemperature;
                }
                else if (overPressure < 0)
                {
                    tempIntegrity -= 0.1 * overPressure;
                }
            }
            else
            {
                tempIntegrity -= 0.005 * timeElapsed; // Natural degradation, takes just over 5.5 hours to decay to 0% naturally so should be good. May reduce though.
                
                if (!dangerIndicator)
                {
                    dangerIndicator = tempIntegrity < 20;
                }
            }

            if (tempIntegrity <= 20)
            {
                dangerIndicator = true;
            }

            if (tempIntegrity < 0 || tempIntegrity > 100)
            {
                MessageBox.Show($"Integrity out of range. Calculated integrity was {tempIntegrity} bar, clamped value to be within range (1% - 100%)", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            integrity = Math.Clamp(tempIntegrity, 0, 100);
        }

        public void calculateFuelEfficiency(double timeElapsed)
        {
            double tempFuelEfficiency = getFuelIntegrity;
        }

    }

    public class Pressuriser
    {
        // Properties
        private double temperature = 330.0;
        private double pressure = 355.0;
        private double waterLevel = 50.0;
        private double heatingPower = 0.0;
        private bool heaterOn = true;
        private bool reliefValveOpen = false;
        private bool sprayNozzlesActive = true;

        public double getTemperature() => temperature;
        public double getPressure() => pressure;
        public double getWaterLevel() => waterLevel;
        public double getHeatingPower() => heatingPower;
        public bool isHeaterOn() => heaterOn;
        public bool isReliefValveOpen() => reliefValveOpen;
        public bool isSprayNozzlesActive() => sprayNozzlesActive;

        // Constants
        private const double minPressure = 145.0;
        private const double maxPressure = 165.0;
        private const double minWaterLevel = 20.0;
        private const double maxWaterLevel = 100.0;

        public void updatePressuriser(double timeElapsed)
        {
            double tempTemperature = temperature;
            double tempPressure = pressure;

            if (heaterOn)
            {
                tempTemperature += heatingPower * 0.01 * timeElapsed;
                tempPressure += heatingPower * 0.005 * timeElapsed;
            }

            if (reliefValveOpen)
            {
                tempPressure -= 0.1 * timeElapsed;
                reliefValveOpen = tempPressure > maxPressure;
            }

            pressure = Math.Clamp(tempPressure, 0, maxPressure + 15);
            temperature = Math.Clamp(tempTemperature, 15, 500);
        }

        public void toggleHeater (bool status)
        {
            heaterOn = status;
            heatingPower = status ? 500 : 0;
        }

        public void toggleSprayNozzles()
        {
            sprayNozzlesActive = true;
            pressure -= 1.0;
            sprayNozzlesActive = false;
        }

        public void toggleReliefValve()
        {
            if (pressure > maxPressure)
            {
                reliefValveOpen = true;
                pressure -= 2.0;
            }
            else if (pressure < minPressure)
            {
                toggleHeater(true);
            }

            if (waterLevel > maxWaterLevel || waterLevel < minWaterLevel)
            {
                MessageBox.Show($"Water level out of range. Calculated water level was {waterLevel} bar, clamped value to be within range (0% - 100%)", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        } 
    }

    public class PrimaryCoolingLoop
    {
        // Properties
        private double coolantTemperature = 300.0;
        private double coolantPressure = 155.0;
        private double coolantFlow = 400.0;
        private double heatTransferEfficiency = 95.0;
        private bool lowFlowIndicator = false;
        private bool highTemperatureIndicator = false;
        private bool lowPressureIndicator = false;

        public double getCoolantTemperature() => coolantTemperature;
        public double getCoolantPressure() => coolantPressure;
        public double getCoolantFlow() => coolantFlow;
        public double getHeatTransferEfficiency() => heatTransferEfficiency;
        public bool isLowFlowIndicator() => lowFlowIndicator;
        public bool isHighTemperatureIndicator() => highTemperatureIndicator;
        public bool isLowPressureIndicator() => lowPressureIndicator;

        public void updatePrimaryCoolingLoop(double reactorTemperature, double timeElapsed)
        {
            calculateHeatTransfer(reactorTemperature);
            updateIndicators();
        }

        private void calculateHeatTransfer(double reactorTemperature)
        {
            double tempCoolantTemperature = coolantTemperature;
            double heatRemoved = reactorTemperature * (coolantFlow / 500.0) * (heatTransferEfficiency / 100.0);
            tempCoolantTemperature += (reactorTemperature - heatRemoved) * 0.01;

            if (tempCoolantTemperature < 15 || tempCoolantTemperature > 500)
            {
                MessageBox.Show($"Temperature out of range. Calculated temperature was {tempCoolantTemperature}°C, clamped value to be within range (15°C - 500°C)", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            coolantTemperature = Math.Clamp(tempCoolantTemperature, 15, 500);
        }

        public void adjustFlow(double newFlow) // Validates that newFlow is within range and updates private variable.
        {
            if (newFlow < 0 || newFlow > 500)
            {
                MessageBox.Show($"Coolant flow rate is out of range. The flow rate was calculated to be {newFlow} m3/min. Clamped value to be within correct range (0 m3/min  - 500 m3/min)", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            coolantFlow = Math.Clamp(newFlow, 0, 500);
        }

        private void updateIndicators() // Method to change the indicators if required.
        {
            lowFlowIndicator = coolantFlow < 100.0;
            highTemperatureIndicator = coolantTemperature >= 340.0;
            lowPressureIndicator = coolantPressure <= 145.0;
        }
    }

    public class SecondaryCoolingLoop
    {
        // Properties (Encapsulated)
        private double steamTemperature = 120.0;
        private double steamPressure = 10.0;
        private double steamFlow = 300.0;
        private double heatTransferEfficiency = 90.0;
        private bool lowFlowIndicator = false;
        private bool highTemperatureIndicator = false;
        private bool lowPressureIndicator = false;

        // Property Getters
        public double getSteamTemperature() => steamTemperature;
        public double getSteamPressure() => steamPressure;
        public double getSteamFlow() => steamFlow;
        public double getHeatTransferEfficiency() => heatTransferEfficiency;
        public bool isLowFlowIndicator() => lowFlowIndicator;
        public bool isHighTemperatureIndicator() => highTemperatureIndicator;
        public bool isLowPressureIndicator() => lowPressureIndicator;

        public void updateSecondaryCoolingLoop(double primaryLoopCoolantTemperature, double timeElapsed) // Method to call the methods for calculations. Runs every "clock" cycle from Reactor class.
        {
            calculateHeatTransfer(primaryLoopCoolantTemperature);
            updateIndicators();
        }

        private void calculateHeatTransfer(double primaryLoopCoolantTemperature) // Method to calculate the steam temperature. Also returns steam pressure.
        {
            double tempSteamTemperature = steamTemperature;
            double heatTransferred = primaryLoopCoolantTemperature * (steamFlow / 500.0) * (heatTransferEfficiency / 100.0);
            tempSteamTemperature += (heatTransferred - steamTemperature) * 0.02;

            if (tempSteamTemperature < 100 || tempSteamTemperature > 400)
            {
                MessageBox.Show($"Temperature out of range. Calculated temperature was {tempSteamTemperature}°C, clamped value to be within range (100°C - 400°C)", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            steamTemperature = Math.Clamp(tempSteamTemperature, 100, 400);
            steamPressure = calculateSteamPressure(steamTemperature);
        }

        private double calculateSteamPressure(double temperature) // Method to calculate the steam pressure. Simple equation.
        {
            double tempPressure = (temperature - 100) * 0.4 + 5;
            
            if (tempPressure > 60 || tempPressure < 5)
            {
                MessageBox.Show($"Steam pressure is out of range. The pressure was calculated to be {tempPressure} bar. Clamped value to be within correct range (5 bar - 60 bar)", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            return Math.Clamp(tempPressure, 5, 60); // Returning the calculated pressure back to "calculateHeatTransfer()".
        }

        public void adjustSteamFlow(double newFlow) // Double checks newFlow is within range and updates once validated.
        {
            if (newFlow < 0 || newFlow > 500)
            {
                MessageBox.Show($"Steam flow rate is out of range. The flow rate was calculated to be {newFlow}m3/min. Clamped value to be within correct range (0m3/min  - 500m3/min)", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            steamFlow = Math.Clamp(newFlow, 0, 500);
        }

        private void updateIndicators()
        {
            double lowFlowIndicator = steamFlow < 100;
            double highTemperatureIndicator = steamTemperature > 270;
            double lowPressureIndicator = steamPressure < 10;
        }
    }

    public class PowerGeneration
    {
        // Properties
        private double powerOutput = 0.0; // MW
        private double thermalEfficiency = 0.33; // %

        public double getPowerOutput() => powerOutput;
        public double getThermalEfficiency() => thermalEfficiency;

        private Reactor reactor;

        public PowerGeneration(Reactor reactor)
        {
            this.reactor = reactor;
        }

        public void updatePowerGeneration(double timeElapsed)
        {
            double temperature = reactor.core.getTemperature();
            double pressure = reactor.core.getPressure();
            double coolantFlow = reactor.core.getCoolantFlow();
            double reactivity = reactor.core.getReactivity();

            double thermalPower = calculateThermalPower(temperature, pressure, coolantFlow, reactivity);
            double tempPowerOutput = thermalPower * thermalEfficiency;

            if (tempPowerOutput < 0)
            {
                MessageBox.Show($"Power output is out of range. The power output was calculated to be {tempPowerOutput}MW. Clamped value to be within correct range (0MW  - 1000MW)", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                tempPowerOutput = 0;
            }

            powerOutput = Math.Clamp(tempPowerOutput, 0, 1000);

            updatePowerIndicators();
        }

        private double calculateThermalPower(double temperature, double pressure, double coolantFlow, double reactivity) // Calculates the thermal power of the reactor, not the final power output.
        {
            double power = (temperature - 300) * 0.1 * (pressure / 150) * (coolantFlow * 400) * (reactivity * 100);
            return Math.Clamp(power, 0, 1100);
        }

        private void updatePowerIndicators() // Fix
        {
            dangerIndicator = reactor.core.isDangerIndicator;

            if (powerOutput > 1000)
            {
                dangerIndicator = true;
            }
        }

        // Need a method to check what is causing danger indicator. E.g. if high power output causes a danger indicator, it should switch off only if no other danger is present when the power goes back down to a safe level.
    }
}