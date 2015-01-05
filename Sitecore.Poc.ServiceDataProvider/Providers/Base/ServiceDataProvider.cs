using System.Collections.Generic;
using System.Linq;
using Sitecore.Data;
using Sitecore.Data.DataProviders;
using Sitecore.Data.IDTables;
using Sitecore.Data.Items;
using Sitecore.Data.Templates;

namespace Sitecore.Poc.ServiceDataProvider.Providers.Base
{
    public class ServiceDataProvider : DataProvider
    {
        /// <summary>
        /// Initializes fields required for the ServiceDataProvider
        /// </summary>
        /// <param name="idTablePrefix"></param>
        /// <param name="templateId"></param>
        /// <param name="parentTemplateId"></param>
        public void Initialize(string idTablePrefix, ID templateId, ID parentTemplateId)
        {
            IdTablePrefix = idTablePrefix;
            TemplateId = templateId;
            ParentTemplateId = parentTemplateId;
        }

        /// <summary>
        /// Checks of parent is based on the allowed parent template id
        /// </summary>
        /// <param name="item"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public bool CanProcessParent(ItemDefinition item, CallContext context)
        {
            return item != null && ParentTemplateId == item.TemplateID;
        }

        /// <summary>
        /// checks if items is based on allowed template and there is a key in the IDTable
        /// </summary>
        /// <param name="item"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public bool CanProcessItem(ItemDefinition item, CallContext context)
        {
            return item != null && TemplateId == item.TemplateID && IDTable.GetKeys(IdTablePrefix, item.ID).Length > 0;
        }

        /// <summary>
        /// Checks if there are any keys in the IDTable which would indicate the item is applicable for the provider
        /// </summary>
        /// <param name="id"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public bool CanProcessItemId(ID id, CallContext context)
        {
            return IDTable.GetKeys(IdTablePrefix, id).Length > 0;
        }

        /// <summary>
        /// Filters template fields to data fields only (excludes fields of a StandardTemplate data template).
        /// </summary>
        /// <param name="template"></param>
        /// <returns></returns>
        public virtual IEnumerable<TemplateField> GetDataFields(Template template)
        {
            return template.GetFields().Where(ItemUtil.IsDataField);
        }

        public string GetIdTableKey(ID id)
        {
            var keys = IDTable.GetKeys(IdTablePrefix, id);
            if (keys != null && keys.Length > 0)
            {
                return keys[0].Key;
            }
            return string.Empty;
        }

        public static ID CreateOrGetId(string idTablePrefix, string key, ItemDefinition item)
        {
            var idEntry = IDTable.GetID(idTablePrefix, key);
            ID newId;
            if (idEntry == null)
            {
                newId = ID.NewID;
                IDTable.Add(idTablePrefix, key, newId, item.ID, key);
            }
            else
            {
                newId = idEntry.ID;
            }
            return newId;
        }

        public string Initialized { get; protected set; }
        public string IdTablePrefix { get; protected set; }
        public ID TemplateId { get; protected set; }
        public ID ParentTemplateId { get; protected set; }
    }
}