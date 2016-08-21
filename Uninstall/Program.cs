using System;

namespace Uninstall
{
    class Program
    {
        static void Main(string[] args)
        {
            var installer = new Installer.Installer();
            installer.UnSetRegistryKeys();
            if(installer.InstallerStatus == Installer.Status.error)
            {
                Console.WriteLine("There were some errors while unistalling the program");
                Console.ReadKey();
            }
        }
    }
}
