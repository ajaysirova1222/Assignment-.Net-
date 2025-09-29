using System.Data;
using System.Security.Cryptography;
using System.Text;
using MySql.Data.MySqlClient;
using RestSharp;



public class dbServices
{

    private readonly Dictionary<string, string> _sms = new Dictionary<string, string>();


    IConfiguration appsettings = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
    //MySqlConnection conn = null; // this will store the connection which will be persistent 
    MySqlConnection connPrimary = null; // this will store the connection which will be persistent 
    MySqlConnection connReadOnly = null;

    public dbServices() // constructor
    {

        // _sms["sender"] = appsettings["sourceSMS:sender"].ToString();
        // _sms["authkey"] = appsettings["sourceSMS:authkey"].ToString();
        // _sms["SmsServerUrl"] = appsettings["sourceSMS:SmsServerUrl"].ToString();
        // _sms["TemplateId"] = appsettings["sourceSMS:TemplateId"].ToString();
        //_appsettings=appsettings;
        // connectDBPrimary();
        // connectDBReadOnly();
    }


    private void connectDBPrimary()
    {

        try
        {
            if (connPrimary == null || connPrimary.State == ConnectionState.Closed)
            {
                if (connPrimary != null)
                {
                    connPrimary.Dispose();
                }
                connPrimary = new MySqlConnection(appsettings["db:connStrPrimary"]);
                connPrimary.Open();
            }

        }
        catch (Exception ex)
        {
            //throw new ErrorEventArgs(ex); // check as this will throw exception error
            Console.WriteLine(ex);
        }
    }
    // private void connectDBReadOnly()
    // {

    //     try
    //     {
    //         if (connReadOnly == null || connReadOnly.State == ConnectionState.Closed)
    //         {
    //             if (connReadOnly != null)
    //             {
    //                 connPrimary.Dispose();
    //             }
    //             connReadOnly = new MySqlConnection(appsettings["db:connStrPrimary"]);
    //             connReadOnly.Open();
    //         }
    //     }
    //     catch (Exception ex)
    //     {
    //         //throw new ErrorEventArgs(ex); // check as this will throw exception error
    //         Console.WriteLine(ex);
    //     }
    // }
    private async Task connectDBPrimaryAsync()
    {
        try
        {
            if (connPrimary == null || connPrimary.State == ConnectionState.Closed)
            {
                connPrimary?.Dispose();
                connPrimary = new MySqlConnection(appsettings["db:connStrPrimary"]);
                await connPrimary.OpenAsync();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
    }

    // public async Task<List<List<object[]>>> executeSQLAsync(string sq, MySqlParameter[] prms)
    // {
    //     var allTables = new List<List<object[]>>();

    //     try
    //     {
    //         // Create a new connection for each call
    //         using (var conn = new MySqlConnection(appsettings["db:connStrPrimary"]))
    //         {
    //             await conn.OpenAsync();

    //             using (var cmd = conn.CreateCommand())
    //             {
    //                 cmd.CommandText = sq;
    //                 if (prms != null)
    //                     cmd.Parameters.AddRange(prms);

    //                 using (var dr = await cmd.ExecuteReaderAsync())
    //                 {
    //                     do
    //                     {
    //                         var tblRows = new List<object[]>();
    //                         while (await dr.ReadAsync())
    //                         {
    //                             var values = new object[dr.FieldCount];
    //                             dr.GetValues(values);
    //                             tblRows.Add(values);
    //                         }
    //                         allTables.Add(tblRows);
    //                     } while (await dr.NextResultAsync());
    //                 }
    //             }
    //         }
    //     }
    //     catch (Exception ex)
    //     {
    //         Console.WriteLine(ex.Message);
    //         return null;
    //     }

    //     return allTables;
    // }

    //          public async Task<List<Dictionary<string, object>[]>> ExecuteSQLNameAsync(string sq, MySqlParameter[] prms)
    // {
    //      List<Dictionary<string, object>[]> allTables = new List<Dictionary<string, object>[]>();


    //     try
    //     {
    //         if (connPrimary == null || connPrimary.State == ConnectionState.Closed)
    //             await connectDBPrimaryAsync();

    //         using (var cmd = connPrimary.CreateCommand())
    //         {
    //             cmd.CommandText = sq;
    //             if (prms != null)
    //                 cmd.Parameters.AddRange(prms);

    //             using (var dr = await cmd.ExecuteReaderAsync())
    //             {
    //                 // while (await dr.ReadAsync())
    //                 // {
    //                 //     for (int i = 0; i < dr.FieldCount; i++)
    //                 //     {
    //                 //         var rowDict = new Dictionary<string, object>();
    //                 //         string columnName = dr.GetName(i);
    //                 //         object value = dr.IsDBNull(i) ? null : dr.GetValue(i);
    //                 //         rowDict.Add(columnName, value);
    //                 //         allRows.Add(rowDict);
    //                 //     }
    //                 // }
    //                 do
    //                     {
    //                         List<Dictionary<string, object>> tblRows = new List<Dictionary<string, object>>();

    //                         while (await dr.ReadAsync())
    //                         {
    //                             Dictionary<string, object> values = new Dictionary<string, object>();

    //                             for (int i = 0; i < dr.FieldCount; i++)
    //                             {
    //                                 string columnName = dr.GetName(i);
    //                                 object columnValue = dr.GetValue(i);
    //                                 values[columnName] = columnValue;
    //                             }

    //                             tblRows.Add(values);
    //                         }

    //                         allTables.Add(tblRows.ToArray());
    //                     } while (await dr.NextResultAsync());
    //             }
    //         }
    //     }
    //     catch (Exception ex)
    //     {
    //         Console.WriteLine(ex.Message);
    //         connPrimary?.Close();
    //         return null;
    //     }
    //     finally
    //     {
    //         connPrimary?.Close();
    //     }
    //     return allTables;
    // }
    public int ExecuteInsertAndGetLastId(string sq, MySqlParameter[] prms)
    {
        MySqlTransaction trans = null;
        int lastInsertedId = -1;

        try
        {
            if (connPrimary == null || connPrimary.State == 0)
                connectDBPrimaryAsync();

            trans = connPrimary.BeginTransaction();

            var cmd = connPrimary.CreateCommand();
            cmd.Transaction = trans; // Associate command with transaction
            cmd.CommandText = sq;

            if (prms != null)
                cmd.Parameters.AddRange(prms);

            // Execute the INSERT query
            cmd.ExecuteNonQuery();

            // Get the last inserted ID
            cmd.CommandText = "SELECT LAST_INSERT_ID();";
            lastInsertedId = Convert.ToInt32(cmd.ExecuteScalar());

            // Commit the transaction
            trans.Commit();
        }
        catch (Exception ex)
        {
            Console.Write(ex.Message);
            if (trans != null)
            {
                trans.Rollback(); // Rollback the transaction on error
            }
        }
        finally
        {
            connPrimary?.Close(); // Ensure connection is closed
        }

        return lastInsertedId;
    }



    public List<List<object[]>> executeSQL(string sq, MySqlParameter[] prms)
    {
        var allTables = new List<List<object[]>>();

        try
        {
            // Ensure the connection is open
            using (var conn = new MySqlConnection(appsettings["db:connStrPrimary"])) // Use the connection string
            {
                conn.Open();

                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = sq;
                    if (prms != null)
                        cmd.Parameters.AddRange(prms);

                    using (var trans = conn.BeginTransaction())
                    {
                        cmd.Transaction = trans;
                        try
                        {
                            using (var dr = cmd.ExecuteReader())
                            {
                                do
                                {
                                    var tblRows = new List<object[]>();
                                    while (dr.Read())
                                    {
                                        var values = new object[dr.FieldCount];
                                        dr.GetValues(values);
                                        tblRows.Add(values);
                                    }
                                    allTables.Add(tblRows);
                                } while (dr.NextResult());
                            }

                            trans.Commit(); // Commit the transaction if no errors
                        }
                        catch
                        {
                            trans.Rollback(); // Rollback on error
                            throw; // Rethrow to be handled outside
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return null; // Handle the exception appropriately
        }

        return allTables;
    }




    public List<Dictionary<string, object>[]> ExecuteSQLName(string sq, MySqlParameter[] prms)
    {
        MySqlTransaction transaction = null;
        List<Dictionary<string, object>[]> allTables = new List<Dictionary<string, object>[]>();

        try
        {
            using (var conn = new MySqlConnection(appsettings["db:connStrPrimary"])) // Use the connection string
            {
                conn.Open();

                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = sq;
                    if (prms != null)
                        cmd.Parameters.AddRange(prms);

                    using (var trans = conn.BeginTransaction())
                    {
                        cmd.Transaction = trans;
                        try
                        {
                            using (MySqlDataReader reader = cmd.ExecuteReader())
                            {
                                do
                                {
                                    List<Dictionary<string, object>> tblRows = new List<Dictionary<string, object>>();

                                    while (reader.Read())
                                    {
                                        Dictionary<string, object> values = new Dictionary<string, object>();

                                        for (int i = 0; i < reader.FieldCount; i++)
                                        {
                                            string columnName = reader.GetName(i);
                                            object columnValue = reader.GetValue(i);
                                            values[columnName] = columnValue;
                                        }

                                        tblRows.Add(values);
                                    }

                                    allTables.Add(tblRows.ToArray());
                                } while (reader.NextResult());
                            }

                            trans.Commit(); // Commit the transaction if no errors
                        }
                        catch
                        {
                            trans.Rollback(); // Rollback on error
                            throw; // Rethrow to be handled outside
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            transaction?.Rollback();
            connPrimary?.Close();
            return null;
        }
        // finally
        // {
        //     // Ensure the connection is closed in the finally block
        //     connPrimary.Close();

        // }

        return allTables;
    }





    // public List<List<Object[]>> executeSQLpcmdb(string sq, MySqlParameter[] prms) // this will return the database response the last partameter is to allow selection of connectio id
    // {

    //     MySqlTransaction trans = null;
    //     List<List<Object[]>> allTables = new List<List<Object[]>>();

    //     try
    //     {
    //         if (connReadOnly == null)
    //             connectDBReadOnly();

    //         trans = connReadOnly.BeginTransaction();

    //         var cmd = connReadOnly.CreateCommand();
    //         cmd.CommandText = sq;
    //         if (prms != null)
    //             cmd.Parameters.AddRange(prms);

    //         using (MySqlDataReader dr = cmd.ExecuteReader())
    //         {
    //             do
    //             {
    //                 List<Object[]> tblRows = new List<Object[]>();
    //                 while (dr.Read())
    //                 {
    //                     object[] values = new object[dr.FieldCount]; // create an array with sixe of field count
    //                     dr.GetValues(values); // save all values here
    //                     tblRows.Add(values); // add this to the list array
    //                 }
    //                 allTables.Add(tblRows);
    //             } while (dr.NextResult());
    //         }
    //     }
    //     catch (Exception ex)
    //     {
    //         Console.Write(ex.Message);
    //         trans.Rollback(); // check these functions
    //         return null; // if error return null
    //     }
    //     Console.Write("Database Operation Completed Successfully");
    //     trans.Commit(); // check thee functions
    //     connReadOnly.Close(); //here is close the connection
    //     return allTables; // if success return allTables
    // }

    // public List<Dictionary<string, object>[]> capExecuteMultipleSQL(List<string> sqlStatements, MySqlParameter[] parametersSets)
    // {
    //     MySqlTransaction transaction = null;
    //     //  List<List<object[]>> allTables = new List<List<object[]>>();
    //     List<Dictionary<string, object>[]> allTables = new List<Dictionary<string, object>[]>();
    //     try
    //     {
    //         if (connPrimary == null || connPrimary.State == 0)
    //             connectDBPrimary();

    //         transaction = connPrimary.BeginTransaction();

    //         using (var cmd = connPrimary.CreateCommand())
    //         {
    //             for (int i = 0; i < sqlStatements.Count; i++)
    //             {
    //                 cmd.CommandText = sqlStatements[i];
    //                 // CLEAR PARAMETERS FOR EACH EXECUTION OF SQL STATEMENT 
    //                 cmd.Parameters.Clear();
    //                 // ONLY ADD PARAMETERS FOR EACH SQL STATEMENT BASED ON PARAMETERS USED IN SQL STATEMENT
    //                 for (int k = 0; k < parametersSets.Length; k++)
    //                 {
    //                     if (cmd.CommandText.Contains(":" + parametersSets[k].ParameterName.ToString()))
    //                     {
    //                         cmd.Parameters.Add(parametersSets[k]);
    //                     }
    //                 }


    //                 using (MySqlDataReader dr = cmd.ExecuteReader())
    //                 {
    //                     List<Dictionary<string, object>> tblRows = new List<Dictionary<string, object>>();
    //                     while (dr.Read())
    //                     {
    //                         // object[] values = new object[dr.FieldCount];
    //                         // dr.GetValues(values);
    //                         Dictionary<string, object> values = new Dictionary<string, object>();

    //                         for (int j = 0; j < dr.FieldCount; j++)
    //                         {
    //                             string columnName = dr.GetName(j);
    //                             object columnValue = dr.GetValue(j);
    //                             values[columnName] = columnValue;
    //                         }

    //                         tblRows.Add(values);

    //                     }
    //                     allTables.Add(tblRows.ToArray());
    //                 }
    //             }

    //             Console.WriteLine("Database Operation Completed Successfully");
    //             transaction?.Commit();
    //             connPrimary?.Close();
    //         }
    //     }
    //     catch (Exception ex)
    //     {
    //         Console.WriteLine(ex.Message);
    //         transaction?.Rollback();
    //         connPrimary?.Close();
    //         return null;
    //     }
    //     finally
    //     {
    //         connPrimary?.Close();
    //     }

    //     return allTables;
    // }

    // public List<List<object[]>> executeMultipleSQL(List<string> sqlStatements, MySqlParameter[] parametersSets)
    // {
    //     MySqlTransaction transaction = null;
    //     List<List<object[]>> allTables = new List<List<object[]>>();

    //     try
    //     {
    //         if (connPrimary == null || connPrimary.State == 0)
    //             connectDBPrimary();

    //         transaction = connPrimary.BeginTransaction();

    //         using (var cmd = connPrimary.CreateCommand())
    //         {
    //             for (int i = 0; i < sqlStatements.Count; i++)
    //             {
    //                 cmd.CommandText = sqlStatements[i];
    //                 // CLEAR PARAMETERS FOR EACH EXECUTION OF SQL STATEMENT 
    //                 cmd.Parameters.Clear();
    //                 // ONLY ADD PARAMETERS FOR EACH SQL STATEMENT BASED ON PARAMETERS USED IN SQL STATEMENT
    //                 for (int k = 0; k < parametersSets.Length; k++)
    //                 {
    //                     if (cmd.CommandText.Contains(":" + parametersSets[k].ParameterName.ToString()))
    //                     {
    //                         cmd.Parameters.Add(parametersSets[k]);
    //                     }
    //                 }
    //                 // if (parametersSets != null && i < parametersSets.Count)
    //                 // if (parametersSets != null)
    //                 //     cmd.Parameters.AddRange(parametersSets);

    //                 using (MySqlDataReader dr = cmd.ExecuteReader())
    //                 {
    //                     List<object[]> tblRows = new List<object[]>();
    //                     while (dr.Read())
    //                     {
    //                         object[] values = new object[dr.FieldCount];
    //                         dr.GetValues(values);
    //                         tblRows.Add(values);
    //                     }
    //                     allTables.Add(tblRows);
    //                 }
    //             }

    //             Console.WriteLine("Database Operation Completed Successfully");
    //             transaction?.Commit();
    //             connPrimary?.Close();
    //         }
    //     }
    //     catch (Exception ex)
    //     {
    //         Console.WriteLine(ex.Message);
    //         transaction?.Rollback();
    //         connPrimary?.Close();
    //         return null;
    //     }
    //     finally
    //     {
    //         connPrimary?.Close();
    //     }

    //     return allTables;
    // }
    // public int commonAuditTrans(Dictionary<string, object> data)
    // {
    //     int transData = 0;
    //     try
    //     {
    //         var events = data["EVENT"];
    //         var currentDateTime = $"{DateTime.UtcNow:yyyy-MM-dd} {(DateTime.UtcNow.TimeOfDay + new TimeSpan(5, 30, 0)):hh\\:mm}";
    //         MySqlParameter[] myparams = new MySqlParameter[]
    //            {
    //                 new MySqlParameter("@uId",data["uId"]),
    //                 new MySqlParameter("@roleId",data["roleId"]),
    //                 new MySqlParameter("@tDate",currentDateTime),
    //                  new MySqlParameter("@auditRemarks",data["AUDIT_REMARKS"])
    //            };

    //         if (events == "UPDATE_ENTRY")
    //         {
    //             var tId = data["tId"];

    //             var auditTarns = $"insert into hlfppt.e_com_audit_dets(T_ID,UID,ROLE_ID,MNU_ID,REMARKS,ENTRY_DATE,AUDIT_REMARK) values({tId},@uId,@roleId,0,'{events}',@tDate,@auditRemarks)";
    //             var dbData = executeSQL(auditTarns, myparams);
    //             return transData;
    //         }
    //         else if (data["EVENT"] == "DELETE_ENTRY")
    //         {

    //         }
    //         else
    //         {

    //             var uId = data["uId"];
    //             var sqTrans = @"insert into hlfppt.e_acc_trans(MNU_ID,ROLE_ID,T_DATE,U_ID,REF_DATE) values(0,@roleId,@tDate,@uId,@tDate)";
    //             // var sqTrans="insert into hlfppt.e_acc_trans(MNU_ID,SUB_MENU_ID,ROLE_ID,T_TYPE,T_NO,T_DATE,U_ID,ACC_ID,ALT_ACC_ID,REMARKS,MODE,REF_DATE,DRCR,FUND_ID,TRANS_STATUS,LAST_SEQ_NO,CUR_SEQ_NO,AUDIT_TYPE_ID,ACTION_TYPE_ID,VERIFY_STARTED,TOTAL_AMOUNT,INTEREST_RATE,RTGS_STATUS,BS_ID) values(@patId,@docId,@apptDate,@bookingDate,@facId,@slot,0)";
    //             transData = ExecuteInsertAndGetLastId(sqTrans, myparams);
    //             var auditTarns = $"insert into hlfppt.e_com_audit_dets(T_ID,UID,ROLE_ID,MNU_ID,REMARKS,ENTRY_DATE) values({transData},@uId,@roleId,0,'{events}',@tDate)";
    //             var auditData = executeSQL(auditTarns, myparams);
    //         }


    //     }
    //     catch (Exception ex)
    //     {
    //         Console.Write(ex.Message);
    //     }



    //     return transData;
    // }

    public async Task<bool> SendOtpHLF(string mobileNumber, string otp, string templateId)
    {
        try
        {
            var client = new RestClient(new RestClientOptions(_sms["SmsServerUrl"]));
            var request = new RestRequest("");
            request.AddHeader("accept", "application/json");
            request.AddHeader("authkey", _sms["authkey"]);

            request.AddJsonBody(new
            {
                template_id = templateId,
                recipients = new[]
                {
                    new
                    {
                        mobiles = "91" + mobileNumber,
                        var = otp
                    }
                }
            });

            var response = await client.PostAsync(request);

            return response.Content.Contains("\"type\":\"success\"");
        }
        catch (Exception ex)
        {
            // Handle exceptions or log errors
            Console.WriteLine($"Error sending SMS: {ex.Message}");
            return false;
        }
    }

    public string CalculateSHA512(string input)
    {
        using (var sha512 = SHA512.Create())
        {
            byte[] bytes = Encoding.UTF8.GetBytes(input);
            byte[] hashBytes = sha512.ComputeHash(bytes);
            return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
        }
    }
}