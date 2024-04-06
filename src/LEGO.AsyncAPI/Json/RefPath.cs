// Copyright (c) The LEGO Group. All rights reserved.
#nullable enable

namespace LEGO.AsyncAPI.Json
{
    using System.Text;
    using System.Text.Json.Nodes;

    /// <summary>
    /// Repersents a string that acts as a reference to a section within this document or
    /// to an external one.
    /// </summary>
    public struct RefPath
    {
        /// <summary>
        /// The seperator used to split path components.
        /// </summary>
        public const char Seperator = '/';

        /// <summary>
        /// The vlaue of the path.
        /// </summary>
        public readonly string Value;

        /// <summary>
        /// Initializes a new instance of the <see cref="RefPath"/> struct.
        /// </summary>
        /// <param name="components">The seperate componets of the path.</param>
        public RefPath(params string[] components)
        {
            if (components.Length == 1)
            {
                this = new RefPath(components[0], true);
            }
            else
            {
                StringBuilder builder = new StringBuilder();
                builder.Append("#");
                foreach (string component in components)
                {
                    builder.Append('/');
                    builder.Append(component);
                }
                this.Value = builder.ToString();
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RefPath"/> struct.
        /// </summary>
        /// <param name="path">A path that should be created.</param>
        /// <param name="normalize">True if the path should be normalized otherwise false.</param>
        private RefPath(string path, bool normalize)
        {
            if (normalize)
            {
                string[] components = path.Split('.', '/', '\\');
                this = new RefPath(components);
            }
            else
            {
                Value = path;
            }
        }

        /// <summary>
        /// Converts a string iplicitly into a <see cref="RefPath"/>.
        /// </summary>
        /// <param name="path">The string to convert into a path.</param>
        public static implicit operator RefPath(string path)
            => new RefPath(path, true);

        /// <inheritdoc cref="object"/>
        public override int GetHashCode()
        => this.Value.GetHashCode();

        /// <inheritdoc cref="object"/>
        public override bool Equals(object obj)
            => obj is RefPath other &&
                other.Value == this.Value;

        /// <summary>
        /// Turns the path into a <see cref="JsonValue"/>.
        /// </summary>
        /// <returns>The created value.</returns>
        public JsonValue ToJsonValue()
            => JsonValue.Create<string>(this.Value)!;

        /// <summary>
        /// Converts this into a <see cref="JsonProperty"/> that is a reference.
        /// </summary>
        /// <returns>The created proeprty.</returns>
        public JsonProperty<JsonValue> ToProperty()
            => new JsonProperty<JsonValue>("$ref", this.ToJsonValue());
    }
}
