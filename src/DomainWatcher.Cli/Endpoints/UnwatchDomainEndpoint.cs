﻿using DomainWatcher.Core.Contracts;
using DomainWatcher.Core.Repositories;
using DomainWatcher.Core.Values;
using DomainWatcher.Infrastructure.HttpServer.Contracts;
using DomainWatcher.Infrastructure.HttpServer.Models;

namespace DomainWatcher.Cli.Endpoints;

public class UnwatchDomainEndpoint(
    IDomainsRepository repository,
    IDomainsQueryQueue queryQueue) : IHttpEndpoint
{
    public static HttpMethod Method => HttpMethod.Delete;

    public static string Path => "/";

    public async Task<HttpResponse> Handle(HttpRequest request)
    {
        var domain = new Domain(request.Body);

        if (!await repository.IsWatched(domain))
        {
            return HttpResponse.BadRequestWithReason($"{domain.FullName} was not watched.");
        }

        await repository.Unwatch(domain);
        queryQueue.TryRemove(domain);

        return HttpResponse.PlainText($"{domain.FullName} unwatched!");
    }
}
