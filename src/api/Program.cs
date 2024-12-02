using PairProgrammingApi.Logging;
using PairProgrammingApi.Setup;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddPairServices(builder.Configuration.GetSection("Db").GetValue<string>("ConnectionString") ?? throw new ArgumentException("Missing connection string"));
// Add Serilog (Console Logs)
builder.Host.UseSerilogSetup();


builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
