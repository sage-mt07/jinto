using KsqlDsl.Messaging.Managers;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KsqlDsl.Extensions;

internal class HeartbeatEnabledKafkaContext : KafkaContext
{
    private readonly HeartbeatManager _heartbeatManager;

    protected HeartbeatEnabledKafkaContext(ILoggerFactory? loggerFactory = null) : base()
    {
        _heartbeatManager = new HeartbeatManager(
            Microsoft.Extensions.Options.Options.Create(new Configuration.KsqlDslOptions()),
            loggerFactory);

        InitializeHeartbeats();
    }

    protected HeartbeatEnabledKafkaContext(
        Core.Context.KafkaContextOptions options,
        ILoggerFactory? loggerFactory = null) : base(options)
    {
        _heartbeatManager = new HeartbeatManager(
            Microsoft.Extensions.Options.Options.Create(new Configuration.KsqlDslOptions()),
            loggerFactory);

        InitializeHeartbeats();
    }

    private void InitializeHeartbeats()
    {
        var entityModels = GetEntityModels();
        foreach (var entityModel in entityModels.Values)
        {
            _heartbeatManager.RegisterHeartbeatForEntityModel(entityModel);
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _heartbeatManager?.Dispose();
        }

        base.Dispose(disposing);
    }

    protected override async System.Threading.Tasks.ValueTask DisposeAsyncCore()
    {
        _heartbeatManager?.Dispose();
        await base.DisposeAsyncCore();
    }
}
