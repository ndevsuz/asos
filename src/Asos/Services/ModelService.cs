using Microsoft.AspNetCore.SignalR.Protocol;
using System.Text.RegularExpressions;

namespace Asos.Services;

public class ModelService
{
    public void SplitModels(IFormFile formFile, string targetModelsPath)
    {
        string inputFilePath = $"/home/ubuntu/TempUnSplittedModels/{formFile.Name}";
        Directory.CreateDirectory(inputFilePath);
        var stream = File.OpenWrite(inputFilePath);

        formFile.CopyToAsync(stream);
        stream.Flush();
        stream.Close();

        string outputDirectory = targetModelsPath;

        if (!Directory.Exists(outputDirectory))
        {
            Directory.CreateDirectory(outputDirectory);
        }

        string fileContent = File.ReadAllText(inputFilePath);
        string classPattern = @"(public|private|protected|internal)?\s*class\s+(\w+)\s*{";
        Regex regex = new Regex(classPattern);
        MatchCollection matches = regex.Matches(fileContent);

        foreach (Match match in matches)
        {
            string className = match.Groups[2].Value;
            int classStartIndex = match.Index;
            int classEndIndex = FindClassEndIndex(fileContent, classStartIndex);

            string classContent = fileContent.Substring(classStartIndex, classEndIndex - classStartIndex + 1);
            string outputFilePath = Path.Combine(outputDirectory, className + ".cs");

            File.WriteAllText(outputFilePath, classContent);
        }

    }

    static int FindClassEndIndex(string content, int startIndex)
    {
        int braceCount = 0;
        for (int i = startIndex; i < content.Length; i++)
        {
            if (content[i] == '{')
            {
                braceCount++;
            }
            else if (content[i] == '}')
            {
                braceCount--;
                if (braceCount == 0)
                {
                    return i;
                }
            }
        }

        throw new Exception("Classning tugash nuqtasi topilmadi.");
    }
}

