// <copyright file="SqlHelper.cs" company="Microsoft">Copyright (c) Microsoft 2013. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Common.Sql
{
    public static class SqlHelper
    {
        public static string MakeConnectionString(string serverName, string databaseName)
        {
            return "Data Source=" + serverName + ";Initial Catalog=" + databaseName + ";Integrated Security=True;Pooling=False";
        }
    }
}
