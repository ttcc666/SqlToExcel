using System;
using System.Collections.Generic;
using SqlToExcel.Models;

namespace SqlToExcel.Services
{
    public static class EventService
    {
        private static readonly Dictionary<Type, List<Action<object>>> _subscriptions = new Dictionary<Type, List<Action<object>>>();

        public static void Subscribe<T>(Action<T> action) where T : class
        {
            var type = typeof(T);
            if (!_subscriptions.ContainsKey(type))
            {
                _subscriptions[type] = new List<Action<object>>();
            }
            _subscriptions[type].Add(obj => action(obj as T));
        }

        public static void Publish<T>(T eventArgs) where T : class
        {
            var type = typeof(T);
            if (_subscriptions.ContainsKey(type))
            {
                foreach (var action in _subscriptions[type])
                {
                    action(eventArgs);
                }
            }
        }
    }

    public class MappingsChangedEvent { }

    public class LoadConfigToMainViewEvent
    {
        public BatchExportConfig Config { get; }
        public bool IsEditMode { get; }
        public string OriginalKey { get; }

        public LoadConfigToMainViewEvent(BatchExportConfig config, bool isEditMode = false, string originalKey = "")
        {
            Config = config;
            IsEditMode = isEditMode;
            OriginalKey = originalKey;
        }
    }
}