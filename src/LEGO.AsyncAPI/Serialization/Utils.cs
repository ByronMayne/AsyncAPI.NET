// Copyright (c) The LEGO Group. All rights reserved.

namespace LEGO.AsyncAPI.Serialization
{
    using System.Text;
    using System.Text.Json;
    using System.Text.Json.Nodes;
    using LEGO.AsyncAPI.Json;

    /// <summary>
    /// Contains utils for working with AsyncApi nodes
    /// </summary>
    internal static class Utils
    {
        private const string RefPropertyName = "$ref";
        private const char RefValuePrefix = '#';

        /// <summary>
        /// Creates a new <see cref="JsonObject"/> that is a reference to another.
        /// </summary>
        /// <param name="components">The path components.</param>
        /// <returns>The json ref object.</returns>
        public static JsonObject CreateRefObject(RefPath propertyPath)
        {
            return new JsonObject()
            {
                [RefPropertyName] = propertyPath.Value,
            };
        }
    }
}
