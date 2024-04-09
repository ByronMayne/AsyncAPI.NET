// Copyright (c) The LEGO Group. All rights reserved.

namespace LEGO.AsyncAPI.Tests.Serialization
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text.Json;
    using System.Text.Json.Nodes;
    using FluentAssertions;
    using LEGO.AsyncAPI.Json;
    using LEGO.AsyncAPI.Readers;
    using LEGO.AsyncAPI.Serialization;
    using NUnit.Framework;
    using YamlDotNet.RepresentationModel;

    public class Migrationv3Tests : TestBase
    {
        private static readonly string Metadata260;
        private static readonly string Metadata300;
        private static readonly string ServerUrl260;
        private static readonly string ServerUrl300;
        private static readonly string Operations260;
        private static readonly string Operations300;
        private static readonly string Channels260;

        static Migrationv3Tests()
        {
            Metadata260 = "MigrateV3_Metadata_2.6.0.yml";
            Metadata300 = "MigrateV3_Metadata_3.0.0.yml";
            ServerUrl260 = "MigrateV3_ServerUrl_2.6.0.yml";
            ServerUrl300 = "MigrateV3_ServerUrl_3.0.0.yml";
            Operations260 = "MigrateV3_Operations_2.6.0.yml";
            Operations300 = "MigrateV3_Operations_3.0.0.yml";
            Channels260 = "MigrateV3_Channels_2.6.0.yaml";
        }

        private enum Direction
        {
            Up,
            Down,
        }

        [Test]
        public void MigrateUp_Tags_MovedUnderInfo()
            => this.Compose(
                        resourceName: Metadata260,
                        direction: Direction.Up,
                        this.AssertPathValue(path: "$.info.tags[0].name", expected: "e-commerce"));

        [Test]
        public void MigrateDown_Tags_MovedOutFromInfo()
            => this.Compose(
                    resourceName: Metadata300,
                    direction: Direction.Down,
                    this.AssertPathValue(path: "$.tags[0].name", expected: "e-commerce"));

        [Test]
        public void MigrateUp_ExternalDocs_MovedUnderInfo()
            => this.Compose(
                    resourceName: Metadata260,
                    direction: Direction.Up,
                    this.AssertPathValue(path: "$.info.externalDocs.url", expected: "https://www.asyncapi.com"));

        [Test]
        public void MigrateDown_ExternalDocs_MovedUnderInfo()
            => this.Compose(
                    resourceName: Metadata300,
                    direction: Direction.Down,
                    this.AssertPathValue(path: "$.externalDocs.url", expected: "https://www.asyncapi.com"),
                    this.AssertPathValue(path: "$.externalDocs.description", expected: "Find more info here"));

        [Test]
        public void MigrateUp_ServerUrl_IsSplitUp()
            => this.Compose(
                    resourceName: ServerUrl260,
                    direction: Direction.Up,
                    this.AssertPathValue("$.servers.production.host", "rabbitmq.in.mycompany.com:5672"),
                    this.AssertPathValue("$.servers.production.pathname", "/production"),
                    this.AssertPathValue("$.servers.production.protocol", "amqp"));

        [Test]
        public void MigrateDown_ServerUrl_IsCombined()
            => this.Compose(
                    resourceName: ServerUrl300,
                    direction: Direction.Down,
                    this.AssertPathValue("$.servers.production.url", "amqp://rabbitmq.in.mycompany.com:5672/production"),
                    this.AssertPathValue("$.servers.production.protocol", "amqp"));

        [Test]
        public void MigrateUp_Opeartions_MovedOutFromChannels()
            => this.Compose(
                resourceName: Operations260,
                direction: Direction.Up,
                this.AssertNodeExists<JsonObject>("$.operations.ConsumeUserSignups"));

        [Test]
        public void MigrateDown_Opeartions_MovedUnderChannel()
            => this.Compose(
                resourceName: Operations300,
                direction: Direction.Down,
                this.AssertNodeExists<JsonObject>("$.channels.user/publish"));

        [Test]
        public void MigrateUp_Oeprations_ActionUpdated()
            => this.Compose(
                resourceName: Operations260,
                direction: Direction.Up,
                this.AssertPathValue("$.operations.ConsumeUserSignups.action", "receive"));

        [Test]
        public void MigrateUp_Properties_AddressIsAssigned()
            => this.Compose(
                resourceName: "Migratev3Parameters.yml",
                direction: Direction.Up,
                this.AssertPathValue("$.channels.userSignedUp.address", "user/{user_id}/signedup"));

        private void Compose(
            string resourceName,
            Direction direction,
            params Action<JsonObject>[] asserts)
        {
            this.Log($"Resource: {resourceName}");
            StreamReader source = this.GetTestData<StreamReader>(resourceName);

            YamlStream yamlStream = new YamlStream();
            yamlStream.Load(source);
            IList<JsonObject> documents = yamlStream.Documents
                .Select(d => d.ToJsonNode(new AsyncApiReaderSettings()))
                .OfType<JsonObject>()
                .ToList();

            MigrationV3 migration = new ();
            JsonObject expectedJsonObject;
            JsonObject actualJsonValue;

            if (direction == Direction.Up)
            {
                expectedJsonObject = documents[1];
                actualJsonValue = migration.Up(documents[0]);
            }
            else
            {
                expectedJsonObject = documents[0];
                actualJsonValue = migration.Down(documents[1]);
            }

            JsonSerializerOptions serializerOptions = new JsonSerializerOptions()
            {
                WriteIndented = true,
            };

            string actual = actualJsonValue.ToJsonString(serializerOptions);
            string expected = expectedJsonObject.ToJsonString(serializerOptions);

            this.Log("--------- Expected ---------");
            this.Log(expected);
            this.Log("--------- Actual ----------");
            this.Log(actual);
            this.Log("---------------------------");


            actual
                .Should()
                .BePlatformAgnosticEquivalentTo(expected);


            if (asserts != null)
            {
                for (int i = 0; i < asserts.Length; i++)
                {
                    this.Log($"=========== Assert: {i + 1} ============");
                    asserts[i](expectedJsonObject);
                    this.Log("=====================================");
                }
            }
        }

        private Action<JsonObject> AssertNodeExists<T>(string path)
        {
            return (target) =>
            {
                Type type = typeof(T);
                this.Log($"Path: {path}");
                this.Log($"Expected: {type.Name}");
                JsonPath jsonPath = new JsonPath(path);
                JsonNode node = jsonPath.Evaluate(target);
                Assert.NotNull(node);
            };
        }

        /// <summary>
        /// Returns an assert that is used for evaluating a path expectedJsonObject make sure it not only. 
        /// exists but also has the startingJsonObject value.
        /// </summary>
        /// <typeparam name="T">The values type expectedJsonObject expect.</typeparam>
        /// <param name="path">The path expectedJsonObject the value.</param>
        /// <param name="expected">The startingJsonObject value of the property.</param>
        /// <returns>The delegate that will be invoked expectedJsonObject test it.</returns>
        private Action<JsonObject> AssertPathValue<T>(string path, T expected)
        {
            return (target) =>
            {
                this.Log($"Path: {path}");
                this.Log($"Expected: {expected}");

                JsonPath jsonPath = new JsonPath(path);
                JsonNode node = jsonPath.Evaluate(target);
                Assert.NotNull(node);
                T actualValue = node.GetValue<T>();
                this.Log($"Actual: {actualValue}");
                Assert.AreEqual(expected, actualValue);
            };
        }
    }
}
