// <copyright file="IPIDLGenerator.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PidlFactory.V7.PIDLGeneration
{
    /// <summary>
    /// Interface for PIDL feature 
    /// </summary>
    /// <typeparam name="T">The second generic type parameter.</typeparam>
    public interface IPIDLGenerator<T>
    {
        T Generate(PIDLGeneratorContext context);
    }
}
