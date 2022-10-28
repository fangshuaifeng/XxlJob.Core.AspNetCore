#### 服务注册
```c#
public class Startup
{
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddControllers();

        // 注入Xxl服务 & 注册所有任务
        services.AddXxlJobService(Configuration).ScanJobHandler(typeof(Program).Assembly);
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();

            // 映射路由[BasePath] , 默认 = xxl-job
            endpoints.MapXxlJob();
        });
    }
}
```

#### appsettings.json 配置 `XxlJobOptions`
```json
  "xxlJob": {
    "AccessToken": "default_token",
    "adminAddresses": [ "http://10.100.23.50:9800/xxl-job-admin" ],
    "appName": "xxl-job-executor-dotnet",
    "port": 5000,
    "BasePath": "xxl-job"
  }
```

#### 示例
```c#
//[JobHandler("demoJobHandler")]
public class DemoJobHandler : IJobBaseHandler
{
    public DemoJobHandler()
    {
    }

    public async Task<ReturnT> Execute(JobContext context)
    {
        context.JobLogger.Log("开始休眠5秒");
        await Task.Delay(5000, context.CancellationToken).ConfigureAwait(false);
        context.JobLogger.Log("休眠5秒结束");
        return ReturnT.SUCCESS;
    }
}
```