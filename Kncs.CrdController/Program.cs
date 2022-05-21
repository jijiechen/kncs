using ContainerSolutions.OperatorSDK;
using Kncs.CrdController.Crd;
using NLog.Fluent;

namespace Kncs.CrdController;

public class Program
{
    static async Task Main(string[] args)
    {
        
        try
        {
            string k8sNamespace = "default";
            if (args.Length > 1)
                k8sNamespace = args[0];

            Controller<CSharpApp>.ConfigLogger();

            var controller = new Controller<CSharpApp>(new CSharpApp(), new CSharpAppController(), k8sNamespace);
            Task reconciliation = controller.SatrtAsync();

            Log.Info($"=== STARTED ===");

            reconciliation.ConfigureAwait(false).GetAwaiter().GetResult();

        }
        catch (Exception ex)
        {
            Log.Fatal(ex.Message + ex.StackTrace);
            throw;
        }
        finally
        {
            Log.Warn($"=== TERMINATING ===");
        }
    }
}