// <copyright file="SqlExceptionExtensions.cs" company="Microsoft">Copyright (c) Microsoft 2013. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Common.Sql
{
    using System.Data.SqlClient;

    public static class SqlExceptionExtensions
    {
        public static bool IsUniqueConstraintViolation(this SqlException sqlException)
        {
            return sqlException.Number == 2627;
        }

        public static bool IsUniqueIndexViolation(this SqlException sqlException)
        {
            return sqlException.Number == 2601;
        }
    }
}
