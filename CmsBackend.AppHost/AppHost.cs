var builder = DistributedApplication.CreateBuilder(args);

var sql = builder.AddSqlServer("sqlserver")
                 .WithDataVolume("cms-sql-data")
                 .AddDatabase("cms-db");

var api = builder.AddProject<Projects.CmsBackend>("cmsbackend")
                 .WithReference(sql)
                 .WaitFor(sql);

builder.AddProject<Projects.CmsBackend_Web>("cmsweb")
       .WithReference(api)
       .WaitFor(api);

builder.Build().Run();
