using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Proj_ICFT.Data;
using Proj_ICFT.Services.FormularioServices.Implementacao;
using Proj_ICFT.Services.FormularioServices.Interface;
using Proj_ICFT.Services.PacienteServices.Interface;
using Proj_ICFT.Services.UsuariosService.Implementacao;
using Proj_ICFT.Services.UsuariosService.Interface;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;
var configuration = builder.Configuration;

// Add services to the container.
builder.Services.AddControllersWithViews();



builder.Services.AddSqlServer<FrequenciaDbContext>(builder.Configuration.GetConnectionString("DefaultConnection"));
builder.Services.AddSqlServer<TipoDbContext>(builder.Configuration.GetConnectionString("DefaultConnection"));
builder.Services.AddSqlServer<CategoriaDbContext>(builder.Configuration.GetConnectionString("DefaultConnection"));
builder.Services.AddSqlServer<InstrucoesAdicionaisDbContext>(builder.Configuration.GetConnectionString("DefaultConnection"));
builder.Services.AddSqlServer<PacienteICTDbContext>(builder.Configuration.GetConnectionString("DefaultConnection"));
builder.Services.AddSqlServer<UsuariosDbContext>(builder.Configuration.GetConnectionString("DefaultConnection"));
builder.Services.AddSqlServer<InstrucoesAdicionaisPacienteDbContext>(builder.Configuration.GetConnectionString("DefaultConnection"));




builder.Services.AddScoped<IFormularioServices, FormularioServices>();
builder.Services.AddScoped<IPacienteServices, PacienteServices>();
builder.Services.AddScoped<IUsuarioService, UsuarioService>();


builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = "https://securetoken.google.com/web-ict-56392";
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = "https://securetoken.google.com/web-ict-56392",
            ValidateAudience = true,
            ValidAudience = "web-ict-56392",
            ValidateLifetime = true
        };
        options.RequireHttpsMetadata = false;
        options.MetadataAddress = "https://securetoken.google.com/web-ict-56392/.well-known/openid-configuration";

    });

services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(60);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = false;
}); //adicionado tempo de sessão ativa

// Add services to the container.
services.AddMvc().AddSessionStateTempDataProvider();
services.AddSession();

var app = builder.Build();

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

app.UseCors(x => x
         .AllowAnyOrigin()
         .AllowAnyMethod()
         .AllowAnyHeader());


//app.UseHttpsRedirection();
app.UseSession();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Home}");

app.Run();
