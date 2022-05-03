// Code taken from: https://github.com/engineerd/kubecontroller-csharp

using k8s;

namespace Kncs.CrdController.Crd
{
    public class Controller<T> where T : CustomResource
    {
        public delegate void CrdEventHandler(WatchEventType ev, T item);

        private readonly Kubernetes _client;
        private readonly CustomResourceDefinition _crd;
        private readonly CrdEventHandler _crdEventHandler;

        public Controller(Kubernetes client, CustomResourceDefinition crd, CrdEventHandler crdEventHandler)
        {
            _client = client;
            _crd = crd;
            _crdEventHandler = crdEventHandler;
        }

        public async Task StartAsync(CancellationToken token)
        {
            var result = await _client.ListClusterCustomObjectWithHttpMessagesAsync(
                group: _crd.ApiVersion.Split('/')[0],
                version: _crd.ApiVersion.Split('/')[1],
                plural: _crd.PluralName,
                watch: true,
                timeoutSeconds: (int)TimeSpan.FromMinutes(60).TotalSeconds)
                .ConfigureAwait(false);

            while (!token.IsCancellationRequested)
            {
                // todo: what is watch type L?
                // todo: handler on Error/onClose, watch event type Error
                result.Watch<T, object>((type, item) => _crdEventHandler(type, item));
            }
        }
    }
}
