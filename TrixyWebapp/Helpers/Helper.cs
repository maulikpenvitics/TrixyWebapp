namespace TrixyWebapp.Helpers
{
    public static class Helper
    {
        public static string GetFullUrl(this HttpContext httpContext, string relativePath)
        {
            if (string.IsNullOrEmpty(relativePath))
                return string.Empty;

            return $"{httpContext.Request.Scheme}://{httpContext.Request.Host}{relativePath}";
        }
        public static void LogFilegenerate(Exception ex, string path, IWebHostEnvironment env)
        {
            string errorMessage = $"DateTime: {DateTime.Now:dd/MM/yyyy hh:mm:ss tt}";
            errorMessage += Environment.NewLine;
            errorMessage += "------------------------Exception-----------------------------------";
            errorMessage += Environment.NewLine;
            errorMessage += $"Path: {path}";
            errorMessage += Environment.NewLine;
            errorMessage += $"Message: {ex.Message}";
            errorMessage += Environment.NewLine;
            errorMessage += $"Details: {ex}";
            errorMessage += Environment.NewLine;
            errorMessage += "-----------------------------------------------------------";
            errorMessage += Environment.NewLine;

            // Get the path to the "ErrorLog" directory in wwwroot
            string logDirectory = Path.Combine(env.WebRootPath, "ErrorLog");

            if (!Directory.Exists(logDirectory))
            {
                Directory.CreateDirectory(logDirectory);
            }

            string logFilePath = Path.Combine(logDirectory, "ErrorLogin.txt");

            using (StreamWriter writer = new StreamWriter(logFilePath, true))
            {
                writer.WriteLine(errorMessage);
            }
        }
    }
}
