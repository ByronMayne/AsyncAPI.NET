// Copyright (c) The LEGO Group. All rights reserved.
namespace LEGO.AsyncAPI.Json
{
#nullable enable

    using System.Text.Json.Nodes;

    /// <summary>
    /// Defines a value pair for json but the vlaue is an explictly typed <see cref="JsonNode"/> type.
    /// </summary>
    /// <typeparam name="T">The type of the json node.</typeparam>
    /// <param name="Name">The name of the property.</param>
    /// <param name="Value">The value of the property.</param>
    public class JsonProperty<T> : JsonProperty
        where T : JsonNode
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="JsonProperty{T}"/> class.
        /// </summary>
        /// <param name="name">The name of the property.</param>
        /// <param name="value">The value of the property.</param>
        public JsonProperty(string name, T value)
            : base(name, value)
        {
        }

        /// <summary>
        /// The value of the property.
        /// </summary>
        public new T Value => (T)base.Value!;
    }
}
