using System.Text;
using BibliotecaApi.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Repositório em memória (Singleton para manter dados entre requisições)
builder.Services.AddSingleton<BibliotecaRepository>();

builder.Services.AddControllers();

// ---------------------------------------------------------------
// Autenticação JWT — PRÉ-PRONTA (tema da Aula 07, não desta aula).
// Está aqui para que existam endpoints protegidos e o Swagger tenha
// o que autorizar no Passo 9 (security scheme + botão Authorize).
// ---------------------------------------------------------------
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Emissor"],
            ValidAudience = builder.Configuration["Jwt:Audiencia"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Chave"]!)),
        };
    });
builder.Services.AddAuthorization();

// ---------------------------------------------------------------
// Passo 2 + 5 + 6b + 9a — Configuração Swagger/OpenAPI
// ---------------------------------------------------------------
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    // Passo 5 — Metadados da API no topo do Swagger
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Version = "v1",
        Title = "API da Biblioteca",
        Description = "API REST para gerenciamento de livros da biblioteca",
        Contact = new Microsoft.OpenApi.Models.OpenApiContact
        {
            Name = "Equipe de Desenvolvimento",
            Email = "202451086472@alunos.newtonpaiva.edu.br",
        }
    });

    // Passo 6b — Inclui os comentários XML na documentação Swagger
    var xmlFilename = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFilename);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }

    // Passo 9a — Security scheme Bearer (.NET 6 + Swashbuckle 6.5.0)
    // Permite que o Swagger exiba o botão "Authorize" com o cadeado para enviar o token JWT
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Informe o token JWT obtido em POST /api/auth/login."
    });

    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// ---------------------------------------------------------------
// Passo 3 + 10 — Middlewares do Swagger
// ---------------------------------------------------------------
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    // Expõe a especificação v1 e coloca a interface na raiz (http://localhost:<porta>/)
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "API da Biblioteca v1");
    options.RoutePrefix = string.Empty;
    options.DocumentTitle = "Documentação - API da Biblioteca";
});

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();