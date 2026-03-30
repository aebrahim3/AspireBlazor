var builder = DistributedApplication.CreateBuilder(args);

//Replaces sqlite with SQL Server, and adds a volume for data persistence.
var sql = builder.AddSqlServer("sqlserver")
                 .WithDataVolume("cms-sql-data")
                 .AddDatabase("cms-db");


var api = builder.AddProject<Projects.AspireBlazor>("AspireBlazor")
                 .WithReference(sql)
                 .WaitFor(sql);

// The web project references the API project, so it can call API endpoints directly without needing to know about the database project.
builder.AddProject<Projects.AspireBlazor_Web>("cmsweb")
       .WithReference(api)
       .WaitFor(api);

builder.Build().Run();
