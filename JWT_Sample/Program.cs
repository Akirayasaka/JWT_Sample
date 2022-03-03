using JWT_Sample;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();

#region 4. �l�[Swagger�ϥ� JWT Token����
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Test01", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme."
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement {
        {
            new OpenApiSecurityScheme {
            Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});
#endregion

#region 1. ���UJWT�覡(�ݭn�w�� Microsoft.AspNetCore.Authentication.JwtBearer)
builder.Services.AddSingleton<JwtHelpers>();
#endregion

#region 2. ���U���Ҥ覡
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
{
    // �����ҥ��ѮɡA�^�����Y�|�]�t WWW-Authenticate ���Y�A�o�̷|��ܥ��Ѫ��Բӿ��~��]
    // �w�]�Ȭ� true�A���ɷ|�S�O����
    options.IncludeErrorDetails = true;

    options.TokenValidationParameters = new TokenValidationParameters
    {
        // �z�L�o���ŧi�A�N�i�H�q "sub" ���Ȩó]�w�� User.Identity.Name
        NameClaimType = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier",

        // �z�L�o���ŧi�A�N�i�H�q "roles" ���ȡA�åi�� [Authorize] �P�_����
        RoleClaimType = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role",

        // �@��ڭ̳��|���� Issuer
        ValidateIssuer = true,
        ValidIssuer = builder.Configuration.GetValue<string>("JwtSettings:Issuer"),

        // �q�`���ӻݭn���� Audience
        ValidateAudience = false,
        //ValidAudience = "JwtAuthDemo", // �����ҴN���ݭn��g

        // �@��ڭ̳��|���� Token �����Ĵ���
        ValidateLifetime = true,

        // �p�G Token ���]�t key �~�ݭn���ҡA�@�볣�u��ñ���Ӥw
        ValidateIssuerSigningKey = false,

        // "1234567890123456" ���ӱq IConfiguration ���o
        IssuerSigningKey =
            new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration.GetValue<string>("JwtSettings:SignKey")))
    };
});

builder.Services.AddAuthorization();
#endregion



var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

#region 3. �ҥ�����
app.UseAuthentication();
#endregion
app.UseAuthorization();

app.MapControllers();

app.Run();
