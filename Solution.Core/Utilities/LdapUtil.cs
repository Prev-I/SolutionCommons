using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.DirectoryServices.Protocols;
using System.Net;
using System.Security.Cryptography;

using log4net;


namespace Solution.Core.Utilities
{
    /// <summary>
    /// A sample LDAP client. For simplicity reasons, this clients only uses synchronous requests.
    /// Based on https://auth0.com/blog/using-ldap-with-c-sharp/
    /// </summary>
    public static class LdapUtil
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);


        /// <summary>
        /// Initialize and return the connection object without make the Bind operation
        /// </summary>
        /// <param name="hostname"></param>
        /// <param name="port"></param>
        /// <param name="domain"></param>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns>Initialized object to LDAP server to be binded by external code</returns>
        public static LdapConnection CreateClient(string hostname, int port, string domain, string username,  string password)
        {
            var credentials = new NetworkCredential(username, password, domain);
            var serverId = new LdapDirectoryIdentifier(hostname, port);

            return new LdapConnection(serverId, credentials);
        }

        /// <summary>
        /// Performs a search in the LDAP server. This method is generic in its return value to show the power
        /// of searches. A less generic search method could be implemented to only search for users, for instance.
        /// </summary>
        /// <param name="connection">An OPEN connection to LDAP server</param>
        /// <param name="baseDn">The distinguished name of the base node at which to start the search</param>
        /// <param name="ldapFilter">An LDAP filter as defined by RFC4515</param>
        /// <returns>A flat list of dictionaries which in turn include attributes and the distinguished name (DN)</returns>
        public static List<Dictionary<string, string>> Search(LdapConnection connection, string baseDn, string ldapFilter)
        {
            var request = new SearchRequest(baseDn, ldapFilter, SearchScope.Subtree, null);
            var response = (SearchResponse)connection.SendRequest(request);

            var result = new List<Dictionary<string, string>>();

            foreach (SearchResultEntry entry in response.Entries)
            {
                var dic = new Dictionary<string, string>
                {
                    ["DN"] = entry.DistinguishedName
                };

                foreach (string attrName in entry.Attributes.AttributeNames)
                {
                    //For simplicity, we ignore multi-value attributes
                    dic[attrName] = string.Join(",", entry.Attributes[attrName].GetValues(typeof(string)));
                }
                result.Add(dic);
            }
            return result;
        }

        /// <summary>
        /// Adds a user to the LDAP server database. This method is intentionally less generic than the search one to
        /// make it easier to add meaningful information to the database.
        /// </summary>
        /// <param name="connection">An OPEN connection to LDAP server</param>
        /// <param name="user">Dictionary with the informations to be added, keys MUST be ldap fileds name</param>
        public static void AddUser(LdapConnection connection, Dictionary<string,string> user)
        {
            var sha1 = new SHA1Managed();
            var digest = Convert.ToBase64String(sha1.ComputeHash(System.Text.Encoding.UTF8.GetBytes(user["userPassword"])));

            //Example of user dictionary
            var u = new Dictionary<string, string>()
            {
                { "dn", "" },
                { "uid", "" },
                { "ou", "" },
                { "userPassword", "" }
            };
            var request = new AddRequest(user["dn"], new DirectoryAttribute[] {
                new DirectoryAttribute("uid", user["uid"]),
                new DirectoryAttribute("ou", user["ou"]),
                new DirectoryAttribute("userPassword", "{SHA}" + digest),
                new DirectoryAttribute("objectClass", new string[] { "top", "account", "simpleSecurityObject" })
            });
            connection.SendRequest(request);
        }

        /// <summary>
        /// This method shows how to modify an attribute.
        /// </summary>
        /// <param name="connection">An OPEN connection to LDAP server</param>
        /// <param name="oldUid">Old user UID</param>
        /// <param name="newUid">New user UID</param>
        public static void ChangeUserUid(LdapConnection connection, string oldUid, string newUid)
        {
            var oldDn = string.Format("uid={0},ou=users,dc=example,dc=com", oldUid);
            var newDn = string.Format("uid={0},ou=users,dc=example,dc=com", newUid);

            DirectoryRequest request = new ModifyDNRequest(oldDn, "ou=users,dc=example,dc=com", "uid=" + newUid);
            connection.SendRequest(request);

            request = new ModifyRequest(newDn, DirectoryAttributeOperation.Replace, "uid", new string[] { newUid });
            connection.SendRequest(request);
        }

        /// <summary>
        /// This method shows how to delete anything by its distinguised name (DN).
        /// </summary>
        /// <param name="connection">An OPEN connection to LDAP server</param>
        /// <param name="dn">Distinguished name of the entry to delete</param>
        public static void Delete(LdapConnection connection, string dn)
        {
            var request = new DeleteRequest(dn);
            connection.SendRequest(request);
        }

        /// <summary>
        /// Searches for a user and compares the password.
        /// We assume all users are at base DN ou=users,dc=example,dc=com and that passwords are
        /// hashed using SHA1 (no salt) in OpenLDAP format.
        /// </summary>
        /// <param name="connection">An OPEN connection to LDAP server</param>
        /// <param name="username">Username</param>
        /// <param name="password">Password</param>
        /// <returns>True if the credentials are valid, false otherwise</returns>
        public static bool ValidateUser(LdapConnection connection, string username, string password)
        {
            var sha1 = new SHA1Managed();
            var digest = Convert.ToBase64String(sha1.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password)));
            var request = new CompareRequest(string.Format("uid={0},ou=users,dc=example,dc=com", username),
                "userPassword", "{SHA}" + digest);
            var response = (CompareResponse)connection.SendRequest(request);
            return response.ResultCode == ResultCode.CompareTrue;
        }

        /// <summary>
        /// Another way of validating a user is by performing a bind. In this case the server
        /// queries its own database to validate the credentials. It is defined by the server
        /// how a user is mapped to its directory.
        /// </summary>
        /// <param name="connection">An OPEN connection to LDAP server</param>
        /// <param name="username">Username</param>
        /// <param name="password">Password</param>
        /// <returns>True if the credentials are valid, false otherwise</returns>
        public static bool ValidateUserByBind(LdapConnection connection, string username, string password)
        {
            bool result = true;
            var credentials = new NetworkCredential(username, password);
            var serverId = new LdapDirectoryIdentifier(connection.SessionOptions.HostName);

            var conn = new LdapConnection(serverId, credentials);
            try
            {
                conn.Bind();
            }
            catch (Exception)
            {
                result = false;
            }
            conn.Dispose();
            return result;
        } 

    }
}
