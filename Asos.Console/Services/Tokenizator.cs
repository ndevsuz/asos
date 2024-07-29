namespace Easy.Services
{
    public class Tokenizator
    {
        public static Dictionary<string, string> GetProperties(string modelPath)
        {
            var props = new Dictionary<string, string>();

            var modelLines = File.ReadAllLines(modelPath);

            foreach (var line in modelLines)
            {
                var splittedLine = line.Trim().Split(" ");

                if (splittedLine[0] == "public" && splittedLine[1] != "class")
                {
                    props.Add(splittedLine[2], splittedLine[1]);
                }
            }

            return props;
        }

        public static (string Property, string Type) GetNamePropery(Dictionary<string, string> properties)
        {
            var nameProperties = new string[]
            {
            "name",
            "firstname",
            "lastname",
            "title",
            "fullname",
            "displayname",
            "username",
            "nickname",
            };

            foreach (var (key, value) in properties)
            {
                if (nameProperties.Contains(key.Trim().ToLower()))
                {
                    return (key, value);
                }
            }

            return (string.Empty, string.Empty);
        }
    }

}
