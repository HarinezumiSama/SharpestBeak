using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace SharpestBeak.Mapper
{
    internal sealed class ObjectMapper
    {
        #region Nested Types

        #region MappingKey Structure

        private struct MappingKey : IEquatable<MappingKey>
        {
            #region Fields

            private readonly Type m_source;
            private readonly Type m_destination;

            #endregion

            #region Constructors

            private MappingKey(Type source, Type destination)
            {
                #region Argument Check

                if (source == null)
                {
                    throw new ArgumentNullException("source");
                }
                if (destination == null)
                {
                    throw new ArgumentNullException("destination");
                }

                #endregion

                m_source = source;
                m_destination = destination;
            }

            #endregion

            #region Public Properties

            public Type Source
            {
                [DebuggerStepThrough]
                get { return m_source; }
            }

            public Type Destination
            {
                [DebuggerStepThrough]
                get { return m_destination; }
            }

            #endregion

            #region Public Methods

            public static MappingKey Create<TSource, TDestination>()
            {
                return new MappingKey(typeof(TSource), typeof(TDestination));
            }

            public override bool Equals(object obj)
            {
                return obj is MappingKey && Equals((MappingKey)obj);
            }

            public override int GetHashCode()
            {
                return m_source.GetHashCode() ^ m_destination.GetHashCode();
            }

            public override string ToString()
            {
                return string.Format(
                    "{0}. {1} -> {2}",
                    GetType().Name,
                    m_source.Name,
                    m_destination.Name);
            }

            #endregion

            #region IEquatable<MappingKey> Members

            public bool Equals(MappingKey other)
            {
                return m_source == other.m_source && m_destination == other.m_destination;
            }

            #endregion
        }

        #endregion

        #region MappingInfo Class

        private sealed class MappingInfo
        {
            #region Constructors

            internal MappingInfo(
                Type baseType,
                ConstructorInfo destinationConstructor,
                LambdaExpression validateExpression)
            {
                #region Argument Check

                if (baseType == null)
                {
                    throw new ArgumentNullException("baseType");
                }
                if (destinationConstructor == null)
                {
                    throw new ArgumentNullException("destinationConstructor");
                }

                #endregion

                this.BaseType = baseType;
                this.DestinationConstructor = destinationConstructor;
                this.ValidateExpression = validateExpression;
            }

            #endregion

            #region Public Properties

            public Type BaseType
            {
                get;
                private set;
            }

            public ConstructorInfo DestinationConstructor
            {
                get;
                private set;
            }

            public LambdaExpression ValidateExpression
            {
                get;
                private set;
            }

            #endregion
        }

        #endregion

        #endregion

        #region Fields

        private static readonly ObjectMapper s_instance = new ObjectMapper();

        private readonly object m_syncLock = new object();
        private readonly Dictionary<MappingKey, MappingInfo> m_typeMappings;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="ObjectMapper"/> class.
        /// </summary>
        private ObjectMapper()
        {
            m_typeMappings = new Dictionary<MappingKey, MappingInfo>();
        }

        #endregion

        #region Private Methods

        private static PropertyInfo FindInterfaceProperty(
            PropertyInfo[] allSourceProperties,
            MethodInfo basePropertyGetter,
            ref InterfaceMapping sourceInterfaceMapping)
        {
            var sourceIndex = Array.IndexOf(sourceInterfaceMapping.InterfaceMethods, basePropertyGetter);
            if (sourceIndex < 0)
            {
                return null;
            }

            var sourcePropertyGetter = sourceInterfaceMapping.TargetMethods[sourceIndex];
            var sourceProperty = allSourceProperties.SingleOrDefault(
                item => item.GetGetMethod(true) == sourcePropertyGetter);
            return sourceProperty;
        }

        private List<KeyValuePair<PropertyInfo, PropertyInfo>> GetMappedProperties(
            Type sourceType,
            Type destinationType,
            Type baseType)
        {
            const BindingFlags propertyBindingFlags = BindingFlags.Instance
                | BindingFlags.Public
                | BindingFlags.NonPublic;

            var allSourceProperties = sourceType.GetProperties(propertyBindingFlags);
            var allDestinationProperties = destinationType.GetProperties(propertyBindingFlags);
            var allBaseProperties = baseType.GetProperties(propertyBindingFlags);

            var result = new List<KeyValuePair<PropertyInfo, PropertyInfo>>(allBaseProperties.Length);
            if (baseType.IsInterface)
            {
                var sourceInterfaceMapping = sourceType.GetInterfaceMap(baseType);
                var destinationInterfaceMapping = destinationType.GetInterfaceMap(baseType);

                foreach (var allBaseProperty in allBaseProperties)
                {
                    var indexParameters = allBaseProperty.GetIndexParameters();
                    if (indexParameters != null && indexParameters.Length != 0)
                    {
                        continue;
                    }

                    var basePropertyGetter = allBaseProperty.GetGetMethod(true);

                    var sourceProperty = FindInterfaceProperty(
                        allSourceProperties,
                        basePropertyGetter,
                        ref sourceInterfaceMapping);
                    if (sourceProperty == null)
                    {
                        continue;
                    }

                    var destinationProperty = FindInterfaceProperty(
                        allDestinationProperties,
                        basePropertyGetter,
                        ref destinationInterfaceMapping);
                    if (destinationProperty == null)
                    {
                        continue;
                    }

                    result.Add(new KeyValuePair<PropertyInfo, PropertyInfo>(sourceProperty, destinationProperty));
                }
            }
            else
            {
                //TODO: [VM] GetMappedProperties: Implement for non-interface types
                throw new NotImplementedException();
            }

            return result;
        }

        private void MapInternal<TSource, TDestination>(
            TSource source,
            ref TDestination destination,
            bool createDestination)
        {
            #region Argument Check

            if (!createDestination)
            {
                if (source == null)
                {
                    throw new ArgumentNullException("source");
                }
                if (destination == null)
                {
                    throw new ArgumentNullException("destination");
                }
            }

            #endregion

            if (source == null)
            {
                destination = default(TDestination);
                return;
            }

            var key = MappingKey.Create<TSource, TDestination>();

            MappingInfo mapping;
            lock (m_syncLock)
            {
                mapping = m_typeMappings.GetValueOrDefault(key);
            }

            if (mapping == null)
            {
                throw new ArgumentException(
                    string.Format(
                        "There is no registered mapping from '{0}' to '{1}'.",
                        key.Source.FullName,
                        key.Destination.FullName));
            }

            if (createDestination)
            {
                // TODO: [VM] Check if constructor can be omitted for structures
                destination = (TDestination)mapping.DestinationConstructor.Invoke((object[])null);
            }

            var validate = (Expression<Func<TSource, string>>)mapping.ValidateExpression;
            if (validate != null)
            {
                var validationResult = validate.Compile().Invoke(source);
                if (!string.IsNullOrWhiteSpace(validationResult))
                {
                    throw new ArgumentException(validationResult, "source");
                }
            }

            var propertyInfoPairs = GetMappedProperties(key.Source, key.Destination, mapping.BaseType);
            foreach (var propertyInfoPair in propertyInfoPairs)
            {
                var propertyValue = propertyInfoPair.Key.GetValue(source, null);
                propertyInfoPair.Value.SetValue(destination, propertyValue, null);
            }
        }

        #endregion

        #region Public Methods

        public ObjectMapper Register<TSource, TDestination, TBase>(Expression<Func<TSource, string>> validate = null)
        {
            var sourceType = typeof(TSource);
            var destinationType = typeof(TDestination);
            var baseType = typeof(TBase);

            #region Argument Check

            if (baseType.IsInterface)
            {
                if (baseType.IsAssignableFrom(sourceType))
                {
                    throw new ArgumentException(
                        string.Format(
                            "The source type '{0}' does not implement the interface '{1}'.",
                            sourceType.FullName,
                            baseType.FullName),
                        "TSource");
                }
                if (baseType.IsAssignableFrom(destinationType))
                {
                    throw new ArgumentException(
                        string.Format(
                            "The destination type '{0}' does not implement the interface '{1}'.",
                            destinationType.FullName,
                            baseType.FullName),
                        "TDestination");
                }
            }
            else
            {
                //TODO: [VM] Register: Validate non-interface types
                throw new NotImplementedException();
            }

            #endregion

            var key = MappingKey.Create<TSource, TDestination>();
            lock (m_syncLock)
            {
                if (m_typeMappings.ContainsKey(key))
                {
                    throw new ArgumentException(
                        string.Format(
                            "The mapping from '{0}' to '{1}' already exists.",
                            sourceType.FullName,
                            destinationType.FullName));
                }

                const BindingFlags constructorBindingFlags = BindingFlags.Instance
                    | BindingFlags.Public
                    | BindingFlags.NonPublic;

                var destinationConstructor = key.Destination.GetConstructor(
                    constructorBindingFlags,
                    Type.DefaultBinder,
                    Type.EmptyTypes,
                    null);
                if (destinationConstructor == null)
                {
                    throw new ArgumentException(
                        string.Format(
                            "The destination type '{0}' has no parameterless constructor.",
                            destinationType.FullName));
                }

                var mapping = new MappingInfo(baseType, destinationConstructor, validate);
                m_typeMappings.Add(key, mapping);
            }

            return this;
        }

        public ObjectMapper Register<TSource, TDestination>(Expression<Func<TSource, string>> validate = null)
        {
            return Register<TSource, TDestination, TDestination>(validate);
        }

        public ObjectMapper Unregister<TSource, TDestination>()
        {
            var key = MappingKey.Create<TSource, TDestination>();
            lock (m_syncLock)
            {
                m_typeMappings.Remove(key);
            }

            return this;
        }

        public TDestination Map<TSource, TDestination>(TSource source)
        {
            TDestination result = default(TDestination);
            MapInternal<TSource, TDestination>(source, ref result, true);
            return result;
        }

        public TDestination Map<TSource, TDestination>(TSource source, TDestination destination)
        {
            MapInternal<TSource, TDestination>(source, ref destination, false);
            return destination;
        }

        #endregion
    }
}