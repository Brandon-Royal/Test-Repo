using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Web;
using System.Web.UI.WebControls;
using System.Xml;
using Sitecore.Poc.DataService.Models;

namespace Sitecore.Poc.DataService.Repositories
{
    public class PeopleRepository : IEntityRepository<Person>
    {
        public bool Exists(string id)
        {
            var path = GetPath();
            if (string.IsNullOrEmpty(path) || !FileExists(path))
            {
                throw new FileNotFoundException();
            }

            var list = new List<Person>();
            var xmlDocument = new XmlDocument();
            xmlDocument.Load(path);

            var query = string.Format("people/person[email='{0}']", id);
            var node = xmlDocument.SelectSingleNode(query);
            return node != null;
        }

        public IEnumerable<Person> GetAll()
        {
            var path = GetPath();
            if (string.IsNullOrEmpty(path) || !FileExists(path))
            {
                throw new FileNotFoundException();
            }
            var list = new List<Person>();
            var xmlDocument = new XmlDocument();
            xmlDocument.Load(path);
            var nodes = xmlDocument.SelectNodes("people/*");
            if (nodes == null || nodes.Count == 0) return Enumerable.Empty<Person>();
            foreach (XmlNode node in nodes)
            {
                if (!node.HasChildNodes) continue;
                var person = GetPerson(node);
                list.Add(person);
            }
            return list;
        }

        public Person GetSingle(string id)
        {
            var email = id;
            var path = GetPath();
            if (string.IsNullOrEmpty(path) || !FileExists(path))
            {
                throw new FileNotFoundException();
            }

            var xmlDocument = new XmlDocument();
            xmlDocument.Load(path);
            var query = string.Format("people/person[email='{0}']", email);
            var node = xmlDocument.SelectSingleNode(query);
            if (node != null && node.HasChildNodes)
            {
                return GetPerson(node);
            }
            return null;
        }

        public void AddSingle(string id, Person person)
        {
            var path = GetPath();
            if (string.IsNullOrEmpty(path) || !FileExists(path))
            {
                throw new FileNotFoundException();
            }

            var xmlDocument = new XmlDocument();
            xmlDocument.Load(path);
            AddPerson(xmlDocument, person);
            xmlDocument.Save(path);
        }

        public void UpdateSingle(string id, Person person)
        {
            var email = id;
            var path = GetPath();
            if (string.IsNullOrEmpty(path) || !FileExists(path))
            {
                throw new FileNotFoundException();
            }

            var xmlDocument = new XmlDocument();
            xmlDocument.Load(path);
            var query = string.Format("people/person[email='{0}']", email);
            var node = xmlDocument.SelectSingleNode(query);
            if (node != null && node.HasChildNodes)
            {
                UpdatePerson(node, person);
            }
            xmlDocument.Save(path);
        }

        public void DeleteSingle(string id)
        {
            var email = id;
            var path = GetPath();
            if (string.IsNullOrEmpty(path) || !FileExists(path))
            {
                throw new FileNotFoundException();
            }

            var xmlDocument = new XmlDocument();
            xmlDocument.Load(path);
            var query = string.Format("people/person[email='{0}']", email);
            var node = xmlDocument.SelectSingleNode(query);
            if (node != null && node.HasChildNodes)
            {
                DeletePerson(node);
            }
            xmlDocument.Save(path);
        }

        private static string GetPath()
        {
            var appPath = HttpContext.Current.Request.PhysicalApplicationPath;
            if (string.IsNullOrEmpty(appPath))
            {
                throw new NullReferenceException("Application path is null or empty");
            }
            var appdataPath = Path.Combine(appPath, "App_Data");
            return Path.Combine(appdataPath, "people.xml");
        }

        private static bool FileExists(string path)
        {
            return File.Exists(path);
        }

        private static Person GetPerson(XmlNode node)
        {
            var person = new Person();
            foreach (XmlNode innerNode in node.ChildNodes)
            {
                switch (innerNode.Name)
                {
                    case "firstName":
                        person.FirstName = innerNode.InnerText;
                        break;
                    case "lastName":
                        person.LastName = innerNode.InnerText;
                        break;
                    case "email":
                        person.Email = innerNode.InnerText;
                        break;
                    case "description":
                        person.Description = innerNode.InnerText;
                        break;
                    case "jobTitle":
                        person.JobTitle = innerNode.InnerText;
                        break;
                }
            }
            return person;
        }

        private static void UpdatePerson(XmlNode node, Person person)
        {
            for (int i = 0; i < node.ChildNodes.Count; i++)
            {
                switch (node.ChildNodes[i].Name)
                {
                    case "firstName":
                        node.ChildNodes[i].InnerText = UpdateField(node.ChildNodes[i].InnerText, person.FirstName);
                        break;
                    case "lastName":
                        node.ChildNodes[i].InnerText = UpdateField(node.ChildNodes[i].InnerText, person.LastName);
                        break;
                    case "email":
                        node.ChildNodes[i].InnerText = UpdateField(node.ChildNodes[i].InnerText, person.Email);
                        break;
                    case "description":
                        node.ChildNodes[i].InnerText = UpdateField(node.ChildNodes[i].InnerText, person.Description);
                        break;
                    case "jobTitle":
                        node.ChildNodes[i].InnerText = UpdateField(node.ChildNodes[i].InnerText, person.JobTitle);
                        break;
                }
            }
        }

        private static string UpdateField(string oldValue, string newValue)
        {
            return !string.IsNullOrEmpty(newValue) ? newValue : oldValue;
        }

        private static void AddPerson(XmlDocument xmlDocument, Person person)
        {
            var parentNode = xmlDocument.CreateElement("person");
            var firstNameNode = xmlDocument.CreateElement("firstName");
            var lastNameNode = xmlDocument.CreateElement("lastName");
            var emailNode = xmlDocument.CreateElement("email");
            var descriptionNode = xmlDocument.CreateElement("description");
            var jobTitleNode = xmlDocument.CreateElement("jobTitle");

            firstNameNode.InnerText = person.FirstName ?? "";
            lastNameNode.InnerText = person.LastName ?? "";
            emailNode.InnerText = person.Email ?? "";
            descriptionNode.InnerText = person.Description ?? "";
            jobTitleNode.InnerText = person.JobTitle ?? "";

            parentNode.AppendChild(firstNameNode);
            parentNode.AppendChild(lastNameNode);
            parentNode.AppendChild(emailNode);
            parentNode.AppendChild(descriptionNode);
            parentNode.AppendChild(jobTitleNode);

            var root = xmlDocument.DocumentElement;
            if (root == null)
            {
                throw new NullReferenceException("Could not add person.  Root document element is null");
            }
            root.AppendChild(parentNode);
        }

        private void DeletePerson(XmlNode node)
        {
            var parentNode = node.ParentNode;
            if (parentNode == null)
            {
                throw new NullReferenceException("Could not delete person. Parent node is null");
            }
            parentNode.RemoveChild(node);
        }
    }
}