// <copyright file="PIDLGenerator.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PidlFactory.V7.PIDLGeneration
{
    /// <summary>
    /// Class to orchestrate the feature enablement
    /// </summary>
    public class PIDLGenerator
    {
        public static T Generate<T>(IPIDLGenerationFactory<T> factory, PIDLGeneratorContext context)
        {
            IPIDLGenerator<T> generator = factory.GetPIDLGenerator(context);
            if (generator == null)
            {
                return default(T);
            }
            else
            {
                return generator.Generate(context);
            }
        }
    }
}