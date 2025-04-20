using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using SharpestBeak.Model;
using SharpestBeak.UI.Properties;

namespace SharpestBeak.UI;

internal sealed class LogicManager
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="LogicManager"/> class.
    /// </summary>
    private LogicManager()
    {
        var logicQualifiedTypes = Settings.Default.Logics.Cast<string>().ToList();

        var logicInfos = new HashSet<LogicInfo>();
        var errors = new StringBuilder();
        foreach (var logicQualifiedType in logicQualifiedTypes)
        {
            LogicInfo logicInfo;
            try
            {
                var type = Type.GetType(logicQualifiedType, true, false);
                if (!typeof(ChickenUnitLogic).IsAssignableFrom(type) || type.IsAbstract
                    || type.IsGenericTypeDefinition)
                {
                    throw new ApplicationException(
                        $"Type '{type.AssemblyQualifiedName}' must inherit from '{
                            typeof(ChickenUnitLogic).FullName}' and must be non-abstract and non-generic.");
                }

                logicInfo = new LogicInfo(type);
            }
            catch (Exception ex)
            {
                errors.AppendLine("*** " + ex.Message);
                continue;
            }

            logicInfos.Add(logicInfo);
        }

        Errors = errors.ToString();

        Logics = logicInfos
            .OrderBy(item => item.Caption)
            .ThenBy(item => item.Type.AssemblyQualifiedName)
            .ToList()
            .AsReadOnly();
    }

    [DebuggerNonUserCode]
    public static LogicManager Instance { get; } = new();

    public string Errors { get; private set; }

    public ReadOnlyCollection<LogicInfo> Logics { get; private set; }
}