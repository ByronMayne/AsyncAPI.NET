// Copyright (c) The LEGO Group. All rights reserved.
#nullable enable

namespace LEGO.AsyncAPI.Json
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Text;
    using System.Text.Json.Nodes;
    using System.Text.RegularExpressions;

    /// <summary>
    /// Used to fetch JsonNodes by using string expressions. This is not the full feature sets of JsonPath.
    /// </summary>
    public sealed class JsonPath
    {

        private static readonly Regex AccessorPattern;

        private readonly int count;
        private readonly string path;
        private readonly IReadOnlyList<string> segments;

        static JsonPath()
        {
            AccessorPattern = new Regex(@"^(?<Member>\w*)(\[(?<Index>\d*)\])?$");
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonPath"/> class.
        /// </summary>
        /// <param name="path">The json jsonPath.</param>
        /// <example>$.property.value.</example>
        public JsonPath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentNullException(nameof(path));
            }

            if (path[0] != '$')
            {
                throw new ArgumentException($"Json paths must start with '$' to be value. The value provided was '{path}");
            }

            this.path = path;
            this.segments = path.Split('.');
            this.count = this.segments.Count;
        }

        /// <summary>
        /// Attempts to evaluate json path and return back the value.
        /// </summary>
        /// <typeparam name="T">The type of node to expect.</typeparam>
        /// <param name="target">The target.</param>
        /// <param name="path">The path to it.</param>
        /// <returns>The node or null if it's not found.</returns>
        public static T? Evaluate<T>(JsonNode target, string path)
            where T : JsonNode
        {
            JsonPath jsonPath = new JsonPath(path);
            return jsonPath.Evaluate(target) as T;
        }

        /// <summary>
        /// Tries to evaluate a json node and get back it's value
        /// </summary>
        /// <param name="jsonNode">The node to evaluate form.</param>
        /// <param name="path">The path to evaluate.</param>
        /// <param name="value">The value if it was found.</param>
        /// <returns>True if the node was found otherwise false</returns>
        public static bool TryEvaluate(JsonNode jsonNode, string path, [NotNullWhen(true)] out JsonNode? value)
        {
            JsonPath jsonPath = new JsonPath(path);
            value = null;
            Result result = jsonPath.EvaluateInternal(jsonNode, 1);
            if (result.Error is not null)
            {
                return false;
            }

            value = result.Value!;
            return value != null;
        }

        /// <summary>
        /// Evaluates the json jsonPath and returns back the node it's pointing at.
        /// </summary>
        /// <param name="jsonNode">The node that was found.</param>
        /// <returns>The node it found</returns>
        public JsonNode Evaluate(JsonNode jsonNode)
        {
            Result result = this.EvaluateInternal(jsonNode, 1);
            if (result.Error is not null)
            {
                throw result.Error;
            }

            return result.Value!;
        }

        private Result EvaluateInternal(JsonNode jsonNode, int segmentIndex)
        {
            string segement = this.segments[segmentIndex];
            Match match = AccessorPattern.Match(segement);
            string member = match.Groups["Member"].Value;
            string rawIndex = match.Groups["Index"].Value;

            JsonNode current = jsonNode;

            if (!string.IsNullOrEmpty(member))
            {
                IDictionary<string, JsonNode?> jsonObject = this.Cast<JsonObject>(jsonNode, segmentIndex);

                if (!jsonObject.ContainsKey(member))
                {
                    string error = $"The Json object at the jsonPath '{this.GetSegementPath(segmentIndex)}' does not contain a member named '{member}'";

                    if (jsonObject.Count == 0)
                    {
                        return new Result(new IndexOutOfRangeException($"{error} as the object has no children."));
                    }
                    else
                    {
                        string memberNames = string.Join("\n - ", jsonObject.Keys);
                        return new Result(new IndexOutOfRangeException($"{error}. The defined members are: \n - {memberNames}"));
                    }
                }

                current = jsonObject[member]!;
            }

            if (int.TryParse(rawIndex, out int index))
            {
                JsonArray jsonArray = this.Cast<JsonArray>(current, segmentIndex);

                if (index < 0)
                {
                    return new Result(new IndexOutOfRangeException($"The Json array at the jsonPath '{this.GetSegementPath(segmentIndex)}' defines the index as {index} which is less then zero and not valid"));
                }

                if (index >= jsonArray.Count)
                {
                    return new Result(new IndexOutOfRangeException($"The Json array at the jsonPath '{this.GetSegementPath(segmentIndex)}' defines the index as {index} greater then the array size which is {jsonArray.Count}."));
                }

                current = jsonArray[index]!;
            }

            segmentIndex++;
            return segmentIndex >= this.count
                ? new Result(current)
                : this.EvaluateInternal(current, segmentIndex);
        }

        /// <summary>
        /// Attempts to cast the node to the given type and throws an helpeful exception if it does not match
        /// </summary>
        /// <typeparam name="T">The type to cast to.</typeparam>
        /// <param name="jsonNode">The node type.</param>
        /// <param name="index">The current segment index.</param>
        /// <returns>The casted type.</returns>
        /// <exception cref="InvalidCastException">The cast failed.</exception>
        private T Cast<T>(JsonNode jsonNode, int index)
        {
            return jsonNode is T asType
                ? asType
                : throw new InvalidCastException($"The node at jsonPath {this.GetSegementPath(index)} " +
                $"was expected to be a {nameof(JsonArray)} but is actually a {jsonNode.GetType().Name}");
        }

        /// <summary>
        /// Returns a jsonPath to a segement at a spserfic index.
        /// </summary>
        /// <param name="index">The index to build the jsonPath to.</param>
        /// <returns>The returns jsonPath.</returns>
        private string GetSegementPath(int index)
        {
            StringBuilder builder = new StringBuilder();

            for (int i = 0; i < index; i++)
            {
                if (i > 0)
                {
                    builder.Append('.');
                }

                builder.Append(this.segments[i]);
            }

            return builder.ToString();
        }

        private struct Result
        {
            public readonly Exception? Error;
            public readonly JsonNode? Value;

            public Result(Exception exception)
            {
                this.Error = exception;
                this.Value = null;
            }

            public Result(JsonNode jsonNode)
            {
                this.Error = null;
                this.Value = jsonNode;
            }
        }

    }
}
