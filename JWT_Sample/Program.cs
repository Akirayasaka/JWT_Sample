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

#region 4. 追加Swagger使用 JWT Token驗證
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

#region 1. 註冊JWT方式(需要安裝 Microsoft.AspNetCore.Authentication.JwtBearer)
builder.Services.AddSingleton<JwtHelpers>();
#endregion

#region 2. 註冊驗證方式
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
{
    // 當驗證失敗時，回應標頭會包含 WWW-Authenticate 標頭，這裡會顯示失敗的詳細錯誤原因
    // 預設值為 true，有時會特別關閉
    options.IncludeErrorDetails = true;

    options.TokenValidationParameters = new TokenValidationParameters
    {
        // 透過這項宣告，就可以從 "sub" 取值並設定給 User.Identity.Name
        NameClaimType = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier",

        // 透過這項宣告，就可以從 "roles" 取值，並可讓 [Authorize] 判斷角色
        RoleClaimType = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role",

        // 一般我們都會驗證 Issuer
        ValidateIssuer = true,
        ValidIssuer = builder.Configuration.GetValue<string>("JwtSettings:Issuer"),

        // 通常不太需要驗證 Audience
        ValidateAudience = false,
        //ValidAudience = "JwtAuthDemo", // 不驗證就不需要填寫

        // 一般我們都會驗證 Token 的有效期間
        ValidateLifetime = true,

        // 如果 Token 中包含 key 才需要驗證，一般都只有簽章而已
        ValidateIssuerSigningKey = false,

        // "1234567890123456" 應該從 IConfiguration 取得
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

#region 3. 啟用驗證
app.UseAuthentication();
#endregion
app.UseAuthorization();

app.MapControllers();

app.Run();
