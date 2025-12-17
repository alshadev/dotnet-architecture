global using System;
global using System.Collections.Generic;
global using System.Linq;
global using System.Linq.Expressions;
global using System.Text.Json;
global using System.Threading;
global using System.Threading.Tasks;

global using Microsoft.EntityFrameworkCore;
global using Microsoft.EntityFrameworkCore.Metadata.Builders;
global using Microsoft.EntityFrameworkCore.Design;
global using Microsoft.EntityFrameworkCore.Diagnostics;
global using Microsoft.EntityFrameworkCore.Storage;
global using Microsoft.Extensions.Caching.Distributed;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Configuration;
global using Microsoft.Extensions.Logging;
global using Microsoft.Extensions.Logging.Abstractions;
global using Microsoft.Extensions.Options;
global using Microsoft.Extensions.Hosting;
global using Microsoft.AspNetCore.Identity;
global using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
global using Microsoft.AspNetCore.Http;

global using System.Security.Claims;

global using MediatR;

global using Arusha.Template.Domain.Abstractions;
global using Arusha.Template.Domain.Primitives;
global using Arusha.Template.Domain.Orders;
global using Arusha.Template.Domain.Products;

global using Arusha.Template.Application.Abstractions.Messaging;
global using Arusha.Template.Application.Abstractions.Persistence;
global using Arusha.Template.Application.Abstractions.Security;
global using Arusha.Template.Application.Abstractions.Caching;
global using Arusha.Template.Application.Results;
global using Arusha.Template.Application.Specifications;

global using Arusha.Template.Infrastructure.Persistence;
global using Arusha.Template.Infrastructure.Persistence.Repositories;
global using Arusha.Template.Infrastructure.Outbox;
global using Arusha.Template.Infrastructure.Idempotency;
global using Arusha.Template.Infrastructure.EventBus;
global using Arusha.Template.Infrastructure.Caching;
global using Arusha.Template.Infrastructure.Security;

