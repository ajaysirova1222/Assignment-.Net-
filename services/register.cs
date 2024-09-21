using System;
using System.Collections;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Microsoft.IdentityModel.Tokens;
using MySql.Data.MySqlClient;
using Newtonsoft.Json.Linq;

namespace Assignment_.Net_.services
{
    public class register
    {
        dbServices ds = new dbServices();
        public async Task<responseData> SignUp(requestData req)
        {
            responseData resData = new responseData();
            resData.eventID = req.eventID;

            try
            {
                // Hash the password before saving
                string plainPassword = req.addInfo["password"].ToString();
                string hashedPassword = HashPassword(plainPassword);

                // Define parameters based on User table fields
                MySqlParameter[] para = new MySqlParameter[] {
            new MySqlParameter("@passwordHash", hashedPassword), // Storing hashed password
            new MySqlParameter("@name", req.addInfo["name"].ToString()),
            new MySqlParameter("@email", req.addInfo["email"].ToString()),
            new MySqlParameter("@address", req.addInfo["address"].ToString()),
            new MySqlParameter("@userType", "Applicant"), // Either 'Applicant' or 'Admin'
            new MySqlParameter("@profileHeadline", req.addInfo["profileHeadline"].ToString())
        };

                // Check if the user with the given email already exists
                var qry = $"SELECT * FROM User WHERE email=@email;";
                var checkResult = ds.executeSQL(qry, para);

                if (checkResult[0].Count() != 0)
                {
                    // Duplicate email found, return error
                    resData.rData["rCode"] = 1;
                    resData.rData["rMessage"] = "Duplicate email found";
                }
                else
                {
                    // Insert the new user into the Users table
                    var insertSql = $@"
                INSERT INTO User 
                (Name, Email, Address, UserType, PasswordHash, ProfileHeadline)
                VALUES (@name, @email, @address, @userType, @passwordHash, @profileHeadline);";

                    var insertId = ds.ExecuteInsertAndGetLastId(insertSql, para);

                    if (insertId != null)
                    {
                        resData.rData["rCode"] = 0;
                        resData.rData["rMessage"] = "User registered successfully";
                    }
                }
            }
            catch (Exception ex)
            {
                // Catch any errors and return appropriate error message
                resData.rData["rCode"] = 1;
                resData.rData["rMessage"] = $"Error: {ex.Message}";
            }

            return resData;
        }

        // Helper method for hashing the password using SHA-256
        private string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }
    }
}