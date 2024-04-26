using System.Data.SqlClient;

namespace Deadlock
{
    internal abstract class Program
    {
        private const string ConnectionString = "Data Source = EMANUEL-LAPTOP\\SQLEXPRESS;" +
                                                " Initial Catalog = Biblioteca; Integrated Security = True";
        private static bool _success;
        private static int _retryCount;
        
        private static void Main()
        {
            _retryCount = 0;
            _success = false;
            
            while (!_success && _retryCount < 3)
            {
                Console.WriteLine("Retry count: " + _retryCount);
                
                var thread1 = new Thread(Run1);
                var thread2 = new Thread(Run2);
                
                thread1.Start();
                thread2.Start();
                thread1.Join();
                thread2.Join();
            }

            Console.WriteLine(_retryCount >= 3
                ? "Exceeded maximum retry attempts. Aborting."
                : "All transactions completed.");
        }
        
        private static void Run1()
        {
            Console.WriteLine("Thread_1(): Thread 1 is running!");
            using var connection = new SqlConnection(ConnectionString);
            connection.Open();
            // Create a new transaction
            using var transaction = connection.BeginTransaction();
            try
            {
                using (var command = connection.CreateCommand())
                {
                    command.Transaction = transaction;
                    // Update statement 1
                    command.CommandText = "update Librarie set nume = 'T1' where id_librarie = 1";
                    command.ExecuteNonQuery();

                    // Delay for 5 seconds
                    Thread.Sleep(5000);

                    // Update statement 2
                    command.CommandText = "update Domeniu set descriere = 'T1' where id_domeniu = 1";
                    command.ExecuteNonQuery();
                }
                // Commit the transaction
                transaction.Commit();
                Console.WriteLine("Thread_1(): Transaction committed successfully.");
                _success = true;
            }
            catch (SqlException ex)
            {
                if (ex.Number == 1205) // Deadlock error number
                {
                    // Handle deadlock, rollback the transaction, and retry
                    Console.WriteLine("Thread_1(): Deadlock occurred. Retrying...");
                    transaction.Rollback();
                    Console.WriteLine("Thread_1(): Transaction rolled back.");
                    _retryCount ++;
                }
                else
                {
                    // Handle other exceptions
                    Console.WriteLine("Thread_1(): Error occurred: " + ex.Message);
                    transaction.Rollback();
                    Console.WriteLine("Thread_1(): Transaction rolled back.");
                }
            }
        }

        private static void Run2()
        {
            Console.WriteLine("Thread_2(): Thread 2 is running!");
            using var connection = new SqlConnection(ConnectionString);
            connection.Open();
            // Set the deadlock priority to HIGH
            using (var setDeadlockPriorityCommand = connection.CreateCommand())
            {
                setDeadlockPriorityCommand.CommandText = "set deadlock_priority high";
                setDeadlockPriorityCommand.ExecuteNonQuery();
            }
            // Create a new transaction
            using var transaction = connection.BeginTransaction();
            try
            {
                using (var command = connection.CreateCommand())
                {
                    command.Transaction = transaction;

                    // Update statement 1
                    command.CommandText = "update Domeniu set descriere = 'T2' where id_domeniu = 1";
                    command.ExecuteNonQuery();

                    // Delay for 5 seconds
                    Thread.Sleep(5000);

                    // Update statement 2
                    command.CommandText = "update Librarie set nume = 'T2' where id_librarie = 1";
                    command.ExecuteNonQuery();
                }

                // Commit the transaction
                transaction.Commit();
                Console.WriteLine("Thread_2(): Transaction committed successfully.");
                _success = true;
            }
            catch (SqlException ex)
            {
                if (ex.Number == 1205) // Deadlock error number
                {
                    // Handle deadlock, rollback the transaction, and retry
                    Console.WriteLine("Thread_2(): Deadlock occurred. Retrying...");
                    transaction.Rollback();
                    Console.WriteLine("Thread_2(): Transaction rolled back.");
                    _retryCount ++;
                }
                else
                {
                    // Handle other exceptions
                    Console.WriteLine("Thread_2(): Error occurred: " + ex.Message);
                    transaction.Rollback();
                    Console.WriteLine("Thread_2(): Transaction rolled back.");
                }
            }
        }
    }
}
