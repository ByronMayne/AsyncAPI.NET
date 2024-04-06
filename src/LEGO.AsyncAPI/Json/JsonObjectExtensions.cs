// Copyright (c) The LEGO Group. All rights reserved.
#nullable enable

namespace LEGO.AsyncAPI.Json
{
    using System;
    using System.Collections.Generic;
    using System.Text.Json.Nodes;
    using LEGO.AsyncAPI.Serialization;

    /// <summary>
    /// Contains extension methods for working with <see cref="JsonObject"/>.
    /// </summary>
    internal static class JsonObjectExtensions
    {
        /// <summary>
        /// Gets if this json object is a $Ref.
        /// </summary>
        /// <param name="target">The target to check.</param>
        /// <returns>True if it's a ref otherwise false</returns>
        public static bool IsRef(this JsonObject target)
            => target.Count == 1 &&
            target.TryGetPropertyValue("$ref", out JsonNode? _);

        /// <summary>
        /// Gets all the properties on the given object.
        /// </summary>
        /// <param name="target">The target to get the properties on.</param>
        /// <returns>The collection of properties.</returns>
        public static IEnumerable<JsonProperty> Properties(this JsonObject? target)
            => Properties<JsonNode>(target);

        /// <summary>
        /// Gets all the properties on the given object.
        /// </summary>
        /// <typeparam name="T">The type of nodes to get.</typeparam>
        /// <param name="target">The target to get the properties on.</param>
        /// <returns>The collection of properties.</returns>
        public static IEnumerable<JsonProperty<T>> Properties<T>(this JsonObject? target)
            where T : JsonNode
        {
            if (target == null)
            {
                yield break;
            }

            IDictionary<string, JsonNode?> memberMap = target;

            foreach (KeyValuePair<string, JsonNode?> member in memberMap)
            {
                if (member.Value is T asType)
                {
                    yield return new JsonProperty<T>(member.Key, asType);
                }
            }
        }

        /// <summary>
        /// Enumerates each properties of the <see cref="JsonObject"/> and invokes a callback for
        /// each non-null member that is defined.
        /// </summary>
        /// <param name="source">The source to enumerate.</param>
        /// <param name="action">The action to call.</param>
        public static void ForEachProperty(this JsonObject source, Action<JsonProperty> action)
        {
            ForEachProperty<JsonNode>(source, prop => action(new JsonProperty(prop.Name, prop.Value)));
        }

        /// <summary>
        /// Loops over all properties and allows you to modify each one of them replacing each value.
        /// </summary>
        /// <typeparam name="T">TThe type of the properties.</typeparam>
        /// <param name="target">The target to get the properties from.</param>
        /// <param name="action">The action to preform.</param>
        /// <exception cref="ArgumentNullException">no action was defined.</exception>
        public static void ReplaceProperties<T>(this JsonObject target, Func<JsonProperty<T>, T> action)
            where T : JsonNode
        {
            if (target == null)
            {
                return;
            }

            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            foreach (JsonProperty<T> property in Properties(target))
            {
                T replacement = action(property);
                target[property.Name] = replacement;
            }
        }

        /// <summary>
        /// Loops over all members of the <see cref="JsonObject"/> if it is not null an calls
        /// the <see cref="Action"/> on it.
        /// </summary>
        /// <typeparam name="T">The types of members to invoke the action on.</typeparam>
        /// <param name="target">The target to attempt to get the values from.</param>
        /// <param name="action">The action to invoke for each one.</param>
        /// <exception cref="ArgumentNullException">There was no action defined.</exception>
        public static void ForEachProperty<T>(this JsonObject? target, Action<JsonProperty<T>> action)
            where T : JsonNode
        {
            if (target == null)
            {
                return;
            }

            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            IDictionary<string, JsonNode?> memberMap = target;

            foreach (KeyValuePair<string, JsonNode?> member in memberMap)
            {
                if (member.Value is T asNode)
                {
                    JsonProperty<T> property = new(member.Key, asNode);
                    action(property);
                }
            }
        }

        /// <summary>
        /// Creates a deep clone of another <see cref="JsonObject"/> but excludes the given properties.
        /// </summary>
        /// <param name="source">The source to copy from.</param>
        /// <param name="excluding">The member names to exclude.</param>
        /// <returns>The deep cloned object.</returns>
        public static JsonObject Copy(this JsonObject source, params string[] exclude)
        {
            return source.Copy(true, exclude);
        }

        /// <summary>
        /// Creates a deep clone of another <see cref="JsonObject"/> but excludes the given properties.
        /// </summary>
        /// <param name="source">The source to copy from.</param>
        /// <param name="ignoreCase">If true the casing will be ignored.</param>
        /// <param name="exclude">The member names to exclude.</param>
        /// <returns>The deep cloned object.</returns>
        public static JsonObject Copy(this JsonObject source, bool ignoreCase, params string[] exclude)
        {
            StringComparer comparer = ignoreCase
                ? StringComparer.OrdinalIgnoreCase
                : StringComparer.Ordinal;

            HashSet<string> set = new(exclude, comparer);
            JsonObject clone = new();
            source.ForEachProperty(property =>
            {
                if (!set.Contains(property.Name))
                {
                    clone[property.Name] = property.Value.DeepClone();
                }
            });
            return clone;
        }

        /// <summary>
        /// Deep clones the json object and returns the result.
        /// </summary>
        /// <param name="source">The source to clone</param>
        /// <returns>The new copy.</returns>
        public static new JsonObject DeepClone(this JsonObject source)
            => (JsonObject)source.DeepClone();

        /// <summary>
        /// Attempts to get a property value based on it's name and casts the node.
        /// </summary>
        /// <typeparam name="T">The type of property value.</typeparam>
        /// <param name="target">The object to try to get the property from.</param>
        /// <param name="propertyName">The name of the property.</param>
        /// <param name="value">The value casted as the correct type.</param>
        /// <returns>True if thevalue could be found and casted otherwise false.</returns>
        public static bool TryGetPropertyValue<T>(this JsonObject? target, string? propertyName, out T? value)
        {
            value = default;

            if (target == null)
            {
                return false;
            }

            if (propertyName == null)
            {
                return false;
            }

            value = default;
            if (target.TryGetPropertyValue(propertyName, out JsonNode? jsonNode))
            {
                value = jsonNode!.GetValue<T>();
                return true;
            }

            return false;
        }

        /// <summary>
        /// Attempts to get the child by the given name of the node, if the child
        /// is not found an exception will be thrown.
        /// </summary>
        /// <typeparam name="T">The type of the child.</typeparam>
        /// <param name="parent">The node to get the child from.</param>
        /// <param name="propertyName">The name of the child.</param>
        /// <returns>The child.</returns>
        /// <exception cref="ChildNotFoundException">There was no child with the given name found.</exception>
        public static T GetProperty<T>(this JsonObject parent, string propertyName)
            where T : JsonNode
        {
            JsonNode? jChild = parent[propertyName];

            if (jChild == null)
            {
                throw new ChildNotFoundException(parent, propertyName);
            }

            T castedType = jChild.As<T>();

            return castedType;
        }
    }
}