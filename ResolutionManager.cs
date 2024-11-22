using System.Windows;

namespace A_Level_Computer_Science_NEA
{
    public static class ResolutionManager
    {
        public static void applyResolution(string resolution, Window targetWindow)
        {
            if (targetWindow == null || string.IsNullOrEmpty(resolution))
            {
                return;

                switch (resolution)
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
}
