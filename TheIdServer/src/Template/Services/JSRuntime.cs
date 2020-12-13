using Microsoft.JSInterop;
using System.Threading;
using System.Threading.Tasks;

namespace Template.Services
{
    public class JSRuntime : IJSRuntime
    {
        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, object[] args)
        {
            return new ValueTask<TValue>();
        }

        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, CancellationToken cancellationToken, object[] args)
        {
            return new ValueTask<TValue>();
        }
    }
}
