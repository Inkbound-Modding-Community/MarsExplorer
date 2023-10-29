using MagmaDataMiner;

namespace MarsExplorer
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {

            const string path = @"C:\Program Files (x86)\Steam\steamapps\common\Inkbound\Inkbound_Data\StreamingAssets\SharedScriptableObjects\SharedScriptableObjects.fb";

            MineDb.Init(path);

            MineDb.LoadAll();

            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();
            Application.Run(new MarsMain());
        }
    }
}