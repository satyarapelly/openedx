// <copyright file="PidlPropertyValidator.cs" company="Microsoft">Copyright (c) Microsoft 2015. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PidlFactory.V7
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Commerce.Payments.PidlModel.V7;

    /// <summary>
    /// An abstract strategy class represeting a Pidl Property Validator
    /// </summary>
    /// <typeparam name="TDataType">The datatype on which the validation would work</typeparam>
    /// <typeparam name="TContext">The context required for validation</typeparam>
    public abstract class PidlPropertyValidator<TDataType, TContext> :
        PidlStrategyExecutor<TDataType, TContext, PidlExecutionResult>
        where TDataType : class
        where TContext : class
    {
        public PidlExecutionResult Validate(string validationType, TDataType inputValue, TContext context)
        {
            return this.ExecuteStrategy(validationType, inputValue, context);
        }
    }
}