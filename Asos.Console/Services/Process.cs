using System.Diagnostics;

namespace Easy.Services
{
    public static class Process
    {
        public static void ExecuteCommand(string command, string message = null)
        {
            System.Diagnostics.Process process = new System.Diagnostics.Process();
            process.StartInfo.FileName = "cmd.exe";
            process.StartInfo.RedirectStandardInput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;

            process.OutputDataReceived += (sender, args) =>
            {
                if (args.Data != null)
                {
                    //Console.WriteLine(args.Data);
                }
            };

            process.ErrorDataReceived += (sender, args) =>
            {
                if (args.Data != null)
                {
                    //Console.WriteLine($"ERROR: {args.Data}");
                }
            };

            process.Start();

            using (var writer = process.StandardInput)
            {
                if (writer.BaseStream.CanWrite)
                {
                    //Console.WriteLine(message ?? "\n");
                    writer.WriteLine(command);
                }
            }

            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            process.WaitForExit();

            //Console.WriteLine($"Command exited with code {process.ExitCode}");
        }
    }
}
