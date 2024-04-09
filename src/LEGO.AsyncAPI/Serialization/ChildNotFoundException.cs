#nullable enable
using System;
using System.Linq;
using System.Text.Json.Nodes;

namespace LEGO.AsyncAPI.Serialization
{
    /// <summary>
    /// Exception thrown when a child is request but it was not found
    /// </summary>
    internal class ChildNotFoundException : Exception
    {
        /// <inheritdoc cref="Exception"/>
        public override string Message { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ChildNotFoundException"/> class.
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="childName"></param>
        public ChildNotFoundException(JsonObject parent, string childName)
        {
            string[] children = Enumerable.Range(0, parent.Count)
                .Select(i => parent[i])
                .OfType<JsonNode>()
                .Select(n => n.GetPropertyName())
                .ToArray();

            string nameList = string.Join("\n - ", children);
            this.Message = $"No child was found with the name '{childName}' under parent '{parent.GetPropertyName()}'. The following children are defined";
        }
    }
}
