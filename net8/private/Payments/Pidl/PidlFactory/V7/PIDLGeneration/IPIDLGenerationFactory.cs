// <copyright file="IPIDLGenerationFactory.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PidlFactory.V7.PIDLGeneration
{
    /// <summary>
    /// Interface for PIDLGenerationFactory
    /// </summary>
    /// <typeparam name="T"> generic type</typeparam>
    public interface IPIDLGenerationFactory<T>
    {
        IPIDLGenerator<T> GetPIDLGenerator(PIDLGeneratorContext context);
    }
}
