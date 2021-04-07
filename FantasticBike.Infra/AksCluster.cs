using System;
using System.Text;
using System.Threading.Tasks;
using Pulumi;
using Pulumi.AzureAD;
using Pulumi.AzureNative.ContainerService;
using Pulumi.AzureNative.ContainerService.Inputs;
using Pulumi.Random;
using Pulumi.Tls;
using ResourceIdentityType = Pulumi.AzureNative.ContainerService.ResourceIdentityType;

namespace FantasticBike.Infra
{
    public record AksClusterArgs
    {
        public string ProjectName { get; init; }
        public Output<string> ResourceGroupName { get; init; }
        public string? VmSize { get; init; }
        public int? VmCount { get; init; }
        public string? K8sVersion { get; init; }
    }
    
    public class AksCluster : ComponentResource
    {
        #region Fields
        
        readonly string stackName = Deployment.Instance.StackName;
        
        #endregion
        
        #region Outputs
        
        [Output] public Output<string> KubeConfig { get; set; }
        [Output] public Output<string> PrincipalId { get; set; }
        
        #endregion 
        
        public AksCluster(string name, AksClusterArgs args) : base(typeof(AksCluster).FullName, name)
        {
            var adAppName = $"{args.ProjectName}-{stackName}-adapp";
            var adApp = new Application(adAppName, new ApplicationArgs
            {
                Name = adAppName
            }, new CustomResourceOptions { Parent = this });
            
            var adSpName = $"{args.ProjectName}-{stackName}-adsp";
            var adSp = new ServicePrincipal(adSpName, new ServicePrincipalArgs
            {
                ApplicationId = adApp.ApplicationId
            }, new CustomResourceOptions { Parent = this });
            
            var password = new RandomPassword($"{adAppName}-password", new RandomPasswordArgs
            {
                Length = 20,
                Special = true
            }, new CustomResourceOptions { Parent = this });
            
            var adSpPassword = new ServicePrincipalPassword($"{adSpName}-secret", new ServicePrincipalPasswordArgs
            {
                Description = $"{adSpName}-secret",
                ServicePrincipalId = adSp.Id,
                Value = password.Result,
                EndDate = "2099-01-01T00:00:00Z"
            }, new CustomResourceOptions { Parent = this });

            var clusterName = $"{args.ProjectName}-{stackName}-aks";
            
            var sshKey = new PrivateKey($"{clusterName}-sshkey", new PrivateKeyArgs
            {
                Algorithm = "RSA",
                RsaBits = 4096
            }, new CustomResourceOptions { Parent = this });
            
            var cluster = new ManagedCluster(clusterName, new ManagedClusterArgs
            {
                ResourceName = clusterName,
                ResourceGroupName = args.ResourceGroupName,
                AddonProfiles =
                {
                    { "KubeDashboard", new ManagedClusterAddonProfileArgs { Enabled = true } }
                },
                AgentPoolProfiles =
                {
                    new ManagedClusterAgentPoolProfileArgs
                    {
                        Count = args.VmCount ?? 2,
                        MaxPods = 100,
                        Mode = "System",
                        Name = "agentpool",
                        OsDiskSizeGB = 30,
                        OsType = "Linux",
                        Type = "VirtualMachineScaleSets",
                        VmSize = args.VmSize ?? "Standard_D2_v3"
                    }
                },
                DnsPrefix = $"{args.ProjectName}aks",
                EnableRBAC = true,
                Identity = new ManagedClusterIdentityArgs { Type = ResourceIdentityType.SystemAssigned },
                KubernetesVersion = args.K8sVersion ?? "1.18.14",
                LinuxProfile = new ContainerServiceLinuxProfileArgs
                {
                    AdminUsername = "fantasticbike",
                    Ssh = new ContainerServiceSshConfigurationArgs
                    {
                        PublicKeys =
                        {
                            new ContainerServiceSshPublicKeyArgs
                            {
                                KeyData = sshKey.PublicKeyOpenssh
                            }
                        }
                    }
                },
                NodeResourceGroup = args.ResourceGroupName,
                ServicePrincipalProfile = new ManagedClusterServicePrincipalProfileArgs
                {
                    ClientId = adApp.ApplicationId,
                    Secret = adSpPassword.Value
                }
            }, new CustomResourceOptions { Parent = this });
            
            KubeConfig = Output.Tuple(args.ResourceGroupName, cluster.Name).Apply(names =>
                GetKubeConfig(names.Item1, names.Item2));
            PrincipalId = cluster.IdentityProfile.Apply(p => p!["kubeletidentity"].ObjectId!);
        }
        static async Task<string> GetKubeConfig(string resourceGroupName, string clusterName)
        {
            var credentials = await ListManagedClusterUserCredentials.InvokeAsync(new ListManagedClusterUserCredentialsArgs
            {
                ResourceGroupName = resourceGroupName,
                ResourceName = clusterName
            });
            var encoded = credentials.Kubeconfigs[0].Value;
            var data = Convert.FromBase64String(encoded);
            return Encoding.UTF8.GetString(data);
        }
    }
}