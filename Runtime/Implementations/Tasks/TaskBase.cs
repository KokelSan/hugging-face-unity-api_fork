using System;
using Newtonsoft.Json.Linq;

namespace HuggingFace.API {
    public abstract class TaskBase : TaskBase<string, string> {
        protected override JObject GetPayload(string input, object context) {
            return new JObject {
                ["inputs"] = input
            };
        }
    }

    public abstract class TaskBase<TInput, TResponse> : TaskBase<TInput, TResponse, object> where TInput : class where TResponse : class {
        protected override bool VerifyContext(object context, out object contextObject) {
            contextObject = context;
            return true;
        }
    }

    public abstract class TaskBase<TInput, TResponse, TContext> : ITask where TInput : class where TResponse : class where TContext : class {
        public abstract string taskName { get; }
        public abstract string defaultEndpoint { get; }

        public virtual void Query(object input, IAPIClient client, IAPIConfig config, Action<object> onSuccess, Action<string> onError, object context = null) {
            if (!config.GetTaskEndpoint(taskName, out TaskEndpoint taskEndpoint)) {
                onError?.Invoke($"Task endpoint for task {taskName} not found");
                return;
            }
            if (!VerifyInput(input, out TInput inputObject)) {
                onError?.Invoke($"Input is not of type {typeof(TInput)}");
                return;
            }
            if (!VerifyContext(context, out TContext contextObject)) {
                onError?.Invoke($"Context is not of type {typeof(TContext)}");
                return;
            }
            JObject payload = GetPayload(inputObject, contextObject);
            client.SendRequest(taskEndpoint.endpoint, config.apiKey, payload, response => {
                if (!PostProcess(response, inputObject, contextObject, out TResponse postProcessedResponse, out string error)) {
                    onError?.Invoke(error);
                    return;
                }
                onSuccess?.Invoke(postProcessedResponse);
            }, onError).RunCoroutine();
        }

        protected virtual bool VerifyInput(object input, out TInput inputObject) {
            inputObject = input as TInput;
            return inputObject != null;
        }

        protected virtual bool VerifyContext(object context, out TContext contextObject) {
            contextObject = context as TContext;
            return contextObject != null;
        }

        protected abstract JObject GetPayload(TInput input, TContext context);

        protected virtual bool PostProcess(object raw, TInput input, TContext context, out TResponse response, out string error) {
            response = raw as TResponse;
            if (response == null) {
                error = $"Failed to cast response to {typeof(TResponse)}";
                return false;
            }
            error = null;
            return true;
        }
    }
}