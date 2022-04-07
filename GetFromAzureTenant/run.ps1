
using namespace System.Net

# Input bindings are passed in via param block.
param($Request, $TriggerMetadata)

# Define the command-line parameters to be used by the script


$serverHost = $ENV:NableHostname
$JWT = $ENV:JWTKey2
$JWT0 = $ENV:JWTKey
$AzureTenantGUID = $Request.Query.ID

#[Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12
#Register-PSRepository -Default

#if ( -Not (Get-Module PS-NCentral)) {
#    Install-Module -Name PS-NCentral
#}


function ProcessData1([Array]$InArray,[String]$PairClass){
    #Write-Host "Process Data"	
    ## Received Dataset KeyPairs 2 List/Columns
    $OutObjects = @()
    
    if ($InArray){
        foreach ($InObject in $InArray) {

#				$ThisObject = New-Object PSObject				## In this routine the object is created at start. Properties are added with values.
            $Props = @{}									## In this routine the object is created at the end. Properties from a list.

            ## Add a Reference-Column at Object-Level
            If ($PairClass -eq "Properties"){
                ## CustomerLink if Available
                if(Get-Member -inputobject $InObject -name "CustomerID"){
#						$ThisObject | Add-Member -MemberType NoteProperty -Name 'CustomerID' -Value $InObject.CustomerID -Force
                    $Props.add('CustomerID',$InObject.CustomerID)
                }
                
                ## DeviceLink if Available
                if(Get-Member -inputobject $InObject -name "DeviceID"){
#						$ThisObject | Add-Member -MemberType NoteProperty -Name 'DeviceID' -Value $InObject.DeviceID -Force
                    $Props.add('DeviceID',$InObject.DeviceID)
                }
            }

            ## Convert all (remaining) keypairs to Properties
            foreach ($item in $InObject.$PairClass) {

                ## Cleanup the Key and/or Value before usage.
                If ($PairClass -eq "Properties"){
                    $Header = $item.label
                }
                Else{
                    If($item.key.split(".")[0] -eq 'asset'){	##Should use ProcessData2 (ToDo)
                        $Header = $item.key
                    }
                    Else{
                        $Header = $item.key.split(".")[1]
                    }
                }

                ## Ensure a Flat Value
                If ($item.value -is [Array]){
                    $DataValue = $item.Value[0]
                }
                Else{
                    $DataValue = $item.Value
                }

                ## Now add the Key/Value pairs.
#					$ThisObject | Add-Member -MemberType NoteProperty -Name $Header -Value $DataValue -Force

                 # if a key is found that already exists in the hashtable
                if ($Props.ContainsKey($Header)) {
                    # either overwrite the value 'Last-One-Wins'
                    # or do nothing 'First-One-Wins'
                    #if ($this.allowOverwrite) { $Props[$Header] = $DataValue }
                }
                else {
                    $Props[$Header] = $DataValue
                }					
#					$Props.add($Header,$DataValue)

            }
            $ThisObject = New-Object -TypeName PSObject -Property $Props	#Alternative option

            ## Add the Object to the list
            $OutObjects += $ThisObject
        }
    }
    ## Return the list of Objects
    Return $OutObjects
#		$OutObjects
#		Write-Output $OutObjects
}

function Get-Properties([String] $CustomerID) {
$PropsRestBody = @"
<soap:Envelope xmlns:soap="http://www.w3.org/2003/05/soap-envelope" xmlns:ei2="http://ei2.nobj.nable.com/">
   <soap:Header/>
   <soap:Body>
      <ei2:organizationPropertyList>
          <ei2:username>$null</ei2:username>
          <ei2:password>$JWT</ei2:password>
         <ei2:customerIds>$CustomerID</ei2:customerIds>
         <ei2:reverseOrder>false</ei2:reverseOrder>
      </ei2:organizationPropertyList>
   </soap:Body>
</soap:Envelope>
"@

Try {
    $PropertiesReturn = (Invoke-RestMethod -Uri $bindingURL -body $PropsRestBody -Method POST).Envelope.body.organizationPropertyListResponse.return
    $PropertiesList = ProcessData1 $PropertiesReturn "properties"
}
Catch {
    Write-Host "Could not connect: $($_.Exception.Message)"
    exit
}
return $PropertiesList
}

#Connect to NC
#New-NCentralConnection -ServerFQDN $serverHost -JWT $JWT | Out-Null

$CustRestBody = 
@"
<soap:Envelope xmlns:soap="http://www.w3.org/2003/05/soap-envelope" xmlns:ei2="http://ei2.nobj.nable.com/">
   <soap:Header/>
   <soap:Body>
      <ei2:customerList>
          <ei2:username>$null</ei2:username>
          <ei2:password>$JWT0</ei2:password>
         <ei2:settings>
            <ei2:key>listSOs</ei2:key>
            <ei2:value>false</ei2:value>
         </ei2:settings>
      </ei2:customerList>
   </soap:Body>
</soap:Envelope>
"@ 
# Bind to the namespace, using the Webserviceproxy
$bindingURL = "https://" + $serverHost + "/dms2/services2/ServerEI2?wsdl"

Try {
    Write-Host "ID: $($AzureTenantGUID)"
    Write-Host "Getting Customers Table"
    $customerlist = (Invoke-RestMethod -Uri $bindingURL -body $CustRestBody -Method POST).Envelope.body.customerListResponse.return
}
Catch {
    #try again, wake up ncentral
    Try {
        Write-Host "ID: $($AzureTenantGUID)"
        Write-Host "Getting Customers Table"
        $customerlist = (Invoke-RestMethod -Uri $bindingURL -body $CustRestBody -Method POST).Envelope.body.customerListResponse.return
    }
    Catch {
        Write-Host "Could not connect: $($_.Exception.Message)"
        Push-OutputBinding -Name Response -Value ([HttpResponseContext]@{
            StatusCode = [HttpStatusCode]::BadGateway
            Body       = "Could not connect: $($_.Exception.Message)"
        })
    }
}
# Set up the "Customers" array, then populate
$Customers = ForEach ($Entity in $CustomerList) {
    $CustomerAssetInfo = @{}
    ForEach ($item in $Entity.Items) { $CustomerAssetInfo[$item.key] = $item.Value }
    [PSCustomObject]@{
        ID                = $CustomerAssetInfo["customer.customerid"]
        RegistrationToken = $CustomerAssetInfo["customer.registrationtoken"]
    }
}

#$Customers | Out-GridView

#$PropertiesList | Out-GridView
#return

#Merge
Write-Host "Finding Tenant ID"
foreach ($Customer in $Customers) {
    #Write-Host "querying: $($Customer.id)"
    $Properties = Get-Properties $Customer.id
    if ($Null -eq $Properties) {
        continue;
    }
    $AzureTenantProperty = $Properties.psobject.properties['AzureTenantGUID']

    if (($AzureTenantProperty) -and ($AzureTenantGUID -eq $AzureTenantProperty.Value)) {
        $retVal = "" | Select-Object customerid, regtoken
        $retVal.customerid = $Customer.id
        $retVal.regtoken = $Customer.registrationtoken
        $json = $retVal | ConvertTo-Json

        Write-Host "CustomerID: $($Customer.id)"
        #return $Customer.registrationtoken
        # Associate values to output bindings by calling 'Push-OutputBinding'.
        Push-OutputBinding -Name Response -Value ([HttpResponseContext]@{
            StatusCode = [HttpStatusCode]::OK
            Body       = $json
        })
        return;
    }
    #$CustomerReport.Add($ReportItem) > $Null
}

#return "fail"
# Associate values to output bindings by calling 'Push-OutputBinding'.
Push-OutputBinding -Name Response -Value ([HttpResponseContext]@{
    StatusCode = [HttpStatusCode]::NotFound
    Body       = $Null
})