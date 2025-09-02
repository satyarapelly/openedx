# Payment Instrument Description Language (PIDL)

## **Polling**
A polling action can be assigned to the action of a DisplayPageHint.  
This tells the `pidlsdk` to make a request every N milliseconds until a desired response is returned.

### **Creating a new poll action**  
`DisplayHintAction pollAction = new DisplayHintAction(DisplayHintActionType.poll.ToString());`    

### **Creating and setting the context, with `ResponseAction` items, for a polling action:**
*A polling Action requires Context.*    
```
PollActionContext pollActionContext = new PollActionContext()
{
    Href = "https://my.example-domain.com/resource/status",
    Method = "GET",
    Interval = 3000,
    CheckPollingTimeOut = false,
    ResponseResultExpression = "myProperty",
};

pollActionContext.AddResponseActionsItem("Succeeded", new DisplayHintAction(DisplayHintActionType.success.ToString()));
pollActionContext.AddResponseActionsItem("Failed", new DisplayHintAction(DisplayHintActionType.success.ToString()));

pollAction.Context = pollActionContext;
```  

In this example, the polling action tells the `pidlsdk` to make a `GET` request to `https://my.example-domain.com/resource/status` every `3000` milliseconds.  
When a response is received, the `pidlsdk` will try to get the value of the key `myProperty` from the response object.  
The `pidlsdk` will then look through the `ResponseActions` for an item whose key matches the value of `myProperty` (In the code example, we added response action items for "Succeeded", "Failed").  
If a match is found then the associated `DisplayHintAction` will be triggered.
  - i.e. if the value of `myProperty` in the response equals "Succeeded" and there is an item with the key value "Succeeded" in the `ResponseActions` list (there is as we added it in the code example above), then pidlsdk will trigger the DisplayHintAction we passed in along with "Succeeded" to `.AddResponseActionItems`.  

If no match is found, then the polling will continue. 

