// <copyright file="SqlCommandExecutor.cs" company="Microsoft Corporation">Copyright (c) Microsoft 2013. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Common.Sql
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.SqlClient;
    using System.Threading.Tasks;
    using Microsoft.Commerce.Payments.Common.Tracing;

    /// <summary>
    /// Base class for SqlCommand executors.
    /// </summary>
    /// <typeparam name="T">The return type of the SQL execution.  Use
    /// bool if there is no expected return value.</typeparam>
    public abstract class SqlCommandExecutor<T> : IDisposable
    {
        /// <summary>
        /// The maximum number of times we will run the command as long as we are still
        /// within the retry window.
        /// </summary>
        private const int MaxAttempts = 5;

        /// <summary>
        /// The amount of time after the command was first issued that we will continue
        /// to retry up to the maximum number of attempts.
        /// </summary>
        private static readonly TimeSpan RetryWindow = TimeSpan.FromSeconds(20);

        /// <summary>
        /// The amount time we would increase the time between consecutive retries.
        /// </summary>
        private static readonly TimeSpan RetryDelayStep = TimeSpan.FromMilliseconds(500);

        /// <summary>
        /// The maximum delay between attempts.  This acts as a cap for the backoff logic
        /// that occurs during retry.
        /// </summary>
        private static TimeSpan maxRetryDelay = TimeSpan.FromSeconds(2);

        private string connectionString;
        private SqlConnection connection;
        private SqlTransaction transaction;
        private EventTraceActivity traceActivityId;

        /// <summary>
        /// Initializes a new instance of the <see cref="SqlCommandExecutor&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="connectionString">The connection string for the target SQL
        /// server.  May or may not include the Initial Catalog depending on what
        /// the subclass expects.</param>
        /// <param name="traceActivityId">trace id</param>
        protected SqlCommandExecutor(string connectionString, EventTraceActivity traceActivityId)
        {
            this.connectionString = connectionString;
            this.traceActivityId = traceActivityId;
        }

        /// <summary>
        /// Gets or sets a value indicating whether a transaction should be created.
        /// if a transaction is created, it will be exposed through the transaction property and the caller is responsible 
        /// for committing the transaction, otherwise it will be aborted when the executor gets disposed.
        /// </summary>
        protected bool CreateTransaction
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the command should be run non-query
        /// (no reader obtained).  ProcessReader Async will not be called if this is true.
        /// Defaults to false and obtains a SqlDataReader.
        /// </summary>
        protected bool ExecuteNonQuery
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the command should be retried if it
        /// throws an exception.
        /// </summary>
        protected bool SuppressRetry
        {
            get;
            set;
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// This method is used to Commit the transaction crated when CreateTransaction is et to true
        /// </summary>
        public void CommitTransaction()
        {
            if (this.transaction != null)
            {
                try
                {
                    this.transaction.Commit();
                }
                finally
                {
                    this.connection.Dispose();
                    this.connection = null;
                }
            }
        }

        /// <summary>
        /// This method is used to Abort the transaction crated when CreateTransaction is et to true
        /// </summary>
        public void AbortTransaction()
        {
            if (this.transaction != null)
            {
                try
                {
                    this.transaction.Rollback();
                }
                finally
                {
                    this.connection.Dispose();
                    this.connection = null;
                }
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.transaction != null)
                {
                    this.transaction.Dispose();
                    this.transaction = null;
                }

                if (this.connection != null)
                {
                    this.connection.Dispose();
                    this.connection = null;
                }
            }
        }

        protected virtual SqlTransaction BeginTransaction(SqlConnection sqlConnection)
        {
            return sqlConnection.BeginTransaction();
        }

        /// <summary>
        /// Execute the actual SQL command.  This method calls the virtual ConfigureCommand
        /// and, if ExecuteNonQuery is false, the ProcessReaderAsync methods to allow subclasses
        /// to control the processing.
        /// </summary>
        /// <returns>A task representing the async work.  The result of this task is the result
        /// of the SQL operation.  If the operation was executed non-query then the default value
        /// of T is returned.</returns>
        protected async Task<T> Execute()
        {
            int attemptsRemaining;
            if (this.SuppressRetry)
            {
                attemptsRemaining = 1;
            }
            else
            {
                attemptsRemaining = MaxAttempts;
            }

            DateTime doNotRetryAfter = DateTime.UtcNow.Add(RetryWindow);

            // The first retry happens immediately.
            TimeSpan retryDelay = TimeSpan.Zero;

            List<Exception> commandExceptions = new List<Exception>();
            string commandText = string.Empty;

            while (attemptsRemaining > 0 && DateTime.UtcNow < doNotRetryAfter)
            {
                attemptsRemaining--;
                SqlConnection connection = null;
                bool exceptionInCurrentLoop = false;

                try
                {
                    connection = new SqlConnection(this.connectionString);
                    await connection.OpenAsync();

                    using (SqlCommand command = new SqlCommand())
                    {
                        command.Connection = connection;

                        if (this.CreateTransaction)
                        {
                            command.Transaction = this.BeginTransaction(connection);
                            this.transaction = command.Transaction;
                            this.connection = connection;
                        }

                        this.ConfigureCommand(command);
                        commandText = command.CommandText;
                        if (this.ExecuteNonQuery)
                        {
                            int rowsAffected = await command.ExecuteNonQueryAsync();
                            return this.ProcessNonQueryResult(rowsAffected);
                        }
                        else
                        {
                            using (SqlDataReader reader = await command.ExecuteReaderAsync(CommandBehavior.SequentialAccess))
                            {
                                return await this.ProcessReaderAsync(reader);
                            }
                        }
                    }
                }
                catch (SqlException e)
                {
                    commandExceptions.Add(e);
                    exceptionInCurrentLoop = true;
                }
                catch (TimeoutException e)
                {
                    commandExceptions.Add(e);
                    exceptionInCurrentLoop = true;
                }
                finally
                {
                    if (exceptionInCurrentLoop || !this.CreateTransaction)
                    {
                        if (connection != null)
                        {
                            connection.Dispose();
                        }
                    }
                }

                if (attemptsRemaining > 0)
                {
                    if (retryDelay != TimeSpan.Zero)
                    {
                        await Task.Delay(retryDelay);

                        // Let's do exponential backoff with a cap.  We double the last delay
                        // and then put a ceiling on it.
                        retryDelay = retryDelay.Add(RetryDelayStep);
                        if (retryDelay > maxRetryDelay)
                        {
                            retryDelay = maxRetryDelay;
                        }
                    }
                    else
                    {
                        // The second retry happens after retryDelayStep.  This time was chosen
                        // arbitrarily.  It is important that this is greater than zero to make sure
                        // we don't hit the DB rapidly in a tight loop.  Based on the logic above,
                        // the retry delay will increase each time around the loop.
                        retryDelay = RetryDelayStep;
                    }
                }
            }

            throw TraceCore.TraceException<AggregateException>(this.traceActivityId, new AggregateException(commandExceptions));
        }

        /// <summary>
        /// Overridden by subclasses to configure the SqlCommand to perform
        /// the desired operation.
        /// </summary>
        /// <param name="command">The SqlCommand to configure.</param>
        protected abstract void ConfigureCommand(SqlCommand command);

        /// <summary>
        /// Overridden by subclasses to process the SqlDataReader returned by the
        /// SqlCommand.  This MUST be overridden if ExecuteNonQuery is false or 
        /// it will throw an exception.
        /// </summary>
        /// <param name="reader">The reader to process.  The reader has not been
        /// advanced in any way (neither Read nor NextResult has been called).</param>
        /// <returns>A task representing the async work.  The result of this task
        /// will be returned by ExecuteAsync.</returns>
        protected virtual Task<T> ProcessReaderAsync(SqlDataReader reader)
        {
            throw TraceCore.TraceException<NotImplementedException>(this.traceActivityId, new NotImplementedException("Subclasses must override ProcessReader unless they set ExecuteNonQuery to true."));
        }

        protected virtual T ProcessNonQueryResult(int rowsAffected)
        {
            return default(T);
        }
    }
}
