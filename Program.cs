using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Assignment.Net.services;
using Assignment_.Net_.services;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authorization;
using MySql.Data.MySqlClient;
//UseUrls("http://localhost:5002").

WebHost.CreateDefaultBuilder().
ConfigureServices(s =>
{
    IConfiguration appsettings = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
    s.AddSingleton<register>();
    s.AddSingleton<login>();
    s.AddSingleton<uploadResume>();
    s.AddSingleton<job>();
    s.AddHttpContextAccessor();
    s.AddAuthorization();
    s.AddAuthentication("SourceJWT").AddScheme<SourceJwtAuthenticationSchemeOptions, SourceJwtAuthenticationHandler>("SourceJWT", options =>
    {
        options.SecretKey = appsettings["jwt_config:Key"].ToString();
        options.ValidIssuer = appsettings["jwt_config:Issuer"].ToString();
        options.ValidAudience = appsettings["jwt_config:Audience"].ToString();
        options.Subject = appsettings["jwt_config:Subject"].ToString();
    });
    s.AddCors();
    s.AddHttpClient<TestServiceRequest>();
    s.AddControllers();



}).
Configure(app =>
 {
     app.UseStaticFiles();
     app.UseRouting();
     app.UseAuthentication();
     app.UseAuthorization();


     app.UseCors(options =>
         options.WithOrigins("https://localhost:7181", "https://210.210.210.31:30804", "https://dev-api.sourceinfosys.in:30810/")
         .AllowAnyHeader().AllowAnyMethod().AllowCredentials());

     app.UseEndpoints(e =>
     {

         var reg = e.ServiceProvider.GetRequiredService<register>();
         var log = e.ServiceProvider.GetRequiredService<login>();
         var upl = e.ServiceProvider.GetRequiredService<uploadResume>();
         var adm = e.ServiceProvider.GetRequiredService<job>();



         e.MapPost("/signup",
                [AllowAnonymous] async (HttpContext http) =>
                {
                    var body = await new StreamReader(http.Request.Body).ReadToEndAsync();
                    requestData rData = JsonSerializer.Deserialize<requestData>(body);
                    if (rData.eventID == "1001")
                        await http.Response.WriteAsJsonAsync(await reg.SignUp(rData));
                });

         e.MapPost("/login",
                        [AllowAnonymous] async (HttpContext http) =>
                        {
                            var body = await new StreamReader(http.Request.Body).ReadToEndAsync();
                            requestData rData = JsonSerializer.Deserialize<requestData>(body);
                            if (rData.eventID == "1001")
                                await http.Response.WriteAsJsonAsync(await log.Login(rData));
                        });

         e.MapPost("/uploadResume",
        [AllowAnonymous] async (HttpContext http) =>
        {
            var body = await new StreamReader(http.Request.Body).ReadToEndAsync();
            requestData rData = JsonSerializer.Deserialize<requestData>(body);
            if (rData.eventID == "1001")
                await http.Response.WriteAsJsonAsync(await upl.UploadResume(rData));
        });
         e.MapPost("/job",
     [AllowAnonymous] async (HttpContext http) =>
     {
         var body = await new StreamReader(http.Request.Body).ReadToEndAsync();
         requestData rData = JsonSerializer.Deserialize<requestData>(body);
         if (rData.eventID == "1001")
             await http.Response.WriteAsJsonAsync(await adm.CreateJobOpening(rData));
         else if (rData.eventID == "1002")
             await http.Response.WriteAsJsonAsync(await adm.GetJobDetailsWithApplicants(rData));
         else if (rData.eventID == "1003")
             await http.Response.WriteAsJsonAsync(await adm.ApplyForJob(rData));
     });
         e.MapGet("/admin/applicants",
        [AllowAnonymous] async (HttpContext http) =>
        {
            var body = await new StreamReader(http.Request.Body).ReadToEndAsync();
            requestData rData = JsonSerializer.Deserialize<requestData>(body);
            if (rData.eventID == "1001")
                await http.Response.WriteAsJsonAsync(await adm.GetAllUsers(rData));
            if (rData.eventID == "1002")
                await http.Response.WriteAsJsonAsync(await adm.GetResumeDetails(rData));
            if (rData.eventID == "1003")
                await http.Response.WriteAsJsonAsync(await adm.GetAllJobs(rData));
        });


         e.MapGet("/bing",
                async c => await c.Response.WriteAsJsonAsync("{'Name':'Ajay','Age':'26'}"));

     });

 }).Build().Run();

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/", () => "Hello Ajay!");

app.Run();
public record requestData
{
    [Required]
    public string eventID { get; set; }
    [Required]
    public IDictionary<string, object> addInfo { get; set; }
}

public record responseData
{
    public responseData()
    {
        eventID = "";
        rStatus = 0;
        rData = new Dictionary<string, object>();
    }
    [Required]
    public int rStatus { get; set; } = 0;
    public string eventID { get; set; }
    public IDictionary<string, object> addInfo { get; set; }
    public IDictionary<string, object> rData { get; set; }
}
public class TestServiceRequest
{
    private readonly HttpClient _httpClient;


    // returns a JSON String 
    public String executeSQL(String sql, String prm)
    {
        return "";
    }

    public TestServiceRequest(HttpClient httpClient)
    {
        var _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri("http://localhost:5002/");

      }

}