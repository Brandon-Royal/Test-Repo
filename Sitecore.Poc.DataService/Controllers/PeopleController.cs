using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using Sitecore.Poc.DataService.Models;
using Sitecore.Poc.DataService.Repositories;

namespace Sitecore.Poc.DataService.Controllers
{
    public class PeopleController : ApiController
    {
        readonly IEntityRepository<Person> _repository;

        public PeopleController(IEntityRepository<Person> repository)
        {
            _repository = repository;
        }

        /// <summary>
        /// Gets all people
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Person> Get()
        {
            return _repository.GetAll();

        }

        /// <summary>
        /// Gets a single person by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Person Get(string id)
        {
            var person = _repository.GetSingle(id);
            if (person == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }
            return person;
        }

        /// <summary>
        /// Updates a person by id
        /// </summary>
        /// <param name="id"></param>
        /// <param name="person"></param>
        public string Put(string id, [FromBody] Person person)
        {
            if (!_repository.Exists(id))
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }
            _repository.UpdateSingle(id, person);
            return string.Format("user {0} updated", id);
        }

        /// <summary>
        /// Adds a person
        /// </summary>
        /// <param name="id"></param>
        /// <param name="person"></param>
        public string Post(string id, [FromBody] Person person)
        {
            if (_repository.Exists(id))
            {
                throw new HttpResponseException(HttpStatusCode.Conflict);
            }

            person.Email = id;
            _repository.AddSingle(id, person);
            return string.Format("user {0} added", id);
        }

        /// <summary>
        /// Deletes a person by id
        /// </summary>
        /// <param name="id"></param>
        public string Delete(string id)
        {
            if (!_repository.Exists(id))
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            _repository.DeleteSingle(id);
            return string.Format("user {0} deleted", id);
        }
    }
}
