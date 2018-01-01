using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.IO;
using System.Linq;
using System.Globalization;

#if !SILVERLIGHT
using System.Runtime.Serialization.Formatters.Binary;
#endif

namespace Universal.Net.Utils
{
    /// <summary>
    /// 程序集相关工具类
    /// </summary>
    public static class AssemblyUtil
    {
        /// <summary>
        /// 根据一个字符串的type名称来创建一个对象
        /// </summary>
        /// <typeparam name="T">创建的对象类型</typeparam>
        /// <param name="type">对象类型字符串名称</param>
        /// <returns>创建的对象</returns>
        public static T CreateInstance<T>(string type)
        {
            return CreateInstance<T>(type, new object[0]);
        }

        /// <summary>
        /// 根据一个字符串的type名称和传入的构造函数参数来创建一个对象
        /// </summary>
        /// <typeparam name="T">创建的对象类型</typeparam>
        /// <param name="type">对象类型字符串名称</param>
        /// <param name="parameters">构造函数参数</param>
        /// <returns>创建的对象</returns>
        public static T CreateInstance<T>(string type, object[] parameters)
        {
            Type instanceType = null;
            var result = default(T);
            //根据类型名称获取到Type
            instanceType = Type.GetType(type, true);

            if (instanceType == null)
                throw new Exception(string.Format("类型 '{0}' 没有找到!", type));
            //创建对象
            object instance = Activator.CreateInstance(instanceType, parameters);
            //强转对象类型
            result = (T)instance;
            return result;
        }

        /// <summary>
        /// Gets the type by the full name, also return matched generic type without checking generic type parameters in the name.
        /// </summary>
        /// <param name="fullTypeName">Full name of the type.</param>
        /// <param name="throwOnError">if set to <c>true</c> [throw on error].</param>
        /// <param name="ignoreCase">if set to <c>true</c> [ignore case].</param>
        /// <returns></returns>
#if !NET35
        public static Type GetType(string fullTypeName, bool throwOnError, bool ignoreCase)
        {
            var targetType = Type.GetType(fullTypeName, false, ignoreCase);

            if (targetType != null)
                return targetType;

            var names = fullTypeName.Split(',');
            var assemblyName = names[1].Trim();

            try
            {
                var assembly = Assembly.Load(assemblyName);

                var typeNamePrefix = names[0].Trim() + "`";

                var matchedTypes = assembly.GetExportedTypes().Where(t => t.IsGenericType
                        && t.FullName.StartsWith(typeNamePrefix, ignoreCase, CultureInfo.InvariantCulture)).ToArray();

                if (matchedTypes.Length != 1)
                    return null;

                return matchedTypes[0];
            }
            catch (Exception e)
            {
                if (throwOnError)
                    throw e;

                return null;
            }
        }
#else
        public static Type GetType(string fullTypeName, bool throwOnError, bool ignoreCase)
        {
            return Type.GetType(fullTypeName, null, (a, n, ign) =>
                {
                    var targetType = a.GetType(n, false, ign);

                    if (targetType != null)
                        return targetType;

                    var typeNamePrefix = n + "`";

                    var matchedTypes = a.GetExportedTypes().Where(t => t.IsGenericType
                            && t.FullName.StartsWith(typeNamePrefix, ign, CultureInfo.InvariantCulture)).ToArray();

                    if (matchedTypes.Length != 1)
                        return null;

                    return matchedTypes[0];
                }, throwOnError, ignoreCase);
        }
#endif

        /// <summary>
        /// 从assemblyy中获取TBaseType的所有的public的非抽象子类类型
        /// </summary>
        /// <typeparam name="TBaseType">父类类型</typeparam>
        /// <param name="assembly">程序集对象</param>
        /// <returns>public的非抽象子类类型集合</returns>
        public static IEnumerable<Type> GetImplementTypes<TBaseType>(this Assembly assembly)
        {
            return assembly.GetExportedTypes().Where(t =>
                t.IsSubclassOf(typeof(TBaseType)) && t.IsClass && !t.IsAbstract);
        }

        /// <summary>
        /// 从assemblyy中获取TBaseInterface的所有的public的接口实现类类型
        /// </summary>
        /// <typeparam name="TBaseInterface">接口</typeparam>
        /// <param name="assembly">程序集</param>
        /// <returns>接口实现类的实例对象集合</returns>
        public static IEnumerable<TBaseInterface> GetImplementedObjectsByInterface<TBaseInterface>(this Assembly assembly)
            where TBaseInterface : class
        {
            return GetImplementedObjectsByInterface<TBaseInterface>(assembly, typeof(TBaseInterface));
        }

        /// <summary>
        /// 从assemblyy中获取TBaseInterface的所有的public的接口实现类类型
        /// </summary>
        /// <typeparam name="TBaseInterface">最基础的接口</typeparam>
        /// <param name="assembly">程序集</param>
        /// <param name="targetType">目标类型</param>
        /// <returns>接口实现类的实例对象集合</returns>
        public static IEnumerable<TBaseInterface> GetImplementedObjectsByInterface<TBaseInterface>(this Assembly assembly, Type targetType)
            where TBaseInterface : class
        {
            //获取程序集中所有公共类型
            Type[] arrType = assembly.GetExportedTypes();

            var result = new List<TBaseInterface>();
            //遍历所有公共类型
            for (int i = 0; i < arrType.Length; i++)
            {
                var currentImplementType = arrType[i];

                if (currentImplementType.IsAbstract)
                    continue;
                //如果currentImplementType不在targetType的继承关系上
                if (!targetType.IsAssignableFrom(currentImplementType))
                    continue;
                //创建currentImplementType的类型实例
                result.Add((TBaseInterface)Activator.CreateInstance(currentImplementType));
            }

            return result;
        }

#if SILVERLIGHT
#else
        /// <summary>
        /// 二进制复制对象
        /// </summary>
        /// <typeparam name="T">数据类型</typeparam>
        /// <param name="target">需要被复制的对象</param>
        /// <returns>复制得到的对象</returns>
        public static T BinaryClone<T>(this T target)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            using (MemoryStream ms = new MemoryStream())
            {
                formatter.Serialize(ms, target);
                ms.Position = 0;
                return (T)formatter.Deserialize(ms);
            }
        }
#endif


        /// <summary>
        /// 复制一个对象的属性值并给另一个对象赋值
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="source">数据源对象</param>
        /// <param name="target">目标对象</param>
        /// <returns>目标对象</returns>
        public static T CopyPropertiesTo<T>(this T source, T target)
        {
            return source.CopyPropertiesTo(p => true, target);
        }

        /// <summary>
        /// 复制一个对象的属性值并给另一个对象赋值
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="source">数据源对象</param>
        /// <param name="predict">对目标对象的属性进行筛选的函数</param>
        /// <param name="target">目标对象</param>
        /// <returns>目标对象</returns>
        public static T CopyPropertiesTo<T>(this T source, Predicate<PropertyInfo> predict, T target)
        {
            //找到源上的所有公共实例属性
            PropertyInfo[] properties = source.GetType()
                .GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty);

            Dictionary<string, PropertyInfo> sourcePropertiesDict = properties.ToDictionary(p => p.Name);
           
            //找到目标上的所有公共实例可写属性，同时进行了一个筛选
            PropertyInfo[] targetProperties = target.GetType()
                .GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.SetProperty)
                .Where(p => predict(p)).ToArray();

            //遍历所有目标对象上获取的属性
            for (int i = 0; i < targetProperties.Length; i++)
            {
                var p = targetProperties[i];
                PropertyInfo sourceProperty;

                //进行赋值操作，要求属性是可序列化的
                if (sourcePropertiesDict.TryGetValue(p.Name, out sourceProperty))
                {
                    if (sourceProperty.PropertyType != p.PropertyType)
                        continue;

                    if (!sourceProperty.PropertyType.IsSerializable)
                        continue;
                    //属性赋值
                    p.SetValue(target, sourceProperty.GetValue(source, null), null);
                }
            }

            return target;
        }


        /// <summary>
        /// 从字符串得到一组Assembly对象
        /// </summary>
        /// <param name="assemblyDef">assembly def.字符串</param>
        /// <returns>一组Assembly对象</returns>
        public static IEnumerable<Assembly> GetAssembliesFromString(string assemblyDef)
        {
            return GetAssembliesFromStrings(assemblyDef.Split(new char[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries));
        }

        /// <summary>
        /// 从字符串得到一组Assembly对象
        /// </summary>
        /// <param name="assemblies">一组Assembly名称.</param>
        /// <returns>一组Assembly对象</returns>
        public static IEnumerable<Assembly> GetAssembliesFromStrings(string[] assemblies)
        {
            List<Assembly> result = new List<Assembly>(assemblies.Length);

            foreach (var a in assemblies)
            {
                result.Add(Assembly.Load(a));
            }

            return result;
        }
    }
}
