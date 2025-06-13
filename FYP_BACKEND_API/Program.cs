var builder = WebApplication.CreateBuilder(args);

// Enable CORS for SignalR & API requests
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy => policy
            .AllowAnyMethod()
            .AllowAnyHeader()
            .SetIsOriginAllowed(_ => true) // ✅ Allow specific origins dynamically
            .AllowCredentials()); // ✅ Needed for SignalR
});

// Add SignalR
builder.Services.AddSignalR();

// Add controllers
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = null; // Keep JSON property names unchanged
    });

// Enable Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Apply CORS before middleware
app.UseCors("AllowAll");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// ✅ Correct placement of SignalR Hub mapping
app.MapHub<LocationHub>("/LocationHub"); // Ensure correct Hub class name

app.Run();



// Add CORS policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.WithOrigins("http://localhost:53851") // Your Flutter origin
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Add SignalR
builder.Services.AddSignalR();


// Middleware pipeline
app.UseHttpsRedirection();
app.UseRouting(); // ⚠️ Must come before CORS

app.UseCors("AllowAll"); // Apply CORS policy

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();


builder.Services.AddCors(options => {
    options.AddPolicy("FlutterCors", policy => {
        policy.WithOrigins("http://localhost:53851")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

builder.Services.AddSignalR();


app.UseRouting();
app.UseCors("FlutterCors");

app.MapHub<LocationHub>("/LocationHub");
app.Run();