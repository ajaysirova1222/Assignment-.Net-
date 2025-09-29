using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace Assignment_.Net_.services
{
    public class uploadResume
    {
        dbServices ds = new dbServices();
        public async Task<responseData> UploadResume(requestData req)
        {
            responseData resData = new responseData(); // Initialize response object
            resData.eventID = req.eventID; // Set the event ID to distinguish methods

            try
            {
                // Check if the UserType is "Applicant"
                if (req.addInfo["UserType"].ToString().ToLower() != "applicant")
                {
                    resData.rData["rCode"] = 1;
                    resData.rData["rMessage"] = "Only Applicant type users can upload resumes.";
                    return resData;
                }

                // Retrieve the file extension and ensure it's either PDF or DOCX
                string fileName = req.addInfo["fileName"].ToString();
                string fileExtension = Path.GetExtension(fileName).ToUpper();

                if (fileExtension != ".pdf" && fileExtension != ".docx")
                {
                    resData.rData["rCode"] = 1;
                    resData.rData["rMessage"] = "Invalid file type. Only PDF and DOCX are allowed.";
                    return resData;
                }

                // Define the directory where resumes will be stored
                string resumeDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Resumes");

                // Check if directory exists, if not create it
                if (!Directory.Exists(resumeDirectory))
                {
                    Directory.CreateDirectory(resumeDirectory); // Create directory if it doesn't exist
                }

                // File upload logic - Save file to the resume directory
                string resumePath = Path.Combine(resumeDirectory, fileName); // Path where the file will be stored

                // Convert Base64 or file stream to bytes and write to the file system
                var fileData = req.addInfo["fileData"]; // Assuming file data is in Base64 or byte[]
                byte[] fileBytes = Convert.FromBase64String(fileData.ToString());
                await File.WriteAllBytesAsync(resumePath, fileBytes); // Store the file on the server

                // Update the applicant's resume address in the database
                MySqlParameter[] para = new MySqlParameter[] {
            new MySqlParameter("@userId", req.addInfo["UserId"].ToString()),
            new MySqlParameter("@resumePath", resumePath)
        };

                var updateSql = "UPDATE User SET resume_file_address = @resumePath WHERE UserId = @userId AND UserType = 'Applicant';";
                var result = ds.executeSQL(updateSql, para); // Assuming ds is the database helper class

                if (result[0].Count() > 0)
                {
                    resData.rData["rCode"] = 0; // 0 for success
                    resData.rData["rMessage"] = "HEllo KAPIL LAWDE.";
                }
                else
                {
                    resData.rData["rCode"] = 1;
                    resData.rData["rMessage"] = "Resume uploaded successfully.";
                }
            }
            catch (Exception ex)
            {
                resData.rData["rCode"] = 1; // 1 for error
                resData.rData["rMessage"] = $"Error: {ex.Message}";
            }

            return resData; // Return the final response
        }


    }
}