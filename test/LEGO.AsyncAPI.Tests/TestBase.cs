// Copyright (c) The LEGO Group. All rights reserved.

namespace LEGO.AsyncAPI.Tests
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Text;
    using System.Text.Json;
    using System.Text.Json.Nodes;
    using FluentAssertions.Formatting;
    using LEGO.AsyncAPI.Readers;
    using NUnit.Framework;
    using YamlDotNet.RepresentationModel;

    /// <summary>
    /// Base class for unit tests across the project. Can contain
    /// helper methods for working with unit tests.
    /// </summary>
    public abstract class TestBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TestBase"/> class.
        /// </summary>
        protected TestBase()
        {
            this.TestContext = TestContext.CurrentContext;
            this.TestDataDirectory = Path.Combine(Environment.CurrentDirectory, "TestData");
        }

        /// <summary>
        /// Gets the directory where we save test data.
        /// </summary>
        protected string TestDataDirectory { get; }

        /// <summary>
        /// Gets the current context of the running text.
        /// </summary>
        protected TestContext TestContext { get; }

        /// <summary>
        /// Writes information to the console which will only be
        /// printed when running in debug mode.
        /// </summary>
        /// <param name="message">The message to print.</param>
        [Conditional("DEBUG")]
        public void Log(string message)
        {
            TestContext.WriteLine(message);
        }


        /// <summary>
        /// Attempts to find the first file that matches the name of the active unit test
        /// and returns it as an expected type.
        /// </summary>
        /// <typeparam name="T">The type to return</typeparam>
        /// <param name="resourceName">The name of the resource file with an optional extension.</param>
        /// <returns>The result</returns>
        protected T GetTestData<T>([CallerMemberName] string resourceName = "")
        {
            string? absolutePath = Path.Combine(this.TestDataDirectory, resourceName);

            if (!File.Exists(absolutePath))
            {
                string extension = Path.GetExtension(resourceName);

                if (string.IsNullOrEmpty(extension))
                {
                    absolutePath = Directory.GetFiles(this.TestDataDirectory, $"{resourceName}.*")
                        .FirstOrDefault();
                }
            }

            if (!File.Exists(absolutePath))
            {
                Assume.That(false, $"Unable to find a test data file named {resourceName} in the directory '{this.TestDataDirectory}'.");
            }

            return this.LoadFromPath<T>(absolutePath);
        }

        private T LoadFromPath<T>(string absolutePath)
        {
            Assume.That(File.Exists(absolutePath), $"No test data file named '{absolutePath}' exists in directory '{absolutePath}'");

            object? result = null;
            Type resultType = typeof(T);

            if (typeof(string) == resultType)
            {
                result = File.ReadAllText(absolutePath);
            }
            else if (typeof(string[]) == resultType)
            {
                result = File.ReadAllLines(absolutePath);
            }
            else if (typeof(Stream) == resultType)
            {
                result = File.OpenRead(absolutePath);
            }
            else if (typeof(StreamReader) == resultType)
            {
                Stream fileStream = this.LoadFromPath<Stream>(absolutePath);
                result = new StreamReader(fileStream);
            }
            else if (typeof(YamlStream) == resultType)
            {
                StreamReader reader = this.LoadFromPath<StreamReader>(absolutePath);
                YamlStream yamlStream = new YamlStream();
                yamlStream.Load(reader);
                result = yamlStream;
            }
            else if (typeof(byte[]) == resultType)
            {
                string content = this.LoadFromPath<string>(absolutePath);
                byte[] bytes = Encoding.UTF8.GetBytes(content);
                result = bytes;
            }
            else if (typeof(JsonObject) == resultType)
            {
                JsonNode jsonNode = this.LoadFromPath<JsonNode>(absolutePath);
                result = jsonNode.AsObject();
            }
            else if (typeof(JsonArray) == resultType)
            {
                JsonNode jsonNode = this.LoadFromPath<JsonNode>(absolutePath);
                result = jsonNode.AsArray();
            }
            else if (typeof(JsonNode) == resultType)
            {
                string fileExtension = Path.GetExtension(absolutePath).ToLower();
                switch (fileExtension)
                {
                    case ".json":
                    case ".jsonc":
                        {
                            Stream stream = this.LoadFromPath<Stream>(absolutePath);
                            result = JsonNode.Parse(stream);
                        }

                        break;
                    case ".yml":
                    case ".yaml":
                        {
                            YamlStream yamls = this.LoadFromPath<YamlStream>(absolutePath);
                            result = yamls.Documents.First().ToJsonNode(new AsyncApiReaderSettings());
                        }

                        break;
                    default:
                        throw new NotImplementedException($"Unable to convert {Path.GetFileName(absolutePath)} to {nameof(JsonNode)}");
                }
            }
            else
            {
                throw new NotImplementedException($"No case has been defined to convering a resource into '{resultType.FullName}'. You can add a new one.");
            }

            return (T)result!;
        }
    }
}
