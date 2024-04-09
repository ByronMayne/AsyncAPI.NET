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
        /// to leverage the already defined exceptions implemented in the base library.
        /// </summary>
        /// <typeparam name="T">The type to convert to.</typeparam>
        /// <param name="node">The node to convert.</param>
        /// <returns>The converted noe.</returns>
        /// <exception cref="InvalidOperationException">The node is not the correct type.</exception>
        public static T As<T>(this JsonNode node)
            where T : JsonNode
            => (T)As(node, typeof(T));

        /// <summary>
        /// Attempts to cast a <see cref="JsonNode"/> into another <see cref="JsonNode"/> type. This is used
        /// to leverage the already defined exceptions implemented in the base library.
        /// </summary>
        /// <param name="node">The node to convert.</param>
        /// <param name="type">The type of node to convert it too.</param>
        /// <returns>The converted noe.</returns>
        /// <exception cref="InvalidOperationException">The node is not the correct type.</exception>
        public static JsonNode As(this JsonNode? node, Type type)
        {
            if (node == null)
            {
                throw new ArgumentNullException(nameof(node));
            }

            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            if (!typeof(JsonNode).IsAssignableFrom(type))
            {
                throw new ArgumentException($"Only types that are of {nameof(JsonNode)} can be used. The type provided was {type.Name}");
            }

            JsonNode result;

            if (typeof(JsonNode) == type)
            {
                result = node;
            }
            else if (typeof(JsonObject) == type)
            {
                result = node.AsObject();
            }
            else if (typeof(JsonArray) == type)
            {
                result = node.AsArray();
            }
            else if (typeof(JsonValue) == type)
            {
                result = node.AsValue();
            }
            else
            {
                throw new NotImplementedException($"No cast defined for converting {nameof(JsonNode)} into a '{type.FullName}'");
            }

            return result;
        }
    }
}
