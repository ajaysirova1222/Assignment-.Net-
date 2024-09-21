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
        public async Task<responseData> AddProfile(requestData req)
        {
            responseData resData = new responseData();
            resData.eventID = req.eventID;

            try
            {
                var userId = req.addInfo["userId"];
                var resumeBase64 = req.addInfo["resumeBase64"].ToString();
                var skills = req.addInfo["skills"];
                var education = req.addInfo["education"];
                var experience = req.addInfo["experience"];

                // Validate that the resume file is Base64 of PDF or DOC
                if (!IsBase64OfPDFOrDoc(resumeBase64))
                {
                    resData.rData["rCode"] = 1;
                    resData.rData["rMessage"] = "Invalid resume file format. Only Base64 of PDF and DOC files are accepted.";
                    return resData;
                }


                var insertProfileQuery = @"
            INSERT INTO Profile (UserId, ResumeFileAddress, Skills, Education, Experience) 
            VALUES (@userId, @resumeBase64, @skills, @education, @experience);";

                var insertProfileParams = new MySqlParameter[]
                {
            new MySqlParameter("@userId", userId),
            new MySqlParameter("@resumeBase64", resumeBase64),
            new MySqlParameter("@skills", skills),
            new MySqlParameter("@education", education),
            new MySqlParameter("@experience", experience),
            // new MySqlParameter("@jobId", jobId)
                };

                ds.executeSQL(insertProfileQuery, insertProfileParams);

                resData.rData["rCode"] = 0;
                resData.rData["rMessage"] = "Profile added successfully.";
            }
            catch (Exception ex)
            {
                resData.rData["rCode"] = 1;
                resData.rData["rMessage"] = $"Error: {ex.Message}";
            }

            return resData;
        }

        private bool IsBase64OfPDFOrDoc(string base64String)
        {
            try
            {
                // Convert Base64 string to byte array
                byte[] fileBytes = Convert.FromBase64String(base64String);

                // Check the file header (magic number) to ensure it's PDF or DOC
                // PDF files start with "%PDF-" (hex: 25 50 44 46)
                if (fileBytes.Length >= 4 &&
                    fileBytes[0] == 0x25 && fileBytes[1] == 0x50 &&
                    fileBytes[2] == 0x44 && fileBytes[3] == 0x46)
                {
                    return true; // It's a PDF
                }

                // DOC files (Microsoft Word binary format) have a magic number (hex: D0 CF 11 E0)
                if (fileBytes.Length >= 4 &&
                    fileBytes[0] == 0xD0 && fileBytes[1] == 0xCF &&
                    fileBytes[2] == 0x11 && fileBytes[3] == 0xE0)
                {
                    return true; // It's a DOC
                }

                // DOCX files are essentially ZIP archives; check for ZIP file signature (hex: 50 4B 03 04)
                if (fileBytes.Length >= 4 &&
                    fileBytes[0] == 0x50 && fileBytes[1] == 0x4B &&
                    fileBytes[2] == 0x03 && fileBytes[3] == 0x04)
                {
                    return true; // It's a DOCX
                }

                return false; // Not a valid PDF or DOC file
            }
            catch
            {
                return false; // Invalid Base64 string
            }
        }

    }
}