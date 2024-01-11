var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var mvcBuilder = builder.Services.AddRazorPages();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    mvcBuilder.AddRazorRuntimeCompilation();
}
else
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

Licensing.RegisterLicense("OSS GPL-3.0 2024 https://github.com/NetCoreApps/GPTMeetingAgent Mst6bPhkcnZJZYYAUoPDOQr7rpfFXUB8bqGZYkqTSfdP0tWzAxhQ3jHKD+Oi1+D74kFFWDNd3N5eMQbHOK7B84wXtq77gGFtafsyB6OrWm2krB586avvxemZC06mcdbhgKc/medZSfMncWaXuwe2MVdl0/xyXBEOc6g9p874D44=");

app.UseServiceStack(new AppHost());

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.UseStatusCodePagesWithReExecute("/Error", "?status={0}");

app.Run();
