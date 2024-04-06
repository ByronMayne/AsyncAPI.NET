// Copyright (c) The LEGO Group. All rights reserved.
#nullable enable

namespace LEGO.AsyncAPI.Json
{
    using System;
    using System.Text.Json.Nodes;

    /// <summary>
    /// Contains extension methods for working with <see cref="JsonNode"/> type.
    /// </summary>
    internal static class JsonNodeExtensions
    {
        /// <summary>
        /// Clones a property from one object to another.
        /// </summary>
        /// <param name="source">The source where to find the property on.</param>
        /// <param name="propertyName">The name of the property.</param>
        /// <param name="target">The target to coypy it to.</param>
        public static void ClonePropTo(this JsonObject? source, string? propertyName, JsonObject target)
        {
            if (source == null)
            {
                return;
            }

            if (propertyName == null)
            {
                return;
            }

            if (source.TryGetPropertyValue(propertyName, out JsonNode? propertyValue))
            {
                target[propertyName] = propertyValue!.DeepClone();
            }
        }

        /// <summary>
        /// Attempts to cast a <see cref="JsonNode"/> into another <see cref="JsonNode"/> type. This is used
        /// to leverage the already defined exceptions impelemented in the base library.
        /// </summary>
        /// <typeparam name="T">The type to convert to.</typeparam>
        /// <param name="node">The node to convert.</param>
        /// <returns>The converted noe.</returns>
        /// <exception cref="InvalidOperationException">The node is not the correct type.</exception>
        public static T As<T>(this JsonNode node)
            where T : JsonNode
        {
            JsonNode result;
            Type type = typeof(T);

            result = type == typeof(JsonNode)
                ? node
                : type == typeof(JsonObject)
                    ? node.AsObject()
                    : type == typeof(JsonValue)
                                    ? node.AsValue()
                                    : type == typeof(JsonArray)
                                                    ? (JsonNode)node.AsArray()
                                                    : throw new NotImplementedException($"No cast defined for converting {nameof(JsonNode)} into a '{type.FullName}'");

            return (T)result;
        }
    }
}
