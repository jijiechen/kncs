using Kncs.CrdController.OperatorSDK;
using Kncs.CrdController.Crd;
using NLog.Fluent;

namespace Kncs.CrdController;

public class Program
{
    static void Main(string[] args)
    {
        try
        {
            string k8sNamespace = "default";
            if (args.Length > 1)
                k8sNamespace = args[0];

            Controller<CSharpApp>.ConfigLogger();
            
            Log.Info($"=== STARTING CSharpApp for {k8sNamespace} ===");

            var controller = new Controller<CSharpApp>(new CSharpApp(), new CSharpAppOperator(k8sNamespace), k8sNamespace);
            Task reconciliation = controller.SatrtAsync();

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