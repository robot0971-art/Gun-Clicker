using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace DI
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class InjectAttribute : Attribute
    {
    }

    public class Container
    {
        private readonly Dictionary<string, object> _bindings = new Dictionary<string, object>();

        public void Bind<T>(T instance, string key = null)
        {
            string bindingKey = GetKey(typeof(T), key);
            _bindings[bindingKey] = instance;
        }

        public void Bind<TInterface, TImplementation>(TImplementation instance, string key = null)
            where TImplementation : TInterface
        {
            string bindingKey = GetKey(typeof(TInterface), key);
            _bindings[bindingKey] = instance;
        }

        public bool TryResolve<T>(out T instance, string key = null)
        {
            string bindingKey = GetKey(typeof(T), key);
            if (_bindings.TryGetValue(bindingKey, out var obj))
            {
                instance = (T)obj;
                return true;
            }
            instance = default;
            return false;
        }

        public bool TryResolve(Type type, out object instance, string key = null)
        {
            string bindingKey = GetKey(type, key);
            return _bindings.TryGetValue(bindingKey, out instance);
        }

        private string GetKey(Type type, string key)
        {
            return string.IsNullOrEmpty(key) ? type.FullName : $"{type.FullName}:{key}";
        }
    }

    public static class DIContainer
    {
        private static readonly List<Container> _containers = new List<Container>();
        private static readonly Dictionary<Type, FieldInfo[]> _fieldCache = new Dictionary<Type, FieldInfo[]>();

        public static int ContainerCount => _containers.Count;

        public static void RegisterContainer(Container container)
        {
            if (container == null)
            {
                Debug.LogError("[DIContainer] Cannot register null container.");
                return;
            }
            _containers.Add(container);
        }

        public static void UnregisterContainer(Container container)
        {
            _containers.Remove(container);
        }

        public static void Register<T>(T instance, string key = null)
        {
            if (_containers.Count == 0)
            {
                Debug.LogError("[DIContainer] No container registered. Create an Installer first.");
                return;
            }
            _containers[_containers.Count - 1].Bind(instance, key);
        }

        public static void Register<TInterface, TImplementation>(TImplementation instance, string key = null)
            where TImplementation : TInterface
        {
            if (_containers.Count == 0)
            {
                Debug.LogError("[DIContainer] No container registered. Create an Installer first.");
                return;
            }
            _containers[_containers.Count - 1].Bind<TInterface, TImplementation>(instance, key);
        }

        public static T Resolve<T>(string key = null)
        {
            for (int i = _containers.Count - 1; i >= 0; i--)
            {
                if (_containers[i].TryResolve<T>(out var instance, key))
                {
                    return instance;
                }
            }
            Debug.LogWarning($"[DIContainer] Could not resolve {typeof(T).FullName}{(string.IsNullOrEmpty(key) ? "" : ":" + key)}");
            return default;
        }

        public static void Inject(GameObject gameObject)
        {
            if (gameObject == null)
            {
                Debug.LogError("[DIContainer] Cannot inject into null GameObject.");
                return;
            }

            var monoBehaviours = gameObject.GetComponents<MonoBehaviour>();
            foreach (var mb in monoBehaviours)
            {
                if (mb == null) continue;
                InjectIntoObject(mb);
            }
        }

        private static void InjectIntoObject(object obj)
        {
            var type = obj.GetType();
            
            if (!_fieldCache.TryGetValue(type, out var fields))
            {
                var allFields = type.GetFields(BindingFlags.Instance | 
                                                BindingFlags.Public | 
                                                BindingFlags.NonPublic);
                
                var injectFields = new List<FieldInfo>();
                foreach (var field in allFields)
                {
                    if (field.GetCustomAttributes(typeof(InjectAttribute), true).Length > 0)
                    {
                        injectFields.Add(field);
                    }
                }
                fields = injectFields.ToArray();
                _fieldCache[type] = fields;
            }
            
            foreach (var field in fields)
            {
                if (TryResolveField(field.FieldType, out var value, field.Name))
                {
                    field.SetValue(obj, value);
                }
            }
        }

        private static bool TryResolveField(Type fieldType, out object value, string fieldName)
        {
            for (int i = _containers.Count - 1; i >= 0; i--)
            {
                if (_containers[i].TryResolve(fieldType, out value, null))
                {
                    return true;
                }
            }
            Debug.LogWarning($"[DIContainer] Could not resolve {fieldType.FullName} for field '{fieldName}'");
            value = null;
            return false;
        }

        public static void Clear()
        {
            _containers.Clear();
            _fieldCache.Clear();
        }
    }

    public abstract class MonoInstaller : MonoBehaviour
    {
        protected Container Container { get; private set; }

        public abstract void InstallBindings();

        protected virtual void Awake()
        {
            Container = new Container();
            DIContainer.RegisterContainer(Container);
            InstallBindings();
        }

        protected virtual void OnDestroy()
        {
            DIContainer.UnregisterContainer(Container);
        }

        protected void Bind<T>(T instance, string key = null)
        {
            Container.Bind(instance, key);
        }

        protected void Bind<TInterface, TImplementation>(TImplementation instance, string key = null)
            where TImplementation : TInterface
        {
            Container.Bind<TInterface, TImplementation>(instance, key);
        }
    }
}