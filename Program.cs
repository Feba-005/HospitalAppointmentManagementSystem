// Initialize the web application builder
var builder = WebApplication.CreateBuilder(args);

// Register standard MVC controllers with views support
builder.Services.AddControllersWithViews();

// Register IHttpClientFactory for making asynchronous outbound HTTP requests
builder.Services.AddHttpClient();

// Register in-memory caching service (IMemoryCache)
builder.Services.AddMemoryCache();

// Register HTTP Response Caching services
builder.Services.AddResponseCaching();

// Register distributed memory cache service required for session state storage
builder.Services.AddDistributedMemoryCache();

// Configure session options and cookie security settings
builder.Services.AddSession(options =>
{
    // Set the session idle expiration window to 20 minutes
    options.IdleTimeout = TimeSpan.FromMinutes(20);
    // Ensure session cookies are accessible only by the server via HTTP
    options.Cookie.HttpOnly = true;
    // Mark the session cookie as essential for application functionality
    options.Cookie.IsEssential = true;
});

// Build the web application instance
var app = builder.Build();

// Configure exception handling for non-development environments
if (!app.Environment.IsDevelopment())
{
    // Use error handler path for unhandled exceptions
    app.UseExceptionHandler("/Home/Error");
    // Enforce HTTP Strict Transport Security
    app.UseHsts();
}

// Enforce HTTPS redirection for incoming connections
app.UseHttpsRedirection();

// Serve static resources like CSS, JavaScript, and images from wwwroot
app.UseStaticFiles();

// Add endpoint routing middleware to the pipeline
app.UseRouting();

// Add response caching middleware before session and authorization
app.UseResponseCaching();

// Enable session state management middleware
app.UseSession();

// Enable authorization middleware
app.UseAuthorization();

// Map default controller route pattern
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Execute and listen on the configured port
app.Run();