using Microsoft.Data.SqlClient;
using Roommates.Models;
using System.Collections.Generic;

namespace Roommates.Repositories
{
    public class ChoreRepository : BaseRepository
    {
        /// <summary>
        ///  When new ChoreRepository is instantiated, pass the connection string along to the BaseRepository
        /// </summary>
        public ChoreRepository(string connectionString) : base(connectionString) { }

        /// <summary>
        ///  Get a list of all Chores in the database
        /// </summary>
        public List<Chore> GetAll()
        {
            // Create the connection to the DB so it can be properly opened/closed. 
            // The "using" block ensures that a proper disconnect takes place even if there is an error
            using (SqlConnection conn = Connection)
            {
                // Open the connection to the DB
                conn.Open();

                // Create the command variable to be used
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    
                    // Command to select the Chore data needed from the SQL Server
                    cmd.CommandText = "SELECT Id, Name FROM Chore";

                    // Exucute the SQL in the DB and get a reader that allows access to the data.
                    SqlDataReader reader = cmd.ExecuteReader();

                    // A list to hold the chores we retrieve from the DB
                    List<Chore> chores = new List<Chore>();

                    // Read data until the reader returns false (no more data found).
                    while(reader.Read())
                    {
                        // Use ordinal to get the position of the column with the name "Id"
                        int idColumnPosition = reader.GetOrdinal("Id");

                        // Now get the numeric value of the Id for each row
                        int idValue = reader.GetInt32(idColumnPosition);

                        // Use ordinal to find the column position of the column with the name "Name".
                        int nameColumnPosition = reader.GetOrdinal("Name");

                        // Get the string from the position provided above
                        string nameValue = reader.GetString(nameColumnPosition);

                        // Create the chore object with the data extracted above.
                        Chore chore = new Chore
                        {
                            Id = idValue,
                            Name = nameValue,
                        };

                        // Add the chore object to the chores list.
                        chores.Add(chore);
                    }

                    // Close the reader to allow other resources to use our DB since we are done.
                    reader.Close();

                    // Return the list of chores to whoever called this method.
                    return chores;
                }
            }
        }

        /// <summary>
        ///  Returns a single chore with the given id.
        /// </summary>
        public Chore GetById(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT Name FROM Chore WHERE Id = @id";
                    cmd.Parameters.AddWithValue("@id", id);
                    SqlDataReader reader = cmd.ExecuteReader();

                    Chore chore = null;

                    if (reader.Read())
                    {
                        chore = new Chore
                        {
                            Id = id,
                            Name = reader.GetString(reader.GetOrdinal("Name")),
                        };
                    }

                    reader.Close();

                    return chore;
                }
            }
        }

        public void Insert(Chore chore)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"INSERT INTO Chore (Name)
                                          OUTPUT INSERTED.Id
                                          VALUES (@name)";
                    cmd.Parameters.AddWithValue("name", chore.Name);
                    int id = (int)cmd.ExecuteScalar();

                    chore.Id = id;
                }
            }
        }

        public List<Chore> UnassignedChores()
        {
            using(SqlConnection conn = Connection)
            {
                conn.Open();
                using(SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT c.Id, c.Name FROM Chore c
                                        LEFT JOIN RoommateChore rc on c.Id = rc.ChoreId
                                        WHERE rc.Id IS NULL;";

                    SqlDataReader reader = cmd.ExecuteReader();

                    List<Chore> unassignedChores = new List<Chore>();

                    while(reader.Read())
                    {
                        Chore chore = new Chore
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            Name = reader.GetString(reader.GetOrdinal("Name")),
                        };

                        unassignedChores.Add(chore);
                    }

                    reader.Close();
                    return unassignedChores;
                }
            }
        }

        public void AssignChore(int roommateId, int choreId)
        {
            using(SqlConnection conn = Connection)
            {
                conn.Open();
                using(SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"INSERT INTO RoommateChore (RoommateId, ChoreId)
                                            VALUES (@roommateId, @choreId)";
                    cmd.Parameters.AddWithValue("@roommateId", roommateId);
                    cmd.Parameters.AddWithValue("@choreId", choreId);

                    cmd.ExecuteNonQuery();
                }
            }
        }

        
        public List<ChoreCount> GetChoreCounts()
        {
            using(SqlConnection conn = Connection)
            {
                conn.Open();

                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT COUNT(c.Name) as NumberOfChores, r.FirstName
                                            FROM Roommate r
                                            LEFT JOIN RoommateChore rc on rc.RoommateId = r.Id
                                            LEFT JOIN Chore c on c.Id = rc.ChoreId
                                        GROUP BY r.Id, r.FirstName";

                    SqlDataReader reader = cmd.ExecuteReader();

                    List<ChoreCount> theChoreCount = new List<ChoreCount>();

                    while(reader.Read())
                    {
                        ChoreCount choreCount = new ChoreCount
                        {
                            Name = reader.GetString(reader.GetOrdinal("FirstName")),
                            NumberOfChores = reader.GetInt32(reader.GetOrdinal("NumberOfChores")),
                        };

                        theChoreCount.Add(choreCount);

                    }

                    reader.Close();
                    return theChoreCount;
                }
            }
        }
        
    }
}
