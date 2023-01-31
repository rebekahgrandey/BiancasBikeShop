using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using BiancasBikeShop.Models;
using Azure;
using System.Reflection.PortableExecutable;
using Microsoft.Extensions.Configuration;
using BiancasBikeShop.Utils;
using Microsoft.Extensions.Hosting;

namespace BiancasBikeShop.Repositories
{
    public class BikeRepository : IBikeRepository
    {
        private SqlConnection Connection
        {
            get
            {
                return new SqlConnection("server=localhost\\SQLExpress;database=BiancasBikeShop;integrated security=true;TrustServerCertificate=true");
            }
        }

        public List<Bike> GetAllBikes()
        {
            using (var conn = Connection)
            {
                conn.Open();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
               SELECT b.Id, b.Brand, b.Color, o.Id AS OwnerId, o.Name
                        
                 FROM Bike b
                 JOIN Owner o ON b.OwnerId = o.Id;
            ";

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {

                        var bikes = new List<Bike>();

                        while (reader.Read())
                        {
                            bikes.Add(new Bike()
                            {
                                Id = DbUtils.GetInt(reader, "Id"),
                                Brand = DbUtils.GetString(reader, "Brand"),
                                Color = DbUtils.GetString(reader, "Color"),
                                Owner = new Owner()
                                {
                                    Id = DbUtils.GetInt(reader, "OwnerId"),
                                    Name = DbUtils.GetString(reader, "Name"),
                                }
                            });
                        }
                        return bikes;
                    }
                }
            }
        }

        public Bike GetBikeById(int id)
        {
            Bike bike = null;

            using (var conn = Connection)
            {
                conn.Open();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"

                        SELECT b.Id, b.Brand, b.Color, o.Id AS OwnerId, o.Name AS OwnerName, o.Address, o.Email, o.Telephone,
                        bt.Id AS BikeTypeId, bt.Name AS BikeTypeName,
                        wo.Id AS WorkOrderId, wo.DateInitiated, wo.Description, wo.DateCompleted, wo.BikeId
                                  FROM Bike b
                                  JOIN Owner o ON b.OwnerId = o.Id
                                  JOIN BikeType bt ON b.BikeTypeId = bt.Id
                                  JOIN WorkOrder wo ON b.Id = wo.BikeId
                                  WHERE b.Id = @id";

                    cmd.Parameters.AddWithValue("@id", id);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            if (bike == null)
                            {
                                bike = new Bike
                                {
                                    Id = id,
                                    Brand = reader.GetString(reader.GetOrdinal("Brand")),
                                    Color = reader.GetString(reader.GetOrdinal("Color")),
                                    Owner = new Owner
                                    {
                                        Id = reader.GetInt32(reader.GetOrdinal("OwnerId")),
                                        Name = reader.GetString(reader.GetOrdinal("OwnerName")),
                                        Address = reader.GetString(reader.GetOrdinal("Address")),
                                        Email = reader.GetString(reader.GetOrdinal("Email")),
                                        Telephone = reader.GetString(reader.GetOrdinal("Telephone"))
                                    },
                                    BikeType = new BikeType
                                    {
                                        Id = reader.GetInt32(reader.GetOrdinal("BikeTypeId")),
                                        Name = reader.GetString(reader.GetOrdinal("BikeTypeName"))
                                    },
                                    WorkOrders = new List<WorkOrder>()
                                };
                                if (DbUtils.IsNotDbNull(reader, "WorkOrderId"))
                                {
                                    WorkOrder workorder = new WorkOrder
                                    {
                                        Id = reader.GetInt32(reader.GetOrdinal("WorkOrderId")),
                                        DateInitiated = reader.GetDateTime(reader.GetOrdinal("DateInitiated")),
                                        Description = reader.GetString(reader.GetOrdinal("Description"))
                                    };
                                    if (DbUtils.IsNotDbNull(reader,"DateCompleted"))
                                    {
                                        workorder.DateCompleted = reader.GetDateTime(reader.GetOrdinal("DateCompleted"));
                                    }
                                    bike.WorkOrders.Add(workorder);
                                }
                            }
                        }
                    }
                }
                return bike;
            }
        }
        public int GetBikesInShopCount()
        {
            int count = 0;
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                                      SELECT COUNT(DISTINCT b.Id) AS InShop
                                        FROM Bike b
                                        JOIN WorkOrder wo ON b.Id = wo.BikeId
                                        WHERE wo.DateCompleted IS NULL";

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            count = reader.GetInt32(reader.GetOrdinal("InShop"));
                        }
                    }
                }
            }
            return count;
        }
    }
}
