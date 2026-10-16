using Loan.Application.Interface;
using Loan.Infrastructure.Data;
using Loan.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Loan.Application.Mapper;

var builder = WebApplication.CreateBuilder(args);

// Configure CORS to allow requests from Angular.
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        policy
            .WithOrigins("http://localhost:4200")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<AppDbContext>(options =>
options.UseSqlServer(
builder.Configuration.GetConnectionString("dbconn")
)
);

builder.Services.AddScoped<ISupportTicketService, SupportTicketService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IReportsService, ReportsService>();
builder.Services.AddScoped<ILMSReportsService, LMSReportsService>();

builder.Services.AddAutoMapper(cfg => { },
    typeof(SupportTicketMappingProfile).Assembly);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("AllowAngular");

app.UseAuthorization();

app.MapControllers();

app.Run();
