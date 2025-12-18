global using System;
global using System.Collections.Generic;
global using System.Linq;
global using System.Linq.Expressions;
global using System.Threading;
global using System.Threading.Tasks;

global using MediatR;
global using FluentValidation;
global using FluentValidation.Results;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Logging;
global using Microsoft.EntityFrameworkCore;

global using Arusha.Template.Domain.Abstractions;
global using Arusha.Template.Domain.Primitives;
global using Arusha.Template.Domain.Orders;
global using Arusha.Template.Domain.Products;

global using Arusha.Template.Application.Abstractions.Messaging;
global using Arusha.Template.Application.Abstractions.Persistence;
global using Arusha.Template.Application.Abstractions.Security;
global using Arusha.Template.Application.Abstractions.Caching;
global using Arusha.Template.Application.Behaviors;
global using Arusha.Template.Application.Results;
global using Arusha.Template.Application.Specifications;
