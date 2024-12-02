using System;
using System.IO;
using System.Windows;
using System.Text.Json;
using System.Windows.Controls;
using System.Windows.Automation;

namespace A_Level_Computer_Science_NEA
{
    public class SettingsData // Class to hold application settings, serialisable.
    {
        public string resolution { get; set; } = "1920x1080";
        public bool fullscreen { get; set; } = false;

        public void Save() // Method which saves settings to the json file.
        {

	    string filePath = "Settings.json";

            try
            {
                var options = new JsonSerializerOptions { WriteIndented = true };
                string json = JsonSerializer.Serialize(this, options);
                File.WriteAllText(filePath, json);
            }
            catch (Exception ex) // Gives the user a warning window popup if the program can't save the settings. More useful for personal debugging.
            {
                MessageBox.Show($"Error saving settings: {ex.Message}\n{ex.StackTrace}.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                throw;
            }
        }

        public static SettingsData Load() // Method to load settings from the json.
        {

	    string filePath = "Settings.json";

            if (!File.Exists(filePath)) // If the file path doesn't exist, create a new one with default values.
            {
		        MessageBox.Show($"Couldn't locate the settings file. Creating a new file and defaulting values.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                var defaultSettings = new SettingsData();
                defaultSettings.Save();
                return defaultSettings;
            }

            try // Reads the json string and deserialises it into a SettingsData object.
            {
                string json = File.ReadAllText(filePath);
                return JsonSerializer.Deserialize<SettingsData>(json) ?? new SettingsData();
            }
            catch (Exception ex) // Similar to last catch, displays an error window if the program can't load the settings.
            {
                MessageBox.Show($"Error loading settings: {ex.Message}\n{ex.StackTrace}.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return new SettingsData();
            }
        }
    }

    public static class SettingsManager
    {
        public static void applySettings(SettingsData settingsData, Menu menu)
        {
            ResolutionManager.applyResolution(settingsData.resolution, settingsData.fullscreen, menu);

            if (settingsData.fullscreen)
            {
                menu.WindowStyle = WindowStyle.None;
                menu.ResizeMode = ResizeMode.NoResize;
                menu.WindowState = WindowState.Maximized;
            }
            else
            {
                menu.WindowStyle = WindowStyle.SingleBorderWindow;
                menu.ResizeMode = ResizeMode.CanResize;
                menu.WindowState = WindowState.Normal;
            }
        }
    }

    public partial class Settings : Window // Partial class representing the settings window.
    {
        private Menu menu;
        private SettingsData settingsData;

        public Settings(Menu menu) // Constructor for the settings window.
        {
            InitializeComponent();
            this.menu = menu;
            this.Topmost = true;

            settingsData = SettingsData.Load();
            applyLoadedSettings();
        }

        private void applyLoadedSettings() // Method to update the UI based on applied settings.
        {
            ResolutionManager.applyResolution(settingsData.resolution, settingsData.fullscreen, menu);

            foreach (ComboBoxItem item in resolutionOption.Items)
            {
                if (item.Content.ToString() == settingsData.resolution)
                {
                    resolutionOption.SelectedItem = item;
                    break;
                }
            }

            fullscreenCheckbox.IsChecked = settingsData.fullscreen;
        }

        private void saveSettings() // Method to save the applied settings to the json file.
        {
            settingsData.resolution = (resolutionOption.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "1920x1080";
            settingsData.fullscreen = fullscreenCheckbox.IsChecked ?? false;
            settingsData.Save();
        }

        private void fullscreenChecked(object sender, RoutedEventArgs e) // Event handler to toggle fullscreen.
        {
            if (fullscreenCheckbox.IsChecked == true)
            {
                menu.WindowStyle = WindowStyle.None;
                menu.ResizeMode = ResizeMode.NoResize;
                menu.WindowState = WindowState.Maximized;
            }
            else
            {
                menu.WindowStyle = WindowStyle.SingleBorderWindow;
                menu.ResizeMode = ResizeMode.CanResize;
                menu.WindowState = WindowState.Normal;
            }
            saveSettings();
        }

        private void applyResolution(object sender, EventArgs e) // Event handler to handle resolution changes.
        {
            if (resolutionOption.SelectedItem != null)
            {
                settingsData.resolution = (resolutionOption.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "1920x1080";
                ResolutionManager.applyResolution(settingsData.resolution, settingsData.fullscreen, menu);
                saveSettings();
            }
        }
    }
}