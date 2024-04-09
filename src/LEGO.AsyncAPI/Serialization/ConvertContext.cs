// Copyright (c) The LEGO Group. All rights reserved.
#nullable enable

namespace LEGO.AsyncAPI.Serialization
{
    using System.Collections.Generic;
    using LEGO.AsyncAPI.Json;

    /// <summary>
    /// Used to keep track of conver operations.
    /// </summary>
    internal readonly ref struct ConvertContext
    {
        /// <summary>
        /// Contains the map of the refs that need to be fixed post convert.
        /// </summary>
        public readonly IDictionary<RefPath, RefPath> Refs;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConvertContext"/> struct.
        /// </summary>
        public ConvertContext()
        {
            this.Refs = new Dictionary<RefPath, RefPath>();
        }
    }
}
