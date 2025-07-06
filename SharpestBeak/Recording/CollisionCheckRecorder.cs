#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
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

    public static JsonSerializer CreateSerializer()
    {
        var serializerSettings = new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            TypeNameHandling = TypeNameHandling.Auto,
            SerializationBinder = CustomSerializationBinder.Instance
        };

        return JsonSerializer.Create(serializerSettings);
    }

    [Conditional("DEBUG")]
    public static void ResetCollisionChecks(bool runGarbageCollection = false)
    {
        if (!IsCollisionCheckRecordingEnabled())
        {
            return;
        }

        lock (CollisionChecks)
        {
            CollisionChecks.Clear();
        }

        if (runGarbageCollection)
        {
            PerformGarbageCollection();
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
            CollisionChecks.Add(new CollisionCheckData(CollisionChecks.Count, firstPrimitive, secondPrimitive, result));
        }
    }

    [Conditional("DEBUG")]
    public static void DumpCollisionChecks(bool reset = false)
    {
        if (!IsCollisionCheckRecordingEnabled())
        {
            return;
        }

        const int ChunkSize = 1_000_000;

        lock (CollisionChecks)
        {
            var collisionCheckCount = CollisionChecks.Count;
            if (collisionCheckCount == 0)
            {
                return;
            }

            var collisionChecksZipFilePath = Path.ChangeExtension(Factotum.GetExecutableLocalPath(), $".CollisionChecks.{collisionCheckCount:D}.zip");
            var collisionChecksBaseFileName = Path.GetFileNameWithoutExtension(collisionChecksZipFilePath);

            using var zipArchive = ZipFile.Open(collisionChecksZipFilePath, ZipArchiveMode.Create, Encoding.UTF8);

            var collisionCheckDataChunks = CollisionChecks.Chunk(ChunkSize).ToArray();

            collisionCheckDataChunks.DoForEach(
                (datas, chunkIndex) =>
                {
                    var collisionChecksFileName = collisionCheckDataChunks.Length == 1
                        ? $"{collisionChecksBaseFileName}.json"
                        : $"{collisionChecksBaseFileName}.{chunkIndex + 1:000000}.json";

                    var zipEntry = zipArchive.CreateEntry(collisionChecksFileName, CompressionLevel.Optimal);

                    using var zipEntryStream = zipEntry.Open();
                    using var streamWriter = new StreamWriter(zipEntryStream, Encoding.UTF8);

                    var serializer = CreateSerializer();
                    serializer.Serialize(streamWriter, datas);
                });

            if (reset)
            {
                CollisionChecks.Clear();
            }
        }
    }

    public static string GetSerializedName(BasePrimitive primitive)
    {
        if (primitive is null)
        {
            throw new ArgumentNullException(nameof(primitive));
        }

        CustomSerializationBinder.Instance.BindToName(primitive.GetType(), out _, out var typeName);
        return typeName.EnsureNotNull();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsCollisionCheckRecordingEnabled() => SettingsCache.Instance.EnableCollisionCheckRecording;

    private static void PerformGarbageCollection()
    {
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
    }

    private sealed class CustomSerializationBinder : ISerializationBinder
    {
        public static readonly CustomSerializationBinder Instance = new();

        private static readonly ImmutableDictionary<Type, string> TypeToTypeNameMap =
            new[]
                {
                    typeof(CirclePrimitive),
                    typeof(ConvexPolygonPrimitive),
                    typeof(LinePrimitive)
                }
                .ToImmutableDictionary(type => type, type => type.Name);

        private static readonly ImmutableDictionary<string, Type> TypeNameToTypeMap = TypeToTypeNameMap.ToImmutableDictionary(
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