var builder = DistributedApplication.CreateBuilder(args);

var api = builder.AddProject<Projects.CmsBackend>("cmsbackend");

builder.AddProject<Projects.CmsBackend_Web>("cmsweb")
       .WithReference(api)
       .WaitFor(api);

builder.Build().Run();
