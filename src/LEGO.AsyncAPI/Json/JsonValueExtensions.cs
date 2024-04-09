// Copyright (c) The LEGO Group. All rights reserved.
#nullable enable

namespace LEGO.AsyncAPI.Json
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Reflection;
    using System.Text.Json;
    using System.Text.Json.Nodes;

    /// <summary>
    /// Contains extension methods for working with <see cref="JsonValue"/>.
    /// </summary>
    internal static class JsonValueExtensions
    {
        /// <summary>
        /// Contains the Method info for <see cref="JsonValue"/>.GetValue.
        /// </summary>
        internal static readonly MethodInfo JsonGetValueMethod;
        private static Dictionary<Type, MethodInfo> methodCache;

        static JsonValueExtensions()
        {
            JsonGetValueMethod = typeof(JsonValue)
                .GetMethod(nameof(JsonValue.GetValue))!;
            methodCache = new Dictionary<Type, MethodInfo>();
        }

        /// <summary>
        /// Attempts to get the value from a json node but supports converting it if possible.
        /// </summary>
        /// <typeparam name="T">The kind to get it.</typeparam>
        /// <param name="jsonValue">The value to convert.</param>
        /// <returns>The result.</returns>
        public static T? GetValue<T>(this JsonValue jsonValue)
        {
            if (jsonValue == null)
            {
                throw new ArgumentNullException(nameof(jsonValue));
            }

            Type targetType = typeof(T);
            Type jsonType = jsonValue.GetType();
            Type? valueType = jsonType
                .GetGenericArguments()
                .FirstOrDefault();

            if (valueType == null)
            {
                throw new ArgumentException($"Unexpected type {jsonType.FullName}");
            }

            if (!methodCache.TryGetValue(valueType, out MethodInfo? getValueMethod))
            {
                getValueMethod = JsonGetValueMethod.MakeGenericMethod(valueType);
                methodCache[valueType] = getValueMethod;
            }

            object? rawValue = getValueMethod.Invoke(jsonValue, null);

            TypeConverter typeConverter = TypeDescriptor.GetConverter(targetType);
            if (typeConverter.CanConvertFrom(valueType))
            {
                return (T)typeConverter.ConvertFrom(rawValue!)!;
            }

            return default!;
        }
    }
}
