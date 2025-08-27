// <copyright file="ConfigurationObject.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;

    public abstract class ConfigurationObject
    {
        private static Dictionary<Type, ConstructorFunction> configurationConstructors = new Dictionary<Type, ConstructorFunction>();

        public delegate ConfigurationObject ConstructorFunction(params object[] args);

        public static void CreateConfigurationConstructor<T>() where T : ConfigurationObject
        {
            Type type = typeof(T);

            ConfigurationObject.CreateConstructor(type, typeof(List<string[]>), typeof(ParsedConfigurationComponent), typeof(Dictionary<string, int>));
        }

        public static T ConstructFromConfiguration<T>(List<string[]> rows, ParsedConfigurationComponent parsedConfigurationComponent, Dictionary<string, int> columnNames) where T : ConfigurationObject
        {
            Type type = typeof(T);

            if (!ConfigurationObject.configurationConstructors.ContainsKey(type))
            {
                ConfigurationObject.CreateConfigurationConstructor<T>();
            }

            return (T)ConfigurationObject.configurationConstructors[type](rows, parsedConfigurationComponent, columnNames);
        }

        private static void CreateConstructor(Type type, params Type[] parameters)
        {
            ConstructorFunction constructorDelegate = null;

            if (ConfigurationObject.configurationConstructors.ContainsKey(type))
            {
                constructorDelegate = ConfigurationObject.configurationConstructors[type];
            }
            else
            {
                var constructorInfo = type.GetConstructor(parameters);

                var paramExpr = Expression.Parameter(typeof(object[]));

                var constructorParameters = parameters.Select((paramType, index) =>
                    Expression.Convert(
                        Expression.ArrayAccess(
                            paramExpr,
                            Expression.Constant(index)),
                        paramType)).ToArray();

                var body = Expression.New(constructorInfo, constructorParameters);

                var constructor = Expression.Lambda<ConstructorFunction>(body, paramExpr);

                constructorDelegate = constructor.Compile();

                ConfigurationObject.configurationConstructors[type] = constructorDelegate;
            }
        }
    }
}