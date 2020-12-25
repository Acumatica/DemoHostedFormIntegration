# AcumaticaDemoHostedFormIntegration

This project is a demo implementation of the Credit Card Plug-in for Acumatica 2020R2. It is only implements the interfaces related to to the Hosted Form functionality.
The purpose of this project is to demonstrate the way how the data could be transfered from Acumatica to Hosted (Payment) Form, 
and how the data that initially appered in hosted form context (e.g. Processing Center response with vaulted Card token) could be retrieved back to Acumatica.
This implementation include both scenarios creation of Payment Profile on Acumatica side and Authorizing/Capturing funds using New Card feature. 

It is attempted to recreate most complex scenario and therefore:

1. Should work on 2020R2
   Since Sales Order Card Prodessing in this version was heavely refactored and processing executed in a separated thread the Http.Request.Current is null.
2. Not using cookies
   The approach of using cookies to pass the data to hosted form and retrieve it back, might be looking feasible, however with recent
   changing default browser settings for SameSite to 'lax' and not beind able to access the data in Http.Request.Current this approach is no longer working.
   At all it is not an ideal situation when the business logic depends on the browser used.
3. Working both on SalesOrder and creation via CMP
   The interfaces related to Hosted form and Hosted Payment Form has different data availability. Addititonally Hosted Form (not Hosted Payment Form) is no longer called on Sales Orders.
4. Should use custom Payment Connector Frame
   The implementation of the Plug-In for Processing Center that does not support Hosted Form functionality usually fallback to using js scripts that will require to build the custom Payment Connector Frame.
   The main difference between the built-in connetor and a custom one is that build-in connector is hardcoded in web.config of the Acuamtica ERP site and therefore lives within session and immune 
   to the SameSite defaults of modern browsers.
   
The content of this repo includes:
    The codefiles wihe implementation of nessecary interfaces for Hosted (Payment) Form functionality
    Custom Payment Connector - Frames/CLPaymentConnector 
    Customization project with CC-Plugin that containes compiled sources and custom frame, that can be deployed and tested
    
The project does not contain the fuctionality to demonatrate the transactiong of already "vaulted" cards like capturing previously authorized tranaction as DoTransaction is not implemented.
It also not including the any other implementation of methods that would enable actions like Extrenally Authorized, Manual Capture, Validate CC, etc as it is not in the scope of this project.

