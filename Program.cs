using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.EntityFrameworkCore;
using News.Entities;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<NewsContext>(options =>
       options.UseSqlServer(builder.Configuration.GetConnectionString("ConnectionString")));

var app = builder.Build();
app.MapGet("/token", () =>
{

    var environmentId = builder.Configuration.GetValue<string>("CKBoxEnvironmentId");
    var accessKey = builder.Configuration.GetValue<string>("CKBoxAccessKey");
    var securityKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(accessKey));

    var signingCredentials = new SigningCredentials(securityKey, "HS256");
    var header = new JwtHeader(signingCredentials);

    var dateTimeOffset = new DateTimeOffset(DateTime.UtcNow);

    var payload = new JwtPayload
    {
        { "aud", environmentId },
        { "iat", dateTimeOffset.ToUnixTimeSeconds() },
        { "sub", "user-123" },
        { "user", new Dictionary<string, string> {
            { "email", "joe.doe@example.com" },
            { "name", "Joe Doe" }
        } },
        { "auth", new Dictionary<string, object> {
            { "ckbox", new Dictionary<string, string> {
                { "role", "admin" }
            } }
        } }
    };

    var securityToken = new JwtSecurityToken(header, payload);
    var handler = new JwtSecurityTokenHandler();

    return handler.WriteToken(securityToken);
});
// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
