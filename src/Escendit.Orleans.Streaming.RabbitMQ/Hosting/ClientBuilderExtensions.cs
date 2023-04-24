﻿// Copyright (c) Escendit Ltd. All Rights Reserved.
// Licensed under the MIT. See LICENSE.txt file in the solution root for full license information.

namespace Orleans.Hosting;

using Configuration;
using Escendit.Orleans.Streaming.RabbitMQ.Options;
using Escendit.Orleans.Streaming.RabbitMQ.Stream;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

/// <summary>
/// Client Builder Extensions.
/// </summary>
public static class ClientBuilderExtensions
{
    /// <summary>
    /// Add Rabbit MQ.
    /// </summary>
    /// <param name="builder">The rabbit mq.</param>
    /// <param name="name">The name.</param>
    /// <returns>The client builder.</returns>
    public static RabbitClientBuilder AddRabbitMqStreaming(
        this IClientBuilder builder,
        string name)
    {
        return new RabbitClientBuilder(builder, name);
    }

    /// <summary>
    /// Add Stream for Rabbit MQ.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <param name="options">The configure options.</param>
    /// <returns>The rabbit client builder.</returns>
    public static RabbitClientBuilder WithStream(
        this RabbitClientBuilder builder,
        Action<RabbitStreamOptions>? options = null)
    {
        return builder
            .WithStream(configure =>
                configure.Configure(options ?? new Action<RabbitStreamOptions>(_ => { })));
    }

    /// <summary>
    /// Add Stream for Rabbit MQ.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <param name="configureOptions">The configure options.</param>
    /// <returns>The rabbit client builder.</returns>
    public static RabbitClientBuilder WithStream(
        this RabbitClientBuilder builder,
        Action<OptionsBuilder<RabbitStreamOptions>>? configureOptions = null)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder
            .ConfigureServices(services =>
            {
                services
                    .TryAddSingleton<DefaultStreamAdapterFactory>();
                configureOptions?
                    .Invoke(services.AddOptions<RabbitStreamOptions>(builder.Name));
                services
                    .ConfigureNamedOptionForLogging<RabbitStreamOptions>(builder.Name);
            });

        _ = new RabbitClusterClientStreamConfigurator(
            builder.Name,
            builder);

        return builder;
    }

    /// <summary>
    /// Add Queue for Rabbit MQ.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <param name="configureOptions">The configure options.</param>
    /// <returns>The rabbit client builder.</returns>
    public static RabbitClientBuilder WithQueue(
        this RabbitClientBuilder builder,
        Action<RabbitQueueOptions> configureOptions)
    {
        builder
            .WithQueue(configure =>
            {
                configure.ConfigureDelegate(services =>
                {
                    services.Configure(configureOptions);
                });
            });

        return builder;
    }

    /// <summary>
    /// Add Rabbit MQ Streams.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <param name="configure">The configure.</param>
    /// <returns>The client builder.</returns>
    public static RabbitClientBuilder WithQueue(
        this RabbitClientBuilder builder,
        Action<RabbitClusterClientStreamConfigurator>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(builder);

        var configurator = new RabbitClusterClientStreamConfigurator(builder.Name, builder);
        configure?.Invoke(configurator);
        return builder;
    }
}
