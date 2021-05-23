using System;
using Microsoft.Xrm.Sdk;
using System.ServiceModel;
using Microsoft.Xrm.Sdk.Query;

namespace MyPlugins
{
    public class PreventTenRecords : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            ITracingService tracingService =
            (ITracingService)serviceProvider.GetService(typeof(ITracingService));

            IPluginExecutionContext context = (IPluginExecutionContext)
                serviceProvider.GetService(typeof(IPluginExecutionContext));

            if (context.InputParameters.Contains("Target") &&
                context.InputParameters["Target"] is Entity)
            {
                Entity rent = (Entity)context.InputParameters["Target"];

                IOrganizationServiceFactory serviceFactory =
                    (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
                IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);

                try
                {
                    const int RENTING_STATUS = 279640002;
                    int statusReason = ((OptionSetValue)rent.Attributes["statuscode"]).Value;

                    if (statusReason == RENTING_STATUS)
                    {
                        Guid ownerId = (rent.Attributes["ownerid"] as EntityReference).Id;
                        QueryExpression query = new QueryExpression("new_rent")
                        {
                            ColumnSet = new ColumnSet(new string[] { "ownerid" })
                        };
                        query.Criteria.AddCondition("ownerid", ConditionOperator.Equal, ownerId);
                        query.Criteria.AddCondition("statuscode", ConditionOperator.Equal, RENTING_STATUS);

                        EntityCollection collection = service.RetrieveMultiple(query);

                        if (collection.Entities.Count >= 10)
                        {
                            throw new InvalidPluginExecutionException("Forbidden to create new rent. You have exceeded the limit(10) of rents in status 'Renting'. Number of records you have created:" + collection.Entities.Count);
                        }
                    }
                }

                catch (FaultException<OrganizationServiceFault> ex)
                {
                    throw new InvalidPluginExecutionException("An error occurred in FollowUpPlugin.", ex);
                }

                catch (Exception ex)
                {
                    tracingService.Trace("FollowUpPlugin: {0}", ex.ToString());
                    throw;
                }
            }
        }

    }
}
