using Easy.Services;


var projectName = default(string);
var invalidChars = new char[]
{
    '.', ',', '(', ')', '[', ']', '{', '}', '/', '\\', '+', '-', '=', '*', '^', '%', '$', '#', '@', '!', '~',
    '|', '?',
};

try
{
    while (true)
    {
        Console.Write("Enter your project name: ");
        projectName = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(projectName))
        {
            Console.WriteLine("Invalid project name!");
        }
        else if (projectName.Any(@char => char.IsSymbol(@char) || invalidChars.Contains(@char)))
        {
            Console.WriteLine("You can't use symbols!");
        }
        else if (char.IsDigit(projectName.First()))
        {
            Console.WriteLine("Project name can't start with number!");
        }
        else if (projectName.Split().Count() > 1)
        {
            Console.WriteLine("Please don't use white spaces.");
        }
        else
        {
            break;
        }
    }

    var modelsFolderPath = string.Empty;
    while (true)
    {
        Console.Write("Enter your models folder path: ");
        modelsFolderPath = Console.ReadLine();

        var check = true;
        try
        {
            var models = Directory.GetFiles(modelsFolderPath);

            if (string.IsNullOrWhiteSpace(modelsFolderPath))
            {
                Console.WriteLine("Invalid path!");
                check = false;
            }
            else if (!models.Any())
            {
                Console.WriteLine("No models in folder.");
                check = false;
            }
            else if (models.Any(model => !model.EndsWith(".cs")))
            {
                Console.WriteLine("All models must be end with .cs !");
                check = false;
            }

            foreach (var model in models)
            {
                var properties = Tokenizator.GetProperties(model);
                if (!properties.ContainsKey("CreatedDate") || !properties.ContainsKey("UpdatedDate"))
                {
                    Console.WriteLine("All models must have CreatedDate and UpdatedDate of DateTimeOffset type");
                    check = false;
                    break;
                }

                var content = File.ReadAllText(model);
                if (content.Contains("internal"))
                {
                    Console.WriteLine("All models must be public!");
                }
            }


            if (check)
            {
                break;
            }
        }
        catch
        {
            Console.WriteLine("Invalid path!");
        }
    }

    var projectFolderPath = string.Empty;
    while (true)
    {
        Console.Write("Enter folder path for project: ");
        projectFolderPath = Console.ReadLine();

        try
        {
            if (!Directory.Exists(projectFolderPath))
            {
                Console.WriteLine("Directory not found!");
            }
            else
            {
                break;
            }
        }
        catch
        {
            Console.WriteLine("Invalid path!");
        }
    }

    var developedBy = string.Empty;
    var header = "// --------------------------------------------------------\r\n// " +
                "Copyright (c) Coalition of Good-Hearted Engineers\r\n// " +
                "Developed by {{DevelopedBy}}\r\n// " +
                " --------------------------------------------------------";
    while (true)
    {
        Console.Write("Developed by? ");
        developedBy = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(developedBy))
        {
            Console.WriteLine("Invalid input!");
        }
        else
        {
            header = header.Replace("{{DevelopedBy}}", developedBy);
            break;
        }
    }

    var host = string.Empty;
    while (true)
    {
        Console.Write("Enter Host for postgresql: ");
        host = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(host))
        {
            Console.WriteLine("Invalid host!");
        }
        else
        {
            break;
        }
    }

    var port = 0;
    while (true)
    {
        Console.Write("Enter port for postgresql: ");
        var inputPort = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(inputPort))
        {
            Console.WriteLine("Invalid port!");
        }
        else if (!int.TryParse(inputPort, out var portNum))
        {
            Console.WriteLine("Post must be number!");
        }
        else
        {
            port = portNum;
            break;
        }
    }

    var database = string.Empty;
    while (true)
    {
        Console.Write("Enter database name: ");
        database = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(database))
        {
            Console.WriteLine("Invalid database name!");
        }
        else if (database.Any(@char => char.IsSymbol(@char) || invalidChars.Contains(@char)))
        {
            Console.WriteLine("You can't use symbols!");
        }
        else if (char.IsDigit(database.First()))
        {
            Console.WriteLine("Database name can't start with number!");
        }
        else if (database.Split().Count() > 1)
        {
            Console.WriteLine("Please don't use white spaces.");
        }
        else
        {
            break;
        }
    }

    var username = string.Empty;
    while (true)
    {
        Console.Write("Enter postgresql username: ");
        username = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(username))
        {
            Console.WriteLine("Invalid username!");
        }
        else if (username.Any(@char => char.IsSymbol(@char) || invalidChars.Contains(@char)))
        {
            Console.WriteLine("You can't use symbols!");
        }
        else if (char.IsDigit(username.First()))
        {
            Console.WriteLine("User name can't start with number!");
        }
        else if (username.Split().Count() > 1)
        {
            Console.WriteLine("Please don't use white spaces.");
        }
        else
        {
            break;
        }
    }

    var password = string.Empty;
    while (true)
    {
        Console.Write($"Enter password for {username}: ");
        password = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(password))
        {
            Console.WriteLine("Invalid password!");
        }
        else
        {
            break;
        }
    }

    var connectionString = $"Host={host};Port={port};Database={database};Username={username};Password={password}";


    var service = new AsosService(
        projectName: projectName,
        modelsFolderPath: modelsFolderPath,
        projectFolderPath: projectFolderPath,
        header: header,
        connectionString: connectionString);

    service.Initialize();

    Console.WriteLine($"{projectName} ready for you!, do you want to open ? Y/N");
    var res = Console.ReadLine();

    if (res.ToLower() == "y")
    {
        Process.ExecuteCommand($"explorer {projectFolderPath}");
    }
}
catch
{
    Console.WriteLine("Error!, please try again.");
}