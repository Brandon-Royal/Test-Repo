using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Policy;
using Newtonsoft.Json;
using Sitecore.Diagnostics;
using Sitecore.Poc.ServiceDataProvider.Models;
using Sitecore.Rules.Conditions;

namespace Sitecore.Poc.ServiceDataProvider.Repositories
{
    public class PeopleRepository
    {
        private readonly string _apiEndpoint;
        private Hashtable hashtable;

        public PeopleRepository(string apiEndpoint)
        {
            Assert.IsNotNullOrEmpty(apiEndpoint, "apiEndpoint");
            _apiEndpoint = apiEndpoint;
            hashtable = new Hashtable();
        }

        public IEnumerable<Person> GetAllPeople()
        {
            var request = WebRequest.Create(_apiEndpoint);
            request.ContentType = "application/json; charset=utf-8";
            var response = (HttpWebResponse) request.GetResponse();
            if (response.StatusCode != HttpStatusCode.OK)
            {
                Log.Error("Endpoint response did not return 200 response", _apiEndpoint);
                return Enumerable.Empty<Person>();
            }

            var responseStream = response.GetResponseStream();
            if (responseStream == null)
            {
                Log.Error("Cannot read file response stream", response);
                return Enumerable.Empty<Person>();
            }

            using (var sr = new StreamReader(responseStream))
            {
                var reader = new JsonTextReader(sr);
                var se = new JsonSerializer();
                var people = se.Deserialize<IEnumerable<Person>>(reader).ToList();
                AddToHash(people);
                return people;
            }
        }

        public bool UpdatePerson(string email, Person person)
        {
            var str = Serialize(person);
            var byteArray = GetBytes(str);
            var endpoint = string.Format("{0}/{1}", _apiEndpoint, email);
            var request = WebRequest.Create(endpoint);
            request.ContentType = "application/json; charset=utf-8";
            request.Method = "PUT";
            var dataStream = request.GetRequestStream();
            dataStream.Write(byteArray, 0, byteArray.Length);
            dataStream.Close();
            var response = (HttpWebResponse)request.GetResponse();
            if (response.StatusCode != HttpStatusCode.OK || response.StatusCode != HttpStatusCode.NoContent)
            {
                Log.Error("Endpoint response did not return 200 response", _apiEndpoint);
            }
            return true;
        }

        public Person GetPerson(string email)
        {
            //write option to get data if null
            return string.IsNullOrEmpty(email) ? null : hashtable[email] as Person;
        }

        public void AddToHash(List<Person> people)
        {
            foreach (var person in people)
            {
                if (hashtable[person.Email] == null)
                {
                    hashtable.Add(person.Email, person);
                }
            }
        }

        public void ClearHash()
        {
            hashtable.Clear();
        }

        public string Serialize(object obj)
        {
            return JsonConvert.SerializeObject(obj);
        }

        public byte[] GetBytes(string str)
        {
            var bytes = new byte[str.Length * sizeof(char)];
            Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
            return bytes;
        }
    }
}