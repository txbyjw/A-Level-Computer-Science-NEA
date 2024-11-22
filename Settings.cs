using System;
using System.Windows;
using System.IO;
using System.Xml.Serialization;

namespace A_Level_Computer_Science_NEA
{
    public partial class Settings : Window
    {
        // [XmlIgnore] // Ensuring that menu is not serialised, 
        // private Menu menu;

        public string resolution { get; set; }

        public Settings()
        {
            InitializeComponent();
            resolution = "2560x1440";
            loadSettings();
        }

        // public Settings(Menu menu) : this()
        // {
            // loadSettings();
            // this.menu = menu;
        // }

        private void saveSettings()
        {
            string filePath = "Settings.xml";

            if (!canWriteToFile(filePath))
            {
                return;
            }

            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(Settings));
                using (StreamWriter writer = new StreamWriter(filePath))
                {
                    serializer.Serialize(writer, this);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving settings: {ex.Message}\n{ex.StackTrace}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void loadSettings()
        {
            string filePath = "Settings.xml";

            if (File.Exists(filePath))
            {
                try
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(Settings));
                    using (StreamReader reader = new StreamReader(filePath))
                    {
                        Settings loadedSettings = (Settings)serializer.Deserialize(reader);
                        this.resolution = loadedSettings.resolution;
                    }
                }
                catch (InvalidOperationException ex)
                {
                    MessageBox.Show($"Error loading settings: {ex.Message}\n{ex.StackTrace}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Settings file not found. Creating a new one.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                this.resolution = "2560x1440";
                saveSettings();
            }
        }

        private void applyResolution(object sender, EventArgs e)
        {
            if (resolutionOption.SelectedItem != null)
            {
                resolution = resolutionOption.SelectedItem.ToString();
                ResolutionManager.applyResolution(resolution, menu);
                saveSettings();
            }
        }

        private bool canWriteToFile(string filePath) // Adding to check that the file can be edited, added originally for debugging an issue I was having but decided to keep for validation.
        {
            try
            {
                using (FileStream fs = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Write))
                {
                    return true;
                }
            }
            catch (UnauthorizedAccessException) // If the program can't access the "Settings.xml" file, throws this error.
            {
                MessageBox.Show("The application does not have permission to write to the file.", "Permission Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Unexpected error checking file permissions: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }
    }
}