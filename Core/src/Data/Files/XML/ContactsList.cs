using LabFusion.Extensions;
using LabFusion.Network;
using LabFusion.Representation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace LabFusion.Data
{
    public sealed class Contact {
        public ulong id;
        public string username;
        public float volume;

        public XElement CreateElement() {
            XElement element = new(nameof(Contact));

            element.SetAttributeValue(nameof(id), id);
            element.SetAttributeValue(nameof(username), username);
            element.SetAttributeValue(nameof(volume), volume);

            return element;
        }

        public Contact(PlayerId id) {
            Update(id);
            volume = 1f;
        }

        public void Update(PlayerId id) {
            this.id = id;
            username = id.GetMetadata(MetadataHelper.UsernameKey);
        }

        public Contact(XElement element) {
            id = 0;
            username = string.Empty;
            volume = 1f;

            if (element.TryGetAttribute(nameof(id), out var rawId)) {
                ulong.TryParse(rawId, out id);
            }

            if (element.TryGetAttribute(nameof(username), out var rawUser)) {
                username = rawUser;
            }

            if (element.TryGetAttribute(nameof(volume), out var rawVolume)) { 
                float.TryParse(rawVolume, out volume);
            }
        }
    }

    public static class ContactsList
    {
        private const string _rootName = "Contacts";
        private const string _fileName = "contacts.xml";

        private static readonly List<Contact> _contacts = new();
        public static IReadOnlyList<Contact> Contacts => _contacts;

        private static XMLFile _file;

        public static event Action<Contact> OnContactUpdated;

        public static void ReadFile()
        {
            _contacts.Clear();

            _file = new XMLFile(_fileName, _rootName);
            _file.ReadFile((d) => {
                d.Descendants(nameof(Contact)).ForEach((element) => {
                    _contacts.Add(new Contact(element));
                });
            });
        }

        private static void WriteFile()
        {
            List<object> entries = new();

            foreach (var contact in Contacts) {
                entries.Add(contact.CreateElement());
            }

            _file.WriteFile(entries);
        }

        public static Contact GetContact(PlayerId id) {
            Contact contact;

            for (var i = 0; i < Contacts.Count; i++) {
                contact = Contacts[i];
                if (contact.id == id) {
                    contact.Update(id);
                    return contact;
                }
            }

            contact = new Contact(id);
            return contact;
        }

        public static void UpdateContact(Contact contact) {
            bool updated = false;

            for (var i = 0; i < Contacts.Count; i++) {
                if (Contacts[i].id == contact.id) {
                    _contacts[i] = contact;
                    updated = true;
                    break;
                }
            }

            if (!updated)
                _contacts.Add(contact);

            OnContactUpdated?.Invoke(contact);

            WriteFile();
        }
    }
}
