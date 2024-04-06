// Copyright (c) The LEGO Group. All rights reserved.

#nullable enable

namespace System.Text.Json.Nodes
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.Json;
    using System.Text.Json.Nodes;
    using LEGO.AsyncAPI.Json;
    using LEGO.AsyncAPI.Serialization;

    /// <summary>
    /// Contains extension methods for working with Json.
    /// </summary>
    internal static class JsonExtensions
    {




        /// <summary>
        /// Gets the member of a child and enforces that it is defined otherwise it will throw an exception.
        /// </summary>
        /// <typeparam name="T">The expected type of the child.</typeparam>
        /// <param name="parent">The jChild to get the vhild member from.</param>
        /// <param name="name">The name of the child.</param>
        /// <returns>The member to return.</returns>
        public static T GetChildValue<T>(this JsonObject parent, string name)
        {
            JsonValue jChild = parent.GetProperty<JsonValue>(name);

            return jChild.GetValue<T>();
        }



    }
}
