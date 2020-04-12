using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Couchbase;
using Microsoft.Extensions.Logging;

namespace LoggingWebExample
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.

        // #tag::configureservices[]
        public void ConfigureServices(IServiceCollection services)
        {
  
            // Get logger factory to pass it along to the couchbase cluster.

            ILoggerFactory loggerFactory = services.BuildServiceProvider()
                .GetService<ILoggerFactory>();

            var options = new ClusterOptions()
            {
                Logging = loggerFactory,
                UserName = "Administrator",
                Password = "password",
                ConnectionString = "http://localhost"
            };

            // Bootstrap Cluster Object.
            var cluster = Cluster.ConnectAsync(options)
                            .GetAwaiter()
                            .GetResult();
                                
            services.AddSingleton(typeof(ICluster), cluster);            

     
            services.AddControllers().AddNewtonsoftJson(options => options.UseMemberCasing());
            services.AddMvc().AddNewtonsoftJson(options => options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore);

            //Swagger related
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "Sample API", Version = "v1" });
            });
        }
        // #end::configureservices[]
        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Sample API"));


        }
    }
}
