﻿using Microsoft.JSInterop;
using Newtonsoft.Json;
using OneOf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GoogleMapsComponents
{
    internal class JsObjectRef1 : IJsObjectRef
    {
        protected Guid _guid;

        public Guid Guid
        {
            get { return _guid; }
        }

        public string GuidString
        {
            get { return _guid.ToString(); }
        }

        public JsObjectRef1(Guid guid)
        {
            _guid = guid;
        }

        [JsonConstructor]
        public JsObjectRef1(string guidString)
        {
            _guid = new Guid(guidString);
        }

        public override bool Equals(object obj)
        {
            var other = obj as JsObjectRef;

            if (other == null)
            {
                return false;
            }
            else
            {
                return other.Guid == _guid;
            }
        }

        public override int GetHashCode()
        {
            return _guid.GetHashCode();
        }
    }

    public class JsObjectRef : IJsObjectRef, IAsyncDisposable
    {
        protected readonly Guid _guid;
        protected readonly IJSRuntime _jsRuntime;

        public Guid Guid
        {
            get { return _guid; }
        }

        public IJSRuntime JSRuntime
        {
            get { return _jsRuntime; }
        }

        public JsObjectRef(
            IJSRuntime jsRuntime,
            Guid guid)
        {
            _jsRuntime = jsRuntime;
            _guid = guid;
        }

        public static ValueTask<JsObjectRef> CreateAsync(
            IJSRuntime jsRuntime,
            string constructorFunctionName,
            params object?[] args)
        {
            return CreateAsync(jsRuntime, Guid.NewGuid(), constructorFunctionName, args);
        }

        public async static ValueTask<Dictionary<string, JsObjectRef>> CreateMultipleAsync(
            IJSRuntime jsRuntime,
            string constructorFunctionName,
            Dictionary<string, object> args)
        {
            Dictionary<string, Guid> internalMapping = args.ToDictionary(e => e.Key, e => Guid.NewGuid());
            Dictionary<Guid, object> dictArgs = internalMapping.ToDictionary(e => e.Value, e => args[e.Key]);
            Dictionary<Guid, JsObjectRef> result = await CreateMultipleAsync(
                jsRuntime,
                constructorFunctionName,
                dictArgs);

            return internalMapping.ToDictionary(e => e.Key, e => result[e.Value]);
        }

        public async ValueTask<Dictionary<string, JsObjectRef>> AddMultipleAsync(
            string constructorFunctionName,
            Dictionary<string, object> args)
        {
            Dictionary<string, Guid> internalMapping = args.ToDictionary(e => e.Key, e => Guid.NewGuid());
            Dictionary<Guid, object> dictArgs = internalMapping.ToDictionary(e => e.Value, e => args[e.Key]);
            Dictionary<Guid, JsObjectRef> result = await CreateMultipleAsync(
                 _jsRuntime,
                 constructorFunctionName,
                 dictArgs);

            return internalMapping.ToDictionary(e => e.Key, e => result[e.Value]);
        }

        public async static ValueTask<JsObjectRef> CreateAsync(
            IJSRuntime jsRuntime,
            Guid guid,
            string functionName,
            params object[] args)
        {
            var jsObjectRef = new JsObjectRef(jsRuntime, guid);

            await jsRuntime.MyInvokeAsync<object>(
                "googleMapsObjectManager.createObject",
                new object[] { guid.ToString(), functionName }
                    .Concat(args).ToArray()
            );

            return jsObjectRef;
        }

        public async static ValueTask<Dictionary<Guid, JsObjectRef>> CreateMultipleAsync(
            IJSRuntime jsRuntime,
            string functionName,
            Dictionary<Guid, object> dictArgs)
        {
            Dictionary<Guid, JsObjectRef> jsObjectRefs = dictArgs.ToDictionary(e => e.Key, e => new JsObjectRef(jsRuntime, e.Key));

            await jsRuntime.MyInvokeAsync<object>(
                "googleMapsObjectManager.createMultipleObject",
                new object[] { dictArgs.Select(e => e.Key.ToString()).ToList(), functionName }
                    .Concat(dictArgs.Values).ToArray()
            );

            return jsObjectRefs;
        }

        public async ValueTask DisposeAsync()
        {
            await _jsRuntime.InvokeAsync<object>(
                "googleMapsObjectManager.disposeObject",
                _guid.ToString()
            );
        }

        public ValueTask<object> DisposeMultipleAsync(List<Guid> guids)
        {
            return _jsRuntime.InvokeAsync<object>(
                "googleMapsObjectManager.disposeMultipleObjects",
                guids.Select(e => e.ToString()).ToList()
            );
        }

        public ValueTask InvokeAsync(string functionName, params object[] args)
        {
            return _jsRuntime.MyInvokeAsync(
                "googleMapsObjectManager.invoke",
                new object[] { _guid.ToString(), functionName }
                    .Concat(args).ToArray()
            );
        }

        public ValueTask InvokeMultipleAsync(string functionName, Dictionary<Guid, object> dictArgs)
        {
            return _jsRuntime.MyInvokeAsync(
                "googleMapsObjectManager.invokeMultiple",
                new object[] { dictArgs.Select(e => e.Key.ToString()).ToList(), functionName }
                    .Concat(dictArgs.Values).ToArray()
            );
        }

        public async ValueTask AddMultipleListenersAsync(string eventName, Dictionary<Guid, object> dictArgs)
        {
            var _ = await _jsRuntime.MyAddListenerAsync(
                "googleMapsObjectManager.addMultipleListeners",
                new object[] { dictArgs.Select(e => e.Key.ToString()).ToList(), eventName }
                    .Concat(dictArgs.Values).ToArray()
            );

            return;
        }

        public ValueTask<T> InvokeAsync<T>(string functionName, params object[] args)
        {
            return _jsRuntime.MyInvokeAsync<T>(
                "googleMapsObjectManager.invoke",
                new object[] { _guid.ToString(), functionName }
                    .Concat(args).ToArray()
            );
        }

        public ValueTask<Dictionary<string, T>> InvokeMultipleAsync<T>(string functionName, Dictionary<Guid, object> dictArgs)
        {
            return _jsRuntime.MyInvokeAsync<Dictionary<string, T>>(
                "googleMapsObjectManager.invokeMultiple",
                new object[] { dictArgs.Select(e => e.Key.ToString()).ToList(), functionName }
                    .Concat(dictArgs.Values).ToArray()
            );
        }

        /// <summary>
        /// Use when returned result will be one of defined types
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="U"></typeparam>
        /// <param name="jsRuntime"></param>
        /// <param name="identifier"></param>
        /// <param name="args"></param>
        /// <returns>Discriminated union of specified types</returns>
        public ValueTask<OneOf<T, U>> InvokeAsync<T, U>(string functionName, params object[] args)
        {
            return _jsRuntime.MyInvokeAsync<T, U>(
                "googleMapsObjectManager.invoke",
                new object[] { _guid.ToString(), functionName }
                    .Concat(args).ToArray()
            );
        }

        /// <summary>
        /// Use when returned result will be one of defined types
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="U"></typeparam>
        /// <typeparam name="V"></typeparam>
        /// <param name="jsRuntime"></param>
        /// <param name="identifier"></param>
        /// <param name="args"></param>
        /// <returns>Discriminated union of specified types</returns>
        public ValueTask<OneOf<T, U, V>> InvokeAsync<T, U, V>(string functionName, params object[] args)
        {
            return _jsRuntime.MyInvokeAsync<T, U, V>(
                "googleMapsObjectManager.invoke",
                new object[] { _guid.ToString(), functionName }
                    .Concat(args).ToArray()
            );
        }

        public async ValueTask<JsObjectRef> InvokeWithReturnedObjectRefAsync(string functionName, params object[] args)
        {
            var guid = await _jsRuntime.MyInvokeAsync<string>(
                "googleMapsObjectManager.invokeWithReturnedObjectRef",
                new object[] { _guid.ToString(), functionName }
                    .Concat(args).ToArray()
            );

            return new JsObjectRef(_jsRuntime, new Guid(guid));
        }

        public ValueTask<T> GetValue<T>(string propertyName)
        {
            return _jsRuntime.MyInvokeAsync<T>(
                "googleMapsObjectManager.readObjectPropertyValue",
                 _guid.ToString(),
                 propertyName);
        }

        public async ValueTask<JsObjectRef> GetObjectReference(string propertyName)
        {
            var guid = await _jsRuntime.MyInvokeAsync<string>(
                "googleMapsObjectManager.readObjectPropertyValueWithReturnedObjectRef",
                 _guid.ToString(),
                 propertyName);

            return new JsObjectRef(_jsRuntime, new Guid(guid));
        }

        public override bool Equals(object obj)
        {
            if (obj is JsObjectRef other)
            {
                return other.Guid == this.Guid;
            }
            else
            {
                return false;
            }
        }

        public override int GetHashCode()
        {
            return _guid.GetHashCode();
        }
    }
}
