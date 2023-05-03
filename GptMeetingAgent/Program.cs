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

Licensing.RegisterLicense("Individual (c) 2023 Darren Reid Pl4FF/yGVm+ySUXKhKUihKJcrfX2nLgI+7niHG4prhfjj2cFJw/lwSsom24PxPHl6faP7xJeAGdUQ+P6+PGUpSun53l09o4KRyjyVoKC4mU5Z3p3KdiyS8l1JB4sL5DxZXqIi/E25RyJ22Gtc8Enp3H2hOwpCFieg68pfA1Qx8k=");

app.UseServiceStack(new AppHost());

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.UseStatusCodePagesWithReExecute("/Error", "?status={0}");

app.Run();