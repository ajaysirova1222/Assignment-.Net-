using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;
using MySql.Data.MySqlClient;

namespace Assignment_.Net_.services
{
    public class login
    {
        dbServices ds = new dbServices();

        private readonly Dictionary<string, string> jwt_config = new Dictionary<string, string>();

        IConfiguration appsettings = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();

        public login()
        {
            jwt_config["Key"] = appsettings["jwt_config:Key"].ToString();
            jwt_config["Issuer"] = appsettings["jwt_config:Issuer"].ToString();
            jwt_config["Audience"] = appsettings["jwt_config:Audience"].ToString();
            jwt_config["Subject"] = appsettings["jwt_config:Subject"].ToString();
            jwt_config["ExpiryDuration_app"] = appsettings["jwt_config:ExpiryDuration_app"].ToString();
            jwt_config["ExpiryDuration_web"] = appsettings["jwt_config:ExpiryDuration_web"].ToString();
        }

        public async Task<responseData> Login(requestData req)
        {
            responseData resData = new responseData();
            try
            {
                // Get the user input (either email or mobile number)
                string input = req.addInfo["UserId"].ToString();
                bool isEmail = IsValidEmail(input);

                string columnName;

                // Determine whether the input is an email or mobile number
                if (isEmail)
                {
                    columnName = "email";
                }

                else
                {
                    resData.rData["rCode"] = 1;
                    resData.rData["rMessage"] = "Invalid Email ";
                    return resData;
                }

                // Parameters for the query
                MySqlParameter[] myParams = new MySqlParameter[] {
            new MySqlParameter("@UserId", input)
        };

                // Query to get the user details based on email or mobile number
                var qry = $"SELECT * FROM User WHERE {columnName} = @UserId";
                var data = ds.ExecuteSQLName(qry, myParams);

                // Check if the user exists
                if (data == null || data[0].Count() == 0)
                {
                    resData.rData["rCode"] = 1;
                    resData.rData["rMessage"] = "Invalid Credentials";
                }
                else
                {
                    var id = data[0][0]["UserId"];

                    var claims = new[]
               {
                             new Claim("userId",id.ToString()),
                             new Claim("guid", cf.CalculateSHA256Hash(req.addInfo["guid"].ToString())),
                        };
                    var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt_config["Key"]));
                    var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256Signature);
                    var tokenDescriptor = new JwtSecurityToken(issuer: jwt_config["Issuer"], audience: jwt_config["Audience"], claims: claims,
                        expires: DateTime.Now.AddMinutes(Int32.Parse(jwt_config["ExpiryDuration_app"])), signingCredentials: credentials);
                    var token = new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);
                    // Retrieve hashed password from the database
                    string storedHash = data[0][0]["PasswordHash"].ToString();

                    // Compare the provided password after hashing it
                    string providedPassword = req.addInfo["password"].ToString();
                    if (VerifyPassword(providedPassword, storedHash))
                    {
                        // Login successful, populate response with user details
                        resData.eventID = req.eventID;
                        resData.rData["rCode"] = 0;
                        resData.rData["rMessage"] = "Login Successfully";
                        resData.rData["UserId"] = data[0][0]["UserId"];
                        resData.rData["Name"] = data[0][0]["Name"];
                        resData.rData["Email"] = data[0][0]["Email"];
                        resData.rData["Address"] = data[0][0]["Address"];
                        resData.rData["UserType"] = data[0][0]["UserType"];
                        resData.rData["ProfileHeadline"] = data[0][0]["ProfileHeadline"];
                        resData.rData["Token"] = token;
                    }

                    else
                    {
                        // Password mismatch
                        resData.rData["rCode"] = 1;
                        resData.rData["rMessage"] = "Invalid Credentials";
                    }
                }
            }
            catch (Exception ex)
            {
                resData.rData["rCode"] = 1;
                resData.rData["rMessage"] = $"Error: {ex.Message}";
            }

            return resData;
        }

        // Method to validate an email address
        public static bool IsValidEmail(string email)
        {
            string pattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
            return Regex.IsMatch(email, pattern);
        }

        public static bool VerifyPassword(string inputPassword, string storedHash)
        {
            // Hash the input password using the same method as during registration
            string hashedInputPassword = CalculateSHA256Hash(inputPassword);

            // Compare the hashed input password with the stored hashed password
            return hashedInputPassword == storedHash;
        }

        // Method to calculate the SHA-256 hash of a string
        public static string CalculateSHA256Hash(string rawData)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                // Compute the hash as a byte array
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));

                // Convert the byte array to a string representation (hexadecimal)
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