// <copyright file="SqlCommandScope.cs" company="Microsoft Corporation">Copyright (c) Microsoft 2013. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Common.Sql
{
    using System;
    using System.Data;
    using System.Data.SqlClient;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Commerce.Payments.Common.Tracing;

    public class SqlCommandScope<T> : IDisposable
    {
        private ServiceInstrumentationCounters counters;
        private string partitionId;
        private string sqlOperationName;
        private Func<Task<SqlConnection>> configSqlConnection;
        private Action<SqlCommand> configCommand;
        private Func<SqlCommand, string> configCommandWithParameters;
        private Func<SqlDataReader, Task<T>> processReader;
        private Func<SqlDataReader, Task<Tuple<T, int>>> processReaderWithParameters;
        private int latencyThreshold;
        private int rowCountThreshold;

        public SqlCommandScope(
            ServiceInstrumentationCounters counters, 
            string partitionId,
            string sqlOperationName,
            Func<Task<SqlConnection>> configSqlConnection,
            Action<SqlCommand> configCommand)
        {
            this.counters = counters;
            this.partitionId = partitionId;
            this.sqlOperationName = sqlOperationName;
            this.configSqlConnection = configSqlConnection;
            this.configCommand = configCommand;
        }

        public SqlCommandScope(
            ServiceInstrumentationCounters counters,
            string partitionId,
            string sqlOperationName,
            Func<Task<SqlConnection>> configSqlConnection,
            Func<SqlCommand, string> configCommandWithParameters,
            int latencyThreshold)
        {
            this.counters = counters;
            this.partitionId = partitionId;
            this.sqlOperationName = sqlOperationName;
            this.configSqlConnection = configSqlConnection;
            this.configCommandWithParameters = configCommandWithParameters;
            this.latencyThreshold = latencyThreshold;
        }

        public SqlCommandScope(
            ServiceInstrumentationCounters counters, 
            string partitionId,
            string sqlOperationName,
            Func<SqlCommand, string> configureCommandWithParameters,
            int latencyThreshold,
            Func<SqlDataReader, Task<Tuple<T, int>>> processReaderWithParameters,
            int rowCountThreshold)
        {
            this.counters = counters;
            this.partitionId = partitionId;
            this.sqlOperationName = sqlOperationName;
            this.configCommandWithParameters = configureCommandWithParameters;
            this.latencyThreshold = latencyThreshold;
            this.processReaderWithParameters = processReaderWithParameters;
            this.rowCountThreshold = rowCountThreshold;
        }

        public SqlCommandScope(
            ServiceInstrumentationCounters counters,
            string partitionId,
            string sqlOperationName,
            Func<Task<SqlConnection>> configSqlConnection,
            Action<SqlCommand> configCommand,
            Func<SqlDataReader, Task<T>> processReader)
        {
            this.counters = counters;
            this.partitionId = partitionId;
            this.sqlOperationName = sqlOperationName;
            this.configSqlConnection = configSqlConnection;
            this.configCommand = configCommand;
            this.processReader = processReader;
        }

        public int LatencyThreshold
        {
            get { return this.latencyThreshold; }
            set { this.latencyThreshold = value; }
        }

        public int RowCountThreshold
        {
            get { return this.rowCountThreshold; }
            set { this.rowCountThreshold = value; }
        }

        public async Task<T> Execute(EventTraceActivity traceId)
        {
            if (traceId == null)
            {
                traceId = new EventTraceActivity();
            }

            string parameters = string.Empty;
            int rowCount = 0;
            using (ServiceInstrumentationScope databaseInstrumentation = new ServiceInstrumentationScope(
                this.counters,
                traceId,
                (scopeTraceId) =>
                {
                },
                (completedSuccessfully, scopeTraceId, latency) =>
                {
                },
                this.sqlOperationName + " " + (string.IsNullOrEmpty(this.partitionId) ? string.Empty : this.partitionId)))
            {
                T result = default(T);

                using (SqlConnection connection = await this.configSqlConnection())
                {
                    using (SqlCommand command = new SqlCommand())
                    {
                        try
                        {
                            command.Connection = connection;

                            if (this.configCommand == null)
                            {
                                parameters = this.configCommandWithParameters(command);
                            }
                            else
                            {
                                this.configCommand(command);
                            }

                            using (SqlDataReader reader = await command.ExecuteReaderAsync())
                            {
                                if (this.processReader != null)
                                {
                                    result = await this.processReader(reader);
                                }
                                else
                                {
                                    Tuple<T, int> parameterizedResult = await this.processReaderWithParameters(reader);

                                    if (parameterizedResult != null)
                                    {
                                        result = parameterizedResult.Item1;
                                        rowCount = parameterizedResult.Item2;
                                        parameters = string.Format("{0}/{1}", parameters, parameterizedResult.Item2);
                                    }
                                }
                            }
                        }
                        catch (Exception)
                        {
                            throw;
                        }
                    }
                }

                databaseInstrumentation.Success();

                return result;
            }
        }

        public async Task<T> Execute(SqlConnection openedSqlConnection, EventTraceActivity traceId)
        {
            if (traceId == null)
            {
                traceId = new EventTraceActivity();
            }

            string parameters = string.Empty;
            int rowCount = 0;
            using (ServiceInstrumentationScope databaseInstrumentation = new ServiceInstrumentationScope(
                this.counters,
                traceId,
                (scopeTraceId) =>
                {
                },
                (completedSuccessfully, scopeTraceId, latency) =>
                {
                },
                this.sqlOperationName + " " + (string.IsNullOrEmpty(this.partitionId) ? string.Empty : this.partitionId)))
            {
                T result = default(T);

                using (SqlCommand command = new SqlCommand())
                {
                    try
                    {
                        command.Connection = openedSqlConnection;

                        if (this.configCommand == null)
                        {
                            parameters = this.configCommandWithParameters(command);
                        }
                        else
                        {
                            this.configCommand(command);
                        }

                        using (SqlDataReader reader = await command.ExecuteReaderAsync())
                        {
                            if (this.processReader != null)
                            {
                                result = await this.processReader(reader);
                            }
                            else
                            {
                                Tuple<T, int> parameterizedResult = await this.processReaderWithParameters(reader);

                                if (parameterizedResult != null)
                                {
                                    result = parameterizedResult.Item1;
                                    rowCount = parameterizedResult.Item2;
                                    parameters = string.Format("{0}/{1}", parameters, parameterizedResult.Item2);
                                }
                            }
                        }
                    }
                    catch (Exception)
                    {
                        throw;
                    }
                }

                databaseInstrumentation.Success();

                return result;
            }
        }

        public async Task<int> ExecuteNonQuery(EventTraceActivity traceId, IsolationLevel? isolationLevel = null)
        {
            if (traceId == null)
            {
                traceId = new EventTraceActivity();
            }

            string parameters = string.Empty;

            using (ServiceInstrumentationScope databaseInstrumentation = new ServiceInstrumentationScope(
                this.counters,
                traceId,
                (scopeTraceId) =>
                {
                },
                (completedSuccessfully, scopeTraceId, latency) =>
                {
                },
                this.sqlOperationName + " " + (string.IsNullOrEmpty(this.partitionId) ? string.Empty : this.partitionId)))
            {
                int result;

                using (SqlConnection connection = await this.configSqlConnection())
                {
                    SqlTransaction sqlTransaction = null;

                    if (isolationLevel.HasValue)
                    {
                        sqlTransaction = connection.BeginTransaction(isolationLevel.Value);
                    }

                    using (SqlCommand command = new SqlCommand())
                    {
                        try
                        {
                            command.Connection = connection;

                            if (this.configCommand == null)
                            {
                                parameters = this.configCommandWithParameters(command);
                            }
                            else
                            {
                                this.configCommand(command);
                            }

                            if (sqlTransaction != null)
                            {
                                command.Transaction = sqlTransaction;
                            }

                            result = await command.ExecuteNonQueryAsync();
                            if (sqlTransaction != null)
                            {
                                sqlTransaction.Commit();
                            }
                        }
                        catch (Exception)
                        {
                            throw;
                        }
                    }
                }

                databaseInstrumentation.Success();

                return result;
            }
        }

        public async Task<int> ExecuteNonQuery(SqlConnection openedSqlConnection, EventTraceActivity traceId)
        {
            return await this.ExecuteNonQuery(openedSqlConnection, null, null, traceId);
        }

        public async Task<int> ExecuteNonQuery(SqlConnection openedSqlConnection, Func<SqlException, bool> sqlWarningAsSuccess, Func<int, bool> verifyResult, EventTraceActivity traceId)
        {
            if (traceId == null)
            {
                traceId = new EventTraceActivity();
            }

            string parameters = string.Empty;

            using (ServiceInstrumentationScope databaseInstrumentation = new ServiceInstrumentationScope(
                this.counters,
                traceId,
                (scopeTraceId) =>
                {
                },
                (completedSuccessfully, scopeTraceId, latency) =>
                {
                },
                this.sqlOperationName + " " + (string.IsNullOrEmpty(this.partitionId) ? string.Empty : this.partitionId)))
            {
                int result;

                using (SqlCommand command = new SqlCommand())
                {
                    try
                    {
                        command.Connection = openedSqlConnection;
                        if (this.configCommand == null)
                        {
                            parameters = this.configCommandWithParameters(command);
                        }
                        else
                        {
                            this.configCommand(command);
                        }

                        result = await command.ExecuteNonQueryAsync();
                    }
                    catch (SqlException sqlException)
                    {
                        if (sqlWarningAsSuccess != null && sqlWarningAsSuccess(sqlException))
                        {
                            databaseInstrumentation.UserError();
                        }

                        throw;
                    }
                    catch (Exception)
                    {
                        throw;
                    }
                }

                if (verifyResult == null || verifyResult(result))
                {
                    databaseInstrumentation.Success();
                }

                return result;
            }
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}
