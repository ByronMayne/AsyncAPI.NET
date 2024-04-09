// Copyright (c) The LEGO Group. All rights reserved.
#nullable enable

namespace LEGO.AsyncAPI.Json
{
    using System.Text.Json.Nodes;
    using System.Collections.Generic;

    /// <summary>
    /// Contains extensions methods for <see cref="JsonProperty"/> and <see cref="JsonProperty{T}"/> types.
    /// </summary>
    internal static class JsonPropertyExtensions
    {
        /// <summary>
        /// Returns back if the property is an extension type, or one named `x-`. 
        /// </summary>
        /// <param name="jsonProperty">The property to check.</param>
        /// <returns>True if it is an extension ortherwise false</returns>
        public static bool IsExtension(this JsonProperty jsonProperty)
        {
            string name = jsonProperty.Name;
            return name.StartsWith("x-");
        }

        /// <summary>
        /// Takes an collection of <see cref="JsonProperty{T}"/> and merges them into a single object.
        /// </summary>
        /// <typeparam name="T">The type of. </typeparam>
        /// <param name="properties">The jsonProperties to merge.</param>
        /// <returns>The created object.</returns>
        public static JsonObject ToJsonObject(this IEnumerable<JsonProperty> properties)
        {
            JsonObject result = new();

            foreach (JsonProperty property in properties)
            {
                result[property.Name] = property.Value;
            }

            return result;
        }

        /// <summary>
        /// Assigns all the json property to the given <see cref="JsonObject"/>.
        /// </summary>
        /// <param name="properties">The proeprties to assign.</param>
        /// <param name="target">The target to set the values on.</param>
        public static void AssignTo(this IEnumerable<JsonProperty> properties, JsonObject target)
        {
            foreach (JsonProperty property in properties)
            {
                property.AssignTo(target);
            }
        }
    }
}
