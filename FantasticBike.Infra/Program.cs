using System.Threading.Tasks;
using Pulumi;

namespace FantasticBike.Infra
{
    internal static class Program
    {
        static Task<int> Main() => Deployment.RunAsync<FantasticBikeStack>();
    }
}
