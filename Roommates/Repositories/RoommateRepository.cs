using Microsoft.Data.SqlClient;
using Roommates.Models;
using System.Collections.Generic;

namespace Roommates.Repositories
{
    class RoommateRepository : BaseRepository
    {
        public RoommateRepository(string connectionString) : base(connectionString) { }

        public List<Roommate> GetAll()
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();

                using(SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT Id, FirstName, LastName FROM Roommate";

                    SqlDataReader reader = cmd.ExecuteReader();

                    List<Roommate> roommates = new List<Roommate>();

                    Roommate roommate = null;

                    while(reader.Read())
                    {
                        roommate = new Roommate
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                            LastName = reader.GetString(reader.GetOrdinal("LastName")),
                        };

                        roommates.Add(roommate);
                    }

                    reader.Close();
                    return roommates;
                }
            }
        }
        public Roommate GetById(int id)
        {
            using(SqlConnection conn = Connection)
            {
                conn.Open();
                using(SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT r.Id, r.FirstName, r.LastName, r.RentPortion, r.MoveInDate, rm.Name 
                                        FROM Roommate r
                                        LEFT JOIN Room rm on r.RoomId = rm.Id
                                        WHERE r.Id = @id";
                    cmd.Parameters.AddWithValue("@id", id);
                    SqlDataReader reader = cmd.ExecuteReader();

                    Room theirRoom = null;

                    Roommate roommate = null;

                    if (reader.Read())
                    {
                        theirRoom = new Room()
                        {
                            Name = reader.GetString(reader.GetOrdinal("Name")),
                        };

                        roommate = new Roommate
                        {
                            Id = id,
                            FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                            LastName = reader.GetString(reader.GetOrdinal("LastName")),
                            RentPortion = reader.GetInt32(reader.GetOrdinal("RentPortion")),
                            MovedInDate = reader.GetDateTime(reader.GetOrdinal("MoveInDate")),
                            Room = theirRoom,
                        };
                    }

                    reader.Close();

                    return roommate;

                }
            }
        }
    }
}
