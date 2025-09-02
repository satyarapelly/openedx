# Customer Impact Query


<table border="1">
  <tr>
    <th>Title</th>
    <th>Query</th>
    <th>Notes</th>
  </tr>
  <tr>
    <td>Service side customer impact</td>
    <td>Execute the following commands in the specified environments:<br> <strong><a href="https://dataexplorer.azure.com/clusters/https%3a%2f%2fpst.kusto.windows.net/databases/Prod?query=H4sIAAAAAAAEAI1SwW4TQQy9I%2fEP1p52pSiVgEslQAopanMIRM024la5s04y7e548XgbUvHxeLMhCVsBvc08P9vPz76m7w1FzamkilS2r1%2f9hM2ahCCfTD%2fP89F0Bh8BV5y%2bWWcYihP4PQTepNkxI2BFEBVF48brGpKpd8KRlzocc1WROBrmgs6H1XBelsPZtznJo3c0CY4rQ7%2fWJKieQwLHqso%2baAoFKt7eYaSL9lELKzsu54raxDEXlJnMd%2bfnbZ4F78npUeoA3OMnS4UPcEk6XrTv1C0ywxeDrvLMVAeSQa8P%2f1b0xWbrB5%2bL6DPMixXpjfh94Lpz%2b4qwOPTaYxek6Mt4AGPNIVKP2YF%2fUkfOcRN0UvSbl6gU3HYaW09iU1Uo%2folgx04zuNs%2by%2b9ceokHe7uOW9rVuLUdvIWzMxhF62cLBV2jgthheYqAxtuwPFigTSx8VB9sT%2f9v15%2fxpD39sCkLqDsk39btlv1ymaanTNMXzLQQIXHdKXoso7JQTIAF%2fsLFp0YosTtJxoesZPcLNiBJkv3b25d4ODjV%2fgs5qqscjwMAAA%3d%3d">[Web]</a></strong> <strong><a href="https://pst.kusto.windows.net/Prod?query=H4sIAAAAAAAEAI1SwW4TQQy9I%2fEP1p52pSiVgEslQAopanMIRM024la5s04y7e548XgbUvHxeLMhCVsBvc08P9vPz76m7w1FzamkilS2r1%2f9hM2ahCCfTD%2fP89F0Bh8BV5y%2bWWcYihP4PQTepNkxI2BFEBVF48brGpKpd8KRlzocc1WROBrmgs6H1XBelsPZtznJo3c0CY4rQ7%2fWJKieQwLHqso%2baAoFKt7eYaSL9lELKzsu54raxDEXlJnMd%2bfnbZ4F78npUeoA3OMnS4UPcEk6XrTv1C0ywxeDrvLMVAeSQa8P%2f1b0xWbrB5%2bL6DPMixXpjfh94Lpz%2b4qwOPTaYxek6Mt4AGPNIVKP2YF%2fUkfOcRN0UvSbl6gU3HYaW09iU1Uo%2folgx04zuNs%2by%2b9ceokHe7uOW9rVuLUdvIWzMxhF62cLBV2jgthheYqAxtuwPFigTSx8VB9sT%2f9v15%2fxpD39sCkLqDsk39btlv1ymaanTNMXzLQQIXHdKXoso7JQTIAF%2fsLFp0YosTtJxoesZPcLNiBJkv3b25d4ODjV%2fgs5qqscjwMAAA%3d%3d&web=0">[Desktop]</a></strong> <strong><a href="https://lens.msftcloudes.com/v2/#/discover/query//results?datasource=(cluster:pst.kusto.windows.net,database:Prod,type:Kusto)&query=H4sIAAAAAAAEAI1SwW4TQQy9I%2fEP1p52pSiVgEslQAopanMIRM024la5s04y7e548XgbUvHxeLMhCVsBvc08P9vPz76m7w1FzamkilS2r1%2f9hM2ahCCfTD%2fP89F0Bh8BV5y%2bWWcYihP4PQTepNkxI2BFEBVF48brGpKpd8KRlzocc1WROBrmgs6H1XBelsPZtznJo3c0CY4rQ7%2fWJKieQwLHqso%2baAoFKt7eYaSL9lELKzsu54raxDEXlJnMd%2bfnbZ4F78npUeoA3OMnS4UPcEk6XrTv1C0ywxeDrvLMVAeSQa8P%2f1b0xWbrB5%2bL6DPMixXpjfh94Lpz%2b4qwOPTaYxek6Mt4AGPNIVKP2YF%2fUkfOcRN0UvSbl6gU3HYaW09iU1Uo%2folgx04zuNs%2by%2b9ceokHe7uOW9rVuLUdvIWzMxhF62cLBV2jgthheYqAxtuwPFigTSx8VB9sT%2f9v15%2fxpD39sCkLqDsk39btlv1ymaanTNMXzLQQIXHdKXoso7JQTIAF%2fsLFp0YosTtJxoesZPcLNiBJkv3b25d4ODjV%2fgs5qqscjwMAAA%3d%3d&runquery=1">[Web (Lens)]</a></strong> <strong><a href="https://pst.kusto.windows.net/Prod?query=H4sIAAAAAAAEAI1SwW4TQQy9I%2fEP1p52pSiVgEslQAopanMIRM024la5s04y7e548XgbUvHxeLMhCVsBvc08P9vPz76m7w1FzamkilS2r1%2f9hM2ahCCfTD%2fP89F0Bh8BV5y%2bWWcYihP4PQTepNkxI2BFEBVF48brGpKpd8KRlzocc1WROBrmgs6H1XBelsPZtznJo3c0CY4rQ7%2fWJKieQwLHqso%2baAoFKt7eYaSL9lELKzsu54raxDEXlJnMd%2bfnbZ4F78npUeoA3OMnS4UPcEk6XrTv1C0ywxeDrvLMVAeSQa8P%2f1b0xWbrB5%2bL6DPMixXpjfh94Lpz%2b4qwOPTaYxek6Mt4AGPNIVKP2YF%2fUkfOcRN0UvSbl6gU3HYaW09iU1Uo%2folgx04zuNs%2by%2b9ceokHe7uOW9rVuLUdvIWzMxhF62cLBV2jgthheYqAxtuwPFigTSx8VB9sT%2f9v15%2fxpD39sCkLqDsk39btlv1ymaanTNMXzLQQIXHdKXoso7JQTIAF%2fsLFp0YosTtJxoesZPcLNiBJkv3b25d4ODjV%2fgs5qqscjwMAAA%3d%3d&saw=1">[Desktop (SAW)]</a></strong>  
      <a href="https://pst.kusto.windows.net/Prod">https://pst.kusto.windows.net/Prod</a><br><br>
      <pre><code>
RequestTelemetry
| where TIMESTAMP > ago(2h) and TIMESTAMP < now()
| where name startswith "Microsoft.Commerce.Tracing.Sll.PXServiceIncomingOperation"
| where toint(data_baseData_protocolStatusCode) > 499
| project TIMESTAMP, cvBase = GetCVBase(cV), cV, data_Partner, data_baseData_operationName, 
  data_baseData_protocolStatusCode, data_baseData_targetUri, data_RequestHeader, 
  data_RequestDetails, data_ResponseHeader, data_ResponseDetails, data_AccountId, 
  data_baseData_latencyMs
| summarize count() by data_AccountId, cvBase, data_baseData_operationName, data_Partner
| where count_ > 3 // Assuming that retries are working
| distinct data_baseData_operationName, data_AccountId, data_Partner
| extend partnerType = iff((data_Partner contains "commercialstores" or data_Partner contains "azure"), 
  "Commercial", "Consumer")
| summarize count() by data_baseData_operationName, data_Partner, partnerType
</code></pre>
    </td>
    <td>
      <strong>Example API Names:</strong><br>
      - <code>PaymentMethodDescriptionsController-GET</code><br>
      - <code>PaymentSessionsController-POST-CreateAndAuthenticate</code>
    </td>
  </tr>

  <tr>
    <td>Reliability based on Region and operation</td>
    <td><a href="https://portal.microsoftgeneva.com/dashboard/paymentexperience-metrics-prod/Service%2520QoS?overrides=%5b%7b%22query%22:%22//*%5bid%3D%27OperationName%27%5d%22,%22key%22:%22value%22,%22replacement%22:%22%22%7d,%7b%22query%22:%22//*%5bid%3D%27CloudLocation%27%5d%22,%22key%22:%22value%22,%22replacement%22:%22%22%7d,%7b%22query%22:%22//*%5bid%3D%27CloudRole%27%5d%22,%22key%22:%22value%22,%22replacement%22:%22%22%7d,%7b%22query%22:%22//*%5bid%3D%27RoleInstance%27%5d%22,%22key%22:%22value%22,%22replacement%22:%22%22%7d,%7b%22query%22:%22//*%5bid%3D%27CloudRoleInstance%27%5d%22,%22key%22:%22value%22,%22replacement%22:%22%22%7d,%7b%22query%22:%22//*%5bid%3D%27CallerName%27%5d%22,%22key%22:%22value%22,%22replacement%22:%22%22%7d%5d%20"> Service QoS | Jarvis (microsoftgeneva.com)</a></td>
    <td></td>
  </tr>
  <tr>
    <td>Outgoing calls failure (To assess if our service is failing or downstream is failing)</td>
    <td>Execute the following commands in the specified environments:<br>
    <strong><a href="https://dataexplorer.azure.com/clusters/https%3a%2f%2fpst.kusto.windows.net/databases/Prod?query=H4sIAAAAAAAEAI1RsU7DQAzdkfiHU6ZEijoglg4glRZBh0BpQlfkXqz0UHIXzk4hiI%2fnTokaSBGwWX5%2bfs%2fPa3xpkDjDEitk256efIjXHVoU2TK5TrNZshKXAgoTnu2iAUxNYyXeQYVUg0ShtAiDGtoKNeNbjVahlliagmpr8uALMweGpy0QZm2NQhrNoDSJIKHJg6HJfcOFUbpI0e6VxHVnLxj47FAOhzULXzgRNtKUKQM3NDc5Rs71%2bXTqeQ58RsnDQbGQ%2bytHFRfiBnm%2b8XUoN5Hrb%2bJu8wosa7TxSMe4y4CV0f7yMXhsYjzBYAvkR6t6oL%2fuFiE%2faPW9BbpcSjo0qTaacDTZNb%2bPzqQ0jeZlPhYvgd1P2oR8JtRUFVj17j%2fgpsNIbNsjfpfSfzLo4%2fKbc0WstIv7b9ZB6ndHP%2b%2f4BBTwaWq5AgAA">[Web]</a></strong> <strong><a href="https://pst.kusto.windows.net/Prod?query=H4sIAAAAAAAEAI1RsU7DQAzdkfiHU6ZEijoglg4glRZBh0BpQlfkXqz0UHIXzk4hiI%2fnTokaSBGwWX5%2bfs%2fPa3xpkDjDEitk256efIjXHVoU2TK5TrNZshKXAgoTnu2iAUxNYyXeQYVUg0ShtAiDGtoKNeNbjVahlliagmpr8uALMweGpy0QZm2NQhrNoDSJIKHJg6HJfcOFUbpI0e6VxHVnLxj47FAOhzULXzgRNtKUKQM3NDc5Rs71%2bXTqeQ58RsnDQbGQ%2bytHFRfiBnm%2b8XUoN5Hrb%2bJu8wosa7TxSMe4y4CV0f7yMXhsYjzBYAvkR6t6oL%2fuFiE%2faPW9BbpcSjo0qTaacDTZNb%2bPzqQ0jeZlPhYvgd1P2oR8JtRUFVj17j%2fgpsNIbNsjfpfSfzLo4%2fKbc0WstIv7b9ZB6ndHP%2b%2f4BBTwaWq5AgAA&web=0">[Desktop]</a></strong> <strong><a href="https://lens.msftcloudes.com/v2/#/discover/query//results?datasource=(cluster:pst.kusto.windows.net,database:Prod,type:Kusto)&query=H4sIAAAAAAAEAI1RsU7DQAzdkfiHU6ZEijoglg4glRZBh0BpQlfkXqz0UHIXzk4hiI%2fnTokaSBGwWX5%2bfs%2fPa3xpkDjDEitk256efIjXHVoU2TK5TrNZshKXAgoTnu2iAUxNYyXeQYVUg0ShtAiDGtoKNeNbjVahlliagmpr8uALMweGpy0QZm2NQhrNoDSJIKHJg6HJfcOFUbpI0e6VxHVnLxj47FAOhzULXzgRNtKUKQM3NDc5Rs71%2bXTqeQ58RsnDQbGQ%2bytHFRfiBnm%2b8XUoN5Hrb%2bJu8wosa7TxSMe4y4CV0f7yMXhsYjzBYAvkR6t6oL%2fuFiE%2faPW9BbpcSjo0qTaacDTZNb%2bPzqQ0jeZlPhYvgd1P2oR8JtRUFVj17j%2fgpsNIbNsjfpfSfzLo4%2fKbc0WstIv7b9ZB6ndHP%2b%2f4BBTwaWq5AgAA&runquery=1">[Web (Lens)]</a></strong> <strong><a href="https://pst.kusto.windows.net/Prod?query=H4sIAAAAAAAEAI1RsU7DQAzdkfiHU6ZEijoglg4glRZBh0BpQlfkXqz0UHIXzk4hiI%2fnTokaSBGwWX5%2bfs%2fPa3xpkDjDEitk256efIjXHVoU2TK5TrNZshKXAgoTnu2iAUxNYyXeQYVUg0ShtAiDGtoKNeNbjVahlliagmpr8uALMweGpy0QZm2NQhrNoDSJIKHJg6HJfcOFUbpI0e6VxHVnLxj47FAOhzULXzgRNtKUKQM3NDc5Rs71%2bXTqeQ58RsnDQbGQ%2bytHFRfiBnm%2b8XUoN5Hrb%2bJu8wosa7TxSMe4y4CV0f7yMXhsYjzBYAvkR6t6oL%2fuFiE%2faPW9BbpcSjo0qTaacDTZNb%2bPzqQ0jeZlPhYvgd1P2oR8JtRUFVj17j%2fgpsNIbNsjfpfSfzLo4%2fKbc0WstIv7b9ZB6ndHP%2b%2f4BBTwaWq5AgAA&saw=1">[Desktop (SAW)]</a></strong> <a href="https://pst.kusto.windows.net/Prod">https://pst.kusto.windows.net/Prod</a><br><br>
      <pre><code>
RequestTelemetry
| where TIMESTAMP > ago(2h)
| where SourceNamespace in ("paymentexperiencelogsprod")
| where data_baseType contains "Ms.Qos.OutgoingServiceRequest"
| where toint(data_baseData_protocolStatusCode) > 499
| project TIMESTAMP, cvBase = GetCVBase(cV), cV, data_Partner, data_baseData_operationName, 
  data_baseData_protocolStatusCode, data_baseData_targetUri, data_RequestHeader, 
  data_RequestDetails, data_ResponseHeader, data_ResponseDetails, data_AccountId, 
  data_baseData_latencyMs
| summarize count() by data_AccountId, cvBase, data_baseData_operationName, data_Partner
| distinct data_baseData_operationName, data_AccountId
| summarize count() by data_baseData_operationName
</code></pre>
    </td>
    <td>To determine if the failure is within our service or downstream.</td>
  </tr>

  <tr>
    <td>Client side customer impact</td>
    <td>
      Execute the following commands in the specified environments:<br>
    <strong><a href="https://dataexplorer.azure.com/clusters/https%3a%2f%2fpst.kusto.windows.net/databases/Prod?query=H4sIAAAAAAAEAFXNwQrCMAwG4Huh7xB26nSDdjoExZOi7DYQH6CsORS7CWkGCj68XT15CUn4%2fiQgQ2RLDEdwlpH9iKrRzbbWpjYGjN63m%2fIgRUgQJ5fYj69hN6b1Fbn3Ltzc42J9mAnPyKmJKqtqiVRQrIq%2fEt%2bRcUSiJxWlFB%2fAFy%2b354iUH9BgWd3T1KX0KXicuOuzdD6ynwbOVgopvvoxdjrBAAAA">[Web]</a></strong> <strong><a href="https://pst.kusto.windows.net/Prod?query=H4sIAAAAAAAEAFXNwQrCMAwG4Huh7xB26nSDdjoExZOi7DYQH6CsORS7CWkGCj68XT15CUn4%2fiQgQ2RLDEdwlpH9iKrRzbbWpjYGjN63m%2fIgRUgQJ5fYj69hN6b1Fbn3Ltzc42J9mAnPyKmJKqtqiVRQrIq%2fEt%2bRcUSiJxWlFB%2fAFy%2b354iUH9BgWd3T1KX0KXicuOuzdD6ynwbOVgopvvoxdjrBAAAA&web=0">[Desktop]</a></strong> <strong><a href="https://lens.msftcloudes.com/v2/#/discover/query//results?datasource=(cluster:pst.kusto.windows.net,database:Prod,type:Kusto)&query=H4sIAAAAAAAEAFXNwQrCMAwG4Huh7xB26nSDdjoExZOi7DYQH6CsORS7CWkGCj68XT15CUn4%2fiQgQ2RLDEdwlpH9iKrRzbbWpjYGjN63m%2fIgRUgQJ5fYj69hN6b1Fbn3Ltzc42J9mAnPyKmJKqtqiVRQrIq%2fEt%2bRcUSiJxWlFB%2fAFy%2b354iUH9BgWd3T1KX0KXicuOuzdD6ynwbOVgopvvoxdjrBAAAA&runquery=1">[Web (Lens)]</a></strong> <strong><a href="https://pst.kusto.windows.net/Prod?query=H4sIAAAAAAAEAFXNwQrCMAwG4Huh7xB26nSDdjoExZOi7DYQH6CsORS7CWkGCj68XT15CUn4%2fiQgQ2RLDEdwlpH9iKrRzbbWpjYGjN63m%2fIgRUgQJ5fYj69hN6b1Fbn3Ltzc42J9mAnPyKmJKqtqiVRQrIq%2fEt%2bRcUSiJxWlFB%2fAFy%2b354iUH9BgWd3T1KX0KXicuOuzdD6ynwbOVgopvvoxdjrBAAAA&saw=1">[Desktop (SAW)]</a></strong>https://pst.kusto.windows.net/Prod
      <pre><code>
let start = datetime(2024-01-11 10:53);
let end = start + 7m;
GetPidlSdkFailureDetails(start, end, "*", "*", "*", "systemerror")
| extend user = strcat(UserId, ClientIP)
| distinct user<br><br>
<table border="1" style="border-collapse: collapse; width: 100%;">
  <thead>
    <tr style="background-color: lightgray;">
      <th style="padding: 8px; text-align: left;">user</th>
    </tr>
  </thead>
  <tbody>
    <tr><td style="padding: 8px;">101.142.192.243</td></tr>
    <tr><td style="padding: 8px;">t:1599163D073264DF1C49023E0657650B95.85.94.66</td></tr>
    <tr><td style="padding: 8px;">t:227916F9349C6F5D33D302FA359C6E4735.76.140.231</td></tr>
    <tr><td style="padding: 8px;">t:0A0DC2BF9C1264CE160FD3C89D686554219.126.113.62</td></tr>
    <tr><td style="padding: 8px;">t:2D82739C5D486DAE1A5E679F59486B1C138.64.71.29</td></tr>
    <tr><td style="padding: 8px;">t:3F8A256353F5639A2774316057F5655749.98.245.163</td></tr>
    <tr><td style="padding: 8px;">t:12827091E0066345214B6371E4066559185.183.146.44</td></tr>
    <tr><td style="padding: 8px;">122.24.227.81</td></tr>
  </tbody>
</table>

</code></pre>
    </td>
    <td>
      Sometimes <code>UserId</code> is logged as empty. Concatenate <code>UserId</code> and <code>ClientIP</code> to create unique identifiers.<br><br>
      </td>
  </tr>
</table>
