using System.IO;
using System.Linq;
using System.Management;

namespace Rack.Shared
{
    public sealed class NetworkPath
    {
        /// <summary>
        /// Определяет, является ли устройство, указанное в корне <paramref name="path" />, сетевым диском и преобразует
        /// путь в вид, пригодный для универсального доступа. Если преобразование невозможно, возвращает null.
        /// </summary>
        /// <param name="path">Путь в файловой системе.</param>
        /// <returns>Преобразованный, если это необходимо, путь.</returns>
        public static string GetNetworkPath(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) return null;
            var root = Path.GetPathRoot(path);
            var driveInfo = DriveInfo.GetDrives().FirstOrDefault(x => x.Name == root);
            if (driveInfo == null) return null;
            if (driveInfo.DriveType != DriveType.Network) return null;
            var managementObject = new ManagementObject($"Win32_LogicalDisk='{root.Substring(0, 2)}'");
            var providerName = managementObject["ProviderName"].ToString();
            return providerName + path.Substring(2);
        }
    }
}