$SubscriptionId = "df2299f6-d601-43d9-8db1-d12ac93e535a";
$deploymentAdSpName = "PX-Deployment-INT-PME";

$context = Get-AzSubscription -SubscriptionId $SubscriptionId 
Set-AzContext $context

$servicePrincipal = Get-AzADServicePrincipal -SearchString $deploymentAdSpName
$objectId = $servicePrincipal.Id

$roleAssignment = Get-AzRoleAssignment -ObjectId $objectId -RoleDefinitionName Contributor -Scope "/subscriptions/$subscriptionId"

if (!$roleAssignment)
{
  New-AzRoleAssignment -ObjectId $objectId -RoleDefinitionName Contributor -Scope "/subscriptions/$SubscriptionId"
}