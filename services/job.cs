using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace Assignment.Net.services
{
    public class job
    {
        dbServices ds = new dbServices();

        public async Task<responseData> CreateJobOpening(requestData req)
        {
            responseData resData = new responseData();
            resData.eventID = req.eventID;

            try
            {

                var userId = req.addInfo["postedByUserId"].ToString();

                // Check if the user exists
                var userCheckQuery = $"SELECT * FROM User WHERE UserID = @userId AND UserType='Admin';";
                var userCheckParams = new MySqlParameter[] { new MySqlParameter("@userId", userId) };
                var userResult = ds.executeSQL(userCheckQuery, userCheckParams);

                if (userResult[0].Count() == 0)
                {
                    resData.rData["rCode"] = 1;
                    resData.rData["rMessage"] = "Access denied. Only Admin users can create job openings.";
                    return resData;
                }


                // Define parameters for job creation
                MySqlParameter[] jobParams = new MySqlParameter[] {
            new MySqlParameter("@title", req.addInfo["title"].ToString()),
            new MySqlParameter("@description", req.addInfo["description"].ToString()),
            new MySqlParameter("@postedOn", DateTime.UtcNow),
            new MySqlParameter("@totalApplications", 0), // Initialize to 0
            new MySqlParameter("@companyName", req.addInfo["companyName"].ToString()),
            new MySqlParameter("@postedByUserId", userId)
        };

                // Insert the new job opening into the Job table
                var insertJobSql = @"
            INSERT INTO Job 
            (Title, Description, PostedOn, TotalApplications, CompanyName, PostedByUserId)
            VALUES (@title, @description, @postedOn, @totalApplications, @companyName, @postedByUserId);";

                var insertJobId = ds.ExecuteInsertAndGetLastId(insertJobSql, jobParams);

                if (insertJobId != null)
                {
                    resData.rData["rCode"] = 0;
                    resData.rData["rMessage"] = "Job opening created successfully";
                }
                else
                {
                    resData.rData["rCode"] = 1;
                    resData.rData["rMessage"] = "Failed to create job opening";
                }
            }
            catch (Exception ex)
            {
                resData.rData["rCode"] = 1;
                resData.rData["rMessage"] = $"Error: {ex.Message}";
            }

            return resData;
        }
        public async Task<responseData> GetJobDetailsWithApplicants(requestData req)
        {
            responseData resData = new responseData();
            resData.eventID = req.eventID;

            try
            {
                var userId = req.addInfo["userId"]; // Assuming userId is passed in the request
                var jobId = req.addInfo["jobId"]; // Job ID to fetch details for

                // Check if the user is an Admin
                var userCheckQuery = $"SELECT * FROM User WHERE UserID = @userId AND UserType='Admin';";
                var userCheckParams = new MySqlParameter[] { new MySqlParameter("@userId", userId) };
                var userResult = ds.executeSQL(userCheckQuery, userCheckParams);

                if (userResult[0].Count() == 0)
                {
                    resData.rData["rCode"] = 1;
                    resData.rData["rMessage"] = "Access denied. Only Admin users can view job details.";
                    return resData;
                }

                // Fetch Job details from the Job table
                var jobDetailsQuery = $"SELECT * FROM Job WHERE JobId = @jobId;";
                var jobDetailsParams = new MySqlParameter[] { new MySqlParameter("@jobId", jobId) };
                var jobDetailsResult = ds.executeSQL(jobDetailsQuery, jobDetailsParams);

                if (jobDetailsResult[0].Count() == 0)
                {
                    resData.rData["rCode"] = 1;
                    resData.rData["rMessage"] = "No job found with the given JobId.";
                    return resData;
                }

                // Prepare the job details to return in the response
                var jobDetails = new Dictionary<string, object>
                {
                    ["Title"] = jobDetailsResult[0][0][1],
                    ["Description"] = jobDetailsResult[0][0][2],
                    ["PostedOn"] = jobDetailsResult[0][0][3],
                    ["TotalApplications"] = jobDetailsResult[0][0][4],
                    ["CompanyName"] = jobDetailsResult[0][0][5]
                };

                // Fetch list of applicants for the job from the Profile table
                var applicantsQuery = @"
        SELECT 
          Profile.UserId, Profile.Skills, Profile.Education, Profile.Experience 
        FROM Profile 
        WHERE Profile.JobID = @jobId;";
                var applicantsParams = new MySqlParameter[] { new MySqlParameter("@jobId", jobId) };
                var applicantsResult = ds.executeSQL(applicantsQuery, applicantsParams);

                // Prepare list of applicants to return
                List<Dictionary<string, object>> applicants = new List<Dictionary<string, object>>();
                foreach (var applicantRow in applicantsResult[0])
                {
                    // Fetch additional details (Name, Email) for each applicant from User table
                    var applicantUserId = applicantRow[0]; // UserId from Profile
                    var qry = "SELECT Name, Email FROM User WHERE UserId = @UserId;";
                    var paramsUser = new MySqlParameter[] { new MySqlParameter("@UserId", applicantUserId) };
                    var resultUser = ds.executeSQL(qry, paramsUser);

                    if (resultUser[0].Count() == 0)
                    {
                        resData.rData["rCode"] = 1;
                        resData.rData["rMessage"] = "No user found for the given applicant UserId.";
                        return resData;
                    }

                    // Prepare the applicant details
                    var applicant = new Dictionary<string, object>
                    {
                        ["UserId"] = applicantUserId,  // UserId from Profile
                        ["Name"] = resultUser[0][0][0],  // Name from User
                        ["Email"] = resultUser[0][0][1], // Email from User
                        ["Skills"] = applicantRow[1],    // Skills from Profile
                        ["Education"] = applicantRow[2], // Education from Profile
                        ["Experience"] = applicantRow[3] // Experience from Profile
                    };
                    applicants.Add(applicant);
                }

                // Populate response data
                resData.rData["rCode"] = 0;
                resData.rData["rMessage"] = "Job details fetched successfully.";
                resData.rData["jobDetails"] = jobDetails;
                resData.rData["applicants"] = applicants; // Return list of applicants
            }
            catch (Exception ex)
            {
                resData.rData["rCode"] = 1;
                resData.rData["rMessage"] = $"Error: {ex.Message}";
            }

            return resData;
        }
        public async Task<responseData> ApplyForJob(requestData req)
        {
            responseData resData = new responseData();
            resData.eventID = req.eventID;

            try
            {
                var userId = req.addInfo["userId"].ToString(); // Assuming userId is passed in the request
                var jobId = req.addInfo["jobId"]; // Job ID to apply for

                // Check if the user exists in the Profile table
                var profileCheckQuery = $"SELECT * FROM Profile WHERE UserId = @userId;";
                var profileCheckParams = new MySqlParameter[] { new MySqlParameter("@userId", userId) };
                var profileResult = ds.executeSQL(profileCheckQuery, profileCheckParams);

                if (profileResult[0].Count() == 0)
                {
                    resData.rData["rCode"] = 1;
                    resData.rData["rMessage"] = "User profile not found. Please create a profile before applying.";
                    return resData;
                }

                // Update JobID in the Profile table for the user
                var updateJobSql = @"
            UPDATE Profile 
            SET JobID = @jobId
            WHERE UserId = @userId;";

                var updateParams = new MySqlParameter[] {
            new MySqlParameter("@userId", userId),
            new MySqlParameter("@jobId", jobId)
        };

                var updateResult = ds.ExecuteInsertAndGetLastId(updateJobSql, updateParams);

                if (updateResult != null)
                {
                    // Increment TotalApplications in the Job table
                    var incrementApplicationsSql = @"
                UPDATE Job 
                SET TotalApplications = TotalApplications + 1 
                WHERE JobId = @jobId;";

                    var incrementParams = new MySqlParameter[] { new MySqlParameter("@jobId", jobId) };
                    ds.executeSQL(incrementApplicationsSql, incrementParams);

                    resData.rData["rCode"] = 0;
                    resData.rData["rMessage"] = "Job application submitted successfully.";
                }
                else
                {
                    resData.rData["rCode"] = 1;
                    resData.rData["rMessage"] = "Failed to update job application.";
                }
            }
            catch (Exception ex)
            {
                resData.rData["rCode"] = 1;
                resData.rData["rMessage"] = $"Error: {ex.Message}";
            }

            return resData;
        }

        public async Task<responseData> GetAllUsers(requestData req)
        {
            responseData resData = new responseData();
            resData.eventID = req.eventID;

            try
            {
                var userId = req.addInfo["userId"]; // Assuming userId is passed in the request

                // Check if the user is an Admin
                var userCheckQuery = "SELECT * FROM User WHERE UserID = @userId AND UserType = 'Admin';";
                var userCheckParams = new MySqlParameter[] { new MySqlParameter("@userId", userId) };
                var userResult = ds.executeSQL(userCheckQuery, userCheckParams);

                if (userResult[0].Count() == 0)
                {
                    resData.rData["rCode"] = 1;
                    resData.rData["rMessage"] = "Access denied. Only Admin users can access this resource.";
                    return resData;
                }

                // Fetch all users from the database
                var allUsersQuery = @"
            SELECT UserID, Name, Email, UserType 
            FROM User WHERE UserType = 'Applicant';";
                var allUsersResult = ds.executeSQL(allUsersQuery, null);

                if (allUsersResult[0].Count() == 0)
                {
                    resData.rData["rCode"] = 1;
                    resData.rData["rMessage"] = "No users found in the system.";
                    return resData;
                }

                // Prepare the list of users to return
                List<Dictionary<string, object>> users = new List<Dictionary<string, object>>();
                foreach (var userRow in allUsersResult[0])
                {
                    var user = new Dictionary<string, object>
                    {
                        ["UserID"] = userRow[0],  // UserID
                        ["Name"] = userRow[1],    // Name
                        ["Email"] = userRow[2],   // Email
                        ["UserType"] = userRow[3] // UserType
                    };
                    users.Add(user);
                }

                // Populate response data
                resData.rData["rCode"] = 0;
                resData.rData["rMessage"] = "Users fetched successfully.";
                resData.rData["users"] = users; // Return the list of users
            }
            catch (Exception ex)
            {
                resData.rData["rCode"] = 1;
                resData.rData["rMessage"] = $"Error: {ex.Message}";
            }

            return resData;
        }

        public async Task<responseData> GetApplicantDetails(requestData req)
        {
            responseData resData = new responseData();
            resData.eventID = req.eventID;

            try
            {
                var adminUserId = req.addInfo["userId"]; // Assuming admin userId is passed in the request
                var applicantUserId = req.addInfo["applicantUserId"]; // Assuming applicant userId is passed in the request

                // Check if the user is an Admin
                var userCheckQuery = "SELECT * FROM User WHERE UserID = @userId AND UserType = 'Admin';";
                var userCheckParams = new MySqlParameter[] { new MySqlParameter("@userId", adminUserId) };
                var userResult = ds.executeSQL(userCheckQuery, userCheckParams);

                if (userResult[0].Count() == 0)
                {
                    resData.rData["rCode"] = 1;
                    resData.rData["rMessage"] = "Access denied. Only Admin users can view applicant details.";
                    return resData;
                }

                // Fetch the applicant's profile data
                var applicantQuery = @"
            SELECT 
                User.UserID, User.Name, User.Email, Profile.Skills, Profile.Education, Profile.Experience 
            FROM Profile 
            INNER JOIN User ON Profile.UserId = User.UserID 
            WHERE Profile.UserId = @applicantUserId;";

                var applicantParams = new MySqlParameter[] { new MySqlParameter("@applicantUserId", applicantUserId) };
                var applicantResult = ds.executeSQL(applicantQuery, applicantParams);

                if (applicantResult[0].Count() == 0)
                {
                    resData.rData["rCode"] = 1;
                    resData.rData["rMessage"] = "No details found for the specified applicant.";
                    return resData;
                }

                // Prepare the applicant details to return in the response
                var applicantDetails = new Dictionary<string, object>
                {
                    ["UserID"] = applicantResult[0][0][0],      // Applicant's UserID
                    ["Name"] = applicantResult[0][0][1],        // Applicant's Name
                    ["Email"] = applicantResult[0][0][2],       // Applicant's Email
                    ["Skills"] = applicantResult[0][0][3],      // Applicant's Skills
                    ["Education"] = applicantResult[0][0][4],   // Applicant's Education
                    ["Experience"] = applicantResult[0][0][5]   // Applicant's Experience
                };

                // Populate response data
                resData.rData["rCode"] = 0;
                resData.rData["rMessage"] = "Applicant details fetched successfully.";
                resData.rData["applicantDetails"] = applicantDetails;
            }
            catch (Exception ex)
            {
                resData.rData["rCode"] = 1;
                resData.rData["rMessage"] = $"Error: {ex.Message}";
            }

            return resData;
        }
        public async Task<responseData> GetAllJobs(requestData req)
        {
            responseData resData = new responseData();
            resData.eventID = req.eventID;

            try
            {
                // var userId = req.addInfo["userId"]; // Assuming userId is passed in the request

                // Check if the user is an Admin
                // var userCheckQuery = "SELECT * FROM User WHERE UserID = @userId AND UserType = 'Admin';";
                // var userCheckParams = new MySqlParameter[] { new MySqlParameter("@userId", userId) };
                // var userResult = ds.executeSQL(userCheckQuery, userCheckParams);

                // if (userResult[0].Count() == 0)
                // {
                //     resData.rData["rCode"] = 1;
                //     resData.rData["rMessage"] = "Access denied. Only Admin users can access this resource.";
                //     return resData;
                // }

                // Fetch all users from the database
                var allUsersQuery = @"
                 SELECT * FROM Job ;";
                var allUsersResult = ds.executeSQL(allUsersQuery, null);

                if (allUsersResult[0].Count() == 0)
                {
                    resData.rData["rCode"] = 1;
                    resData.rData["rMessage"] = "No users found in the system.";
                    return resData;
                }

                // Prepare the list of users to return
                List<Dictionary<string, object>> Vacancy = new List<Dictionary<string, object>>();
                foreach (var userRow in allUsersResult[0])
                {
                    var jobs = new Dictionary<string, object>
                    {
                        ["Title"] = userRow[0],  // UserID
                        ["Description"] = userRow[1],    // Name
                        ["PostedOn"] = userRow[2],   // Email
                        ["CompanyName"] = userRow[3] // UserType
                    };
                    Vacancy.Add(jobs);
                }

                // Populate response data
                resData.rData["rCode"] = 0;
                resData.rData["rMessage"] = "Users fetched successfully.";
                resData.rData["users"] = Vacancy; // Return the list of users
            }
            catch (Exception ex)
            {
                resData.rData["rCode"] = 1;
                resData.rData["rMessage"] = $"Error: {ex.Message}";
            }

            return resData;
        }

    }
}