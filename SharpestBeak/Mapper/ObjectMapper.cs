using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace SharpestBeak.Mapper
{
    internal sealed class ObjectMapper
    {
        #region Constants and Fields

        private static readonly ObjectMapper InstanceField = new ObjectMapper();

        private readonly object _syncLock = new object();
        private readonly Dictionary<MappingKey, MappingInfo> _typeMappings;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="ObjectMapper"/> class.
        /// </summary>
        private ObjectMapper()
        {
            _typeMappings = new Dictionary<MappingKey, MappingInfo>();
        }

        #endregion

        #region Public Properties

        public ObjectMapper Instance
        {
            [DebuggerStepThrough]
            get
            {
                return InstanceField;
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
                            CultureInfo.InvariantCulture,
                            "The source type '{0}' does not implement the interface '{1}'.",
                            sourceType.FullName,
                            baseType.FullName));
                }

                if (baseType.IsAssignableFrom(destinationType))
                {
                    throw new ArgumentException(
                        string.Format(
                            CultureInfo.InvariantCulture,
                            "The destination type '{0}' does not implement the interface '{1}'.",
                            destinationType.FullName,
                            baseType.FullName));
                }
            }
            else
            {
                // TODO: [VM] Register: Validate non-interface types
                throw new NotImplementedException();
            }

            #endregion

            var key = MappingKey.Create<TSource, TDestination>();
            lock (_syncLock)
            {
                if (_typeMappings.ContainsKey(key))
                {
                    throw new ArgumentException(
                        string.Format(
                            "The mapping from '{0}' to '{1}' already exists.",
                            sourceType.FullName,
                            destinationType.FullName));
                }

                const BindingFlags ConstructorBindingFlags = BindingFlags.Instance
                    | BindingFlags.Public
                    | BindingFlags.NonPublic;

                var destinationConstructor = key.Destination.GetConstructor(
                    ConstructorBindingFlags,
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
                _typeMappings.Add(key, mapping);
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
            lock (_syncLock)
            {
                _typeMappings.Remove(key);
            }

            return this;
        }

        public TDestination Map<TSource, TDestination>(TSource source)
        {
            var result = default(TDestination);
            MapInternal(source, ref result, true);
            return result;
        }

        public TDestination Map<TSource, TDestination>(TSource source, TDestination destination)
        {
            MapInternal(source, ref destination, false);
            return destination;
        }

        #endregion

        #region Private Methods

        private static PropertyInfo FindInterfaceProperty(
            IEnumerable<PropertyInfo> allSourceProperties,
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

        private static KeyValuePair<PropertyInfo, PropertyInfo>[] GetMappedProperties(
            Type sourceType,
            Type destinationType,
            Type baseType)
        {
            const BindingFlags PropertyBindingFlags = BindingFlags.Instance
                | BindingFlags.Public
                | BindingFlags.NonPublic;

            var allSourceProperties = sourceType.GetProperties(PropertyBindingFlags);
            var allDestinationProperties = destinationType.GetProperties(PropertyBindingFlags);
            var allBaseProperties = baseType.GetProperties(PropertyBindingFlags);

            var resultList = new List<KeyValuePair<PropertyInfo, PropertyInfo>>(allBaseProperties.Length);
            if (baseType.IsInterface)
            {
                var sourceInterfaceMapping = sourceType.GetInterfaceMap(baseType);
                var destinationInterfaceMapping = destinationType.GetInterfaceMap(baseType);

                foreach (var allBaseProperty in allBaseProperties)
                {
                    var indexParameters = allBaseProperty.GetIndexParameters().EnsureNotNull();
                    if (indexParameters.Length != 0)
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

                    resultList.Add(new KeyValuePair<PropertyInfo, PropertyInfo>(sourceProperty, destinationProperty));
                }
            }
            else
            {
                // TODO: [VM] GetMappedProperties: Implement for non-interface types
                throw new NotImplementedException();
            }

            return resultList.ToArray();
        }

        private void MapInternal<TSource, TDestination>(
            TSource source,
            ref TDestination destination,
            bool createDestination)
        {
            #region Argument Check

            if (!createDestination)
            {
                if (ReferenceEquals(source, null))
                {
                    throw new ArgumentNullException("source");
                }

                if (ReferenceEquals(destination, null))
                {
                    throw new ArgumentNullException("destination");
                }
            }

            #endregion

            if (ReferenceEquals(source, null))
            {
                destination = default(TDestination);
                return;
            }

            var key = MappingKey.Create<TSource, TDestination>();

            MappingInfo mapping;
            lock (_syncLock)
            {
                mapping = _typeMappings.GetValueOrDefault(key);
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
                destination = (TDestination)mapping.DestinationConstructor.Invoke(null);
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

        #region Nested Types

        #region MappingKey Structure

        private struct MappingKey : IEquatable<MappingKey>
        {
            #region Fields

            private readonly Type _source;
            private readonly Type _destination;

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

                _source = source;
                _destination = destination;
            }

            #endregion

            #region Public Properties

            public Type Source
            {
                [DebuggerStepThrough]
                get { return _source; }
            }

            public Type Destination
            {
                [DebuggerStepThrough]
                get { return _destination; }
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
                return _source.GetHashCode() ^ _destination.GetHashCode();
            }

            public override string ToString()
            {
                return string.Format(
                    "{0}. {1} -> {2}",
                    GetType().Name,
                    _source.Name,
                    _destination.Name);
            }

            #endregion

            #region IEquatable<MappingKey> Members

            public bool Equals(MappingKey other)
            {
                return _source == other._source && _destination == other._destination;
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
    }
}