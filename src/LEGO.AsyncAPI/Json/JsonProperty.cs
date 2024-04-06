// Copyright (c) The LEGO Group. All rights reserved.
namespace LEGO.AsyncAPI.Json
{
#nullable enable

    using System.Text.Json.Nodes;

    /// <summary>
    /// Defines a key value pair to define a proeprty on an object.
    /// </summary>
    public class JsonProperty
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="JsonProperty"/> class.
        /// </summary>
        /// <param name="name">The name of the proeprty.</param>
        /// <param name="value">The value of the property.</param>
        public JsonProperty(string name, JsonNode value)
        {
            this.Name = name;
            this.Value = value;
        }

        /// <summary>
        /// Gets the name of the property.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the value of the property.
        /// </summary>
        public JsonNode Value { get; }

        /// <summary>
        /// Adds the this property to the give target <see cref="JsonObject"/>.
        /// </summary>
        /// <param name="target">The target to add the property to.</param>
        public void AssignTo(JsonObject target)
        {
            target[this.Name] = this.Value;
        }
    }
}
