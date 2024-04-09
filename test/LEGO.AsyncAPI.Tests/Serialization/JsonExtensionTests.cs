// Copyright (c) The LEGO Group. All rights reserved.

namespace LEGO.AsyncAPI.Tests.Serialization
{
    using System.Text.Json.Nodes;
    using LEGO.AsyncAPI.Json;
    using NUnit.Framework;

    internal class JsonExtensionTests : TestBase
    {
        [Test]
        public void GetValueMethod_IsFOund()
        {
            Assert.NotNull(JsonValueExtensions.JsonGetValueMethod, $"The method GetValue was not found on type {nameof(JsonValue)}");
        }
    }
}
