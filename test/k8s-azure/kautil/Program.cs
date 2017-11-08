using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.Azure.Management.Compute.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent.Authentication;

namespace kautil
{
    class Program
    {
        static void Main(string[] args)
        {
            var flows = new string[] {"create", "delete", "clean", "location"};
            if (args.Length == 0 || !flows.Contains(args[0])){
                Console.WriteLine($"kautil <{string.Join("|", flows)}>");
                return;
            }

            var kautil = KaUtil.FromEnv();
            switch (args[0]) {
                case "create":
                    if (args.Length < 3) {
                        Console.WriteLine("kautil create <name> <location>");
                        return;
                    }
                    kautil.Create(args[1], args[2]);
                    break;
                case "delete":
                    if (args.Length < 2) {
                        Console.WriteLine("kautil delete <name>");
                        return;
                    }
                    kautil.Delete(args[1]);
                    break;
                case "clean":
                    kautil.Clean(args.Length>=2 && "-f" == args[1]);
                    break;
                case "location":
                    Console.WriteLine(kautil.SelectLocation());
                    break;
            }
        }
    }

    class KaUtil {
        private const string TagCreatorKey = "creator";
        private const string TagCreatorVal = "k8s-ci";
        private const string TagSinceKey = "since";
        private  const string TimeFormat = "u";
        
        private TimeSpan cleanupTimeout = TimeSpan.FromDays(1);
        private IResourceGroups resourceGroups;
        private IComputeUsages computeUsages;

        public static KaUtil FromEnv(){
            var credential = new AzureCredentials(
                new ServicePrincipalLoginInformation{
                    ClientId = getEnv("K8S_AZURE_SPID"),
                    ClientSecret = getEnv("K8S_AZURE_SPSEC"),
                },
                getEnv("K8S_AZURE_TENANTID"),
                AzureEnvironment.AzureGlobalCloud
            );
            var envTimeout = getEnv("K8S_AZURE_CLEANUP_TIMEOUT", false);
            var timeout = TimeSpan.Parse(envTimeout??"6:00", CultureInfo.InvariantCulture);
            
            return new KaUtil(credential, getEnv("K8S_AZURE_SUBSID"), timeout);
        }

        public KaUtil(AzureCredentials credential, string subscription, TimeSpan timeout) {
            this.resourceGroups = ResourceManager.Authenticate(credential).WithSubscription(subscription).ResourceGroups;
            this.computeUsages = ComputeManager.Authenticate(credential, subscription).Usages;
            this.cleanupTimeout = timeout;
        }

        public void Create(string name, string location) 
            => this.resourceGroups.Define(name).WithRegion(location)
                .WithTag(TagCreatorKey, TagCreatorVal)
                .WithTag(TagSinceKey, getTime())
                .Create();
        
        public IEnumerable<IResourceGroup> ListByTag()
            => this.resourceGroups.ListByTag($"'{TagCreatorKey}'", $"'{TagCreatorVal}'");
        
        public void Delete(string name) => this.resourceGroups.BeginDeleteByName(name);

        public void Clean(bool force = false) {
            var now = DateTimeOffset.Now;
            foreach(var group in this.ListByTag()){
                var gap = TimeSpan.MaxValue;
                if (group.Tags.TryGetValue(TagSinceKey, out var since)
                    && DateTimeOffset.TryParseExact(since, TimeFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dto)) {
                    gap = now - dto;
                }
                Console.WriteLine($"{group.Name} => {gap}");
                if (force || gap > cleanupTimeout) {
                    Console.WriteLine($"Delete group {group.Name}");
                    this.Delete(group.Name);
                }
            }
        }

        public string SelectLocation() {
            var locationsStr = getEnv("K8S_AZURE_LOCATIONS", false);
            if (string.IsNullOrEmpty(locationsStr)) {
                Console.Error.WriteLine("K8S_AZURE_LOCATIONS not set");
                return string.Empty;
            }
            var selected = string.Empty;
            var min = 100;
            var locations = locationsStr.Split(',');
            foreach(var location in locations) {
                var usages = this.computeUsages.ListByRegion(location);
                var cores = usages.Where(item=> item.Name.Value == "cores").SingleOrDefault();
                if(cores.CurrentValue< min) {
                    min =cores.CurrentValue; selected = location;
                }
            }
            return selected;
        }

        private static string getEnv(string name, bool panic = true) {
            var result = Environment.GetEnvironmentVariable(name);
            if (string.IsNullOrEmpty(result) && panic){
                Console.Error.WriteLine($"Environment variable: {name} is not set");
                Environment.Exit(1);
            }

            return result;
        }

        private static string getTime() => DateTimeOffset.Now.ToString(TimeFormat, CultureInfo.InvariantCulture);
    }
}
