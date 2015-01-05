using System;
using Sitecore.Caching;
using Sitecore.Collections;
using Sitecore.Data;
using Sitecore.Data.DataProviders;
using Sitecore.Data.IDTables;
using Sitecore.Data.Items;
using Sitecore.Data.Managers;
using Sitecore.Data.Templates;
using Sitecore.Diagnostics;
using Sitecore.Poc.ServiceDataProvider.Models;
using Sitecore.Poc.ServiceDataProvider.Repositories;
using Sitecore.Reflection;

namespace Sitecore.Poc.ServiceDataProvider.Providers
{
    public class PeopleProvider : Base.ServiceDataProvider
    {
        private readonly PeopleRepository _peopleRepository;
        private readonly string _idTablePrefix;
        private readonly ID _templateId;
        private readonly ID _parentTemplateId;

        public PeopleProvider(string idTablePrefix, string templateId, string parentTemplateId, string apiEndpoint)
        {
            Assert.IsNotNullOrEmpty(idTablePrefix, "idTablePrefix");
            Assert.IsNotNullOrEmpty(templateId, "templateId");
            Assert.IsNotNullOrEmpty(parentTemplateId, "parentTemplateId");
            Assert.IsNotNullOrEmpty(apiEndpoint, "apiEndpoint");

            _idTablePrefix = idTablePrefix;
            _templateId = ID.Parse(templateId);
            _parentTemplateId = ID.Parse(parentTemplateId);
            _peopleRepository = new PeopleRepository(apiEndpoint);

            Initialize(_idTablePrefix, _templateId, _parentTemplateId);
        }

        public override ItemDefinition GetItemDefinition(ID itemId, CallContext context)
        {
            if (CanProcessItemId(itemId, context))
            {
                var key = GetIdTableKey(itemId);
                var person = _peopleRepository.GetPerson(key);
                if (person != null)
                {
                    var itemName = string.Format("{0} {1}", person.FirstName, person.LastName);
                    var item = new ItemDefinition(itemId, itemName, _templateId, ID.Null);
                    try
                    {
                        ItemCache itemCache = CacheManager.GetItemCache(context.DataManager.Database);
                        if (itemCache != null)
                        {
                            ReflectionUtil.CallMethod(itemCache, "RemoveItem", true, false, false, new object[] { item.ID });
                        }
                    }
                    catch (Exception exception)
                    {
                        Log.Error("Can't clear cache for item", exception, this);
                    }
                    ((ICacheable)item).Cacheable = false;
                    return item;
                }
            }
            return base.GetItemDefinition(itemId, context);
        }

        public override IDList GetChildIDs(ItemDefinition item, CallContext context)
        {
            if (CanProcessParent(item, context))
            {
                var idList = new IDList();
                _peopleRepository.ClearHash();
                var people = _peopleRepository.GetAllPeople();
                foreach (var person in people)
                {
                    if (string.IsNullOrEmpty(person.Email)) continue;
                    idList.Add(CreateOrGetId(_idTablePrefix, person.Email, item));
                }
                context.DataManager.Database.Caches.DataCache.Clear();
                return idList;
            }
            return base.GetChildIDs(item, context);
        }

        public override ID GetParentID(ItemDefinition item, CallContext context)
        {
            if (CanProcessItem(item, context))
            {
                context.Abort();
                var idEntries = IDTable.GetKeys(_idTablePrefix, item.ID);

                if (idEntries != null && idEntries.Length > 0)
                {
                    return idEntries[0].ParentID;
                }
                return null;
            }
            return base.GetParentID(item, context);
        }

        public override FieldList GetItemFields(ItemDefinition item, VersionUri versionUri, CallContext context)
        {
            var fields = new FieldList();
            if (CanProcessItem(item, context))
            {
                var template = TemplateManager.GetTemplate(_templateId, context.DataManager.Database);
                if (template != null)
                {
                    var key = GetIdTableKey(item.ID);
                    var person = _peopleRepository.GetPerson(key);
                    if (person != null)
                    {
                        foreach (var field in GetDataFields(template))
                        {
                            var fieldValue = GetFieldValue(field, person);
                            if (!string.IsNullOrEmpty(fieldValue))
                            {
                                fields.Add(field.ID, fieldValue);
                            }
                        }
                    }
                }
                return fields;
            }
            return base.GetItemFields(item, versionUri, context);
        }

        public override IDList GetPublishQueue(DateTime from, DateTime to, CallContext context)
        {
            var list = new IDList();
            foreach (var idEntry in IDTable.GetKeys(_idTablePrefix))
            {
                list.Add(idEntry.ID);
            }
            return list;
        }

        // This method must return first version for every language to make info appear in content editor.
        public override VersionUriList GetItemVersions(ItemDefinition item, CallContext context)
        {
            if (CanProcessItem(item, context))
            {
                var versionUriList = new VersionUriList();
                foreach (var language in LanguageManager.GetLanguages(context.DataManager.Database))
                {
                    versionUriList.Add(language, Sitecore.Data.Version.First);
                }
                context.Abort();
                return versionUriList;
            }
            return base.GetItemVersions(item, context);
        }

        public override bool SaveItem(ItemDefinition itemDefinition, ItemChanges changes, CallContext context)
        {
            if (CanProcessItem(itemDefinition, context))
            {
                var person = new Person()
                {
                    Email = changes.Item["Email"],
                    FirstName = changes.Item["FirstName"],
                    LastName = changes.Item["LastName"]
                };
                var emailFieldId = changes.Item.Fields["Email"].ID;
                var firstNameFieldId = changes.Item.Fields["FirstName"].ID;
                var lastNameFieldId = changes.Item.Fields["LastName"].ID;

                foreach (FieldChange fieldChange in changes.FieldChanges)
                {
                    if (fieldChange.FieldID == emailFieldId)
                    {
                        person.Email = fieldChange.Value;
                    }
                    if (fieldChange.FieldID == firstNameFieldId)
                    {
                        person.FirstName = fieldChange.Value;
                    }
                    if (fieldChange.FieldID == lastNameFieldId)
                    {
                        person.LastName = fieldChange.Value;
                    }
                }
                return _peopleRepository.UpdatePerson(person.Email, person);
            }
            return base.SaveItem(itemDefinition, changes, context);
        }

        // Should return null in order not to add duplicated languages to common result.
        public override LanguageCollection GetLanguages(CallContext context)
        {
            return null;
        }

        private static string GetFieldValue(TemplateField field, Person person)
        {
            switch (field.Name)
            {
                case "FirstName":
                    return person.FirstName;
                case "LastName":
                    return person.LastName;
                case "Email":
                    return person.Email;
                case "Title":
                    return string.Format("{0}-{1}", person.FirstName, person.LastName);
                case "Abstract":
                    return string.Format("<p>{0}</p>", person.Description);
                case "Body":
                    return string.Format("<p>{0}</p>", person.Description);
                case "Quote":
                    return string.Format("My name is {0}", person.FirstName);
                case "Job Title":
                    return person.JobTitle;
                case "Menu Title":
                    return string.Format("{0}-{1}", person.FirstName, person.LastName);

            }
            return string.Empty;
        }
    }
}