        // GET_LOCK is a mutex named lock, not a table lock
	// GET_LOCK is used to synchronise not just writes, but even reads too
	// In this method, a mutex lock named 'order_number' is acquired and is active for 15 seconds
        // When another method attempts to acquire a lock with the same name and it has been acquired already
	// It now has to wait for the named lock to be released before it can proceed 
	// Here, both reading the order number and incrementing it is inside the same lock
        // Ensuring that all instances will always read different values
        private int RetrieveIncrementOrderNumber()
        {
            int newOrder = 0, oldOrder = 0;
            using (var connect = new MySqlConnection(cs))
            {
                connect.Open();
                using (MySqlTransaction transaction = connect.BeginTransaction())
                {
                    using (MySqlCommand command = new MySqlCommand("SELECT GET_LOCK('order_number', 15)", connect, transaction))
                    {
                        try
                        {
                            command.ExecuteNonQuery();
                            command.CommandText = "SELECT value FROM order_number LIMIT 1";
                            newOrder = Convert.ToInt32(command.ExecuteScalar());
                            oldOrder = newOrder;
                            newOrder++;
                            command.CommandText = "UPDATE order_number SET value = @newVal LIMIT 1";
                            command.Parameters.AddWithValue("@newVal", newOrder);
                            command.ExecuteNonQuery();

                            command.CommandText = "SELECT RELEASE_LOCK('order_number')";
                            command.ExecuteNonQuery();
                            transaction.Commit();
                        }
                        catch (MySqlException)
                        {
                            command.CommandText = "SELECT RELEASE_LOCK('order_number')";
                            command.ExecuteNonQuery();
                            transaction.Rollback();
                        }
                    }
                }
            }
            return oldOrder;
        }