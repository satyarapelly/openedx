// <copyright file="PidlPropertyTransformer.cs" company="Microsoft">Copyright (c) Microsoft 2015. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PidlFactory.V7
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Commerce.Payments.PidlModel.V7;

    /// <summary>
    /// An abstract strategy class represeting a Pidl Property Transformation strategy
    /// </summary>
    /// <typeparam name="TDataType">The datatype on which the transformer would work</typeparam>
    /// <typeparam name="TContext">The context required for transformation</typeparam>
    public abstract class PidlPropertyTransformer<TDataType, TContext> : 
        PidlStrategyExecutor<TDataType, TContext, PidlTransformationResult<TDataType>>
        where TDataType : class
        where TContext : class
    {
        public PidlTransformationResult<TDataType> Transform(string transformationType, TDataType inputValue, TContext context)
        {
            return this.ExecuteStrategy(transformationType, inputValue, context);
        }
    }
}
