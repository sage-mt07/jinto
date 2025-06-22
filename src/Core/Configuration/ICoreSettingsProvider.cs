using System;

namespace KsqlDsl.Core.Configuration;
internal interface ICoreSettingsProvider
{
    CoreSettings GetSettings();
    void UpdateSettings(CoreSettings settings);
    event EventHandler<CoreSettingsChangedEventArgs>? SettingsChanged;
}
