﻿// ----------------------------------------------------------------------------------
//
// Copyright Microsoft Corporation
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// ----------------------------------------------------------------------------------

using Microsoft.Azure.Commands.Common.Resources;

namespace Microsoft.Azure.Commands.ResourceManager.Cmdlets.Extensions
{
    using Commands.Utilities.Common;
    using Microsoft.Azure.Commands.ResourceManager.Cmdlets.Components;
    using Microsoft.Azure.Commands.ResourceManager.Cmdlets.Entities.Resources;
    using Models;
    using Newtonsoft.Json.Linq;
    using System.Collections.Generic;
    using System.Linq;
    using System.Management.Automation;
    /// <summary>
    /// A helper class that handles common tasks that deal with the <see cref="Resource{JToken}"/> class.
    /// </summary>
    internal static class ResourceExtensions
    {
        internal static PSResourceObject ToPSResourceObject(this Resource<JToken> resource)
        {
            var resourceType = string.IsNullOrEmpty(resource.Id)
                ? null
                : ResourceIdUtility.GetResourceType(resource.Id);

            var extensionResourceType = string.IsNullOrEmpty(resource.Id)
                ? null
                : ResourceIdUtility.GetExtensionResourceType(resource.Id);

            var objectDefinition = new Dictionary<string, object>
            {
                { "ExtensionResourceName", string.IsNullOrEmpty(resource.Id) ? null : ResourceIdUtility.GetExtensionResourceName(resource.Id) },
                { "ExtensionResourceType", extensionResourceType },
                { "Kind", resource.Kind },
                { "Plan", resource.Plan == null ? null : resource.Plan.ToJToken() },
                { "Properties", (ResourceExtensions.GetProperties(resource) as PSObject).SerializeAsJson() },
                { "CreatedTime", resource.CreatedTime },
                { "ChangedTime", resource.ChangedTime },
                { "ETag", resource.ETag },
                { "Sku", resource.Sku == null ? null : resource.Sku.ToJToken() },
            };

            var psObject = new PSResourceObject();
            psObject.Name = resource.Name;
            psObject.ResourceId = string.IsNullOrEmpty(resource.Id) ? null : resource.Id;
            psObject.ResourceName = string.IsNullOrEmpty(resource.Id) ? null : ResourceIdUtility.GetResourceName(resource.Id);
            psObject.ResourceType = resourceType;
            psObject.ResourceGroupName = string.IsNullOrEmpty(resource.Id) ? null : ResourceIdUtility.GetResourceGroupName(resource.Id);
            psObject.Location = resource.Location;
            psObject.SubscriptionId = string.IsNullOrEmpty(resource.Id) ? null : ResourceIdUtility.GetSubscriptionId(resource.Id);
            psObject.Tags = TagsHelper.GetTagsHashtables(resource.Tags);


            var objectProperties = objectDefinition.Where(kvp => kvp.Value != null).SelectManyArray(kvp => new[] { kvp.Key, kvp.Value });

            for(int i=0; i< objectProperties.Length; i+=2)
            {
                psObject.Properties.Add(objectProperties[i].ToString(), objectProperties[i + 1].ToString());
            }
            
            return psObject;
        }

        /// <summary>
        /// Gets the properties object
        /// </summary>
        /// <param name="resource">The <see cref="Resource{JToken}"/> object.</param>
        private static object GetProperties(Resource<JToken> resource)
        {
            if (resource.Properties == null)
            {
                return null;
            }

            return (object)resource.Properties.ToPsObject();
        }

        /// <summary>
        /// Converts a <see cref="JToken"/> to a <see cref="Resource{JToken}"/>.
        /// </summary>
        /// <param name="jtoken">The <see cref="JToken"/>.</param>
        internal static Resource<JToken> ToResource(this JToken jtoken)
        {
            return jtoken.ToObject<Resource<JToken>>(JsonExtensions.JsonMediaTypeSerializer);
        }

        /// <summary>00
        /// Converts a <see cref="JToken"/> to a <see cref="Resource{JToken}"/>.
        /// </summary>
        /// <typeparam name="TType">The type of the properties.</typeparam>
        /// <param name="jtoken">The <see cref="JToken"/>.</param>
        internal static Resource<TType> ToResource<TType>(this JToken jtoken)
        {
            return jtoken.ToObject<Resource<TType>>(JsonExtensions.JsonMediaTypeSerializer);
        }
    }
}