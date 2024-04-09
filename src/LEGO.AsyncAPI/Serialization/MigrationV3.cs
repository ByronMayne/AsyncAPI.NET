// Copyright (c) The LEGO Group. All rights reserved.
#nullable enable

namespace LEGO.AsyncAPI.Serialization
{
    using System;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using System.Text.Json.Nodes;
    using LEGO.AsyncAPI.Json;

    //internal enum DiagnosticSeverity
    //{
    //    /// <summary>
    //    /// Information that does not indicate a problem (i.e. not prescriptive).
    //    /// </summary>
    //    Info = 1,

    //    /// <summary>
    //    /// Something suspicious but allowed.
    //    /// </summary>
    //    Warning = 2,

    //    /// <summary>
    //    /// Something not allowed by the rules of the language or other authority.
    //    /// </summary>
    //    Error = 3,
    //}

    //internal class DiagnosticDescriptor
    //{
    //    public string Category { get; }
    //    public DiagnosticSeverity Severity { get; }
    //    public string Path { get; }
    //}

    /// <summary>
    /// Used for migration from 2.6.0 to 3.0.0 on the async api schema
    /// </summary>
    internal sealed class MigrationV3
    {
        private const string MetadataVersionKey = "asyncapi";
        private const string MetadataInfoKey = "info";
        private const string MetadataTagsKey = "tags";
        private const string ExternalDocsKey = "externalDocs";
        private const string ServersKey = "servers";

        /// <summary>
        /// Initializes a new instance of the <see cref="MigrationV3"/> class.
        /// </summary>
        public MigrationV3()
        {
        }

        /// <summary>
        /// Migrates a async api 2.x version to 3.x version of the schema.
        /// </summary>
        /// <param name="source">The document to upgrade.</param>
        /// <returns>A new JsonObject that contains the dParameters information.</returns>
        public JsonObject Up(JsonObject source)
        {
            if (source == null)
            {
                throw new ArgumentException(nameof(source));
            }

            ConvertContext context = new ConvertContext();
            JsonObject destination = new();

            this.UpVersion(source, destination, ref context);
            this.UpInfo(source, destination, ref context);
            this.UpMetadata(source, destination, ref context);
            this.UpServer(source, destination, ref context);
            this.UpChannels(source, destination, ref context);

            return destination;
        }

        /// <summary>
        /// Migrates a async api 3.x document into 2.6 version of the schema.
        /// </summary>
        /// <param name="source">The object to convert.</param>
        /// <returns>A new JsonObject that contains the dParameters information.</returns>
        public JsonObject Down(JsonObject source)
        {
            if (source == null)
            {
                throw new ArgumentException(nameof(source));
            }

            ConvertContext context = new ConvertContext();
            JsonObject destination = new();

            this.DownVersion(source, destination, ref context);
            this.DownMetadata(source, destination, ref context);
            this.DownServer(source, destination, ref context);
            this.DownChannels(source, destination, ref context);

            return destination;
        }

        private void UpVersion(JsonObject source, JsonObject destination, ref ConvertContext context)
        {
            destination[MetadataVersionKey] = JsonValue.Create("3.0.0");
        }

        private void DownVersion(JsonObject source, JsonObject destination, ref ConvertContext context)
        {
            destination[MetadataVersionKey] = JsonValue.Create("2.6.0");
        }

        private void UpInfo(JsonObject source, JsonObject destination, ref ConvertContext context)
        {
            if (source.TryGetProperty(MetadataInfoKey, out JsonProperty<JsonObject>? infoProperty))
            {
                destination[infoProperty.Name] = infoProperty.DeepCloneValue();
            }
        }

        private void UpMetadata(JsonObject source, JsonObject destination, ref ConvertContext context)
        {
            if (!source.TryGetProperty(MetadataVersionKey, out JsonProperty<JsonObject>? infoProperty))
            {
                return;
            }

            JsonObject dInfo = infoProperty.DeepCloneValue();

            destination[MetadataInfoKey] = dInfo;

            // Tags
            if (source[MetadataTagsKey] is JsonArray sTags)
            {
                dInfo[MetadataTagsKey] = sTags.DeepClone();
            }

            // External Docs
            if (source[ExternalDocsKey] is JsonObject sExternalDocs)
            {
                dInfo[ExternalDocsKey] = sExternalDocs.DeepClone();
            }
        }

        private void DownMetadata(JsonObject source, JsonObject destination, ref ConvertContext context)
        {
            JsonObject? sInfo = source[MetadataInfoKey] as JsonObject;

            if (sInfo is null)
            {
                return;
            }

            if (sInfo[MetadataTagsKey] is JsonArray sTags)
            {
                destination[MetadataTagsKey] = sTags.DeepClone();
            }

            if (sInfo[ExternalDocsKey] is JsonObject sExternalDocs)
            {
                destination[ExternalDocsKey] = sExternalDocs.DeepClone();
            }
        }

        private void UpServer(JsonObject source, JsonObject destination, ref ConvertContext context)
        {
            if (source[ServersKey] is not JsonObject sServers || sServers.Count == 0)
            {
                return;
            }

            JsonObject dServers = new JsonObject();
            destination[ServersKey] = dServers;

            sServers.ForEachProperty<JsonObject>(sServer =>
            {
                JsonObject dServer = new JsonObject();
                dServers[sServer.Name] = dServer;

                // Copy all members expects for url which has to be split
                sServer.Value.ForEachProperty<JsonNode>(property =>
                {
                    if (property.Name != "url")
                    {
                        dServer[property.Name] = property.Value.DeepClone();
                    }

                    if (Uri.TryCreate(property.Value.GetValue<string>(), UriKind.Absolute, out Uri uri))
                    {
                        dServer["host"] = uri.GetComponents(UriComponents.HostAndPort, UriFormat.Unescaped);
                        dServer["pathname"] = uri.PathAndQuery;
                    }
                });
            });
        }

        private void DownServer(JsonObject source, JsonObject destination, ref ConvertContext context)
        {
            if (source[ServersKey] is not JsonObject sServers || sServers.Count == 0)
            {
                return;
            }

            JsonObject dServers = new JsonObject();
            destination[ServersKey] = dServers;

            sServers.ForEachProperty<JsonObject>(sServer =>
            {
                JsonObject dServer = sServer.Value.Copy("host", "pathname");

                dServers[sServer.Name] = dServer;

                if (sServer.Value.TryGetPropertyValue("host", out JsonNode? sHost))
                {
                    string url = sHost!.GetValue<string>();

                    if (sServer.Value.TryGetPropertyValue("pathname", out JsonNode? sPathName))
                    {
                        url += sPathName!.GetValue<string>();
                    }

                    if (sServer.Value.TryGetPropertyValue("protocol", out JsonNode? sProtocol))
                    {
                        url = $"{sProtocol!.GetValue<string>()}://{url}";
                    }

                    dServer["url"] = url;
                }
            });
        }

        /// <summary>
        /// Split Channel Objects to the Channel Objects and Operation Objects.
        /// </summary>
        /// <param name="source">The srcParameters schema.</param>
        /// <param name="destination">The place to copy them too.</param>
        private void UpChannels(JsonObject source, JsonObject destination, ref ConvertContext context)
        {
            JsonObject dOperations = new JsonObject();
            JsonObject dChannels = new JsonObject();

            if (!source.TryGetPropertyValue("channels", out JsonObject? sChannels))
            {
                return;
            }

            foreach (JsonProperty<JsonObject> sChannel in sChannels.Properties<JsonObject>())
            {
                string channelId = this.GenerateChannelName(sChannel);
                JsonObject dChanel = this.UpChannel(sChannel, destination, false, ref context);
                dChannels[channelId] = dChanel;
            }

            destination["channels"] = dChannels;
            destination["operations"] = dOperations;
        }

        private JsonObject UpChannel(JsonProperty<JsonObject> source, JsonObject document, bool inComponent, ref ConvertContext context)
        {
            RefPath oldPath = inComponent ? $"components/channels/{source.Name}" : $"channels/{source.Name}";
            string channelId = this.GenerateChannelName(source);
            RefPath newPath = inComponent ? $"components/channels/{channelId}" : $"channels/{channelId}";

            context.Refs[oldPath] = newPath;

            JsonObject dChannel = new JsonObject();
            JsonObject sChannel = source.Value;

            if (dChannel.IsRef())
            {
                return dChannel;
            }

            // https://www.asyncapi.com/docs/reference/specification/v3.0.0#channelObject
            dChannel["address"] = source.Name;
            sChannel.ClonePropTo("title", dChannel);
            sChannel.ClonePropTo("summary", dChannel);
            sChannel.ClonePropTo("examples", dChannel);

            // Change server names to refs
            if (sChannel.TryGetProperty("servers", out JsonProperty<JsonObject>? serversProperty))
            {
                dChannel[serversProperty.Name] = serversProperty.Value
                    .Select(s => new JsonProperty<JsonObject>(
                        s.Key,
                        Utils.CreateRefObject($"servers/{s.Key}")))
                    .ToJsonObject();
            }

            if (sChannel.TryGetProperty("parameters", out JsonProperty<JsonObject>? parametersProperty))
            {
                JsonObject dParameters = this.UpParameters(parametersProperty.Value, ref context);
                dChannel[parametersProperty.Name] = dParameters;
            }

            sChannel.ClonePropTo("tags", dChannel);
            sChannel.ClonePropTo("externalDocs", dChannel);
            sChannel.ClonePropTo("bindings", dChannel);
            return dChannel;
        }

        private JsonObject UpParameters(JsonObject srcParameters, ref ConvertContext context)
        {
            JsonObject dstParameters = new JsonObject();
            foreach (JsonProperty<JsonObject> property in srcParameters.Properties<JsonObject>())
            {
                dstParameters[property.Name] = this.UpParameter(property.Value, ref context);
            }

            return dstParameters;
        }

        private JsonObject UpParameter(JsonObject sParameter, ref ConvertContext context)
        {
            if (sParameter.IsRef())
            {
                return sParameter.Copy();
            }

            JsonObject dParameter = new JsonObject();

            if (JsonPath.TryEvaluate(sParameter, "$.schema.enum", out JsonNode? enumValue))
            {
                dParameter["enum"] = enumValue.DeepClone();
            }

            if (JsonPath.TryEvaluate(sParameter, "$.schema.default", out JsonNode? defaultValue))
            {
                dParameter["default"] = defaultValue.DeepClone();
            }

            if (JsonPath.TryEvaluate(sParameter, "$.description", out JsonNode? description))
            {
                dParameter["description"] = description.DeepClone();
            }

            if (JsonPath.TryEvaluate(sParameter, "$.schema.examples", out JsonNode? examples))
            {
                dParameter["examples"] = examples.DeepClone();
            }

            if (JsonPath.TryEvaluate(sParameter, "$.location", out JsonNode? location))
            {
                dParameter["location"] = location.DeepClone();
            }

            sParameter.Properties()
                .Where(JsonPropertyExtensions.IsExtension)
                .AssignTo(dParameter);

            return dParameter;
        }

        private void DownChannels(JsonObject source, JsonObject destination, ref ConvertContext context)
        {

        }

        private string GenerateChannelName(JsonProperty<JsonObject> proeprty)
        {
            if (proeprty.Value.IsRef())
            {
                return proeprty.Name;
            }

            if (!proeprty.Value.AsObject().TryGetPropertyValue("x-channelId", out string? channelId))
            {
                string url = proeprty.Name;
                int curleyBraceStack = 0;
                int letterIndex = 0;
                StringBuilder builder = new StringBuilder();

                foreach (char c in url)
                {
                    if (c == '{')
                    {
                        curleyBraceStack++;
                    }
                    else if (c == '}')
                    {
                        curleyBraceStack--;
                    }
                    else if (curleyBraceStack > 0)
                    {
                        continue;
                    }
                    else if (char.IsLetter(c))
                    {
                        if (letterIndex > 0)
                        {
                            builder.Append(char.ToUpper(c));
                        }
                        else
                        {
                            builder.Append(c);
                        }

                        letterIndex = -1;
                    }
                    else if (char.IsDigit(c))
                    {
                        if (builder.Length == 0)
                        {
                            builder.Append('_');
                        }

                        builder.Append(c);
                    }

                    letterIndex++;
                }

                channelId = builder.ToString();
            }

            return channelId!;
        }
    }
}
