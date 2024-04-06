// Copyright (c) The LEGO Group. All rights reserved.

namespace LEGO.AsyncAPI.Json
{
    using System;
    using System.ComponentModel;
    using System.Text.Json;
    using System.Text.Json.Nodes;

    /// <summary>
    /// Contains extension methods for working with <see cref="JsonValue"/>.
    /// </summary>
    internal static class JsonValueExtensions
    {
        /// <summary>
        /// Attempts to get the value from a json node but supports converting it if possible.
        /// </summary>
        /// <typeparam name="T">The kind to get it.</typeparam>
        /// <param name="jsonValue">The value to convert.</param>
        /// <returns>The result.</returns>
        public static T GetValue<T>(this JsonValue jsonValue)
        {
            JsonValueKind kind = jsonValue.GetValueKind();

            switch (kind)
            {
                case JsonValueKind.Null:
                    return default;
                case JsonValueKind.True:
                    return (T)(object)true;
                case JsonValueKind.False:
                    return (T)(object)false;
                case JsonValueKind.String:
                    if (typeof(T) == typeof(string))
                    {
                        return (T)(object)jsonValue.GetValue<string>();
                    }

                    TypeConverter typeConverter = TypeDescriptor.GetConverter(typeof(T));
                    if (typeConverter.CanConvertFrom(typeof(string)))
                    {
                        return (T)typeConverter.ConvertFrom(jsonValue.GetValue<string>());
                    }

                    throw new InvalidCastException($"Unable to convert from string to {typeof(T).Name} as no converter has been defined.");
                default:
                    throw new NotImplementedException($"No converter implemented for {kind} to {typeof(T).FullName}");
            }
        }
    }
}
