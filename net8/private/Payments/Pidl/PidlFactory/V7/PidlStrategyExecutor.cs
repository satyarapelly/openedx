// <copyright file="PidlStrategyExecutor.cs" company="Microsoft">Copyright (c) Microsoft 2015. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PidlFactory.V7
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Commerce.Payments.PidlModel.V7;

    /// <summary>
    /// An abstract strategy class represeting a Pidl Property Execution strategy
    /// </summary>
    /// <typeparam name="TDataType">The datatype on which the Execution would work</typeparam>
    /// <typeparam name="TContext">The context required for Execution</typeparam>
    /// <typeparam name="TResultType">The result type required for Execution</typeparam>
    public abstract class PidlStrategyExecutor<TDataType, TContext, TResultType>
        where TDataType : class
        where TContext : class
        where TResultType : PidlExecutionResult
    {
        private Dictionary<string, Func<TDataType, TContext, TResultType>> strategies = new Dictionary<string, Func<TDataType, TContext, TResultType>>();

        public PidlStrategyExecutor()
        {
        }

        protected Dictionary<string, Func<TDataType, TContext, TResultType>> Strategies
        {
            get
            {
                return this.strategies;
            }
        }

        protected TResultType ExecuteStrategy(string strategyType, TDataType inputValue, TContext context)
        {
            if (!this.strategies.ContainsKey(strategyType))
            {
                TResultType result = default(TResultType);
                result.Status = PidlExecutionResultStatus.Failed;
                result.ErrorMessage = string.Format("The strategy type : {0} is not supported by the executor : {1}", strategyType, this.GetName());
                return result;
            }

            return this.strategies[strategyType](inputValue, context);
        }
        
        protected abstract void InitializeStrategy();

        protected abstract string GetName();
    }
}
