using backend.Services;

var builder = WebApplication.CreateBuilder(args);
var jwtIssuer = builder.Configuration["Jwt:Issuer"];
var jwtKey = builder.Configuration["Jwt:Key"];

builder.Services.AddOpenApi();
builder.Services.HandleCors();

builder.Services.HandleAuthentication(jwtIssuer!, jwtKey!);
builder.Services.AddAuthorizationBuilder();
builder.Services.AddAuthorization();
builder.Services.AddCustomSingeltons();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

//app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();

app.RegisterEndpoints();
app.Run();