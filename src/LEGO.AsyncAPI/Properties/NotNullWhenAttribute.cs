// Copyright (c) The LEGO Group. All rights reserved.
#pragma warning disable IDE0130

#if NETSTANDARD
namespace System.Diagnostics.CodeAnalysis
{
    /// <summary>
    ///  Specifies that when a method returns <see cref="ReturnValue"/>, the
    ///  parameter will not be <see langword="null"/> even if the
    ///  corresponding type allows it.
    /// </summary>
    [DebuggerNonUserCode]
    [ExcludeFromCodeCoverage]
    internal sealed class NotNullWhenAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NotNullWhenAttribute"/> class.
        /// </summary>
        /// <param name="returnValue">What value the return should be to show that it won't be null.</param>
        public NotNullWhenAttribute(bool returnValue)
        {
            this.ReturnValue = returnValue;
        }
        /// <summary>
        /// Gets the return value for the condition.
        /// </summary>
        public bool ReturnValue { get; }
    }
}
#endif