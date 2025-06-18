#nullable enable

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Omnifactotum;
using SharpestBeak.Presentation;
using SharpestBeak.Presentation.Primitives;

namespace SharpestBeak.Recording;

public static class CollisionCheckRecorder
{
    private static readonly List<CollisionCheckData> CollisionChecks = [];

    public static void ResetCollisionChecks()
    {
        if (!IsCollisionCheckRecordingEnabled())
        {
            return;
        }

        lock (CollisionChecks)
        {
            CollisionChecks.Clear();
        }
    }

    [Conditional("DEBUG")]
    public static void RecordCollisionCheck(ICollidablePrimitive first, ICollidablePrimitive second, bool result)
    {
        if (!IsCollisionCheckRecordingEnabled())
        {
            return;
        }

        if (first is null)
        {
            throw new ArgumentNullException(nameof(first));
        }

        if (second is null)
        {
            throw new ArgumentNullException(nameof(second));
        }

        if (first is not BasePrimitive firstPrimitive)
        {
            throw new ArgumentException($"The object is incompatible with {nameof(BasePrimitive)}.", nameof(first));
        }

        if (second is not BasePrimitive secondPrimitive)
        {
            throw new ArgumentException($"The object is incompatible with {nameof(BasePrimitive)}.", nameof(second));
        }

        lock (CollisionChecks)
        {
            CollisionChecks.Add(new CollisionCheckData(firstPrimitive, secondPrimitive, result));
        }
    }

    [Conditional("DEBUG")]
    public static void DumpCollisionChecks(bool reset = false)
    {
        if (!IsCollisionCheckRecordingEnabled())
        {
            return;
        }

        lock (CollisionChecks)
        {
            var collisionChecksLogFilePath = Path.ChangeExtension(Factotum.GetExecutableLocalPath(), $".CollisionChecks.{CollisionChecks.Count:D}.json");

            var collisionChecksZipFilePath = Path.ChangeExtension(collisionChecksLogFilePath, ".zip");
            using var zipArchive = ZipFile.Open(collisionChecksZipFilePath, ZipArchiveMode.Create, Encoding.UTF8);

            var zipEntry = zipArchive.CreateEntry(Path.GetFileName(collisionChecksLogFilePath), CompressionLevel.Optimal);

            // using var fileStream = File.Open(collisionChecksLogFilePath, FileMode.Create, FileAccess.ReadWrite, FileShare.Read);
            // using var streamWriter = new StreamWriter(fileStream, Encoding.UTF8);
            using var zipEntryStream = zipEntry.Open();
            using var streamWriter = new StreamWriter(zipEntryStream, Encoding.UTF8);

            var serializerSettings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                TypeNameHandling = TypeNameHandling.Auto,
                SerializationBinder = CustomSerializationBinder.Instance
            };

            var serializer = JsonSerializer.Create(serializerSettings);

            serializer.Serialize(streamWriter, CollisionChecks);

            if (reset)
            {
                CollisionChecks.Clear();
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsCollisionCheckRecordingEnabled() => SettingsCache.Instance.EnableCollisionCheckRecording;

    private sealed class CustomSerializationBinder : ISerializationBinder
    {
        public static readonly CustomSerializationBinder Instance = new();

        private static readonly Dictionary<Type, string> TypeToTypeNameMap =
            new[]
                {
                    typeof(CirclePrimitive),
                    typeof(ConvexPolygonPrimitive),
                    typeof(LinePrimitive)
                }
                .ToDictionary(type => type, type => type.Name);

        private static readonly Dictionary<string, Type> TypeNameToTypeMap = TypeToTypeNameMap.ToDictionary(
            pair => pair.Value,
            pair => pair.Key,
            StringComparer.Ordinal);

        private CustomSerializationBinder()
        {
        }

        public Type BindToType(string? assemblyName, string typeName)
        {
            if (typeName is null)
            {
                throw new ArgumentNullException(nameof(typeName));
            }

            Factotum.Assert(assemblyName is null);

            return TypeNameToTypeMap.TryGetValue(typeName, out var type)
                ? type
                : throw new JsonSerializationException($"Unexpected type name {typeName.ToUIString()}.");
        }

        public void BindToName(Type serializedType, out string? assemblyName, out string? typeName)
        {
            if (serializedType is null)
            {
                throw new ArgumentNullException(nameof(serializedType));
            }

            assemblyName = null;

            if (!TypeToTypeNameMap.TryGetValue(serializedType, out typeName))
            {
                throw new JsonSerializationException($"Unexpected type {serializedType.GetFullName().ToUIString()}.");
            }
        }
    }
}