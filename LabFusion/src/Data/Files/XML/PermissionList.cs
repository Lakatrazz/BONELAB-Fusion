using System.Xml.Linq;

using LabFusion.Extensions;
using LabFusion.Representation;

namespace LabFusion.Data
{
    public static class PermissionList
    {
        private const string _rootName = "PermissionList";

        private const string _fileName = "permissionList.xml";

        private const string _elementName = "Permission";
        private const string _idName = "id";
        private const string _userName = "username";
        private const string _levelName = "level";

        private static readonly List<Tuple<ulong, string, PermissionLevel>> _permittedUsers = new();

        public static IReadOnlyList<Tuple<ulong, string, PermissionLevel>> PermittedUsers => _permittedUsers;

        private static XMLFile _file;

        public static void ReadFile()
        {
            _permittedUsers.Clear();

            _file = new XMLFile(_fileName, _rootName);
            _file.ReadFile((d) =>
            {
                d.Descendants(_elementName).ForEach((element) =>
                {
                    if (element.TryGetAttribute(_idName, out string rawId) && element.TryGetAttribute(_userName, out string rawUser) && element.TryGetAttribute(_levelName, out string rawLevel))
                    {
                        if (ulong.TryParse(rawId, out ulong id) && Enum.TryParse(rawLevel, out PermissionLevel level))
                        {
                            _permittedUsers.Add(new Tuple<ulong, string, PermissionLevel>(id, rawUser, level));
                        }
                    }
                });
            });
        }

        private static void WriteFile()
        {
            List<object> entries = new();

            foreach (var tuple in _permittedUsers)
            {
                XElement entry = new(_elementName);

                entry.SetAttributeValue(_idName, tuple.Item1);
                entry.SetAttributeValue(_userName, tuple.Item2);
                entry.SetAttributeValue(_levelName, tuple.Item3);

                entries.Add(entry);
            }

            _file.WriteFile(entries);
        }

        public static void SetPermission(ulong longId, string username, PermissionLevel level)
        {
            var tuple = new Tuple<ulong, string, PermissionLevel>(longId, username, level);

            foreach (var user in _permittedUsers.ToArray())
            {
                if (user.Item1 == longId)
                {
                    _permittedUsers.Remove(user);
                    break;
                }
            }

            _permittedUsers.Add(tuple);

            WriteFile();
        }
    }
}
