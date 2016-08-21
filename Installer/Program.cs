using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.AccessControl;
using System.Security.Principal;

namespace Installer
{
    public enum Status
    {
        error,
        success,
        nothing
    }

    public class Installer
    {
        private string _destinationPath = @"C:\GetFullPath";
        
        private string baseRegistry = "HKEY_CLASSES_ROOT";
        private List<string> directoryRegistryKeys;
        private List<string> displayRegistryKeys;
        private List<string> fileRegistryKeys;

        public Status InstallerStatus = Status.nothing;

        public Installer()
        {
            directoryRegistryKeys = new List<string>
            {
                @"Directory\shell\getFullPath\command",
                @"Directory\Background\shell\getFullPath\command"
            };

            displayRegistryKeys = new List<string>
            {
                @"Directory\shell\getFullPath",
                @"Directory\Background\shell\getFullPath",
                @"*\shell\getFullPath",
            };

            fileRegistryKeys = new List<string>
            {
                @"*\shell\getFullPath\command"
            };
        }

        public void Install()
        {
            var currentDirectory = Directory.GetCurrentDirectory();
            var executables = new List<string> { Path.Combine(currentDirectory, "GetFullPath.exe"), Path.Combine(currentDirectory, "Uninstall.exe"),
                                                Path.Combine(currentDirectory, "Installer.exe")};
            CopyExecutables(executables);
            SetRegistries();
        }

        private void CopyExecutables(List<string> paths)
        {
            Directory.CreateDirectory(_destinationPath);
            paths.ForEach(path =>
            {
                File.Copy(path, Path.Combine(_destinationPath, Path.GetFileName(path)), true);
            });
        }

        public void UnSetRegistryKeys()
        {
            displayRegistryKeys.ForEach(key =>
            {
                var parentKeyName = Path.GetDirectoryName(key);
                var subKeyName = Path.GetFileName(key);
                var parentKey = Registry.ClassesRoot.OpenSubKey(parentKeyName, RegistryKeyPermissionCheck.ReadWriteSubTree, RegistryRights.FullControl);
                try
                {
                    parentKey.DeleteSubKeyTree(subKeyName);
                }
                catch (Exception ex)
                {
                    HandleInstallerError(ex);
                }
            });

            UpdateInstallerStatus();
        }

        private void SetRegistries()
        {
            string exePath = Path.Combine(_destinationPath, "GetFullPath.exe");
            string directoryValue = exePath +  @" ""%V""";
            string fileValue = exePath + @" ""%1""";
            string displayValue = @"Get Full Path";
            RegistrySecurity rs = SetRegistrySecurity();

            try
            {
                foreach (var key in displayRegistryKeys)
                {
                    Registry.ClassesRoot.CreateSubKey(key, RegistryKeyPermissionCheck.ReadWriteSubTree, rs);
                    Registry.SetValue(Path.Combine(baseRegistry, key), "", displayValue);
                }

                foreach (var key in directoryRegistryKeys)
                {
                    Registry.ClassesRoot.CreateSubKey(key, RegistryKeyPermissionCheck.Default, rs);
                    Registry.SetValue(Path.Combine(baseRegistry, key), "", directoryValue);
                }

                foreach (var key in fileRegistryKeys)
                {
                    Registry.ClassesRoot.CreateSubKey(key, RegistryKeyPermissionCheck.Default, rs);
                    Registry.SetValue(Path.Combine(baseRegistry, key), "", fileValue);
                }
            }
            catch(Exception ex)
            {
                HandleInstallerError(ex);
            }

            UpdateInstallerStatus();
        }

        private static RegistrySecurity SetRegistrySecurity()
        {
            RegistrySecurity rs = new RegistrySecurity();
            string user = Environment.UserDomainName + "\\" + Environment.UserName;

            // Allow the current user to read and delete the key.
            rs.AddAccessRule(new RegistryAccessRule(user,
                RegistryRights.FullControl,
                InheritanceFlags.None,
                PropagationFlags.None,
                AccessControlType.Allow));
            rs.SetOwner(new NTAccount(user));
            return rs;
        }

        private void UpdateInstallerStatus()
        {
            if (InstallerStatus == Status.nothing)
            {
                InstallerStatus = Status.success;
            }
        }

        private void HandleInstallerError(Exception ex)
        {
            Console.WriteLine(ex.Message);
            InstallerStatus = Status.error;
        }


        private static void GetDefaultKeyVaue(string key)
        {
            var value = Registry.GetValue(key, "", "vishesh");
            if (value != null)
            {
                Console.WriteLine($"registry : {value}");
            }
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var installer = new Installer();
            installer.Install();
            if(installer.InstallerStatus == Status.error)
            {
                Console.WriteLine("Utility was not installed properly");
                Console.ReadKey();
            }
        }
    }
}
