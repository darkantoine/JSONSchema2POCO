using System;
using System.IO;
using System.Text.Json;
using ClassGenerator;
using JSONSchema2POCO;

namespace ClassGeneratorApp
{
    class Program
    {
        static void Main(string[] args)
        {
            var inputFile = args[0];
            string fullPath;
            try
            {
                fullPath = Path.GetFullPath(inputFile);
            }
            catch
            {
                Console.WriteLine(string.Format("{0} is not a valid File Path", inputFile));
                return;
            }

            string inputJson;

            try
            {
                inputJson= File.ReadAllText(fullPath);
            }
            catch
            {
                Console.WriteLine(string.Format("{0} cannot be read or is not a valid file", inputFile));
                return;
            }

            JsonDocument inputSchema;
            try
            {
                inputSchema = JsonDocument.Parse(inputJson);
            }
            catch
            {
                Console.WriteLine(string.Format("{0} is not a valid json", inputFile));
                return;
            }
            
            var schema = new JSONSchema(inputSchema);

            var generator = new ClassGeneratorFromJsonSchema(schema, schema.Title ?? "GeneratedClass");
            generator.GenerateAll();
            var results = generator.PrintAll();

            string outputFolder = args[1];
            Directory.CreateDirectory(outputFolder);

            string[] existingFiles = Directory.GetFiles(outputFolder);
            foreach (string file in existingFiles)
            {
                if (file.EndsWith(".cs"))
                {
                    File.Delete(file);
                }
            }

            foreach (var result in results)
            {
                string filePath = Path.Combine(outputFolder, string.Format("{0}.cs", result.Key));
                File.WriteAllText(filePath, result.Value);
            }
        }
    }
}
