using System.Linq;
using System.Reflection;
using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using XxlJob.Core.TaskExecutors;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;
using XxlJob.Core.Config;
using XxlJob.AspNetCore;

namespace XxlJob.Core.AspNetCore.XxlJobExtensions
{
    public static class XxlJobBuilderExtensions
    {
        public static IXxlJobBuilder AddJobHandler<TJob>(this IXxlJobBuilder builder) where TJob : class, IJobBaseHandler
        {
            builder.Services.Configure(delegate (JobOptions options)
            {
                options.AddJob<TJob>();
            }).TryAddScoped<TJob>();
            return builder;
        }

        public static IXxlJobBuilder AddJobHandler<TJob>(this IXxlJobBuilder builder, string jobName) where TJob : class, IJobBaseHandler
        {
            string jobName2 = jobName;
            builder.Services.Configure(delegate (JobOptions options)
            {
                options.AddJob<TJob>(jobName2);
            }).TryAddScoped<TJob>();
            return builder;
        }

        /// <summary>
        /// 注入服务
        /// </summary>
        public static IXxlJobBuilder AddXxlJobService(this IServiceCollection services, IConfiguration configuration, Action<XxlJobOptions> configAction = null)
        {
            services.AddSingleton<IJobFactory, DefaultJobFactory>();
            services.AddSingleton<ITaskExecutor, DefaultBeanTaskExecutor>();

            services.AddXxlJob(configuration);
            return services.AddXxlJob((cfg) =>
            {
                if (cfg == null) throw new Exception("请在配置文件配置xxlJob节点");

                if (string.IsNullOrEmpty(cfg.IpAddress))
                {
                    var strIp = Environment.GetEnvironmentVariable("ip");
                    cfg.IpAddress = strIp;
                }

                if (cfg.Port == 0)
                {
                    var strPort = Environment.GetEnvironmentVariable("port");
                    if (!string.IsNullOrEmpty(strPort))
                    {
                        cfg.Port = Convert.ToInt32(strPort);
                    }
                }

                configAction?.Invoke(cfg);
            });
        }

        /// <summary>
        /// 扫描注册所有任务
        /// </summary>
        public static IXxlJobBuilder ScanJobHandler(this IXxlJobBuilder builder, params Assembly[] assemblies)
        {
            if (assemblies == null || assemblies!.Length < 1)
            {
                return builder;
            }

            foreach (Type type in assemblies.SelectMany(s => s.GetTypes().Where(t =>
            {
                return t.IsPublic && t.IsClass && !t.IsAbstract && t.GetInterfaces().Contains(typeof(IJobBaseHandler));
            })))
            {
                JobHandlerAttribute customAttribute = type.GetCustomAttribute<JobHandlerAttribute>();
                string jobName = customAttribute == null ? type.Name : customAttribute.Name;
                builder.Services.Configure(delegate (JobOptions options)
                {
                    options.AddJob(jobName, type);
                }).TryAddScoped(type);
            }

            return builder;
        }

        /// <summary>
        /// 映射路由，默认：xxl-job
        /// </summary>
        public static IEndpointConventionBuilder MapXxlJob(this IEndpointRouteBuilder endpoints)
        {
            return endpoints.MapXxlJob(endpoints.ServiceProvider.GetRequiredService<IOptions<XxlJobOptions>>().Value.BasePath);
        }
    }
}
