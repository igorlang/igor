using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Igor.Mapper
{
    public abstract class MapperBase
    {
        private readonly ConditionalWeakTable<object, object> cache = new ConditionalWeakTable<object, object>();

        private abstract class MapConfig
        {
            public abstract object Map(object source);
        }

        private class MapConfig<TSource, TDest> : MapConfig
        {
            public Func<TSource, TDest> Mapper { get; }

            public override object Map(object source) => Mapper((TSource)source);

            public MapConfig(Func<TSource, TDest> mapper)
            {
                this.Mapper = mapper;
            }
        }

        private readonly Dictionary<Type, MapConfig> Types = new Dictionary<Type, MapConfig>();

        protected Func<TSource, TDest> MapToValue<TSource, TDest>(TDest val) => _ => val;

        protected void Register<TSource, TDest>(Func<TSource, TDest> mapper)
        {
            var sourceType = typeof(TSource);
            if (Types.ContainsKey(sourceType))
                throw new InvalidOperationException($"Type {sourceType.FullName} already registered");
            Types.Add(sourceType, new MapConfig<TSource, TDest>(mapper));
        }

        protected TDest CachedOrProject<TSource, TDest>(TSource source)
        {
            return CachedOrProject<TDest>((object) source);
        }

        protected TDest CachedOrProject<TDest>(object source)
        {
            if (source == null)
            {
                return default(TDest);
            }
            else
            {
                if (cache.TryGetValue(source, out var result))
                {
                    try
                    {
                        return (TDest)result;
                    }
                    catch
                    {
                        throw new InvalidCastException($"Failed to cast {source.GetType()} to {typeof(TDest)}");
                    }
                }
                else
                {
                    if (Types.TryGetValue(source.GetType(), out var mapConfig))
                        return (TDest)mapConfig.Map(source);
                    else
                        throw new InvalidCastException($"No mapper configured for type {source.GetType().FullName}");
                }
            }
        }

        protected void Cache(object source, object dest)
        {
            cache.Add(source, dest);
        }

        protected void RegisterAuto<TSource, TDest>()
        {
            var destType = typeof(TDest);
            var sourceType = typeof(TSource);

            Expression thisExpr = Expression.Constant(this);
            var sourceExpr = Expression.Parameter(sourceType);
            var destExpr = Expression.Variable(destType);

            Expression Project(string name, Type destTypeInfo) => Projection(sourceType, name, destTypeInfo, sourceExpr, thisExpr);

            List<Expression> bodyExpressions = new List<Expression>();

            var ctor = destType.GetConstructors(BindingFlags.Public | BindingFlags.Instance).FirstOrDefault();
            if (ctor == null)
                throw new Exception($"Type {destType.FullName} does not have a public constructor.");
            var destProps = destType.GetProperties().Where(p => !p.IsDefined(typeof(ObsoleteAttribute))).ToDictionary(p => p.Name.ToLowerInvariant(), p => p.Name);
            string FindProperty(string str)
            {
                if (destProps.TryGetValue(str.ToLowerInvariant(), out var value))
                    return value;
                else
                    throw new InvalidOperationException($"cannot find property behind parameter {str} in {destType}");
            }
            var ctorArgs = ctor.GetParameters().Select(param => Project(FindProperty(param.Name), param.ParameterType));
            bodyExpressions.Add(Expression.Assign(destExpr, Expression.New(ctor, ctorArgs)));

            var cacheMethod = GetType().GetMethod(nameof(Cache), BindingFlags.NonPublic | BindingFlags.Instance);
            bodyExpressions.Add(Expression.Call(Expression.Constant(this), cacheMethod, sourceExpr, destExpr));

            foreach (var destMember in destType.GetProperties(BindingFlags.Instance | BindingFlags.FlattenHierarchy | BindingFlags.Public))
            {
                if (destMember.CanWrite)
                {
                    var assignment = Expression.Assign(Expression.MakeMemberAccess(destExpr, destMember), Project(destMember.Name, destMember.PropertyType));
                    bodyExpressions.Add(assignment);
                }
            }

            bodyExpressions.Add(destExpr);
            var body = Expression.Block(destType, new[] { destExpr }, bodyExpressions);
            var compiled = Expression.Lambda<Func<TSource, TDest>>(body, sourceExpr).Compile();
            Register(compiled);
        }

        protected List<TDestArg> ProjectList<TSourceArg, TDestArg>(ICollection<TSourceArg> source)
        {
            if (source == null)
                return null;
            var result = new List<TDestArg>(source.Count);
            foreach (var item in source)
                result.Add(CachedOrProject<TDestArg>(item));
            return result;
        }

        protected List<TDestArg> ProjectReadOnlyList<TSourceArg, TDestArg>(IReadOnlyCollection<TSourceArg> source)
        {
            if (source == null)
                return null;
            var result = new List<TDestArg>(source.Count);
            foreach (var item in source)
                result.Add(CachedOrProject<TDestArg>(item));
            return result;
        }

        private Expression CachedOrProjectExpression(Expression thisExpr, Expression arg, Type destType)
        {
            return Expression.Call(thisExpr, nameof(CachedOrProject), new[] { arg.Type, destType }, arg);
        }

        private Expression ValueProjection(Expression sourceMemberExpr, Type sourceTypeInfo, Type destTypeInfo, Expression thisExpr)
        {
            if (sourceTypeInfo == destTypeInfo)
            {
                return sourceMemberExpr;
            }
            else if (ReflectionHelper.IsCollection(sourceTypeInfo) && destTypeInfo.IsGenericType && destTypeInfo.FullName.Contains("System.Collections.Generic"))
            {
                var argType = ReflectionHelper.EnumerableItemType(sourceTypeInfo);
                var targetArgType = destTypeInfo.GetGenericArguments()[0];
                return Expression.Call(thisExpr, nameof(ProjectList), new[] { argType, targetArgType }, sourceMemberExpr);
            }
            else if (ReflectionHelper.IsReadOnlyCollection(sourceTypeInfo) && destTypeInfo.IsGenericType && destTypeInfo.FullName.Contains("System.Collections.Generic"))
            {
                var argType = ReflectionHelper.EnumerableItemType(sourceTypeInfo);
                var targetArgType = destTypeInfo.GetGenericArguments()[0];
                return Expression.Call(thisExpr, nameof(ProjectReadOnlyList), new[] { argType, targetArgType }, sourceMemberExpr);
            }
            else
            {
                return CachedOrProjectExpression(thisExpr, sourceMemberExpr, destTypeInfo);
            }
        }

        private Expression Projection(Type sourceType, string name, Type destTypeInfo, Expression source, Expression thisExpr)
        {
            MemberInfo sourceMember = sourceType.GetMember(name, MemberTypes.Property | MemberTypes.Field, BindingFlags.Instance | BindingFlags.FlattenHierarchy | BindingFlags.Public).FirstOrDefault();

            if (sourceMember != null)
            {
                var sourceMemberExpr = Expression.MakeMemberAccess(source, sourceMember);
                var sourceTypeInfo = sourceMember is PropertyInfo sourceProperty ? sourceProperty.PropertyType : ((FieldInfo)sourceMember).FieldType;
                return ValueProjection(sourceMemberExpr, sourceTypeInfo, destTypeInfo, thisExpr);
            }
            else
            {
                var methodName = $"Map_{name}";
                var methodInfo = GetType().GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public).FirstOrDefault(m => m.Name == methodName);
                if (methodInfo != null)
                {
                    return ValueProjection(Expression.Call(thisExpr, methodInfo, source), methodInfo.ReturnType, destTypeInfo, thisExpr);
                }
                else
                {
                    throw new InvalidOperationException($"Property {sourceType}.{name} not found");
                }
            }
        }

        protected void Register<TSource, TDest>(TDest dest)
        {
            Register<TSource, TDest>(_ => dest);
        }

        public TDest Map<TSource, TDest>(TSource source)
        {
            return CachedOrProject<TDest>(source);
        }
    }
}
