using Easy.Constants;
using Easy.Extensions;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Text;

namespace Easy.Services
{
    public class ProjectService
    {
        private string apiProjectPath;
        private string apiProjectName;
        private string buildProjectPath;
        private string buildProjectName;
        private string testProjectPath;
        private string testProjectName;

        private readonly string projectName;
        private readonly string modelsFolderPath;
        private readonly string projectFolderPath;
        private readonly string header;
        private readonly string connectionString;

        public ProjectService(
            string projectName,
            string modelsFolderPath,
            string projectFolderPath,
            string header,
            string connectionString)
        {
            this.projectName = projectName;
            this.modelsFolderPath = modelsFolderPath;
            this.projectFolderPath = projectFolderPath;
            this.header = header;
            this.connectionString = connectionString;

            this.apiProjectName = $"{projectName}.Api";
            this.testProjectName = $"{apiProjectName}.Tests.Unit";
            this.buildProjectName = $"{projectName}.Infrastructure.Build";
        }

        public (string ProjectPath, string ApiProjectPath, string TestProjectPath, string BuildProjectPath) CreateProjectStructure()
        {
            var projectPath = Path.Combine(projectFolderPath, projectName);
            Process.ExecuteCommand($"dotnet new sln --name {projectName} --output {projectFolderPath}", "Creating solution");

            Process.ExecuteCommand($"dotnet new webapi --name {apiProjectName} --output {projectFolderPath}/{apiProjectName}", "Creating webapi project");
            Process.ExecuteCommand($"dotnet sln {projectFolderPath}/{projectName}.sln add {projectFolderPath}/{apiProjectName}/{apiProjectName}.csproj");

            Process.ExecuteCommand($"dotnet new xunit --name {testProjectName} --output {projectFolderPath}/{testProjectName}", "Creating test project");
            Process.ExecuteCommand($"dotnet sln {projectFolderPath}/{projectName}.sln add {projectFolderPath}/{testProjectName}/{testProjectName}.csproj");

            Process.ExecuteCommand($"dotnet new console --name {buildProjectName} --output {projectFolderPath}/{buildProjectName}", "Creating build project");
            Process.ExecuteCommand($"dotnet sln {projectFolderPath}/{projectName}.sln add {projectFolderPath}/{buildProjectName}/{buildProjectName}.csproj");

            this.apiProjectPath = Path.Combine(projectFolderPath, apiProjectName);
            this.testProjectPath = Path.Combine(projectFolderPath, testProjectName);
            this.buildProjectPath = Path.Combine(projectFolderPath, buildProjectName);

            return (projectPath, apiProjectPath, testProjectPath, buildProjectPath);
        }

        public void InitializeBuildProject()
        {
            var content = File.ReadAllText(Path.Combine("..", "..", "..", StringConstants.Source, StringConstants.BuildProject, "BuildPipelineCode.txt"));
            content = content
                .Replace("{{ProjectName}}", projectName)
                .Replace("{{NameSpace}}", this.buildProjectName)
                .Replace("{{DotnetVersion}}", "8.0.300");

            File.Delete($"{buildProjectPath}/Program.cs");
            File.Create($"{buildProjectPath}/Program.cs").Close();

            File.WriteAllText($"{buildProjectPath}/Program.cs", content);
        }

        public void CreateModels()
        {
            Console.WriteLine("Creating... Models");
            var models = Directory.GetFiles(this.modelsFolderPath);

            if (!Directory.Exists(Path.Combine(apiProjectPath, StringConstants.Models)))
            {
                Directory.CreateDirectory(Path.Combine(apiProjectPath, StringConstants.Models));
            }

            foreach (var existModelPath in models)
            {
                var modelName = existModelPath.PathToName();
                var modelNamePlural = modelName.NameToPlural();

                var modelsPath = Path.Combine(apiProjectPath, StringConstants.Models, modelNamePlural);
                if (!Directory.Exists(modelsPath))
                {
                    Directory.CreateDirectory(modelsPath);
                }

                var content = File.ReadAllLines(existModelPath).ToList();

                string contentString = string.Empty;
                var @namespace = $"namespace {apiProjectName}.{StringConstants.Models}.{modelNamePlural}";
                
                if (content.First().Trim().Contains("namespace"))
                {
                    content.RemoveAt(0);
                    content.Insert(0, @namespace);
                    content.Insert(0, $"{this.header}\n");
                    contentString = string.Empty;
                    foreach (var line in content)
                    {
                        contentString += line + "\n";
                    }
                }
                else
                {
                    contentString = string.Empty;
                    contentString += this.header + "\n\n";
                    contentString += @namespace + "\n";
                    contentString += "{\n";
                    foreach(var line in content)
                    {
                        contentString += $"\t{line}\n";
                    }
                    contentString += "}";

                }

                var modelFilePath = Path.Combine(modelsPath, modelName.AddCSharpExtension());
                FileStream fileStream;
                File.Create(modelFilePath).Close();
                fileStream = File.Open(modelFilePath, FileMode.Open, FileAccess.ReadWrite);
                fileStream.Write(Encoding.UTF8.GetBytes(contentString));
                fileStream.Flush();
                fileStream.Close();
            }
        }

        public void CreateExceptions()
        {
            Console.WriteLine("Creating exceptions for models");
            var models = Directory.GetFiles(this.modelsFolderPath);

            foreach (var modelPath in models)
            {
                var modelName = modelPath.PathToName();
                var modelNamePlural = modelName.NameToPlural();

                var exceptionsFolderPath = Path.Combine(apiProjectPath, StringConstants.Models, modelNamePlural, StringConstants.Exceptions);

                if (!Directory.Exists(exceptionsFolderPath))
                {
                    Directory.CreateDirectory(exceptionsFolderPath);
                }

                var exceptionsPath = Directory.GetFiles(
                    Path.Combine("..", "..", "..", StringConstants.Source, StringConstants.Exceptions));

                var exceptionNameSpace = $"{apiProjectName}.{StringConstants.Models}.{modelNamePlural}.{StringConstants.Exceptions}";

                foreach (var exceptionPath in exceptionsPath)
                {
                    var exceptionName = exceptionPath.PathToName().Replace("Entity", modelName);
                    var exceptionContent = File.ReadAllText(exceptionPath)
                        .Replace("{{Header}}", this.header)
                        .Replace("{{NameSpace}}", exceptionNameSpace)
                        .Replace("{{ModelName}}", modelName)
                        .Replace("{{modelName}}", modelName.ToLowFirstLetter());

                    var exceptionFilePath = Path.Combine(exceptionsFolderPath, exceptionName.AddCSharpExtension());
                    File.Create(exceptionFilePath).Close();
                    var fileStream = File.Open(exceptionFilePath, FileMode.Open, FileAccess.Write);
                    fileStream.Write(Encoding.UTF8.GetBytes(exceptionContent));
                    fileStream.Flush();
                    fileStream.Close();
                }
            }
        }

        public void CreateBrokers()
        {
            Console.WriteLine("Creating brokers...");
            var brokersPath = Path.Combine(apiProjectPath, StringConstants.Brokers);
            var brokersBasePath = Path.Combine("..", "..", "..", StringConstants.Source, StringConstants.Brokers);

            Directory.CreateDirectory(brokersPath);

            AddDateTimeBroker(brokersPath, brokersBasePath);
            AddLoggingBroker(brokersPath, brokersBasePath);
            AddStorageBroker(brokersPath, brokersBasePath);

            WriteStorageBrokers();
        }

        private void AddStorageBroker(string brokersPath, string brokersBasePath)
        {
            var storageBrokerPath = Path.Combine(apiProjectPath, StringConstants.Brokers, StringConstants.Storages);
            if (!Directory.Exists(storageBrokerPath))
            {
                Directory.CreateDirectory(storageBrokerPath);
            }

            var storageBrokerNameSpace = $"{apiProjectName}.Brokers.Storages";
            var storageBrokerInterfaceContent = File.ReadAllText(Path.Combine(brokersBasePath, "IStorageBroker.txt"))
                .Replace("{{Header}}", this.header)
                .Replace("{{NameSpace}}", storageBrokerNameSpace);

            var storageBrokerInterfaceFilePath = Path.Combine(storageBrokerPath, "IStorageBroker.cs");
            File.Create(storageBrokerInterfaceFilePath).Close();
            var fileStream = File.Open(storageBrokerInterfaceFilePath, FileMode.Open, FileAccess.ReadWrite);
            fileStream.Write(Encoding.UTF8.GetBytes(storageBrokerInterfaceContent));
            fileStream.Flush();
            fileStream.Close();

            var storageBrokerContent = File.ReadAllText(Path.Combine(brokersBasePath, "StorageBroker.txt"))
                .Replace("{{Header}}", this.header)
                .Replace("{{NameSpace}}", storageBrokerNameSpace);
            var storageBrokerFilePath = Path.Combine(storageBrokerPath, "StorageBroker.cs");
            File.Create(storageBrokerFilePath).Close();
            var fileStream2 = File.Open(storageBrokerFilePath, FileMode.Open, FileAccess.ReadWrite);
            fileStream2.Write(Encoding.UTF8.GetBytes(storageBrokerContent));
            fileStream2.Flush();
            fileStream2.Close();
        }

        private void AddLoggingBroker(string brokersPath, string brokersBasePath)
        {
            var loggingBrokerPath = Path.Combine(brokersPath, StringConstants.Loggings);

            if (!Directory.Exists(loggingBrokerPath))
            {
                Directory.CreateDirectory(loggingBrokerPath);
            }

            var loggingBrokersNameSpace = $"{apiProjectName}.Brokers.Loggings";
            var loggingBrokerInterfaceContent = File.ReadAllText($"{brokersBasePath}/ILoggingBroker.txt")
                .Replace("{{Header}}", this.header)
                .Replace("{{NameSpace}}", loggingBrokersNameSpace);
            var loggingBrokerInterfaceFilePath = Path.Combine(loggingBrokerPath, "ILoggingBroker.cs");
            File.Create(loggingBrokerInterfaceFilePath).Close();
            var fileStream = File.Open(loggingBrokerInterfaceFilePath, FileMode.Open, FileAccess.ReadWrite);
            fileStream.Write(Encoding.UTF8.GetBytes(loggingBrokerInterfaceContent));
            fileStream.Flush();
            fileStream.Close();

            var loggingBrokerContent = File.ReadAllText($"{brokersBasePath}/LoggingBroker.txt")
                .Replace("{{Header}}", this.header)
                .Replace("{{NameSpace}}", loggingBrokersNameSpace);
            var loggingBrokerFilePath = Path.Combine(loggingBrokerPath, "Loggingbroker.cs");
            File.Create(loggingBrokerFilePath).Close();
            var fileStream2 = File.Open(loggingBrokerFilePath, FileMode.Open, FileAccess.ReadWrite);
            fileStream2.Write(Encoding.UTF8.GetBytes(loggingBrokerContent));
            fileStream2.Flush();
            fileStream2.Close();
        }

        private void AddDateTimeBroker(string brokersPath, string brokersBasePath)
        {
            var datetimeBrokersPath = Path.Combine(brokersPath, StringConstants.DateTimes);

            if (!Directory.Exists(datetimeBrokersPath))
            {
                Directory.CreateDirectory(datetimeBrokersPath);
            }

            var @namespace = $"{apiProjectName}.Brokers.DateTimes";

            var datetimeBrokerInterfaceContent = File.ReadAllText($"{brokersBasePath}/IDateTimeBroker.txt")
                .Replace("{{Header}}", this.header)
                .Replace("{{NameSpace}}", @namespace);

            var datetimeBrokerContent = File.ReadAllText($"{brokersBasePath}/DateTimeBroker.txt")
                .Replace("{{Header}}", this.header)
                .Replace("{{NameSpace}}", @namespace);

            var datetimeBrokerInterfaceFilePath = $"{datetimeBrokersPath}/IDateTimeBroker.cs";
            var datetimeBrokerFilePath = $"{datetimeBrokersPath}/DateTimeBroker.cs";

            File.Create(datetimeBrokerInterfaceFilePath).Close();
            var fileStream = File.Open(datetimeBrokerInterfaceFilePath, FileMode.Open, FileAccess.ReadWrite);
            fileStream.Write(Encoding.UTF8.GetBytes(datetimeBrokerInterfaceContent));
            fileStream.Flush();
            fileStream.Close();

            File.Create(datetimeBrokerFilePath).Close();
            var fileStream2 = File.Open(datetimeBrokerFilePath, FileMode.Open, FileAccess.ReadWrite);
            fileStream2.Write(Encoding.UTF8.GetBytes(datetimeBrokerContent));
            fileStream2.Flush();
            fileStream2.Close();
        }

        private void WriteStorageBrokers()
        {
            var confContent = File.ReadAllText(Path.Combine("..", "..", "..", StringConstants.Source, StringConstants.Configurations, "AppSettings.txt"))
                .Replace("{{ConnectionString}}", this.connectionString);

            var appsettingsFilePath = Path.Combine(apiProjectPath, "appsettings.json");
            var fileStream = File.Open(appsettingsFilePath, FileMode.Open, FileAccess.Write);
            fileStream.Write(Encoding.UTF8.GetBytes(confContent));
            fileStream.Flush();
            fileStream.Close();
            fileStream = null;

            var models = Directory.GetFiles(this.modelsFolderPath);
            var storageBrokerNameSpace = $"{apiProjectName}.Brokers.Storages";

            foreach (var modelPath in models)
            {
                var modelName = modelPath.PathToName();
                var modelNamePlural = modelName.NameToPlural();

                var interfaceContent = File.ReadAllText(Path.Combine("..", "..", "..", StringConstants.Source, StringConstants.Brokers, "IStorageBroker.Entity.txt"))
                    .Replace("{{Header}}", this.header)
                    .Replace("{{NameSpace}}", storageBrokerNameSpace)
                    .Replace("{{ProjectName}}", apiProjectName)
                    .Replace("{{ModelName}}", modelName)
                    .Replace("{{ModelNamePlural}}", modelNamePlural)
                    .Replace("{{modelName}}", modelName.ToLowFirstLetter());

                var interfaceFilePath = Path.Combine(apiProjectPath, StringConstants.Brokers, StringConstants.Storages, $"IStorageBroker.{modelNamePlural}.cs");
                File.Create(interfaceFilePath).Close();
                fileStream = File.Open(interfaceFilePath, FileMode.Open, FileAccess.ReadWrite);
                fileStream.Write(Encoding.UTF8.GetBytes(interfaceContent));
                fileStream.Flush();
                fileStream.Close();
                fileStream = null;

                var content = File.ReadAllText(Path.Combine("..", "..", "..", StringConstants.Source, StringConstants.Brokers, "StorageBroker.Entity.txt"))
                    .Replace("{{Header}}", this.header)
                    .Replace("{{NameSpace}}", storageBrokerNameSpace)
                    .Replace("{{ProjectName}}", apiProjectName)
                    .Replace("{{ModelName}}", modelName)
                    .Replace("{{ModelNamePlural}}", modelNamePlural)
                    .Replace("{{modelName}}", modelName.ToLowFirstLetter());

                var filePath = Path.Combine(apiProjectPath, StringConstants.Brokers, StringConstants.Storages, $"StorageBroker.{modelNamePlural}.cs");
                File.Create(filePath).Close();
                fileStream = File.Open(filePath, FileMode.Open, FileAccess.Write);
                fileStream.Write(Encoding.UTF8.GetBytes(content));
                fileStream.Flush();
                fileStream.Close();

                AddMigrations(modelNamePlural);
            }
        }

        private void AddMigrations(string modelNamePlural)
        {
            Process.ExecuteCommand($"dotnet build {apiProjectPath}");
            Process.ExecuteCommand($"dotnet ef migrations add \"Create{modelNamePlural}Table\" --project {apiProjectPath}", $"Adding migration for {modelNamePlural}");
        }

        public void CreateServices()
        {
            Console.WriteLine("Creating services...");

            var servicesPath = Path.Combine(apiProjectPath, StringConstants.Services, StringConstants.Foundations);

            if (!Directory.Exists(servicesPath))
            {
                Directory.CreateDirectory(servicesPath);
            }

            var modelsPath = Directory.GetFiles(this.modelsFolderPath);

            var servicesBasePath = Path.Combine("..", "..", "..", StringConstants.Source, StringConstants.Services);

            foreach (var modelPath in modelsPath)
            {
                var modelName = modelPath.PathToName();
                var modelNamePlural = modelName.NameToPlural();
                var @namespace = $"{apiProjectName}.Services.Foundations.{modelNamePlural}";

                var modelServicePath = Path.Combine(servicesPath, modelNamePlural);
                Directory.CreateDirectory(modelServicePath);

                var serviceInterfacePath = Path.Combine(modelServicePath, $"I{modelName}Service.cs");
                File.Create(serviceInterfacePath).Close();
                var fileStream = File.Open(serviceInterfacePath, FileMode.Open, FileAccess.ReadWrite);
                var interfaceContent = File.ReadAllText(Path.Combine(servicesBasePath, "IEntityService.txt"))
                    .Replace("{{Header}}", this.header)
                    .Replace("{{NameSpace}}", @namespace)
                    .Replace("{{ProjectName}}", apiProjectName)
                    .Replace("{{ModelName}}", modelName)
                    .Replace("{{ModelNamePlural}}", modelNamePlural)
                    .Replace("{{modelName}}", modelName.ToLowFirstLetter());
                fileStream.Write(Encoding.UTF8.GetBytes(interfaceContent));
                fileStream.Flush();
                fileStream.Close();

                var servicePath = Path.Combine(modelServicePath, $"{modelName}Service.cs");
                File.Create(servicePath).Close();
                var serviceFS = File.Open(servicePath, FileMode.Open, FileAccess.ReadWrite);
                var serviceContent = File.ReadAllText(Path.Combine(servicesBasePath, "EntityService.txt"))
                    .Replace("{{Header}}", this.header)
                    .Replace("{{ProjectName}}", apiProjectName)
                    .Replace("{{NameSpace}}", @namespace)
                    .Replace("{{ModelName}}", modelName)
                    .Replace("{{ModelNamePlural}}", modelNamePlural)
                    .Replace("{{modelName}}", modelName.ToLowFirstLetter());
                serviceFS.Write(Encoding.UTF8.GetBytes(serviceContent));
                serviceFS.Flush();
                serviceFS.Close();

                var exceptionPath = Path.Combine(modelServicePath, $"{modelName}Service.Exceptions.cs");
                File.Create(exceptionPath).Close();
                var exceptionFS = File.Open(exceptionPath, FileMode.Open, FileAccess.ReadWrite);
                var exceptionContent = File.ReadAllText(Path.Combine(servicesBasePath, "EntityService.Exceptions.txt"))
                    .Replace("{{Header}}", this.header)
                    .Replace("{{ProjectName}}", apiProjectName)
                    .Replace("{{NameSpace}}", @namespace)
                    .Replace("{{ModelName}}", modelName)
                    .Replace("{{ModelNamePlural}}", modelNamePlural)
                    .Replace("{{modelName}}", modelName.ToLowFirstLetter());
                exceptionFS.Write(Encoding.UTF8.GetBytes(exceptionContent));
                exceptionFS.Flush();
                exceptionFS.Close();

                var validationPath = Path.Combine(modelServicePath, $"{modelName}Service.Validations.cs");
                File.Create(validationPath).Close();
                var validationFS = File.Open(validationPath, FileMode.Open, FileAccess.ReadWrite);

                var validations = string.Empty;
                var properties = Tokenizator.GetProperties(modelPath);

                foreach (var prop in properties)
                {
                    if (prop.Value == "string" ||
                       prop.Value == "Guid" ||
                       prop.Value == "DateTimeOffset")
                    {
                        validations += $"\t\t\t\t(Rule: IsInvalid([[modelName]].{prop.Key}), Parameter: nameof([[ModelName]].{prop.Key})),\n";
                    }
                }

                var validationContent = File.ReadAllText(Path.Combine(servicesBasePath, "EntityService.Validations.txt"))
                    .Replace("{{Validations}}", validations)
                    .Replace("{{Header}}", this.header)
                    .Replace("{{ProjectName}}", apiProjectName)
                    .Replace("{{NameSpace}}", @namespace)
                    .Replace("{{ModelName}}", modelName)
                    .Replace("{{ModelNamePlural}}", modelNamePlural)
                    .Replace("{{modelName}}", modelName.ToLowFirstLetter())
                    .Replace("[[ModelName]]", modelName)
                    .Replace("[[modelName]]", modelName.ToLowFirstLetter());
                validationFS.Write(Encoding.UTF8.GetBytes(validationContent));
                validationFS.Flush();
                validationFS.Close();
            }
        }

        public void CreateControllers()
        {
            Console.WriteLine("Creating controllers...");
            var controllersPath = Path.Combine(apiProjectPath, StringConstants.Controllers);
            if (!Directory.Exists(controllersPath)) Directory.CreateDirectory(controllersPath);

            var modelsPath = Directory.GetFiles(this.modelsFolderPath);

            foreach (var modelPath in modelsPath)
            {
                var modelName = modelPath.PathToName();
                var modelNamePlural = modelName.NameToPlural();

                var controllerPath = Path.Combine(controllersPath, $"{modelNamePlural}Controller.cs");
                File.Create(controllerPath).Close();
                var fileStream = File.Open(controllerPath, FileMode.Open, FileAccess.ReadWrite);
                var content = File.ReadAllText(Path.Combine("..", "..", "..", StringConstants.Source, StringConstants.Controllers, "EntitiesController.txt"))
                    .Replace("{{Header}}", this.header)
                    .Replace("{{ProjectName}}", apiProjectName)
                    .Replace("{{ModelName}}", modelName)
                    .Replace("{{ModelNamePlural}}", modelNamePlural)
                    .Replace("{{modelName}}", modelName.ToLowFirstLetter());
                fileStream.Write(Encoding.UTF8.GetBytes(content));
                fileStream.Flush();
                fileStream.Close();
            }
        }

        public void ConfigureProject()
        {
            Console.WriteLine("Configuring Startup...");
            var startupFilePath = Path.Combine(apiProjectPath, "Startup.cs");
            File.Create(startupFilePath).Close();
            var fs = File.Open(startupFilePath, FileMode.Open, FileAccess.Write);
            var content = File.ReadAllText(Path.Combine("..", "..", "..", StringConstants.Source, StringConstants.Configurations, "Startup.txt"))
                .Replace("{{Header}}", this.header)
                .Replace("{{ProjectName}}", apiProjectName);

            var usings = string.Empty;
            var serviceConf = string.Empty;
            var modelsPath = Directory.GetFiles(this.modelsFolderPath);
            foreach (var model in modelsPath)
            {
                var modelName = model.PathToName();
                var modelNamePlural = modelName.NameToPlural();

                usings += model == modelsPath.Last() ? $"using {this.projectName}.Api.Services.Foundations.{modelNamePlural};" : $"using {this.projectName}.Api.Services.Foundations.{modelNamePlural};\n";
                serviceConf += $"\t\t\tservices.AddTransient<I{modelName}Service, {modelName}Service>();\n";
            }

            content = content
                .Replace("{{Usings}}", usings)
                .Replace("{{AddServices}}", serviceConf);

            fs.Write(Encoding.UTF8.GetBytes(content));
            fs.Flush();
            fs.Close();

            var content2 = File.ReadAllText(Path.Combine("..", "..", "..", StringConstants.Source, StringConstants.Configurations, "Program.txt"))
                .Replace("{{Header}}", this.header)
                .Replace("{{ProjectName}}", apiProjectName);

            File.Delete(Path.Combine(apiProjectPath, "Program.cs"));
            File.Create(Path.Combine(apiProjectPath, "Program.cs")).Close();
            var fs2 = File.Open(Path.Combine(apiProjectPath, "Program.cs"), FileMode.Open, FileAccess.ReadWrite);
            fs2.Write(Encoding.UTF8.GetBytes(content2));
            fs2.Flush();
            fs2.Close();
        }

        internal void WriteTests()
        {
            Console.WriteLine("Creating unit tests...");
            Process.ExecuteCommand($"dotnet add {this.testProjectPath}/{this.testProjectName}.csproj reference {this.apiProjectPath}/{apiProjectName}.csproj");

            var unitTest1FilePath = Path.Combine(testProjectPath, "UnitTest1.cs");
            File.Delete(unitTest1FilePath);

            var modelsPath = Directory.GetFiles(this.modelsFolderPath);

            foreach (var modelPath in modelsPath)
            {
                var modelName = modelPath.PathToName();
                var modelNamePlural = modelName.NameToPlural();

                var modelTestsPath = Path.Combine(this.testProjectPath, StringConstants.Services, StringConstants.Foundations, modelNamePlural);
                Directory.CreateDirectory(modelTestsPath);

                AddTestsFile(modelName, modelNamePlural, modelTestsPath);
                AddLogicTests(modelName, modelNamePlural, modelTestsPath);
                AddExceptionsTests(modelName, modelNamePlural, modelTestsPath);

                var properties = Tokenizator.GetProperties(modelPath);
                var nameProperty = Tokenizator.GetNamePropery(properties);

                if (nameProperty.Property != string.Empty && nameProperty.Type != string.Empty)
                {
                    AddValidationsTests(modelName, modelNamePlural, modelTestsPath, properties);
                }
            }
        }

        private void AddValidationsTests(string modelName, string modelNamePlural, string modelTestsPath, Dictionary<string, string> properties)
        {
            AddValidationsTestAdd(modelName, modelNamePlural, modelTestsPath, properties);
            AddValidationsTestModify(modelName, modelNamePlural, modelTestsPath, properties);
            AddValidationsTestRemoveById(modelName, modelNamePlural, modelTestsPath, properties);
            AddValidationsTestRetrieveById(modelName, modelNamePlural, modelTestsPath, properties);
        }

        private void AddValidationsTestRetrieveById(string modelName, string modelNamePlural, string modelTestsPath, Dictionary<string, string> properties)
        {
            var content = File.ReadAllText(
               Path.Combine("..", "..", "..", StringConstants.Source, StringConstants.Tests, StringConstants.Validations, "EntityServiceTests.Validations.RetrieveById.txt"))
                   .Replace("{{Header}}", this.header)
                   .Replace("{{NameSpace}}", $"{testProjectName}.Services.Foundations.{modelNamePlural}")
                   .Replace("{{ProjectName}}", apiProjectName)
                   .Replace("{{ModelName}}", modelName)
                   .Replace("{{modelName}}", modelName.ToLowFirstLetter())
                   .Replace("{{ModelNamePlural}}", modelNamePlural);

            var filePath = Path.Combine(modelTestsPath, $"{modelName}ServiceTests.Validations.Retrieve.cs");
            File.Create(filePath).Close();
            var fileStream = File.Open(filePath, FileMode.Open, FileAccess.Write);

            fileStream.Write(Encoding.UTF8.GetBytes(content));
            fileStream.Flush();
            fileStream.Close();
        }

        private void AddValidationsTestRemoveById(string modelName, string modelNamePlural, string modelTestsPath, Dictionary<string, string> properties)
        {
            var content = File.ReadAllText(
                Path.Combine("..", "..", "..", StringConstants.Source, StringConstants.Tests, StringConstants.Validations, "EntityServiceTests.Validations.RemoveById.txt"))
                    .Replace("{{Header}}", this.header)
                    .Replace("{{NameSpace}}", $"{testProjectName}.Services.Foundations.{modelNamePlural}")
                    .Replace("{{ProjectName}}", apiProjectName)
                    .Replace("{{ModelName}}", modelName)
                    .Replace("{{modelName}}", modelName.ToLowFirstLetter())
                    .Replace("{{ModelNamePlural}}", modelNamePlural);

            var filePath = Path.Combine(modelTestsPath, $"{modelName}ServiceTests.Validations.RemoveById.cs");
            File.Create(filePath).Close();

            var fileStream = File.Open(filePath, FileMode.Open, FileAccess.Write);
            fileStream.Write(Encoding.UTF8.GetBytes(content));
            fileStream.Flush();
            fileStream.Close();
        }

        private void AddValidationsTestModify(string modelName, string modelNamePlural, string modelTestsPath, Dictionary<string, string> properties)
        {
            var filePath = Path.Combine(modelTestsPath, $"{modelName}ServiceTests.Validations.Modify.cs");
            File.Create(filePath).Close();
            var fileStream = File.Open(filePath, FileMode.Open, FileAccess.Write);

            string validations = string.Empty;
            foreach (var property in properties)
            {
                if (property.Value == "Guid")
                {
                    validations += $"\t\t\t\tinvalid[[ModelName]]Exception.AddData(\n";
                    validations += $"\t\t\t\t\tkey: nameof([[ModelName]].{property.Key}),\n";
                    validations += $"\t\t\t\t\tvalues: \"Id is required\");\n\n";
                }
                else if (property.Value == "string")
                {
                    validations += $"\t\t\t\tinvalid[[ModelName]]Exception.AddData(\n";
                    validations += $"\t\t\t\t\tkey: nameof([[ModelName]].{property.Key}),\n";
                    validations += $"\t\t\t\t\tvalues: \"Text is required\");\n\n";
                }
                else if (property.Value == "DateTimeOffset"
                    && property.Key != "CreatedDate" && property.Key != "UpdatedDate")
                {
                    validations += $"\t\t\t\tinvalid[[ModelName]]Exception.AddData(\n";
                    validations += $"\t\t\t\t\tkey: nameof([[ModelName]].{property.Key}),\n";
                    validations += $"\t\t\t\t\tvalues: \"Date is required\");\n\n";
                }
            }

            var nameProp = Tokenizator.GetNamePropery(properties);

            var content = File.ReadAllText(
                Path.Combine("..", "..", "..", StringConstants.Source, StringConstants.Tests, StringConstants.Validations, "EntityServiceTests.Validations.Modify.txt"))
                    .Replace("{{Header}}", this.header)
                    .Replace("{{ProjectName}}", apiProjectName)
                    .Replace("{{NameSpace}}", $"{testProjectName}.Services.Foundations.{modelNamePlural}")
                    .Replace("{{Validations}}", validations)
                    .Replace("{{NameProp}}", nameProp.Property)
                    .Replace("{{ModelName}}", modelName)
                    .Replace("{{modelName}}", modelName.ToLowFirstLetter())
                    .Replace("{{ModelNamePlural}}", modelNamePlural)
                    .Replace("[[ModelName]]", modelName);

            fileStream.Write(Encoding.UTF8.GetBytes(content));
            fileStream.Flush();
            fileStream.Close();
        }

        private void AddValidationsTestAdd(string modelName, string modelNamePlural, string modelTestsPath, Dictionary<string, string> properties)
        {
            var @namespace = $"{testProjectName}.Services.Foundations.{modelNamePlural}";

            var nameProp = Tokenizator.GetNamePropery(properties);

            var validations = string.Empty;
            foreach (var prop in properties)
            {
                if (prop.Value == "Guid")
                {
                    validations += "\t\t\t\tinvalid[[ModelName]]Exception.AddData(\n";
                    validations += $"\t\t\t\t\tkey: nameof([[ModelName]].{prop.Key}),\n";
                    validations += $"\t\t\t\t\tvalues: \"Id is required\");\n\n";
                }
                if (prop.Value == "string")
                {
                    validations += "\t\t\t\tinvalid[[ModelName]]Exception.AddData(\n";
                    validations += $"\t\t\t\t\tkey: nameof([[ModelName]].{prop.Key}),\n";
                    validations += $"\t\t\t\t\tvalues: \"Text is required\");\n\n";
                }
                if (prop.Value == "DateTimeOffset")
                {
                    validations += "\t\t\t\tinvalid[[ModelName]]Exception.AddData(\n";
                    validations += $"\t\t\t\t\tkey: nameof([[ModelName]].{prop.Key}),\n";
                    validations += $"\t\t\t\t\tvalues: \"Date is required\");\n\n";
                }
            }

            var content = File.ReadAllText(Path.Combine("..", "..", "..", StringConstants.Source, StringConstants.Tests, StringConstants.Validations, "EntityServiceTests.Validations.Add.txt"))
                .Replace("{{NameProp}}", nameProp.Property)
                .Replace("{{Validations}}", validations)
                .Replace("{{Header}}", this.header)
                .Replace("{{ProjectName}}", apiProjectName)
                .Replace("{{NameSpace}}", @namespace)
                .Replace("{{ModelName}}", modelName)
                .Replace("[[ModelName]]", modelName)
                .Replace("{{modelName}}", modelName.ToLowFirstLetter())
                .Replace("{{ModelNamePlural}}", modelNamePlural);

            var filePath = Path.Combine(modelTestsPath, $"{modelName}ServiceTests.Validations.Add.cs");
            File.Create(filePath).Close();
            var fileStream = File.Open(filePath, FileMode.Open, FileAccess.Write);

            fileStream.Write(Encoding.UTF8.GetBytes(content));
            fileStream.Flush();
            fileStream.Close();
        }

        private void AddExceptionsTests(string modelName, string modelNamePlural, string modelTestsPath)
        {
            var testsPath = Directory.GetFiles(Path.Combine("..", "..", "..", StringConstants.Source, StringConstants.Tests, StringConstants.Exceptions));

            var @namespace = $"{testProjectName}.Services.Foundations.{modelNamePlural}";

            foreach (var testPath in testsPath)
            {
                var testName = testPath.Split("\\").Last().Replace(".txt", ".cs").Replace("Entity", modelName);
                var filePath = Path.Combine(modelTestsPath, testName);
                File.Create(filePath).Close();
                var fileStream = File.Open(filePath, FileMode.Open, FileAccess.Write);

                var content = File.ReadAllText(testPath)
                    .Replace("{{Header}}", this.header)
                    .Replace("{{ProjectName}}", apiProjectName)
                    .Replace("{{NameSpace}}", @namespace)
                    .Replace("{{ModelName}}", modelName)
                    .Replace("{{modelName}}", modelName.ToLowFirstLetter())
                    .Replace("{{ModelNamePlural}}", modelNamePlural);

                fileStream.Write(Encoding.UTF8.GetBytes(content));
                fileStream.Flush();
                fileStream.Close();
            }
        }

        private void AddLogicTests(string modelName, string modelNamePlural, string modelTestsPath)
        {
            var testsPath = Directory.GetFiles(
                Path.Combine("..", "..", "..", StringConstants.Source, StringConstants.Tests, StringConstants.Logic));

            var @namespace = $"{testProjectName}.Services.Foundations.{modelNamePlural}";

            foreach (var testPath in testsPath)
            {
                var testName = testPath.Split("\\").Last();
                var filePath = Path.Combine(modelTestsPath, testName.Replace(".txt", ".cs").Replace("Entity", modelName));
                File.Create(filePath).Close();
                var fileStream = File.Open(filePath, FileMode.Open, FileAccess.Write);
                var content = File.ReadAllText(testPath)
                    .Replace("{{Header}}", this.header)
                    .Replace("{{ApiProjectName}}", apiProjectName)
                    .Replace("{{NameSpace}}", @namespace)
                    .Replace("{{ModelName}}", modelName)
                    .Replace("{{modelName}}", modelName.ToLowFirstLetter())
                    .Replace("{{ModelNamePlural}}", modelNamePlural);
                fileStream.Write(Encoding.UTF8.GetBytes(content));
                fileStream.Flush();
                fileStream.Close();
            }

        }

        private void AddTestsFile(string modelName, string modelNamePlural, string modelTestsPath)
        {
            var content = File.ReadAllText(
                Path.Combine("..", "..", "..", StringConstants.Source, StringConstants.Tests, "EntityServiceTests.txt"))
                    .Replace("{{Header}}", this.header)
                    .Replace("{{ApiProjectName}}", this.apiProjectName)
                    .Replace("{{ModelNamePlural}}", modelNamePlural)
                    .Replace("{{NameSpace}}", $"{testProjectName}.{StringConstants.Services}.{StringConstants.Foundations}.{modelNamePlural}")
                    .Replace("{{ModelName}}", modelName)
                    .Replace("{{modelName}}", modelName.ToLowFirstLetter());

            var filePath = Path.Combine(modelTestsPath, $"{modelName}ServiceTests.cs");
            File.Create(filePath).Close();
            var fileStream = File.Open(filePath, FileMode.Open, FileAccess.Write);
            fileStream.Write(Encoding.UTF8.GetBytes(content));
            fileStream.Flush();
            fileStream.Close();
        }

        internal void BuildBuildProject()
        {
            Process.ExecuteCommand($"dotnet run --project {buildProjectPath}");
        }
    }
}