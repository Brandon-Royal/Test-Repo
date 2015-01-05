using System;
using System.Reflection;

namespace Sitecore.Poc.DataService.ModelDescriptions
{
    public interface IModelDocumentationProvider
    {
        string GetDocumentation(MemberInfo member);

        string GetDocumentation(Type type);
    }
}