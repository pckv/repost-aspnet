using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;

[assembly: ApiController]

namespace RepostAspNet
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<DatabaseContext>(options =>
                options.UseInMemoryDatabase("RepostDatabase"));

            services.AddControllers(options =>
                options.Filters.Add(new ErrorResponseFilter()));

            services.AddSwaggerGen(options => options.SwaggerDoc("openapi", new OpenApiInfo
            {
                Title = "Repost",
                Version = "0.0.1",
                Description = "Repost API written in ASP.NET Core 3.1 Web APIs\n\n" +
                              "[View source code on GitHub](URL_HERE)\n\n" +
                              "Authors: pckv, EspenK, jonsondrem"
            }));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseSwagger(options => options.RouteTemplate = "{documentName}.json");
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/openapi.json", "Repost API 0.0.1");
                options.RoutePrefix = "api/swagger";
            });

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints => endpoints.MapControllers());
        }
    }
}