using System.Windows;

namespace A_Level_Computer_Science_NEA
{
    public static class ResolutionManager // Class for the resolution manager.
    {
        public static void applyResolution(string resolution, bool fullscreen, Window targetWindow) // Method to update resolution of the window.
        {
            if (targetWindow == null || string.IsNullOrEmpty(resolution)) // Checks it can actually provide valid data to prevent an error.
            {
                return;
            }

            if (fullscreen) // If fullscreen is on, resolution doesn't apply so no need for it.
            {
                targetWindow.WindowStyle = WindowStyle.None;
                targetWindow.ResizeMode = ResizeMode.NoResize;
                targetWindow.WindowState = WindowState.Maximized;
                return; // Stops the next part which reapplies the borders and resolution.
            }

            targetWindow.WindowStyle = WindowStyle.SingleBorderWindow;
            targetWindow.ResizeMode = ResizeMode.CanResize;
            targetWindow.WindowState = WindowState.Normal;

            switch (resolution) // Changes the resolution of the target window based on the resolution parsed.
            {
                case "2560x1440":
                    targetWindow.Width = 2560;
                    targetWindow.Height = 1440;
                    break;
                case "1920x1080":
                    targetWindow.Width = 1920;
                    targetWindow.Height = 1080;
                    break;
                case "1366x768":
                    targetWindow.Width = 1366;
                    targetWindow.Height = 768;
                    break;
                case "1280x1024":
                    targetWindow.Width = 1280;
                    targetWindow.Height = 1024;
                    break;
                case "1440x900":
                    targetWindow.Width = 1440;
                    targetWindow.Height = 900;
                    break;
                case "1600x900":
                    targetWindow.Width = 1600;
                    targetWindow.Height = 900;
                    break;
                case "1680x1050":
                    targetWindow.Width = 1680;
                    targetWindow.Height = 1050;
                    break;
                case "1280x800":
                    targetWindow.Width = 1280;
                    targetWindow.Height = 800;
                    break;
                case "1024x768":
                    targetWindow.Width = 1024;
                    targetWindow.Height = 768;
                    break;
                default:
                    break;
            }
        }
    }
}
